using System;
using CommandLine;

namespace SSDTBuilder
{
    internal class Options
    {
        [Option('p', "project", Required = true)]
        public string ProjectPath { get; set; }

        [Option('u', "profile", Required = false, HelpText = "Use profile to fine-tune publish options.")]
        public string ProfilePath { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output folder where to save generated files.")]
        public string Output { get; set; }

        [Option('d', "dacpac", HelpText = "Specify dacpac filename. If not provided, dacpac will not be generated.", Group = "Output")]
        public string DacpacName { get; set; }

        [Option('s', "script", HelpText = "Specify script filename. If not provided, script will not be generated.", Group = "Output")]
        public string ScriptName { get; set; }

        [Option('c', "copy-loose", HelpText = "When true, all loose (maked 'copy always' or 'copy if newer') sql scripts will be copied.")]
        public bool CopyLooseScripts { get; set; }

        [Option("remove-sqlcmd-variables", Default = false, HelpText = "When set, all ':setvar' instructions are removed.")]
        public bool CommentOutSetVarDeclarations { get; set; }

        [Option("target-db-name", HelpText = "Name of the target database. If Dac profile is used, this value will override one specified there.")]
        public string TargetDatabaseName { get; set; }

        [Option("target-dac-name", Default = "Database", HelpText = "Dacpac name.")]
        public string TargetDacName { get; set; }

        [Option("target-dac-version", Default = "1.0.0", HelpText = "Dacpac version.")]
        public string TargetDacVersion { get; set; }

        [Option("create-new-db", HelpText = "If set, script will have DROP IF EXISTS and CREATE DATABASE statements.")]
        public bool CreateNewDatabase { get; internal set; }

        [Option("silent", HelpText = "When set, no messages are emitted.")]
        public bool IsSilent { get; set; }

        [Option("verbose", HelpText = "When set, all messages are emitted.")]
        public bool IsVerbose { get; set; }

        [Option("sqlserver-version", Default = "Sql150", HelpText = "Specific SQL Server releases. See https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac.model.sqlserverversion.")]
        public string SqlServerVersion { get; set; }
        
        //[Option("runner", Default = "None", HelpText = "Options: none | linux | windows.")]
        //public string GenerateRunner { get; set; }

        public bool GenerateDacPac => !string.IsNullOrEmpty(DacpacName);

        public bool GenerateScript => !string.IsNullOrEmpty(ScriptName);

        public bool Generate => GenerateDacPac || GenerateScript;
    }

}

/*
 
 */
