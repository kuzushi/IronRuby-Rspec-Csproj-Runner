using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Rspec.Project.Runner
{
    public class PropertyGroup
    {
        private string _basePath;

        #region Properties

        public string AssemblyGroup { get; set; }
        public string TargetFrameworkVersion { get; set; }
        public string OutputType { get; set; }

        #endregion

        #region Constructor(s)

        public PropertyGroup(string basePath)
        {
            this._basePath = basePath;
        }

        #endregion

        public string GetFullpath
        {
            get 
            {
                string extension = "dll";

                if (!Helpers.IsNullOrTrimedEmpty(this.OutputType))
                {
                    switch (this.OutputType.ToLower())
                    {
                        case "library":
                            extension = "dll";
                            break;
                        case "exe":
                            extension = "exe";
                            break;
                        default:
                            extension = this.OutputType;
                            break;
                    }
                }

                return Path.GetFullPath(_basePath + "//bin//Debug//") + AssemblyGroup + "." + extension;
            }
        }
    }
}
