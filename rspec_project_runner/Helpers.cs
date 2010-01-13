using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Rspec.Project.Runner
{
    public static class Helpers
    {
        public static string GetElementValue(this XElement element)
        {
            return element != null ? element.Value : string.Empty;
        }

        public static bool IsNullOrTrimedEmpty(string value)
        {
            return (value == null || value.Trim() == string.Empty);
        }
    }
}
