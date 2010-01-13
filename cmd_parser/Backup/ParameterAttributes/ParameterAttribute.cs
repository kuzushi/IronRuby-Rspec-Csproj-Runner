using System;
using System.Globalization;

namespace CmdParser
{
	/// <summary>
	/// Summary description for ParameterAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=true)]
	public sealed class ParameterAttribute : ParameterBaseAttribute
	{
		private int pos;
		private string setName;
		private bool mandatory;
		private bool valueFromRemainingArguments;

		/// <summary>
		/// 
		/// </summary>
		public ParameterAttribute() : this(-1, "")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mandatory"></param>
		public ParameterAttribute(bool mandatory) : this(-1, mandatory)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="position"></param>
		public ParameterAttribute(int position) : this(position, "")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameterSetName"></param>
		public ParameterAttribute(string parameterSetName) : this(-1, parameterSetName)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="position"></param>
		/// <param name="parameterSetName"></param>
		public ParameterAttribute(int position, string parameterSetName) : this(position, parameterSetName, false, false)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="mandatory"></param>
		public ParameterAttribute(int position, bool mandatory) : this(position, "", mandatory, false)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="parameterSetName"></param>
		/// <param name="mandatory"></param>
		public ParameterAttribute(int position, string parameterSetName, bool mandatory) : this(position, parameterSetName, mandatory, false)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="parameterSetName"></param>
		/// <param name="mandatory"></param>
		/// <param name="valueFromRemainingArguments"></param>
		public ParameterAttribute(int position, string parameterSetName, bool mandatory, bool valueFromRemainingArguments)
		{
			if ( position < -1 )
				throw new ArgumentOutOfRangeException("position", "Position must be >= -1.");
			this.pos = position;
			if ( parameterSetName == null )
				parameterSetName = "";			// Default set is "".
			if ( string.Compare(setName, "default", true, CultureInfo.InvariantCulture) == 0 )
				parameterSetName = "";
			this.setName = parameterSetName.Trim();
			this.mandatory = mandatory;
			this.valueFromRemainingArguments = valueFromRemainingArguments;
		}

		/// <summary>
		/// Gets the position of the parameter that is expected at the command line.
		/// </summary>
		public int Position
		{
			get { return this.pos; }
			set
			{
				if ( value < -1 )
					throw new ArgumentOutOfRangeException("value", "Position must be >= -1");
				this.pos = value;
			}
		}

		/// <summary>
		/// Gets the parameter set name this parameter is a member of.
		/// </summary>
		public string ParameterSetName
		{
			get { return this.setName; }
			set
			{
				if ( value == null )
					value = "";
				value = value.Trim();
				this.setName = value;
			}
		}

		/// <summary>
		/// Returns true if the parameter is mandatory; otherwise false.
		/// </summary>
		public bool Mandatory
		{
			get { return this.mandatory; }
			set { this.mandatory = value; }
		}

		/// <summary>
		/// Returns true if parameter is a variable list.
		/// </summary>
		public bool ValueFromRemainingArguments
		{
			get { return this.valueFromRemainingArguments; }
			set { this.valueFromRemainingArguments = value; }
		}
	}
}
