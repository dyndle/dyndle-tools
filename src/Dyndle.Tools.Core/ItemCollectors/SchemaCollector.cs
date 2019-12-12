using log4net;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;
using Dyndle.Tools.Core.Models;
using Dyndle.Tools.Core.Utils;

namespace Dyndle.Tools.Core.ItemCollectors
{
    public class SchemaCollector
    {

        private SessionAwareCoreServiceClient Client { get; set; }
        private ILog log;

        public SchemaCollector(IConfiguration config)
        {
            log = LogManager.GetLogger(this.GetType());
            Logger.Setup(config);
            log.Debug("started SchemaCollector");
            Client = CoreserviceClientFactory.GetClient();
        }


        public IEnumerable<SchemaData> FindSchema(string schemaId)
        {
            log.Info("Called FindSchema with schemaId " + schemaId);
            SchemaData mainSchema = Client.Read(schemaId, new ReadOptions()) as SchemaData;
            IList<SchemaData> allSchemas = new List<SchemaData> { mainSchema };
            AddUsedSchemas(mainSchema, allSchemas);
            return allSchemas;
        }

        public IEnumerable<SchemaData> FindSchemasForFolder(string folderId)
        {
            log.Info("Called FindSchemasForFolder with folderId " + folderId);
            OrganizationalItemItemsFilterData filterData = new OrganizationalItemItemsFilterData();
            filterData.ItemTypes = new[] { ItemType.Schema };
            filterData.Recursive = true;
            XElement resultXml = Client.GetListXml(folderId, filterData);
            IList<SchemaData> convertibleSchemas = GetConvertibleSchemas(resultXml);
            for (var i = 0; i < convertibleSchemas.Count; i++) // using a for-loop (not a foreach) because the collection will be modified
            {
                var schema = convertibleSchemas[i];
                AddUsedSchemas(schema, convertibleSchemas);
            }
            return convertibleSchemas;
        }

        public IEnumerable<SchemaData> FindSchemasForPublication(string publicationId)
        {
            log.Info("Called FindSchemasForPublication with publicationId " + publicationId);
            RepositoryItemsFilterData filterData = new RepositoryItemsFilterData();
            filterData.ItemTypes = new[] { ItemType.Schema };
            filterData.Recursive = true;
            XElement resultXml = Client.GetListXml(publicationId, filterData);
            IList<SchemaData> convertibleSchemas = GetConvertibleSchemas(resultXml);
            return convertibleSchemas;
        }

        private IList<SchemaData> GetConvertibleSchemas(XElement rawList)
        {
            return GetConvertibleSchemas(rawList.Descendants());
        }

        private IList<SchemaData> GetConvertibleSchemas(IEnumerable<XElement> rawList)
        {
            List<SchemaData> list = new List<SchemaData>();
            foreach (var item in rawList)
            {
                try
                {

                    SchemaData schema = (SchemaData)Client.Read(item.Attribute("ID").Value, new ReadOptions());
                    if (schema.Purpose == SchemaPurpose.Embedded)
                    {
                        list.Insert(0, schema);// embedded schemas are always needed by other schemas, it saves us some rounds of parsing if we insert them at the top of the list
                        continue;
                    }
                    if (schema.Purpose == SchemaPurpose.Component || schema.Purpose == SchemaPurpose.Multimedia || schema.Purpose == SchemaPurpose.Metadata)
                    {
                        list.Add(schema);
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("R6"))
                    {
                        log.Error($"cannot process schema {item.Attribute("Title").Value} because it contains elements which were introduced in Tridion 9.0; please call this script with the parameter --tridion9");
                        throw e; // TODO: handle gracefully
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            return list;
        }

        private void AddUsedSchemas(SchemaData mainSchema, IList<SchemaData> allSchemas)
        {
            log.Info($"Called AddUsedSchemas to find dependent schemas for {mainSchema.Title} - total number of schemas found so far: {allSchemas.Count()}");
            UsedItemsFilterData filter = new UsedItemsFilterData
            {
                ItemTypes = new ItemType[] { ItemType.Category }
            };
            XElement list = Client.GetListXml(mainSchema.Id, filter);
            var usedCategories = list.Descendants();
            if (usedCategories.Count() > 0)
            {
                foreach (var elmt in usedCategories)
                {
                    CategoryData cat = (CategoryData)Client.Read(elmt.Attribute("ID").Value, new ReadOptions());
                    if (cat.KeywordMetadataSchema != null && cat.KeywordMetadataSchema.IdRef != "tcm:0-0-0")
                    {
                        if (!allSchemas.Select(s => s.Id).Contains(cat.KeywordMetadataSchema.IdRef))
                        {
                            SchemaData s = Client.Read(cat.KeywordMetadataSchema.IdRef, new ReadOptions()) as SchemaData;
                            allSchemas.Add(s);
                            AddUsedSchemas(s, allSchemas);
                        }
                    }
                }
            }
            filter = new UsedItemsFilterData
            {
                ItemTypes = new ItemType[] { ItemType.Schema }
            };
            list = Client.GetListXml(mainSchema.Id, filter);
            var unprocessedSchemas = list.Descendants().Where(i => !(allSchemas.Select(s => s.Id).Contains(i.Attribute("ID").Value)));
            if (unprocessedSchemas.Count() > 0)
            {
                IEnumerable<SchemaData> usedSchemas = GetConvertibleSchemas(unprocessedSchemas);
                //allSchemas = allSchemas.Union(usedSchemas).ToList();
                allSchemas.AddRange(usedSchemas.Where(p => !allSchemas.Any(p2 => p2.Id == p.Id)));
                foreach (SchemaData usedSchema in usedSchemas)
                {
                    AddUsedSchemas(usedSchema, allSchemas);
                }
            }
        }
    }
}
