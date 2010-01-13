using System;

namespace CmdParser
{
	/// <summary>
	/// Summary description for ValidationPatternAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=false)]
	public sealed class ValidatePatternAttribute : ParameterBaseAttribute
	{
		private string regexPattern;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="pattern"></param>
		public ValidatePatternAttribute(string pattern)
		{
			if ( regexPattern == null )
				throw new ArgumentNullException("pattern");
			this.regexPattern = pattern;
		}

		/// <summary>
		/// Gets the Regular expression string that will be used to validate the
		/// parameter.  The parm must match.
		/// </summary>
		public string Pattern
		{
			get { return this.regexPattern; }
		}
	}
}
