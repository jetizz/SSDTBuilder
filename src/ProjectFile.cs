using System;

namespace SSDTBuilder
{
    internal struct ProjectFile
    {
        public ProjectFile(string filename, string path, string root)
        {
            Filename = filename;
            Path = path;
            Root = root;
        }

        public string Filename { get; }
        public string Path { get; }
        public string Root { get; }

        public string PhysicalPath => System.IO.Path.Combine(Root, Path, Filename);
    }
}
