using System;

namespace CmdParser
{
	/// <summary>
	/// Defines a maximum and minimum number of items allowed for the parameter.
	/// Usage: Attribute will be used on field or property and can be defined only once per field or property.  This attribute can be applied to a collection type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=false)]
	public sealed class ValidateCountAttribute : ParameterBaseAttribute
	{
		private int min;
		private int max;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public ValidateCountAttribute(int min, int max)
		{
			if ( min < 0 )
				throw new ArgumentOutOfRangeException("min");
			if ( max < 0 )
				throw new ArgumentOutOfRangeException("max");
			if ( max < min )
				throw new ArgumentOutOfRangeException("max", "max must be >= min");

			this.min = min;
			this.max = max;
		}

		/// <summary>
		/// Gets the minimum allowed length of the array.
		/// </summary>
		public int Min
		{
			get { return this.min; }
		}

		/// <summary>
		/// Gets the maximum allowed length of the array.
		/// </summary>
		public int Max
		{
			get { return this.max; }
		}
	}
}
