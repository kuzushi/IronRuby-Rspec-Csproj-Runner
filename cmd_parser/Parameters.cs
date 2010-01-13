//#define BETA
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Globalization;

namespace CmdParser
{
	/// <summary>
	/// Object to contain a collection of Parameter objects organized into ParameterSets.
	/// The ParameterAttribute attributes define what set a parameter belongs to and
	/// what validation rules are followed for input parsing.
	/// </summary>
	public sealed class Parameters
	{
		private bool helpRequested;
		private string helpChars = "";
		private bool versionRequested;
		private ParameterSet activeSet;
		private readonly ArrayList pSets = new ArrayList();
		private Parameter[] parameters;
		private CmdHelpAttribute cmdHelp;
		private string defaultSetName;

		private Parameters()
		{
		}

		/// <summary>
		/// Gets the default set name.
		/// </summary>
		public string DefaultSetName
		{
			get { return this.defaultSetName; }
		}

		/// <summary>
		/// Gets the CmdHelpAttribute defined on the class or null.
		/// </summary>
		public CmdHelpAttribute CmdHelp
		{
			get { return this.cmdHelp; }
		}

		/// <summary>
		/// Gets the active ParameterSet in use. The set name will be the name
		/// of the set defined by the parameter data object or empty string ("")
		/// if the default set is the active set.
		/// </summary>
		public ParameterSet ActiveSet
		{
			get { return this.activeSet; }
		}

		/// <summary>
		/// Returns true if user supplied one of the help arguments (i.e. -h, -help, -?, -??).
		/// </summary>
		/// <remarks>
		/// Test this parameter after command line has been parsed to determine
		/// if you need to display help to the user.  You can display different
		/// levels of help depending on the <c>HelpString</c> selected.
		/// </remarks>
		public bool IsHelpNeeded
		{
			get { return this.helpRequested; }
		}

		/// <summary>
		/// Gets the help parameter the user selected.  If <code>IsHelpNeeded</code> is true,
		/// this will return one of the following string (h, help, ?, ??).
		/// </summary>
		public string HelpChars
		{
			get { return this.helpChars; }
		}

		/// <summary>
		/// Returns true if user supplied "-Version" parameter at the command line.
		/// </summary>
		/// <remarks>
		/// Test this parameter after the command line has been parsed to determine
		/// if you need to display the application version information.
		/// </remarks>
		public bool IsVersionNeeded
		{
			get { return this.versionRequested; }
		}

		/// <summary>
		/// Gets number of parameters that have been set.
		/// </summary>
		public int BeenSetCount
		{
			get
			{
				int parmsSet = 0;
				foreach(Parameter p in this.parameters)
				{
					if ( p.BeenSet )
						parmsSet++;
				}
				return parmsSet;
			}
		}

		/// <summary>
		/// Gets array of all the Parameter objects defined in the data object.
		/// </summary>
		public Parameter[] GetParameterArray()
		{
			return (Parameter[])parameters.Clone();
		}

		/// <summary>
		/// Gets array of ParameterSet objects that are defined in the instance object.
		/// Any parameter that is not explicitly set to a ParameterSet will belong to
		/// the default set (i.e. set name is an empty string).
		/// </summary>
		public ParameterSet[] GetParameterSets()
		{
			return (ParameterSet[])this.pSets.ToArray(typeof(ParameterSet)); //.pSets;
		}

		/// <summary>
		/// Set members with ParameterAttribute(s) in instance object to values set
		/// in passed args.  A Parameters object is returned.  The instance object will
		/// scaned using reflection for public fields and properties that have
		/// ParameterAttribute(s) set.  The set of ParameterAttributes applied to a 
		/// member will define the validation and parsing behavior.  Respective
		/// exceptions will be thrown if the rules defined by the ParameterAttributes
		/// are violated by the input args. 
		/// </summary>
		/// <param name="instance">The object that declares the ParameterAttribute(s).</param>
		/// <param name="args">The args[] passed at the command line.</param>
		/// <returns>Parameters object.</returns>
		public static Parameters CreateParameters(object instance, string[] args)
		{
			if ( instance == null )
				throw new ArgumentNullException("instance");

			#if BETA
				if ( DateTime.Now > DateTime.Parse("7/1/2005", CultureInfo.InvariantCulture) )
					throw new InvalidOperationException("Beta expired on 7/1/2005.  Please go to www.mvptools.com for update.");
			#endif

			// Create Parameter sets using attributes of instance object.
			Parameters parms = Parameters.CreateParameters(instance);

            //foreach ( Parameter p in parms.GetParameterArray() )
            //{
            //    Console.WriteLine(p.Name);
            //}
			// Set the fields/parameters using cmdline args.
			// Instance object will have all *passed parms set and verified.
			// Any parm in instance not set (and not mandatory) will retain
			// its' default value (i.e. not be set).
			parms.activeSet = parms.ParseCmdLine(args);
			return parms;
		}

		/// <summary>
		/// Creates an instance of a Parameters object using the instance object.
		/// The instance object will be scaned using reflection for public fields
		/// and properties that have ParameterAttribute(s) set.
		/// The set of ParameterAttributes applied to a member will define the validation
		/// and parsing behavior.  Exception will be thrown if the rules defined by the
		/// ParameterAttributes are violated by the input args. 
		/// </summary>
		/// <param name="instance">The object that declares the ParameterAttribute(s).</param>
		public static Parameters CreateParameters(object instance)
		{
			if ( instance == null )
				throw new ArgumentNullException("instance");
			Type type = instance.GetType();
			if ( ! (type.IsPublic && type.IsClass) )
				throw new ArgumentException("instance must be a public class.");

			// Get any ParameterBase attributes in the class.  This does no validations, but
			// just gets the attributes.
			Parameter[] parmArray = GetPublicReadWriteCmdMembers(instance);
			
			if ( parmArray == null || parmArray.Length == 0 )
				throw new ArgumentException("instance does not contain any parameters.");
			
			// Create a parameters object that contains ParameterSets.
			// ParameterSet contains parameters.
			Parameters parameters = Parameters.CreateParameters(parmArray);

			// Get the CmdHelpAttribute on class if it exists.
			object[] cmdAtts = type.GetCustomAttributes(typeof(CmdHelpAttribute), true);
			for(int i=0; i < cmdAtts.Length; i++)
			{
				if ( cmdAtts[i] is CmdHelpAttribute )
					parameters.cmdHelp = (CmdHelpAttribute)cmdAtts[i];
			}

			// Get the DefaultSetAttribute if it exists.  If not, default set will be
			// "" set or first set if no "" set.
			object[] defAtts = type.GetCustomAttributes(typeof(DefaultSetAttribute), true);
			if ( defAtts.Length == 0 )
			{
				ParameterSet firstSet = null;
				foreach(ParameterSet ps in parameters.GetParameterSets())
				{
					if ( firstSet == null )
						firstSet = ps;
					if ( ps.SetName.Length == 0 )
					{
						parameters.defaultSetName = "";
						break;
					}
				}
				if ( firstSet == null )
					throw new CmdException("No parameter sets created.");
				if ( parameters.defaultSetName == null )
					parameters.defaultSetName = firstSet.SetName;
			}
			else
			{
				// Check that the default set name actually matches one of the sets.
				// If so, the default set name is that; otherwise error.
				DefaultSetAttribute dsa = (DefaultSetAttribute)defAtts[0];
				foreach(ParameterSet ps in parameters.GetParameterSets())
				{
					if ( string.Compare(dsa.SetName, ps.SetName, true, CultureInfo.InvariantCulture) == 0 )
					{
						parameters.defaultSetName = dsa.SetName;
						break;
					}
				}
				if ( parameters.defaultSetName == null )
					throw new CmdException("Default set name does not match a named set.");
			}
			
			return parameters;
		}

		private static Parameters CreateParameters(Parameter[] parmArray)
		{
			if ( parmArray == null )
				throw new ArgumentNullException("parmArray");
			if ( parmArray.Length == 0 )
				throw new ArgumentException("count is zero.", "parmArray");

			Parameters ps = new Parameters();

			foreach(Parameter p in parmArray)
			{
				if ( p == null )
					throw new ArgumentNullException("parameter array contains a null element or element not of type Parameter.");
				ParameterSet pSet = ps.Find(p.SetName);
				if ( pSet == null )
				{
					pSet = ParameterSet.CreateParameterSet(p.SetName);
					ps.Add(pSet);	// Add the new parm set.
				}
				pSet.Add(p);		// Add the parm.  If dup, will error.
			}
			// Changed 4/13
			// Enum parms and remove dups.  You will get dups when ParameterAttribute is
			// used multiple times to add parm to different sets.
			Hashtable ht = new Hashtable();
			foreach(Parameter p in parmArray)
			{
				ht[p.Name] = p;
			}
			ArrayList al = new ArrayList();
			foreach(Parameter p in ht.Values)
			{
				al.Add(p);
			}
			// Sort al by parameter name.
			al.Sort(new ParameterNameComparer());
			ps.parameters = (Parameter[])al.ToArray(typeof(Parameter));
			return ps;
		}

		private static bool IsParmASwitch(string[] switches, string parmName)
		{
			if ( switches == null || switches.Length == 0 )
				return false;
			if ( parmName == null )
				throw new ArgumentNullException("parmName");

			foreach(string s in switches)
			{
				if ( string.Compare(s, parmName, true, CultureInfo.InvariantCulture) == 0 )
					return true;
			}
			return false;
		}

		private static string NormalizeTrueFalse(string s)
		{
			if ( s == null )
				return null;
			switch(s.ToLower(CultureInfo.InvariantCulture))
			{
				case "t":
				case "tr":
				case "tru":
				case "true":
				case "1":
					return "true";
				case "f":
				case "fa":
				case "fal":
				case "fals":
				case "false":
				case "0":
					return "false";
			}
			return null;
		}

//		private static bool IsTrueFalseString(string s)
//		{
//			if ( s == null )
//				return false;
//			switch(s.ToLower())
//			{
//				case "t":
//				case "true":
//				case "f":
//				case "false":
//				case "1":
//				case "0":
//					return true;
//			}
//			return false;
//		}

		private static bool ContainsColon(string s)
		{
			foreach(Char c in s)
			{
				if ( c == ':' )
					return true;
			}
			return false;
		}

		private ArrayList GetAllSetsThatContainNames(ArrayList namedParms)
		{
			ArrayList al = new ArrayList();
			if ( namedParms == null || namedParms.Count == 0 )
				return al;
			
			foreach(ParameterSet pSet in this.GetParameterSets())
			{
				bool good = false;
				foreach(string name in namedParms)
				{
					if ( ! pSet.Contains(name) )
					{
						good = false;
						break;
					}
					else
						good = true;
				}
				if ( good )
					al.Add(pSet);
			}
			return al;
		}

		private ArrayList GetSetsWithGreaterNumberOfPositionParameters(int posParmCount)
		{
			ArrayList sets = new ArrayList();
			foreach(ParameterSet pSet in this.GetParameterSets())
			{
				if ( pSet.GetNumberOfPositionParameters() > posParmCount )
					sets.Add(pSet);
			}
			return sets;
		}

		private ArrayList GetSetsWithNumberOfPositionParameters(int posParmCount)
		{
			ArrayList sets = new ArrayList();
			foreach(ParameterSet pSet in this.GetParameterSets())
			{
				if ( pSet.GetNumberOfPositionParameters() == posParmCount )
					sets.Add(pSet);
			}
			return sets;
		}

		/// <summary>
		/// Find the parameter set to use.  A set can match if 
		/// </summary>
		/// <param name="namedParms"></param>
		/// <param name="unnamedParms"></param>
		/// <returns></returns>
		private ParameterSet GetMatchingCmdSet(ArrayList namedParms, ArrayList unnamedParms)
		{
			if ( namedParms.Count == 0 && unnamedParms.Count == 0 )
			{
				// No parms set at cmd line.  Still, we may want to prompt for manditory parms.
				// So return the default set.
				foreach(ParameterSet ps in this.GetParameterSets())
				{
					if ( string.Compare(ps.SetName, this.defaultSetName, true, CultureInfo.InvariantCulture) == 0 )
						return ps;
				}
				throw new CmdException("No set found matching parameters. Use '-?' for help.");
			}

			// No named parms supplied, only pos parms.  Figure out set via pos parms.
			if ( namedParms.Count == 0 )
			{
				// No named parms given.  Must find best set via pos parms.
				// Find set with exact number of pos parms.
				ArrayList posSets = GetSetsWithNumberOfPositionParameters(unnamedParms.Count);
				if ( posSets.Count == 0 )
				{
					// No sets had exact number of pos parms.
					// Find set with varlist if exists.  VarList set takes priority at this point.
					foreach(ParameterSet ps in this.GetParameterSets())
					{
						if ( ps.HasVariableListParameter() )
							return ps;
					}

					// Find sets with > number of pos parms.
					posSets = GetSetsWithGreaterNumberOfPositionParameters(unnamedParms.Count);
					if ( posSets.Count == 1 )
						return (ParameterSet)posSets[0];
					
					// Return set that is the default set as none is better.
					foreach(ParameterSet ps in posSets)
					{
						if ( string.Compare(ps.SetName, this.defaultSetName, true, CultureInfo.InvariantCulture) == 0 )
							return ps;
					}
					throw new CmdException("No set found matching position parameters. Use '-?' for help.");
				}
				else
				{
					// We have one or more sets with same number of pos parms.
					// Return default set if given or error as we have no clue.
					if ( posSets.Count == 1 )
						return (ParameterSet)posSets[0];
					foreach(ParameterSet ps in posSets)
					{
						if ( string.Compare(ps.SetName, this.defaultSetName, true, CultureInfo.InvariantCulture) == 0 )
							return ps;
					}
					throw new CmdException("No set found matching position parameters. Use '-?' for help.");
				}
			}

			// Get all sets that contain the named parms.
			ArrayList setsMatchNames = GetAllSetsThatContainNames(namedParms);
			
			if ( setsMatchNames.Count == 0 )
				throw new CmdException("No set found matching parameter names. Use '-?' for help.");
						
			// If we only have one set that matches all names, return that.
			if ( setsMatchNames.Count == 1 )
				return (ParameterSet)setsMatchNames[0];

			if ( unnamedParms.Count == 0 )
			{
				// More sets exist that contain the same named parms and no pos parms
				// are given that would allow a better match.  So what set to return?
				// Return the default set.
				foreach(ParameterSet ps in setsMatchNames)
				{
					if ( string.Compare(ps.SetName, this.defaultSetName, true, CultureInfo.InvariantCulture) == 0 )
						return ps;
				}
				throw new CmdException("Multiple sets match the named parameters and none of them are the default set.");
			}

			// See if we have a set that has *exact number of pos parms left to fill
			// after subtracting the named parms that may have also been posParms.
			foreach(ParameterSet pSet in setsMatchNames)
			{
				int parmsLeft = pSet.GetNumberOfPositionParametersLeft(namedParms);
				if ( parmsLeft == unnamedParms.Count )
					return pSet;
			}

			// No Set matched ParmsLeft PosParms left to fill exactly.
			// See if we have a set that contains a VarList.
			foreach(ParameterSet pSet in setsMatchNames)
			{
				if ( pSet.HasVariableListParameter() )
					return pSet;
			}

			// We may have had a set that contains more pos parms left to fill
			// then we had unnamedParms *and no VarList set existed in prior test.
			// This returns the first set.  Is there a better one?
			foreach(ParameterSet pSet in setsMatchNames)
			{
				int parmsLeft = pSet.GetNumberOfPositionParametersLeft(namedParms);
				if ( parmsLeft > unnamedParms.Count )
					return pSet;
			}

			// No sets matches, so return null.
			return null;
		}

		internal string GetCanonicalParmName(string name)
		{
			// Match exact first, then try to match parcial.
			if ( name == null )
				throw new ArgumentNullException("name");
			name = name.ToLower(CultureInfo.InvariantCulture);

			bool alreadyMatched = false;
			Parameter foundParm = null;
			// Find exact match.
			foreach(Parameter p in this.parameters)
			{
				string pName = p.Name.ToLower(CultureInfo.InvariantCulture);
				if ( pName == name )
					return p.Name;
				if ( pName.StartsWith(name) )
				{
					if ( alreadyMatched )
						Throw.AmbiguousParameter(name);
					alreadyMatched = true;
					foundParm = p;
				}
			}

			// No exact match found.  Try parcial.
			if ( foundParm == null )
			{
				foreach(Parameter p in this.parameters)
				{
					string pName = p.Name.ToLower(CultureInfo.InvariantCulture);
					if ( pName.StartsWith(name) )
					{
						if ( alreadyMatched )
							Throw.AmbiguousParameter(name);
						alreadyMatched = true;
						foundParm = p;
					}
				}
			}

			if ( foundParm != null )
				return foundParm.Name;
			return null; // Parm name not found.
		}

		/// <summary>
		/// Returns the system generated detailed help using the attributes set on the data object.
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		public string GetDetailedHelp(Assembly assembly)
		{
			// Following currently not supported:
				// ParsingAllowPipelineInput
				// ParsingDefaultParameterSet
				// ExpandWildCards
				// PrerequisiteUITypeAttribute
				// ParsingInteractionAttribute
				// ProcessingTrimStringAttribute
				// ProcessingTrimCollectionAttribute
			/*
			From "Writing a Cmdlet" doc.
			Although it is not required, the convention is that a parameter should be
			documented with its name on the first line, its type in square brackets
			on the second line, any additional metadata information in square brackets
			on subsequent lines, followed by a description:
			-Path pathname
			[String]
			[pipeline input allowed]
			[allows globbing]
			The pathname parameter…
			*/
			if ( assembly == null )
				throw new ArgumentNullException("assembly");

			string exeName = assembly.GetName().Name;

			//
			// Header.
			//
			StringBuilder sb = new StringBuilder();
			string header = exeName + " Help";
			sb.Append(header + "\n");
			sb.Append(new string('=', 70) + "\n");
			// Copyright.
			if ( CmdHelp != null && CmdHelp.Copyright != null )
				sb.Append(CmdHelp.Copyright + "\n");
			// URI
			if ( CmdHelp != null && CmdHelp.HomeUri != null )
				sb.Append("Web Site: " + CmdHelp.HomeUri.ToString() + "\n");
			// Version
			string ver = assembly.GetName().Version.ToString();
			sb.Append("Version : " + ver + "\n");
			// Summary.
			if ( this.CmdHelp != null && CmdHelp.LongDescription != null )
				sb.Append(CmdHelp.LongDescription + "\n");

			//
			// Parameters.
			//
			sb.Append("\n");
			sb.Append("Parameters\n");
			sb.Append(new string('-', 70) + "\n");

			foreach(Parameter p in this.parameters)
			{
				/*
				 * ParmName
				 * [Type]
				 * [Set]
				 * [Range]
				 * [Count]
				 * [ValLen]
				 * Summary:
				 * (Summary)
				 * Description:
				 * (Summary)
				 */

				// Name.
				sb.Append("-" + p.Name + "\n");

				// Type.
				sb.Append("[" + p.Type.Name + "]\n" );

				// Switch.
				if ( p.IsSwitch )
					sb.Append("[Switch]\n");

				// Mandatory.
				if ( p.IsMandatory )
					sb.Append("[Mandatory]\n");

				// Mappings.
				sb.Append("[Mappings:");
				bool hasSets = false;
				foreach(ParameterSet pset in this.pSets)
				{
					foreach(Parameter tp in pset.Parameters)
					{
						if ( tp.Name == p.Name )
						{
							string setName = (tp.SetName == "") ? "Default" : tp.SetName;
							sb.AppendFormat("{0}({1}),", setName, tp.Position);
							hasSets = true;
						}
					}
				}
				if ( hasSets )
					sb.Remove(sb.Length - 1, 1);
				sb.Append("]\n");

				// Val Len.
				if ( p.ValLen != null)
					sb.AppendFormat("[Val Length=({0}-{1})]\n",p.ValLen.Min, p.ValLen.Max); 

				// Val Count.
				if ( p.ValCount != null )
					sb.AppendFormat("[Val Count=({0}-{1})]\n", p.ValCount.Min, p.ValCount.Max);
				
				// Val Pattern.
				if ( p.ValPattern != null )
					sb.AppendFormat("[Val Pattern={0}]\n", p.ValPattern);
				
				// Val Set.
				if ( p.ValSet != null && p.ValSet.Length > 0 )
				{
					sb.Append("[Valid Set=(");
					foreach(object o in p.ValSet)
					{
						string s = o.ToString();
						sb.Append(s + ",");
					}
					if ( sb[sb.Length - 1] == ',' )
						sb.Remove(sb.Length - 1, 1);
					sb.Append(")]\n");
				}

				// Val Range.
				if ( p.ValRange != null )
				{
					sb.AppendFormat("[Val Range=({0}-{1})]\n", p.ValRange.Min.ToString(), p.ValRange.Max.ToString());
				}

				// Prompt String.
				if ( p.PromptString != null )
					sb.AppendFormat("[Prompt='{0}']\n", p.PromptString);

				// Long Help.
				if ( p.Help != null && p.Help.LongDescription != null )
					sb.Append(p.Help.LongDescription + "\n");

				sb.Append("\n");
			}

			// Examples.
			if ( this.CmdHelp != null && CmdHelp.Example != null )
				sb.Append(CmdHelp.Example + "\n");

			return sb.ToString();
		}

		/// <summary>
		/// Returns the default usage help string built using reflection
		/// over the data class.
		/// </summary>
		/// <param name="assembly">The primary assembly to get the name and version.</param>
		/// <param name="padding">The column width of the parameter names column.  Set this to the length that allows your short descriptions to line up.  The minimum value is 1.  9 or 10 is an average value.</param>
		/// <returns>The usage string.</returns>
		public string GetUsageString(Assembly assembly, int padding)
		{
			if ( assembly == null )
				throw new ArgumentNullException("assembly");
			if ( padding < 1 )
				padding = 1;

			string exeName = assembly.GetName().Name;
			StringBuilder sb = new StringBuilder();
			foreach(ParameterSet ps in this.GetParameterSets())
			{
				sb.Append(exeName + " ");
				string usage = ps.GetUsage();
				if ( usage == null )
					usage = "";
				sb.Append(usage + "\n");
			}

			sb.Append("\nParameters:\n");
			foreach(Parameter p in this.GetParameterArray())
			{
				string pName = p.Name.PadRight(padding);
				sb.Append(pName + " - ");
				if ( p.Help != null && p.Help.ShortDescription != null )
					sb.Append(p.Help.ShortDescription + "\n");
				else
					sb.Append("\n");
			}
			return sb.ToString();
		}

		/// <summary>
		/// Using the parameter names and/or position parameters given in args,
		/// this method finds the best Set to use and fills the matching
		/// fields with parameter data.  The selected ParameterSet is returned.
		/// </summary>
		/// <param name="args">string array with parameters.</param>
		/// <returns>ParameterSet</returns>
		public ParameterSet ParseCmdLine(string[] args)
		{
			if ( args == null )
				throw new ArgumentNullException("args");

			#if ( BETA )
			if ( DateTime.Now >= DateTime.Parse("11/01/2005", CultureInfo.InvariantCulture) )
				throw new CmdException("Beta expired.  Please visit www.mvptools.com for full version.");
			#endif

			string[] switches = GetSwitches();

			// Parse CmdLine.  Get named parms and left over pos parms.
			Hashtable namedParms = new Hashtable();
			ArrayList posParms = new ArrayList();
			
			int maxIndex = args.Length - 1;
			for(int i=0; i < args.Length; i++)
			{
				string parm = args[i];

				// Get Named parms.
				// Test if help is needed.  If help is anywhere on the cmdline,
				// processing stops and the Help delegate is called.
				string lparm = parm.ToLower(CultureInfo.InvariantCulture);
				switch(lparm)
				{
					case "-?":		// Usage.
					case "-??":		// Detailed help.
					//case "-h":		// Detailed help.
					case "-help":	// Detailed help.
						this.helpRequested = true;
						this.helpChars = lparm.Remove(0, 1);
						return null;
					case "-version":	// Application version.
						this.versionRequested = true;
						return null;
				}

				if ( parm.StartsWith("--") )
				{
					break; // End of cmd line processing.
				}

				if ( parm.StartsWith("-") )
				{
					parm = parm.Remove(0, 1);
					if ( parm.Length == 0 )
						Throw.InvalidParmName("No name");

					// Check if contains ":".
					if ( ContainsColon(parm) )
					{
						string[] parts = parm.Split(new char[]{':'}, 2);
						if ( parts.Length != 2 )
							Throw.InvalidParmValue(parm, parm);
						if ( parts[0].Length == 0 )
							Throw.InvalidParmName("No name");
						if ( parts[1].Length == 0 )
							Throw.InvalidParmValue(parts[0], "No value");
						parm = parts[0];
						parm = GetCanonicalParmName(parm);
						if ( parm == null )
							Throw.InvalidParmName(args[i]);

						if ( IsParmASwitch(switches, parm) )
						{
							string tfString = NormalizeTrueFalse(parts[1]);
							if ( tfString == null )
								Throw.ConversionError(parm, typeof(bool), parts[1]);
							parts[1] = tfString;
						}
						if ( namedParms.ContainsKey(parm) )
							Throw.DuplicateParmError(parm);
						namedParms.Add(parm, parts[1]);
						continue;
					}

					//
					// Check if unambiguous name.
					//
					string tmpName = parm;
					parm = GetCanonicalParmName(parm);
					if ( parm == null )
						Throw.InvalidParmName(tmpName);

					// Check if switch.  If switch, we don't look for next value.
					if ( IsParmASwitch(switches, parm) )
					{
						namedParms.Add(parm, "true");
						continue;
					}

					//
					// Get the Value next to the name.
					//
					int next = i + 1;
					if ( next > maxIndex )
						Throw.InvalidParmValue(parm, "No value");
					string val = args[next];
					if ( val.StartsWith("-") )
					{
						// If only '-' given, this is an error.
						if ( val.Length <= 1 )
							Throw.InvalidParmValue(parm, "Expected a value, not a '-'.");
						// If digit after '-' then this may be an integer, so let it pass.
						if ( ! Char.IsDigit(val[1]) )
						{
							// Was a char after '-', so error as expected a value, not
							// another parm name.
							string s = string.Format(CultureInfo.InvariantCulture, "'{0}' invalid.  Expected a value, not a named parameter.", val);
							Throw.InvalidParmValue(parm, s);
						}
					}
					else if ( val.StartsWith(@"\") )
					{
						// Escape char.  Anything following is ok.
						if ( val.Length <=1 )
							Throw.InvalidParmValue(parm, @"No value after escape character '\'.");
						// Remove the escape char.
						val = val.Remove(0, 1);
					}
					if ( namedParms.ContainsKey(parm) )
						Throw.DuplicateParmError(parm);
					namedParms.Add(parm, val);
					i = next;
				}
				else if ( parm.StartsWith(@"\") )
				{
					// parm starts with escape char so it is explicitly a pos parm or varlist parm.
					if ( parm.Length <= 1 )
						Throw.ParameterMismatch();
					parm = parm.Remove(0, 1);
					posParms.Add(parm);
				}
				else
				{
					// Pos parm.
					posParms.Add(parm);
				}
			}

			// Now have Named parms and unnamed parms (which may be pos parms).
			// Find the ParameterSet to use for the parms passed.
			ArrayList namedParmsList = new ArrayList(namedParms.Keys);
			ParameterSet pSetToUse = GetMatchingCmdSet(namedParmsList, posParms);
			if ( pSetToUse == null )
				Throw.ParameterMismatch();

			// Set *Named parms on Instance object.  Any value not given will not be changed and will keep
			// its default value.  Any error in converstions or validations will throw exception.
			foreach(DictionaryEntry de in namedParms)
			{
				string parm = de.Key as string;
				string sVal = de.Value as string;
				Parameter p = pSetToUse.Find(parm);
				p.SetValue(sVal);
			}

			// Set *Position parms.
			// The Named parms are stripped first.  Any parameter that are left
			// are set in the position parms from left to right.  If a parameter
			// is a position parm, but has already been set via a name, that
			// parm is skipped and the next position parm is used.
			// Array is sorted by pos number.
			ArrayList posParmsNotSet = pSetToUse.GetPositionParametersNotBeenSet();

			int posParmsSet = 0;
			int curUnnamedIndex = 0;
			foreach(Parameter p in posParmsNotSet)
			{
				if ( (curUnnamedIndex + 1) > posParms.Count )
					break;	// Set all values in posParms array.
				string val = (string)posParms[curUnnamedIndex++];
				p.SetValue(val);
				posParmsSet++;
			}

			if ( posParmsSet >= posParms.Count )
			{
				// We set all the posParms.  So continue.
			}
			else
			{
				// We set all the posParms we could.  But have some left over so
				// set the VarList if it exists; otherwise error.
				Parameter varp = pSetToUse.GetVariableListParameter();
				if ( varp == null )
					throw new CmdException("Too many position parameter(s) for set.");
				ArrayList al = new ArrayList();
				for(int i=posParmsSet; i < posParms.Count; i++)
				{
					al.Add(posParms[i]);
				}
				string s = String.Join(",", (string[])al.ToArray(typeof(string)));
				varp.SetValue(s);
			}
		
			// Check all Manditory parms have been set.  May also prompt for input.
			// Note: Only mandatory parms for the current set are required.
			if ( ! pSetToUse.IsAllMandatoryParametersBeenSet )
			{
				ArrayList notSetList = pSetToUse.GetMandatoryParmsNotSet();
				// Check all mandatory parms that have not been set to see if they
				// support a prompt.
				foreach(Parameter p in notSetList)
				{
					// If a parm is mandatory and a prompt has not been set,
					// then we throw error as dev did not want to prompt for input.
					if ( p.PromptString == null )
						Throw.MandatoryError();
				}

				foreach(Parameter p in notSetList)
				{
					// Format the prompt.
					string defAnswer;
					if ( p.DefaultAnswer == null )
						defAnswer = "";
					else
						defAnswer = p.DefaultAnswer;
					string prompt;
					try
					{
						prompt = string.Format(CultureInfo.InvariantCulture, p.PromptString, defAnswer);
					}
					catch
					{
						throw new CmdException("Invalid prompt string.");
					}
					// Write the prompt.
					Console.Write(prompt);
					string input = Console.ReadLine();
					if ( input == null )
					{
						// Ctrl-C hit, so Exit.
						throw new CmdException("Ctrl-C Hit.");
					}
					if ( input.Length == 0 ) // User hit enter with no data.
					{
						if ( p.DefaultAnswer == null )
							input = "";
						else
							input = p.DefaultAnswer;
					}
					// Let SetValue throw any context specific exception.
					p.SetValue(input);
				}
			}

			return pSetToUse;
		}

		private static bool ArrayContainsString(ArrayList al, string s)
		{
			if ( al == null )
				throw new ArgumentNullException("al");
			foreach(string ss in al)
			{
				if ( ss == null )
					continue;
				if ( string.Compare(s, ss, true, CultureInfo.InvariantCulture) == 0 )
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns string[] of all parameters that are switches
		/// </summary>
		/// <returns></returns>
		internal string[] GetSwitches()
		{
			ArrayList al = new ArrayList();
			foreach(ParameterSet pSet in this.GetParameterSets())
			{
				foreach(Parameter p in pSet.Parameters)
				{
					if ( p.IsSwitch )
						if ( ! ArrayContainsString(al, p.Name) )
							al.Add(p.Name);
				}
			}
			return (string[])al.ToArray(typeof(string));
		}

		internal void Add(ParameterSet pSet)
		{
			if ( pSet == null )
				throw new ArgumentNullException("pSet");
			ParameterSet ps = Find(pSet.SetName);
			if ( ps != null )
				throw new ArgumentException("Duplicate parameter sets not allowed.");
			pSets.Add(pSet);
		}

		internal bool Contains(string setName)
		{
			object o = Find(setName);
			if ( o == null )
				return false;
			return true;
		}

		internal ParameterSet Find(string setName)
		{
			if ( setName == null )
				return null;
			foreach(ParameterSet pset in pSets)
			{
				if ( string.Compare(pset.SetName, setName, true, CultureInfo.InvariantCulture) == 0 )
					return pset;
			}
			return null;
		}

		private static Parameter[] GetPublicReadWriteCmdMembers(object instance)
		{
			if ( instance == null )
				throw new ArgumentNullException("instance");

			ArrayList al = new ArrayList();
			Type type = instance.GetType();
			ArrayList members = new ArrayList();
			members.AddRange(type.GetProperties());
			members.AddRange(type.GetFields());

			if ( members.Count == 0 )
				throw new ArgumentException("No public members in type.");

			// TODO: Add logic to add more parms if multiple ParameterAttributes on a member.
			// Get array of ParameterAttributes.
			// Loop and add to members arraylist.
			// Probably ignore teh ParameterAttribute in CreateParameter()
			// and add a ParmAttribute to the constructor.  That way it only "sees"
			// the single ParmAttribute we pass.  Loop in case below if member
			// has ParmAttribute for each parmAttribute.  That will add that number
			// of parms to the al.
			foreach(MemberInfo mi in members)
			{
				// Only add members that have ParameterBaseAttribute(s).
				if ( ! mi.IsDefined(typeof(ParameterBaseAttribute), true) )
					continue;
				
				switch(mi.MemberType)
				{
					case MemberTypes.Property:
						PropertyInfo pi = (PropertyInfo)mi;
						if ( ! (pi.PropertyType.IsPublic && pi.CanRead && pi.CanWrite) )
							throw new ArgumentException("All CMD members must be public readable and writeable.");

						// Loop here on members if parameterAttributes.
						object[] pArray = pi.GetCustomAttributes(typeof(ParameterAttribute), true);
						if ( pArray != null && pArray.Length > 0 )
						{
							foreach(ParameterAttribute pa in pArray)
							{
								Parameter p = Parameter.CreateParameter(instance, mi, pa);
								al.Add(p);
							}
						}
						else
						{
							// Use default ParameterAttribute.
							ParameterAttribute pa = new ParameterAttribute();
							Parameter p = Parameter.CreateParameter(instance, mi, pa);
							al.Add(p);
						}
						break;
					case MemberTypes.Field:
						FieldInfo fi = (FieldInfo)mi;
						if ( ! fi.FieldType.IsPublic )
							throw new ArgumentException("All Cmd members must be public");

						object[] pArray2 = fi.GetCustomAttributes(typeof(ParameterAttribute), true);
						if ( pArray2 != null && pArray2.Length > 0 )
						{
							foreach(ParameterAttribute pa in pArray2)
							{
								Parameter p = Parameter.CreateParameter(instance, mi, pa);
								al.Add(p);
							}
						}
						else
						{
							// Use default ParameterAttribute.
							ParameterAttribute pa = new ParameterAttribute();
							Parameter p = Parameter.CreateParameter(instance, mi, pa);
							al.Add(p);
						}

//						//TODO
//						Parameter p2 = Parameter.CreateParameter(instance, mi, null);
//						al.Add(p2);
						break;
					default:
						break;
				}
			}
			return (Parameter[])al.ToArray(typeof(Parameter));
		}

		/// <summary>
		/// Splits any string using seperators string.  This is different from the
		/// string.Split method as we ignore delimiters inside double quotes and
		/// will *ignore multiple delimiters in a row (i.e. "One,,,,two" will split
		/// to two fields if comma is a delimiter).
		/// Example:
		/// Delims: " \t," (space, tab, comma)
		/// Input: "one two" three four,five
		/// Returns (4 strings):
		/// one two
		/// three
		/// four
		/// five
		/// </summary>
		/// <param name="text">The string to split.</param>
		/// <param name="delimiters">The characters to split on.</param>
		/// <returns></returns>
		internal static string[] SplitQuoted(string text, string  delimiters)
		{
			// Default delimiters are a space and tab (e.g. " \t").
			// All delimiters not inside quote pair are ignored.  
			// Default quotes pair is two double quotes ( e.g. '""' ).
			if ( text == null )
				throw new ArgumentNullException("text", "text is null.");
			if ( delimiters == null || delimiters.Length < 1 )
				delimiters = " \t"; // Default is a space and tab.

			ArrayList res = new ArrayList();

			// Build the pattern that searches for both quoted and unquoted elements
			// notice that the quoted element is defined by group #2 (g1)
			// and the unquoted element is defined by group #3 (g2).
			
			string pattern =
				@"""([^""\\]*[\\.[^""\\]*]*)""" +
				"|" +
				@"([^" + delimiters + @"]+)";

			// Search the string.
			foreach ( System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text, pattern) )
			{
				//string g0 = m.Groups[0].Value;
				string g1 = m.Groups[1].Value;
				string g2 = m.Groups[2].Value;
				if ( g2 != null && g2.Length > 0 )
				{
					res.Add(g2);
				}
				else
				{
					// get the quoted string, but without the quotes in g1;
					res.Add(g1);
				}
			}
			return (string[])res.ToArray(typeof(string));
		}
	}
}
