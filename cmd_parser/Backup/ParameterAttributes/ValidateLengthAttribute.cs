using System;

namespace CmdParser
{
	/// <summary>
	/// This defines the maximum and minimum length of a string value allowed in the parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=false)]
	public sealed class ValidateLengthAttribute : ParameterBaseAttribute
	{
		private int min;
		private int max;
		private bool trim;

		/// <summary>
		/// This defines the maximum and minimum length of a string value allowed in the parameter.
		/// Does not trim the string of leading/trailing spaces.
		/// </summary>
		/// <param name="min">The minimum length of the string input.</param>
		/// <param name="max">The maximum length of the string input.</param>
		public ValidateLengthAttribute(int min, int max) : this(min, max, false)
		{
		}

		/// <summary>
		/// This defines the maximum and minimum length of a string value allowed in the parameter.
		/// </summary>
		/// <param name="min">The minimum length of the string input.</param>
		/// <param name="max">The maximum length of the string input.</param>
		/// <param name="trimFirst">Set to true to trim the string of leading and trailing spaces first.</param>
		public ValidateLengthAttribute(int min, int max, bool trimFirst)
		{
			if ( min < 0 )
				throw new ArgumentOutOfRangeException("min", "min must be positive");
			if ( max < 0 )
				throw new ArgumentOutOfRangeException("max", "max must be positive");
			if ( min > max )
				throw new ArgumentOutOfRangeException("min", "min must be <= max");

			this.min = min;
			this.max = max;
			this.trim = trimFirst;
		}

		/// <summary>
		/// Gets the minimum string length allowed for the parameter.
		/// </summary>
		public int Min
		{
			get { return this.min; }
		}

		/// <summary>
		/// Gets the maximum string length allow for the parameter.
		/// </summary>
		public int Max
		{
			get { return this.max; }
		}

		/// <summary>
		/// Returns true if parser should trim leading and trailing
		/// spaces before the Min/Max test.
		/// </summary>
		public bool TrimFirst
		{
			get { return this.trim; }
		}

		/// <summary>
		/// Returns true if the comparand is within Min and Max inclusive.
		/// </summary>
		/// <param name="comparand">The value to compare.</param>
		/// <returns>true if value is within range; otherwise false.</returns>
		public bool IsInRange(int comparand)
		{
			if ( comparand >= min && comparand <= max )
				return true;
			return false;
		}
	}
}
