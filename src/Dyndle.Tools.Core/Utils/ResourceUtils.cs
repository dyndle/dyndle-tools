using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dyndle.Tools.Core.Utils
{
    public static class ResourceUtils
    {
        public static string GetResourceAsString(string resourcePath)
        {
            var assembly = Assembly.GetCallingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

        public static void StoreResourceOnDisk(string resourcePath, DirectoryInfo targetDirectory, string filename)
        {
            var assembly = Assembly.GetCallingAssembly();
            if (filename == null)
            {
                throw new Exception("filename cannot be null");
            }
            using (Stream input = assembly.GetManifestResourceStream(resourcePath))
            using (Stream output = File.Create(Path.Combine(targetDirectory.FullName, filename)))
            {
                CopyStream(input, output);
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            // Insert null checking here for production
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
