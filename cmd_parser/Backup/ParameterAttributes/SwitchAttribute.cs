using System;

namespace CmdParser
{
	/// <summary>
	/// Summary description for SwitchParameterAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=false)]
	public sealed class SwitchAttribute : ParameterBaseAttribute
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public SwitchAttribute()
		{
		}
	}
}
