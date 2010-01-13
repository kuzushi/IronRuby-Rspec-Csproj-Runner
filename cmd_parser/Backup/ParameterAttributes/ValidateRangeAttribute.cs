using System;

namespace CmdParser
{
	/// <summary>
	/// This defines the range of values allowed in the parameter.
	/// Usage: Attribute will be used on field or property and can be
	/// defined only once per field or property.  The field or property
	/// type must implement IComparable.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=false)]
	public sealed class ValidateRangeAttribute : ParameterBaseAttribute
	{
		private object minRange;
		private object maxRange;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="minRange"></param>
		/// <param name="maxRange"></param>
		public ValidateRangeAttribute(object minRange, object maxRange)
		{
			if ( minRange == null )
				throw new ArgumentNullException("minRange");
			if ( maxRange == null )
				throw new ArgumentNullException("maxRange");

			this.minRange = minRange;
			this.maxRange = maxRange;
		}

		/// <summary>
		/// Gets the object that will represent the minimum allowed value for the parameter.
		/// </summary>
		public object MinRange
		{
			get { return this.minRange; }
		}

		/// <summary>
		/// Gets the object that will represent the maximum allowed value for the parameter.
		/// </summary>
		public object MaxRange
		{
			get { return this.maxRange; }
		}
	}
}
