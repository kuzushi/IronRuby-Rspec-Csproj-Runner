using System;
using System.Collections;
using System.Text;
using System.Globalization;

namespace CmdParser
{
	/// <summary>
	/// This is the abstract base class for all command parsing attributes.
	/// All command attributes derive from this base class.
	/// </summary>
	public sealed class ParameterSet
	{
		private string setName;				// Set name.
		private readonly ArrayList parms;	// Array of Parameters.

		private ParameterSet(string setName)
		{
			if ( setName == null )
				setName = "";
			setName = setName.Trim();
			this.setName = setName;
			parms = new ArrayList();
		}

		internal Parameter[] GetParmsWithPrompt()
		{
			ArrayList al = new ArrayList();
			foreach(Parameter p in parms)
			{
				if ( p.PromptString != null )
					al.Add(p);
			}
			return (Parameter[])al.ToArray(typeof(Parameter));
		}

		/// <summary>
		/// Returns a new parameter set.
		/// </summary>
		/// <param name="setName"></param>
		/// <returns></returns>
		internal static ParameterSet CreateParameterSet(string setName)
		{
			ParameterSet pSet = new ParameterSet(setName);
			return pSet;
		}

		/// <summary>
		/// Gets the name of this parameter set.
		/// </summary>
		public string SetName
		{
			get { return this.setName; }
		}

		/// <summary>
		/// Gets the list of parameters in this set.
		/// </summary>
		public ArrayList Parameters
		{
			get
			{
				return this.parms;
			}
		}

		/// <summary>
		/// Gets the number of parameters in the set.
		/// </summary>
		public int Count
		{
			get { return this.parms.Count; }
		}

		/// <summary>
		/// Returns the usage string for this parameter set.
		/// </summary>
		/// <returns>Usage string.</returns>
		public string GetUsage()
		{
			StringBuilder sb = new StringBuilder();
			foreach(Parameter p in this.parms)
			{
				if ( p.IsOptional )
					sb.Append("[");
				sb.Append("-");
				sb.Append(p.Name);
				if ( ! p.IsSwitch )
				{
					sb.Append(" ");
					sb.Append(p.Type.Name.ToLower(CultureInfo.InvariantCulture));
				}
				if ( p.IsOptional )
					sb.Append("]");
				sb.Append(" ");
			}
			return sb.ToString();
		}

		internal void Add(Parameter parm)
		{
			if ( parm == null )
				throw new ArgumentNullException("parm");

			Parameter p = Find(parm.Name);
			if ( p != null )
				throw new ArgumentException("Duplicate parameters not allowed.");
			
			// Is this parm in the set?
			if ( string.Compare(parm.SetName, this.SetName, true, CultureInfo.InvariantCulture) != 0 )
				throw new InvalidOperationException("Parameter not in set.");

			// Check parm pos in set is not already owned by another parm in same set.
			// -1 pos allowed to have dups, so don't check.
			if ( parm.Position >= 0 )
			{
				foreach(Parameter pp in this.parms)
				{
					if ( pp.Position == parm.Position )
						throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Parameter set '{0}' already contains a parameter at position {1}.", setName, parm.Position));
				}
			}

			//
			// Misc validations.
			//

			// Check only one VarList parm.
			if ( parm.ValueFromRemainingArguments )
			{
				foreach(Parameter pp in this.Parameters)
				{
					if ( pp.ValueFromRemainingArguments )
						throw new CmdException("Only one VarList parameter allowed per parameter set.");
				}
			}

			parms.Add(parm);
		}

		/// <summary>
		/// Returns the count of how many parameters start with the string.
		/// </summary>
		/// <param name="startsWith">The string to search for.</param>
		/// <returns>Number of parameters that start with string.</returns>
		internal int MatchCount(string startsWith)
		{
			ArrayList al = FindMatch(startsWith);
			if ( al == null )
				return 0;
			return al.Count;
		}

		/// <summary>
		/// Returns parameter matching name if exists; otherwise null.
		/// The search is case insensitive.
		/// </summary>
		/// <param name="parameterName">The complete parameter name to find.</param>
		/// <returns>The parameter if found; otherwise null.</returns>
		public Parameter Find(string parameterName)
		{
			if ( parameterName == null )
				return null;
			foreach(Parameter p in parms)
			{
				if ( string.Compare(p.Name, parameterName, true, CultureInfo.InvariantCulture) == 0 )
					return p;
			}
			return null;
		}

		/// <summary>
		/// Returns the parameter that is defined at the position.
		/// </summary>
		/// <param name="position">The position of the position parameter to find.</param>
		/// <returns>Parameter if exists; otherwise null.</returns>
		public Parameter Find(int position)
		{
			if ( position < 0 )
				throw new ArgumentOutOfRangeException("position");

			foreach(Parameter p in this.parms)
			{
				if ( p.Position == position )
					return p;
			}
			return null;
		}

		/// <summary>
		/// Return list of parms that starts with the parmName.  All parameters that start
		/// with the string will be returned.  This can be used to find ambiguous pattern.
		/// </summary>
		/// <param name="startsWith">The string to match names against.</param>
		/// <returns>ArrayList containing any matching parameters.</returns>
		public ArrayList FindMatch(string startsWith)
		{
			if ( startsWith == null )
				throw new ArgumentNullException("startsWith");

			startsWith = startsWith.ToLower(CultureInfo.InvariantCulture);
			ArrayList al = new ArrayList();
			foreach(Parameter p in parms)
			{
				string pName = p.Name.ToLower(CultureInfo.InvariantCulture);
				if ( pName.StartsWith(startsWith) )
				{
					al.Add(p);
				}
			}
			return al;
		}

		/// <summary>
		/// Returns true if set contains parameter by name; otherwise false.
		/// </summary>
		/// <param name="parameterName"></param>
		/// <returns>true if found; otherwise false.</returns>
		public bool Contains(string parameterName)
		{
			Parameter p = Find(parameterName);
			if ( p == null )
				return false;
			return true;
		}

		/// <summary>
		/// Returns true if ParameterSet contains a parm at pos.
		/// </summary>
		/// <param name="position"></param>
		/// <returns>true if found; otherwise false.</returns>
		public bool Contains(int position)
		{
			Parameter p = Find(position);
			if ( p == null )
				return false;
			return true;
		}

		/// <summary>
		/// Returns the array list of all Position parms for this set name that
		/// have not been set yet.
		/// </summary>
		/// <returns></returns>
		public ArrayList GetPositionParametersNotBeenSet()
		{
			ArrayList al = new ArrayList();
			foreach(Parameter p in this.Parameters)
			{
				if ( p.Position > -1 &&	(p.BeenSet == false) )
					al.Add(p);
			}
			if ( al.Count > 1 )
			{
				ParameterPositionComparer.SetName = this.setName;
				al.Sort(new ParameterPositionComparer());
			}
			return al;
		}

		/// <summary>
		/// Returns true if this set has a VarList parameter.
		/// </summary>
		/// <returns></returns>
		public bool HasVariableListParameter()
		{
			foreach(Parameter p in this.Parameters)
			{
				if ( p.ValueFromRemainingArguments )
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns all the mandatory parameter for *this set that have not been set.
		/// </summary>
		/// <returns></returns>
		internal ArrayList GetMandatoryParmsNotSet()
		{
			ArrayList al = new ArrayList();
			foreach(Parameter p in this.Parameters)
			{
				if ( p.IsMandatory && ! p.BeenSet )
					al.Add(p);
			}
			return al;
		}

		/// <summary>
		/// Returns true if all mandatory parameters have been set.
		/// </summary>
		public bool IsAllMandatoryParametersBeenSet
		{
			get
			{
				foreach(Parameter p in this.Parameters)
				{
					if ( p.IsMandatory && ! p.BeenSet )
						return false;
				}
				return true;
			}
		}

		private static bool NameMatchesOneInList(string name, ArrayList namedParms)
		{
			foreach(string np in namedParms)
			{
				if ( string.Compare(np, name, true, CultureInfo.InvariantCulture) == 0 )
					return true;
			}
			return false;
		}
		
		/// <summary>
		/// Returns the total number of position parms in this set.
		/// </summary>
		/// <returns></returns>
		public int GetNumberOfPositionParameters()
		{
			int numPosParms = 0;
			foreach(Parameter p in Parameters)
			{
				if ( p.Position > -1 )
					numPosParms++;
			}
			return numPosParms;
		}

		/// <summary>
		/// Returns the number of pos parms that are left after subtracting all the named
		/// parms in "namedParms" that are also pos parms.
		/// </summary>
		/// <param name="namedParameters"></param>
		/// <returns></returns>
		public int GetNumberOfPositionParametersLeft(ArrayList namedParameters)
		{
			int namedPosParms = 0;
            // Each parameter must match a namedParameter.  Is this right?  I don't think so.

            // Foreach parameter, see if it is a NamedParm on the commandline.  If so, see if it is a
            // Position parm and if so, increment namedPosParms counter.
			foreach(Parameter p in this.Parameters)
			{
				if ( NameMatchesOneInList(p.Name, namedParameters) )
				{
					if ( p.Position > -1 )
						namedPosParms++;
				}
                //else
                //    throw new CmdException("Parameter name in namedParameters list does not exist in this set."); 
			}

			int totPosParms = GetNumberOfPositionParameters();
			return totPosParms - namedPosParms;
		}

		/// <summary>
		/// Returns the VarLengthParameter in this set.  Only one can exist per set.
		/// </summary>
		/// <returns>The variable length parameter if it exists; otherwise null.</returns>
		public Parameter GetVariableListParameter()
		{
			foreach(Parameter p in this.parms)
			{
				if ( p.ValueFromRemainingArguments )
					return p;
			}
			return null;
		}
	}
}
