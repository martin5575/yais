using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yais.Data
{
    public static class ResourceReader
    {
        public static string ReadResource(string fileName)
        {
            using (var stream = typeof(ResourceReader).Assembly.GetManifestResourceStream($"Yais.Data.{fileName}"))
            {
                using (StreamReader sr = new StreamReader(stream))
                    return sr.ReadToEnd();
            }
        }
    }
}
