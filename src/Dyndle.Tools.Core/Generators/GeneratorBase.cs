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
using Dyndle.Tools.Core.ItemCollectors;
using Dyndle.Tools.Core.CodeWriters;

namespace Dyndle.Tools.Core.Generators
{
    public abstract class GeneratorBase
    {
        protected abstract string NuspecTemplate { get; }

        protected ICodeWriter CodeWriter { get; set; }
        private ModelDefinition currentViewModel = null;
        protected SessionAwareCoreServiceClient Client { get; set; }
        protected IConfiguration Config { get; set; }
        protected ILog log;
        protected SchemaCollector SchemaCollector { get; set; }
        protected TemplateCollector TemplateCollector { get; set; }
        public abstract string FileExtension { get; }

        public GeneratorBase(IConfiguration config)
        {
            log = LogManager.GetLogger(GetType());
            Logger.Setup(config);
            log.Debug($"started {GetType().Name}");
            Config = config;
            SchemaCollector = new SchemaCollector(config);
            TemplateCollector = new TemplateCollector(config);
            Client = CoreserviceClientFactory.GetClient();
            
        }

        public string Generate()
        {
            log.Info("Started generator");

            CollectInput();

            if (Config.AnalyzeOnly)
            {
                Analyze();
                return "";
            }
            IDictionary<string, string> classes = CodeWriter.WriteCode();
            string path = Package(classes);
            log.Debug($"Schemas created in {path}");
            return path;

        }
        protected abstract void CollectInput();
        protected abstract void Analyze();

        private void Analyze(IEnumerable<SchemaData> schemas)
        {
            foreach (var schema in schemas)
            {
                Console.WriteLine($"{schema.Id};{schema.Title};{schema.RootElementName};{schema.NamespaceUri};{schema.Purpose};{schema.LocationInfo.WebDavUrl}");
            }
            return;
        }

        public string GetPackageVersion()
        {
            if (!Config.PackageAddUniqueBuildNumber)
            {
                return Config.PackageVersion;
            }
            return Config.PackageVersion + "-build" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

      

        public string Package(IDictionary<string, string> code)
        {
            if (Config.PackageStyle == PackageStyle.Nuget)
            {
                return PackageNuGet(code);
            }
            return PackageCsFile(code);
        }

        [Obsolete]
        public string PackageCsFile(IDictionary<string, string> code)
        {
            StringBuilder sb = new StringBuilder();
            // if you chose to package as a cs file, there is only one file with multiple classes 
            // so the using statements must be added only once
            sb.Append(CodeWriter.WriteHeader());
            foreach (string className in code.Keys)
            {
                sb.Append(code[className]);
            }
            sb.Append(CodeWriter.WriteFooter());
            string csFile = Path.Combine(Config.WorkFolder.FullName, "Models.cs");
            string fullCode = sb.ToString();
            File.WriteAllText(csFile, fullCode);
            return csFile;
        }


        public string PackageNuGet(IDictionary<string, string> code)
        {
            // create temp folder for the package
            log.Debug("started PackageNuGet");
            string packageVersion = GetPackageVersion();
            log.Debug($"found package version {packageVersion}, combining with {Config.WorkFolder}");

            if (!Config.WorkFolder.Exists)
            {
                log.Error($"error creating temporary work folder {Config.WorkFolder.FullName}");
                return $"error creating temporary work folder {Config.WorkFolder.FullName}";
            }
            DirectoryInfo contentDir = Directory.CreateDirectory(Path.Combine(Config.WorkFolder.FullName, "content"));
            log.Debug("contentDir = {contentDir}");
            if (!contentDir.Exists)
            {
                log.Error($"error creating content directory {contentDir.FullName}");
                return $"error creating content directory {contentDir.FullName}";
            }
            string folderInProject = Config.FolderInProject.Replace("\\", "/");
            foreach (var f in folderInProject.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                log.Debug($"about to combine {contentDir.FullName} and {Config.FolderInProject}");
                contentDir = Directory.CreateDirectory(Path.Combine(contentDir.FullName, f));
                log.Debug($"contentDir = {contentDir}");
            }
            if (!contentDir.Exists)
            {
                log.Error($"error creating content directory {contentDir.FullName}");
                return $"error creating content directory {contentDir.FullName}";
            }

            // create code file in the temp folder
            foreach (string className in code.Keys)
            {
                var targetDir = contentDir.FullName;
                var fileName = className;
                if (className.Contains("/"))
                {
                    var segments = className.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var f in segments.Take(segments.Length-1)) // take everything except the last segment (which is the file name)
                    {
                        log.Debug($"about to combine {targetDir} and {f}");
                        targetDir = Path.Combine(targetDir, f);
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                        log.Debug($"targetDir = {targetDir}");
                    }
                    fileName = segments.Last(); // the last segment is the file name
                }
                // if you chose to package as a NuGET package, there is one file per class
                // so the using statements must be added for each file/class
                StringBuilder sb = new StringBuilder();
                sb.Append(CodeWriter.WriteHeader());
                sb.Append(code[className]);
                sb.Append(CodeWriter.WriteFooter());
                File.WriteAllText(Path.Combine(targetDir, fileName + FileExtension), sb.ToString());
            }
            // read nuspec as resource
            string nuspec = GetResourceAsString(NuspecTemplate);

            // save nuspec as file in the temp folder
            File.WriteAllText(Path.Combine(Config.WorkFolder.FullName, Config.PackageName + ".nuspec"), nuspec); // is this even necessary?

            ManifestMetadata metadata = new ManifestMetadata()
            {
                Authors = Config.Author,
                Owners = Config.Owner,
                Version = packageVersion,
                Id = Config.PackageName,
                Description = "ViewModels generated from schemas in Tridion"
            };

            PackageBuilder builder = new PackageBuilder();
            var files = Directory.GetFiles(Config.WorkFolder.FullName, "*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".nuspec"))
                .Select(f => new ManifestFile { Source = f, Target = f.Replace(Config.WorkFolder.FullName, "") })
                .ToList();
            builder.PopulateFiles("", files);
            builder.Populate(metadata);


            builder.DependencySets.Add(
                new PackageDependencySet(
                    new FrameworkName(".NETFramework, Version=4.0"),
                    new[] { new PackageDependency("DD4T.Core", VersionUtility.ParseVersionSpec(Config.MinDD4TCoreVersion)), new PackageDependency("DD4T." + Config.MvcVersion, VersionUtility.ParseVersionSpec(Config.MinDD4TMvcVersion)) }
                ));
            builder.DependencySets.Add(
                new PackageDependencySet(
                    new FrameworkName(".NETFramework, Version=4.5"),
                    new[] { new PackageDependency("DD4T.Core", VersionUtility.ParseVersionSpec(Config.MinDD4TCoreVersion)), new PackageDependency("DD4T." + Config.MvcVersion, VersionUtility.ParseVersionSpec(Config.MinDD4TMvcVersion)) }
                ));

            string packageFile = Path.Combine(Config.WorkFolder.FullName, builder.Id + "." + builder.Version) + ".nupkg";
            log.Debug($"about to create package {packageFile}");
            if (File.Exists(packageFile))
            {
                log.Debug("package already exists");
                File.Delete(packageFile);
                log.Debug("deleted package");
            }
            log.Debug($"package {packageFile} exists? {File.Exists(packageFile)}");

            using (FileStream stream = File.Open(packageFile, FileMode.OpenOrCreate))
            {
                builder.Save(stream);
            }

            // finally, move package to the specified output folder
            if (string.IsNullOrEmpty(Config.OutputFolder))
            {
                return packageFile;
            }
            var outputFileName = Path.Combine(Config.OutputFolder, builder.Id + "." + builder.Version + ".nupkg");
            File.Move(packageFile, outputFileName);
            return outputFileName;
        }

      
        private static string GetResourceAsString(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
   
    }
}
