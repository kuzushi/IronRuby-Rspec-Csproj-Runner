using System;

namespace CmdParser
{
	/// <summary>
	/// Summary description for IntRange.
	/// </summary>
	public sealed class IntRange
	{
		private int min;
		private int max;

		internal IntRange(int min, int max)
		{
			if ( min < 0 )
				throw new ArgumentOutOfRangeException("min", "min must be >=0");
			if ( max < 0 )
				throw new ArgumentOutOfRangeException("max", "max must be >=0");
			if ( min > max )
				throw new ArgumentOutOfRangeException("min", "min must be <= max.");

			this.min = min;
			this.max = max;
		}

		/// <summary>
		/// Gets the Minimum the value can be.
		/// </summary>
		public int Min
		{
			get { return this.min; }
		}

		/// <summary>
		/// Gets the Maximum the value can be.
		/// </summary>
		public int Max
		{
			get { return this.max; }
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
