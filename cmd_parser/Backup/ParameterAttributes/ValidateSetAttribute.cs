using System;

namespace CmdParser
{
	/// <summary>
	/// Summary description for ValidationSetAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=false)]
	public sealed class ValidateSetAttribute : ParameterBaseAttribute
	{
		private string vSet;
        private bool caseInsensitive;

		/// <summary>
		/// Creates a ValidateSetAttribute object.
		/// </summary>
		/// <param name="setString">Comma delimited string of allowed values.</param>
		public ValidateSetAttribute(string setString)
		{
			if ( setString == null )
				throw new ArgumentNullException("setString");
			this.vSet = setString.Trim();
		}

        /// <summary>
        /// Creates a ValidateSetAttribute object.
        /// </summary>
        /// <param name="setString">Comma delimited string of allowed values.</param>
        /// <param name="caseInsensitive">True for case insensitive compare; otherwise false.</param>
        public ValidateSetAttribute(string setString, bool caseInsensitive)
        {
            if ( setString == null )
                throw new ArgumentNullException("setString");
            this.vSet = setString.Trim();
            this.caseInsensitive = caseInsensitive;
        }

		/// <summary>
		/// Gets or sets the string used to describe the set of valid values.  The parameter
		/// must match one of these values.
		/// </summary>
		public string SetString
		{
			get { return this.vSet; }
            set { this.vSet = value; }
		}

        /// <summary>
        /// Gets or sets value to indicate if allowed values are compared case insensitive.
        /// </summary>
        public bool CaseInsensitive
        {
            get { return this.caseInsensitive;  }
            set { this.caseInsensitive = value; }
        }
	}
}
