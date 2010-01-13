using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CmdParser;
using System.Reflection;

namespace Rspec.Project.Runner
{
    public class ProgramArguments
    {
        #region Constructor(s)

        public ProgramArguments(){ }

        public ProgramArguments(string target, bool recursive, string outputDirectory)
        {
            this.T = target;
            this.O = outputDirectory;
            this.R = recursive;
            this.Initialize();
        }

        private void Initialize()
        {
            if (!Helpers.IsNullOrTrimedEmpty(this.T))
            {
                FileInfo file = new FileInfo(this.T);
                this.ProjectFile = file;

                // default name of spec
                if (Helpers.IsNullOrTrimedEmpty(this.SpecName))
                {
                    this.SpecName = file.Name.Remove(
                        file.Name.IndexOf(file.Extension), file.Extension.Length)
                        + "_spec_helper.rb";
                }

                if (Helpers.IsNullOrTrimedEmpty(this.O))
                {
                    // target directory to save file to
                    this.O = file.DirectoryName;
                }
            }
        }

        public static ProgramArguments Parse(string[] args)
        {
            var arguments = new ProgramArguments();
            arguments.Parameters = Parameters.CreateParameters(arguments, args);
            arguments.Initialize();
            return arguments;
        }

        #endregion

        #region Properties

        public ValidationError Error { get; private set; }

        public Parameters Parameters { get; private set; }

        [Parameter()]
        [Help("Address of Target Project file", null)]
        public string T { get; set; }

        [Parameter()]
        [Help("Output location of spec file", null)]
        public string O { get; set; }

        public FileInfo ProjectFile { get; private set; }
        
        [Switch]
        [Help("Recursively walk project references", 
            "By default simply emits referenced project .dll's, otherwise walks each referenced project and builds a spec helper for them")]
        public bool R { get; set; }
        
        [Parameter]
        [Help("Set custom spec name. Default: $(assembly)_spec.rb", null)]
        public string SpecName { get; set; }

        #endregion

        #region Methods

        public bool IsValid()
        {
            ValidationError issue = null;

            // user passed in help (?, ?? or -help)
            if (this.Parameters.IsHelpNeeded)
            {
                issue = new ValidationError() { Message = this.GetHelpString(), Severity = 0 };
                this.Error = issue;
                return false;
            }

            // user passed in -version
            if (this.Parameters.IsVersionNeeded)
            {
                Assembly assem = Assembly.GetExecutingAssembly();
                string ver = assem.GetName().Version.ToString();

                issue = new ValidationError() { Severity = 0, Message = string.Format("Version: {0}", ver) };
                this.Error = issue;
                return false;
            }

            // file's gotta exist dude
            if (!this.ProjectFile.Exists)
            {
                issue = new ValidationError() { Severity = 1, Message = "File does not exist" };
                this.Error = issue;
                return false;
            }

            // only supporting csharp projects currently
            if (this.ProjectFile.Extension.ToLower() != ".csproj")
            {
                issue = new ValidationError() { Severity = 1, Message = "Currently only supports .csprojs" };
                this.Error = issue;
                return false;
            }

            this.Error = issue;
            return true;
        }

        public string GetUsageString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(this.Parameters.GetUsageString(Assembly.GetExecutingAssembly(), 12));
            sb.AppendLine("Note: For detailed help, use -?? or -help.");

            return sb.ToString();
        }

        public string GetHelpString()
        {
            StringBuilder sb = new StringBuilder();

            switch (this.Parameters.HelpChars)
            {
                case "?":
                    return this.GetUsageString();
                case "help":
                case "??":
                    sb.AppendLine();
                    sb.AppendLine(this.Parameters.GetDetailedHelp(Assembly.GetExecutingAssembly()));
                    sb.AppendLine("Note: For a usage summary, use -?.");
                    break;
            }

            return sb.ToString();
        }

        #endregion
    }
}
