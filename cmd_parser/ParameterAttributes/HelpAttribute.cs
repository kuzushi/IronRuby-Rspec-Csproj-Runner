using System;

namespace CmdParser
{
	/// <summary>
	/// Used to declare the help strings for a parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=false)]
	public sealed class HelpAttribute : ParameterBaseAttribute
	{
		private string shortDesc;
		private string longDesc;

		/// <summary>
		/// Declares the help for the parameter.
		/// </summary>
		/// <param name="shortDescription"></param>
		/// <param name="longDescription"></param>
		public HelpAttribute(string shortDescription, string longDescription)
		{
		    ShortDescription = shortDescription;
            // Short cut to set both to shortDesc so you don't have to type same twice.
            if ( longDesc == null )
                LongDescription = shortDescription;
            else
			    LongDescription = longDescription;
		}

		/// <summary>
		/// Short description of parameter.  Used in Usage help.
		/// </summary>
		public string ShortDescription
		{
			get { return this.shortDesc; }
            set
            {
                if ( value == null )
                    value = "";
                this.shortDesc = value;
            }
		}

		/// <summary>
		/// Long description of parameter.  Used in Detail help.
		/// </summary>
		public string LongDescription
		{
			get { return this.longDesc; }
            set
            {
                if ( value == null )
                    value = "";
                this.longDesc = value;
            }
		}
	}
}
