using System;

namespace CmdParser
{
	/// <summary>
	/// Summary description for ParsingParameterHelpAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
	public sealed class CmdHelpAttribute : ParameterBaseAttribute
	{
		private string shortDesc;
		private string longDesc;
		private string homeUri;
		private string copyright;
		private string example;

		/// <summary>
		/// Declares the help for the program.
		/// </summary>
		/// <param name="shortDescription"></param>
		/// <param name="longDescription"></param>
		public CmdHelpAttribute(string shortDescription, string longDescription) : this(shortDescription, longDescription, null, null, null)
		{
		}

		/// <summary>
		/// Declares the help for the program.
		/// </summary>
		/// <param name="shortDescription"></param>
		/// <param name="longDescription"></param>
		/// <param name="homeUri"></param>
		/// <param name="copyright"></param>
		/// <param name="example"></param>
		public CmdHelpAttribute(string shortDescription, string longDescription, string homeUri, string copyright, string example)
		{
			if ( shortDescription == null )
				throw new ArgumentNullException("shortDescription");
			this.shortDesc = shortDescription;
			this.longDesc = longDescription;
			this.homeUri = homeUri;
			this.copyright = copyright;
			this.example = example;
		}

		/// <summary>
		/// Summary description of parameter.
		/// </summary>
		public string ShortDescription
		{
			get { return this.shortDesc; }
		}

		/// <summary>
		/// Detail description of parameter.
		/// </summary>
		public string LongDescription
		{
			get { return this.longDesc; }
		}

		/// <summary>
		/// The link to support or command home page.
		/// </summary>
		public string HomeUri
		{
			get { return this.homeUri; }
		}

		/// <summary>
		/// The program copyright string.
		/// </summary>
		public string Copyright
		{
			get { return this.copyright; }
		}

		/// <summary>
		/// Example usage of the program such as code example(s).
		/// </summary>
		public string Example
		{
			get { return this.example; }
		}
	}
}
