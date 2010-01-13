using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Rspec.Project.Runner
{
    /// <summary>
    /// Represents an assembly reference
    /// </summary>
    public class Reference
    {
        #region Fields

        private string _basePath;

        #endregion

        #region Properties

        public string Include { get; set; }
        public string SpecificVersion { get; set; }
        public string HintPath { get; set; }
        public string RequiredFrameworkVersion { get; set; }

        #endregion

        #region Constructor(s)

        private Reference() { }

        public Reference(string basePath)
        {
            this._basePath = basePath;
        }

        #endregion

        #region Methods

        /* There are two different types of references:
         * 
         *      External File References are included in rspec as:
         *          require 'c:/fullpath/to/somefile.dll'
         *      
         *      Global References require the public key, namespace & version number of reference:
         *          require 'System.Web.Mvc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
         */

        public string GetFullPath()
        {
            if (!Helpers.IsNullOrTrimedEmpty(this.HintPath))
            {
                // basepath + relative resolves us to fullpath (must move up a directory)
                return Path.GetFullPath(this._basePath + "..\\" + this.HintPath);
            }
            else
            {
                // Include is just referenced name.  To find this, we gotta search the GAC and find public key
                Assembly assembly = Assembly.LoadWithPartialName(this.Include);

                if (assembly != null)
                    return assembly.FullName;
            }

            return string.Empty;
        }

        #endregion
    }
}
