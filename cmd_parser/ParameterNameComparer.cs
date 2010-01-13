using System;
using System.Collections;
using System.Globalization;

namespace CmdParser
{
	/// <summary>
	/// Summary description for ParameterNameComparer.
	/// </summary>
	internal class ParameterNameComparer : IComparer
	{
		public ParameterNameComparer()
		{
		}
		#region IComparer Members

		public int Compare(object x, object y)
		{
			Parameter p1 = (Parameter)x;
			Parameter p2 = (Parameter)y;
			return string.Compare(p1.Name, p2.Name, true, CultureInfo.InvariantCulture);
		}

		#endregion
	}
}
