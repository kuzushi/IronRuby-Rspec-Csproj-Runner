using System;
using System.Collections;

namespace CmdParser
{
	/// <summary>
	/// Comparer to compare the positions of two Parameter objects.  Both 
	/// parameter must be in same set and both must be position parameters (i.e. > -1)
	/// Set the value of the SetName property before using the comparer.
	/// </summary>
	public class ParameterPositionComparer : IComparer
	{
		private static string setName;

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public ParameterPositionComparer()
		{
		}

		/// <summary>
		/// Gets or sets the set name that both Parameters must be a member of.
		/// This is required because a parameter can a member of many sets.
		/// </summary>
		public static string SetName
		{
			get { return setName; }
			set
			{
				if ( value == null )
					throw new ArgumentNullException("value", "setName");
				setName = value;
			}
		}

		#region IComparer Members

		/// <summary>
		/// Returns -1 if Parameter x pos is less then Parameter y.
		/// Returns 0 if equal.
		/// Returns 1 if Parameter x pos is greater then Parameter y.
		/// </summary>
		/// <param name="x">First Parameter object.</param>
		/// <param name="y">Second Parameter object.</param>
		/// <returns></returns>
		public int Compare(object x, object y)
		{
			Parameter p1 = (Parameter)x;
			Parameter p2 = (Parameter)y;
			if ( setName == null )
				throw new InvalidOperationException("SetName must be set to use Comparer.");
			int pos1 = p1.Position;
			int pos2 = p2.Position;
			if ( pos1 == -1 )
				throw new InvalidOperationException("Parameter x must be a position parameter in the set.");
			if ( pos2 == -1 )
				throw new InvalidOperationException("Parameter y must be a position parameter in the set.");

			return pos1.CompareTo(pos2);
		}

	
		#endregion
	}
}
