using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SSDTBuilder
{
    internal class SqlProject
    {
        private readonly XmlDocument _doc;
        private readonly StatusWriter _log;

        public SqlProject(StatusWriter log, string path)
        {
            SqlProjPath = path;
            _log = log;
            _doc = new XmlDocument();
            _doc.Load(path);
        }

        public string SqlProjPath { get; }
        public string Root => Path.GetDirectoryName(SqlProjPath);

        public IEnumerable<ProjectFile> GetBuildFiles()
        {
            XmlNodeList nodes = _doc.GetElementsByTagName("Build");

            foreach (XmlNode node in nodes)
            {
                var path = NormalizePath(node.Attributes["Include"]?.Value);

                if (path == null)
                    continue;

                if (!Path.GetExtension(path).Equals(".sql", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(Root, path);
                }

                _log.Verbose("Build: {0}", path);
                
                yield return new ProjectFile(Path.GetFileName(path), Path.GetDirectoryName(path), Root);
            }
        }

        public IEnumerable<ProjectFile> GetLooseFiles()
        {
            XmlNodeList nodes = _doc.GetElementsByTagName("None");

            foreach (XmlNode node in nodes)
            {
                /* Example: 
                    <None Include="Scripts\Sample\SampleClients.sql">
                       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                    </None>
                 */
                if (node.FirstChild?.Name != "CopyToOutputDirectory" || node.FirstChild.InnerText == "DoNotCopy")
                    continue;

                var path = NormalizePath(node.Attributes["Include"]?.Value);

                if (path == null)
                    continue;

                if (!Path.GetExtension(path).Equals(".sql", StringComparison.OrdinalIgnoreCase))
                    continue;

                _log.Verbose("Loose: {0}", path);

                yield return new ProjectFile(Path.GetFileName(path), Path.GetDirectoryName(path), Root);
            }
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            //if (Environment.OSVersion.Platform == PlatformID.Unix)
            return path
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }
    }
}
