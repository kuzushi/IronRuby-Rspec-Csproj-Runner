using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Rspec.Project.Runner
{
    /// <summary>
    /// Represents a project reference
    /// </summary>
    public class ProjectReference
    {
        #region Fields

        private ProgramArguments _args; // we need this to render the children appropriately TODO: move to unity contain resolves
        private string _basePath;
        private FileInfo _fileInfo;

        #endregion

        #region Properties

        public string Include { get; set; }
        public string ProjectGuid { get; set; }
        public string Name { get; set; }

        #endregion

        #region Constructor(s)

        private ProjectReference() { }

        public ProjectReference(ProgramArguments args)
        {
            this._args = args;
            this._basePath = args.ProjectFile.DirectoryName;
        }

        #endregion

        #region Methods

        public string GetProjectFullpath()
        {
            this.GetProjectFile();
            var args = new ProgramArguments(this._fileInfo.FullName, false, null);
            SpecBuilder projectSpec = new SpecBuilder(args);

            return projectSpec.SelfAssembly.GetFullpath;
        }

        public string RenderProjectAndChildren()
        {
            this.GetProjectFile();
            var args = new ProgramArguments(this._fileInfo.FullName, true, this._args.O);
            SpecBuilder projectSpec = new SpecBuilder(args);
            projectSpec.Render();

            return args.O + "\\" + args.SpecName;
        }

        private void GetProjectFile()
        {
            // relative directory makes me have to traverse up one extra path from the include hint.
            if (this._fileInfo == null)
                this._fileInfo = new FileInfo(Path.GetFullPath(this._basePath + "..\\" + this.Include));
        }

        #endregion
    }
}
