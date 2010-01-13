using System;
using System.Globalization;

namespace CmdParser
{
	/// <summary>
	/// Summary description for Range.
	/// </summary>
	public sealed class Range
	{
		private Type type;
		private object min;
		private object max;

		internal Range(Type type, string min, string max)
		{
			if ( min == null )
				throw new ArgumentNullException("min");
			if ( max == null )
				throw new ArgumentNullException("max");
			if ( type == null )
				throw new ArgumentNullException("Parameter type is null.");
			if ( type.IsAbstract )
				throw new Exception("Parameter type can not be abstract.");
			if ( ! (type.IsClass || type.IsValueType) )
				throw new Exception("Parameter type must be reference type or value type.");
			if ( ! IsComparableAndConvertable(type) )
				throw new Exception("Parameter type must implement IComparable and IConvertible.");
			
			this.type = type;
			this.min = Convert.ChangeType(min, type, CultureInfo.InvariantCulture);
			this.max = Convert.ChangeType(max, type, CultureInfo.InvariantCulture);
			IComparable imin = (IComparable)this.min;
			if ( imin.CompareTo(this.max) > 0  )
				throw new ArgumentException("Min must be <= max.");
		}

		/// <summary>
		/// Gets the type this range is associated with.
		/// </summary>
		public Type Type
		{
			get { return this.type; }
		}

		/// <summary>
		/// Gets the object that is used as the minimum in a compare.
		/// </summary>
		public object Min
		{
			get { return this.min; }
		}

		/// <summary>
		/// Gets the object that is used as the maximum in a compare.
		/// </summary>
		public object Max
		{
			get { return this.max; }
		}

		/// <summary>
		/// Returns true of obj is between Min and Max objects inclusive.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool IsInRange(object obj)
		{
			if ( obj == null )
				return false;
			
			Type type = obj.GetType();
			if ( type != this.type )
				throw new ArgumentException("Type must be same as range type.");
			object o = Convert.ChangeType(obj, this.type, CultureInfo.InvariantCulture);
			IComparable io = (IComparable)o;
			
			if ( io.CompareTo(min) >= 0 && io.CompareTo(max) <= 0 )
				return true;
			return false;
		}

		private static bool IsComparableAndConvertable(Type type)
		{
			if ( type.GetInterface("IConvertible", true) == null || 
				type.GetInterface("IComparable", true) == null )
				return false;
			return true;
		}
	}
}
