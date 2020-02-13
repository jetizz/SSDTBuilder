using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;

namespace SSDTBuilder
{
    internal class Generator
    {
        private readonly SqlProject _project;
        private readonly Options _options;
        private readonly StatusWriter _log;
        private DacProfile _dacProfile;
        private string _dacpacFilename;
        private string _scriptFileName;

        public Generator(StatusWriter log, SqlProject project, Options options)
        {
            _log = log;
            _project = project;
            _options = options;
        }

        public void Build()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            _log.Info("Build started.");

            LoadDacProfile();

            TSqlModel model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions { });
            PackageMetadata meta = new PackageMetadata { 
                Name = _options.TargetDacVersion, 
                Version = _options.TargetDacVersion 
            };

            _log.Info("Loading build files...");
            var sqlFiles = _project.GetBuildFiles().ToList();
            foreach (var sqlFile in sqlFiles)
            {
                var sql = File.ReadAllText(sqlFile.PhysicalPath);
                model.AddObjects(sql);
            }
            _log.Info("{0} file(s) added to build queue.", sqlFiles.Count);

            using (Stream buffer = GetDacpacStream())
            {
                _log.Info("Building dacpac...");
                DacPackageExtensions.BuildPackage(buffer, model, meta);
                buffer.Seek(0, SeekOrigin.Begin);
                _log.Success("Dacpac generated ({0}).", _options.GenerateDacPac ? "in-memory" : "file");

                if (_options.GenerateScript)
                {
                    GenerateScript(buffer);
                }
            }

            _log.Success("Build complete. Elapsed {0:F3}s.", stopwatch.Elapsed.TotalSeconds);
            if (_options.GenerateDacPac)
                _log.Success("  Dacpac: " + _dacpacFilename ?? "N/A");
            if (_options.GenerateScript)
                _log.Success("  Script: " + _scriptFileName ?? "N/A");

            Environment.ExitCode = 0;
        }

        private void GenerateScript(Stream dacpac)
        {
            _log.Info("Building script...");

            _log.Verbose("DacPackage loading...");
            var package = DacPackage.Load(dacpac, dacpac is MemoryStream ? DacSchemaModelStorageType.Memory : DacSchemaModelStorageType.File);

            var dacOptions = GetDacDeployOptions();
            var targetDbName = GetTargetDatabaseName();
            _log.Verbose("Transforming dacpac to create script.");
            var script = DacServices.GenerateCreateScript(package, targetDbName, dacOptions);

            var scriptFileName = GetScriptFilename();
            using (var output = File.CreateText(scriptFileName))
                output.Write(script);
            _log.Success("Build script created ({0:N0}kb): {1}", script.Length / 1024f, scriptFileName);

            if (_options.CopyLooseScripts)
            {
                _log.Verbose("Attempting to copy loose files.");

                _log.Info("Loading loose files...");
                var looseFiles = _project.GetLooseFiles().ToList();
                foreach (var file in looseFiles)
                {
                    string src = file.PhysicalPath;
                    string dest = Path.Combine(_options.Output, file.Path, file.Filename);
                    CopyLooseFile(src, dest);
                }
                _log.Success("Loose file(s) copied ({0}).", looseFiles.Count);
            }
        }

        private void EnsureDirectory(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private void CopyLooseFile(string src, string dest)
        {
            var dir = Path.GetDirectoryName(dest);
            EnsureDirectory(dir);

            try
            {
                var srcInfo = new FileInfo(src);
                _log.Verbose("Copying {0:N0}b: {1} -> {2}", srcInfo.Length, src, dest);
                File.Copy(src, dest, overwrite: true);
            }
            catch (IOException ex)
            {
                _log.Error("Copy failed: " + ex.Message);
            }
        }

        private void LoadDacProfile()
        {
            if (File.Exists(_options.ProfilePath))
            {
                _log.Info("Using Dac profile: " + _options.ProfilePath);
                _dacProfile = DacProfile.Load(_options.ProfilePath);
            }
            else
                _log.Info("Dac profile not used.");
        }

        private string GetTargetDatabaseName()
        {
            return _options.TargetDatabaseName ??
                _dacProfile?.TargetDatabaseName ??
                "TargetDB";
        }

        private DacDeployOptions GetDacDeployOptions()
        {
            if (_dacProfile != null)
                return _dacProfile.DeployOptions;

            var result = new DacDeployOptions
            {
                CommentOutSetVarDeclarations = _options.CommentOutSetVarDeclarations,

                // Sensible defaults, might need to expose in Options?
                CreateNewDatabase = true,
                BlockOnPossibleDataLoss = false,
                IgnoreAnsiNulls = true,
                IgnoreCryptographicProviderFilePath = true,
                IgnoreFileAndLogFilePath = true,
                IgnoreFileSize = true
            };

            _log.Verbose("Created Default Dac deploy options.");

            return result;
        }

        private string GetDacpacFilename()
        {
            string name = Path.Combine(_options.Output, _options.DacpacName);
            if (!Path.GetExtension(name).Equals(".dacpac", StringComparison.OrdinalIgnoreCase))
                name += ".dacpac";
            return name;
        }

        private string GetScriptFilename()
        {
            _scriptFileName = Path.Combine(_options.Output, _options.ScriptName);
            if (!Path.GetExtension(_scriptFileName).Equals(".sql", StringComparison.OrdinalIgnoreCase))
                _scriptFileName += ".sql";
            return _scriptFileName;
        }

        private Stream GetDacpacStream()
        {
            if (_options.GenerateDacPac)
            {
                _dacpacFilename = GetDacpacFilename();
                return new FileStream(_dacpacFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }

            return new MemoryStream();
        }
    }
}
