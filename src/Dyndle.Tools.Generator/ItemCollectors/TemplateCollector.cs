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
using Dyndle.Tools.Core;
using Tridion.ContentManager.CoreService.Client;
using Dyndle.Tools.Generator.Models;
using Dyndle.Tools.Core.Utils;
using Tridion.CoreService.Tools;

namespace Dyndle.Tools.Generator.ItemCollectors
{
    public class TemplateCollector
    {

        private ICoreService Client { get; set; }
        private ILog log;

        public TemplateCollector(IGeneratorConfiguration config)
        {
            log = LogManager.GetLogger(this.GetType());
            Logger.Setup(config);
            log.Debug("started SchemaCollector");
            Client = CoreserviceClientFactory.GetClient();   
        }

        public IEnumerable<TemplateData> FindTemplate(string templateId, bool pageTemplatesOnly = false)
        {
            log.Info($"Called FindTemplate with templateId {templateId} (pageTemplatesOnly = {pageTemplatesOnly})");
            var template = Client.Read(templateId, new ReadOptions()) as TemplateData;
            if (template == null)
            {
                log.Warn($"template with id {templateId} cannot be found or it is not a template");
                return new List<TemplateData>();
            }
            if (pageTemplatesOnly && ! (template is PageTemplateData))
            {
                log.Warn($"template with id {templateId} is not a page template");
                return new List<TemplateData>();
            }
            return new[] { template };
        }

        public IEnumerable<TemplateData> FindTemplatesForFolder(string folderId, bool pageTemplatesOnly = false)
        {
            log.Info("Called FindTemplatesForFolder with folderId " + folderId);
            OrganizationalItemItemsFilterData filterData = new OrganizationalItemItemsFilterData();
            filterData.ItemTypes = pageTemplatesOnly ? new[] { ItemType.PageTemplate } : new[] { ItemType.PageTemplate, ItemType.ComponentTemplate };
            filterData.Recursive = true;
            XElement resultXml = Client.GetListXml(folderId, filterData);
            return GetConvertibleTemplates(resultXml);
        }

        public IEnumerable<TemplateData> FindTemplatesForPublication(string publicationId, bool pageTemplatesOnly = false)
        {
            log.Info("Called FindSchemasForPublication with publicationId " + publicationId);
            RepositoryItemsFilterData filterData = new RepositoryItemsFilterData();
            filterData.ItemTypes = pageTemplatesOnly ? new[] { ItemType.PageTemplate } : new[] { ItemType.PageTemplate, ItemType.ComponentTemplate };
            filterData.Recursive = true;
            XElement resultXml = Client.GetListXml(publicationId, filterData);
            return GetConvertibleTemplates(resultXml);
        }

        private IEnumerable<TemplateData> GetConvertibleTemplates(XElement listOfTemplates)
        {
            return listOfTemplates.Descendants().Select(t => (TemplateData)Client.Read(t.Attribute("ID").Value, new ReadOptions()));
        }
    }
}
