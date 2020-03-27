using Dyndle.Tools.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dyndle.Tools.Core
{
    public enum ExportType { Models, Views }
    public class Configuration : IConfiguration
    {
        public static readonly string DEFAULT_AUTHOR = "Anonymous";
        public static readonly string DEFAULT_MIN_DD4T_CORE_VERSION = "2.5";
        public static readonly string DEFAULT_MIN_DD4T_MVC_VERSION = "2.5";
        public static readonly string DEFAULT_MODEL_NAMESPACE = "MyCompany.MyProject.Models";
        public static readonly string DEFAULT_MVC_VERSION = "MVC5";
        public static readonly string DEFAULT_PACKAGE_NAME = "MyModels";
        public static readonly string DEFAULT_PACKAGE_VERSION = "1.0.0";
        public static readonly string DEFAULT_PATH_MODELS = "/Generated/Models";
        public static readonly string DEFAULT_PATH_VIEWS = "/Areas/Core/Views";
        public static readonly bool DEFAULT_PACKAGE_ADD_UNIQUE_BUILD_NUMBER = true;
        public static readonly int DEFAULT_INDENT_NR_OF_SPACES = 4;
        public static readonly string DEFAULT_LOG_FOLDER = Path.GetTempPath();
        public static readonly string DEFAULT_MODEL_ATTRIBUTE_NAME = "ContentModel";


        public string OutputFolder { get; set; }

        DirectoryInfo _workFolder;
        public DirectoryInfo WorkFolder
        {
            get
            {
                if (_workFolder == null)
                {
                    var workFolderPath = Path.Combine(Path.GetTempPath(), "dyndle_" + DateTime.Now.Ticks);
                    if (Directory.Exists(workFolderPath))
                    {
                        throw new Exception($"unexpected error: temporary workfolder {workFolderPath} already exists");
                    }
                    _workFolder = Directory.CreateDirectory(workFolderPath);
                }
                return _workFolder;
            }
        }

        public string ModelNamespace { get; set; }
        private string folderInProject;
        public string FolderInProject
        {
            get
            {
                return folderInProject ?? (ExportType == ExportType.Models ? DEFAULT_PATH_MODELS : DEFAULT_PATH_VIEWS);
            }
            set
            {
                folderInProject = value;
            }
        }
        public string MvcVersion { get; set; }
        private string minDD4TCoreVersion;
        public string MinDD4TCoreVersion
        {
            get
            {
                return minDD4TCoreVersion ?? DEFAULT_MIN_DD4T_CORE_VERSION;
            }
            set
            {
                packageName = value;
            }
        }
        private string minDD4TMvcVersion;
        public string MinDD4TMvcVersion
        {
            get
            {
                return minDD4TMvcVersion ?? DEFAULT_MIN_DD4T_MVC_VERSION;
            }
            set
            {
                packageName = value;
            }
        }
        public string Author { get; set; }
        private string packageName;
        public string PackageName
        {
            get
            {
                return packageName ?? (ExportType == ExportType.Models ? "MyModels" : "MyViews");
            }
            set
            {
                packageName = value;
            }
        }

        public string PackageVersion { get; set; }
        public bool PackageAddUniqueBuildNumber { get; set; }
        public int IndentNrOfSpaces { get; set; }
        public string LogFolder { get; set; }
        public bool IdentifyModelsBySchemaTitle { get; set; }
        public bool AnalyzeOnly { get; set; }
        public string ModelAttributeName { get; set; }
        public string PageModelAttributeName { get; set; }
        public IEnumerable<string> UsingNamespaces { get; set; }
        public IEnumerable<string> BaseClasses { get; set; }
        public IEnumerable<string> BaseClassesForMultimedia { get; set; }
        public IEnumerable<string> BaseClassesForEmbedded { get; set; }
        public IEnumerable<string> BaseClassesForPages { get; set; }

        public bool NoRenderData { get; set; }
        public string Owner { get; set; }

        private string _publicationId;
        public string PublicationId
        {
            get
            {
                return _publicationId;
            }
            set
            {
                if (value == null || value.StartsWith("tcm:"))
                {
                    _publicationId = value;
                }
                else
                {
                    _publicationId = "tcm:" + value;
                }
            }
        }
        private string _folderId;
        public string FolderId
        {
            get
            {
                return _folderId;
            }
            set
            {
                if (value == null || value.StartsWith("tcm:"))
                {
                    _folderId = value;
                }
                else
                {
                    _folderId = "tcm:" + value;
                }
            }
        }
        private string _schemaId;
        public string SchemaId
        {
            get
            {
                return _schemaId;
            }
            set
            {
                if (value == null || value.StartsWith("tcm:"))
                {
                    _schemaId = value;
                }
                else
                {
                    _schemaId = "tcm:" + value;
                }
            }
        }

        private string _templateId;
        public string TemplateId
        {
            get
            {
                return _templateId;
            }
            set
            {
                if (value == null || value.StartsWith("tcm:"))
                {
                    _templateId = value;
                }
                else
                {
                    _templateId = "tcm:" + value;
                }
            }
        }
        public PackageStyle PackageStyle { get; set; }
        public ExportType ExportType { get; set; }
    }
}
