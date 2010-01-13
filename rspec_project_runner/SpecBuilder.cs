using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace Rspec.Project.Runner
{
    public class SpecBuilder
    {
        private static string _requireFormat = "require '{0}'";
        private static string _requireGems = "require 'rubygems'";
        private static string _requireSpec = "require 'spec'";
        private static string _requireMocks = "require 'spec/mocks'";
        
        #region Fields

        private IEnumerable<Reference> _references;
        private IEnumerable<ProjectReference> _projectReferences;
        private ProgramArguments _options;
        private PropertyGroup _selfAssembly;
        
        #endregion

        #region Properties

        public PropertyGroup SelfAssembly
        {
            get
            {
                return _selfAssembly;
            }
        }

        #endregion

        #region Constructor(s)

        public SpecBuilder(ProgramArguments renderOptions)
        {
            this._options = renderOptions;
            this.Initialize();
        }

        private void Initialize()
        {
            // get paths
            string basePath = this._options.ProjectFile.DirectoryName;
            string fileName = this._options.ProjectFile.FullName;

            // build document
            XDocument document = XDocument.Load(fileName);
            XNamespace ns = document.Elements().FirstOrDefault().Name.Namespace;

            // query 
            this._projectReferences = from r in document.Descendants()
                                      where r.Name.LocalName.ToLower() == "projectreference"
                                      select new ProjectReference(this._options)
                                      {
                                          Include = r.Attribute("Include").Value,
                                          Name = r.Element(ns + "Name").Value,
                                          ProjectGuid = r.Element(ns + "Project").Value
                                      };

            this._references = from r in document.Descendants()
                               where r.Name.LocalName == "Reference"
                               select new Reference(basePath)
                               {
                                   Include = r.Attribute("Include").Value,
                                   HintPath = r.Element(ns + "HintPath").GetElementValue(),
                                   SpecificVersion = r.Element(ns + "SpecificVersion").GetElementValue(),
                                   RequiredFrameworkVersion = r.Element(ns + "RequiredFrameworkVersion").GetElementValue()
                               };

            this._selfAssembly = (from r in document.Descendants()
                                where (r.Name.LocalName == "PropertyGroup"
                                && r.Attributes().Count() == 0)
                                select new PropertyGroup(basePath)
                                {
                                    AssemblyGroup = r.Element(ns + "AssemblyName").GetElementValue(),
                                    TargetFrameworkVersion = r.Element(ns + "TargetFrameworkVersion").GetElementValue(), 
                                    OutputType = r.Element(ns + "OutputType").GetElementValue()
                                }).FirstOrDefault();
        }

        #endregion

        public override string ToString()
        {
            /* For a phase two implementation of this, it'd be nice to have
             * some views that I can render as templates.
             * This will make parsing different type of specs easier to maintain
             * but for phase 1, this is suffient
             */

            StringBuilder builder = new StringBuilder();

            builder.AppendLine(_requireGems);
            builder.AppendLine(_requireSpec);
            builder.AppendLine(_requireMocks);

            if (this._references.Any())
            {
                foreach (Reference reference in this._references)
                {
                    builder.AppendLine(string.Format(_requireFormat,  reference.GetFullPath()));
                }
            }

            if (this._projectReferences.Any())
            {
                foreach (ProjectReference reference in this._projectReferences)
                {
                    if (this._options.R)
                    {
                        builder.AppendLine(string.Format(_requireFormat, reference.RenderProjectAndChildren()));
                    }
                    else
                    {
                        builder.AppendLine(string.Format(_requireFormat, reference.GetProjectFullpath()));
                    }
                }
            }

            if (this._selfAssembly != null)
                builder.AppendLine(string.Format(_requireFormat, this._selfAssembly.GetFullpath));

            return builder.ToString();
        }

        public void Render()
        {
            FileStream fs = File.Create(_options.O + "\\" + _options.SpecName);
            StreamWriter stream = new StreamWriter(fs);
            stream.Write(this.ToString());
            stream.Close();
            fs.Close();
        }
    }
}
