using System;

namespace CmdParser
{
	/// <summary>
	/// Declares which set is the default set name that will be used when
	/// given parameters could match more then one set.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
	public sealed class DefaultSetAttribute : ParameterBaseAttribute
	{
		string setName;

		/// <summary>
		/// Creates instance of the DefaultSetAttribute.
		/// </summary>
		/// <param name="setName"></param>
		public DefaultSetAttribute(string setName)
		{
			if ( setName == null )
				setName = "";
			this.setName = setName;
		}

		/// <summary>
		/// Gets the default set name.
		/// </summary>
		public string SetName
		{
			get { return this.setName; }
		}
	}
}
