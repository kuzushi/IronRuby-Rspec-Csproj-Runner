using System;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace CmdParser
{
	/// <summary>
	/// Parameter object used to define a single command line parameter (i.e. argument).
	/// </summary>
	public sealed class Parameter
	{
		private static Regex splitRx = new Regex(@",\s*");//, RegexOptions.Compiled); // split on comma with 0 or more whitespace.

		private string name;		// Name of parameter.
		private Type type;			// CLR type of parameter.
		private object value;		// Value that will be set by cmd line.
		private string setName;
		private int position;
		private bool mandatory;		// true if parm is mandatory.  false if optional.
		private bool isSwitch;		// true if parm is a switch.  Only valid on bool type.
		private ValidateLengthAttribute valLen;	// Min/Max length of string.  Only valid on string type.
		private IntRange valCount;	// Min/Max Array elements required.  Only valid on array types.
		private Range valRange;		// Object must fall within Range.
		private Array valSet;	    // Set of objects to verify input to.  Type of each member must be convertable to Parm Type.
        private bool valSetCaseInsensitive;
		private bool varList;		// If true, type must be string[].
		internal bool beenSet;		// true only after set be system.
		private string valPattern;
		private string promptString;
		private string defaultAnswer;
		internal MemberInfo memberInfo;
		private object instance;
		private HelpAttribute help;

		private Parameter()
		{
			position = -1;
			setName = "";
		}

		private Parameter(string name, Type type, object value, object instance, bool mandatory) : this()
		{
			this.name = ValidateName(name);
			this.type = ValidateType(type); // Type must be comparable and convertable.
			this.value = value;
			this.mandatory = mandatory;
			this.instance = instance;
			this.setName = "";
			this.position = -1;
		}
		
		/// <summary>
		/// Gets or sets the position of this parameter in the parameter set.
		/// Must be >= -1.
		/// </summary>
		public int Position
		{
			get { return this.position; }
			set
			{
				if ( value < -1 )
					throw new ArgumentOutOfRangeException("value", "value must be >= -1.");
				this.position = value;
			}
		}

		/// <summary>
		/// Gets or sets the ParameterSetName this parameter belongs to.
		/// </summary>
		public string SetName
		{
			get { return this.setName; }
			set
			{
				if ( value == null )
					value = "";
				value = value.Trim();
				this.setName = value;
			}
		}

		/// <summary>
		/// Gets the Help attribute that was applied to the parameter.
		/// </summary>
		public HelpAttribute Help
		{
			get { return this.help; }
		}

		/// <summary>
		/// Gets the MemberInfo object that this parameter refers to.
		/// </summary>
		public MemberInfo MemberInfo
		{
			get { return this.memberInfo; }
		}

		private static bool InArrayList(ArrayList al, string name)
		{
			foreach(string s in al)
			{
				if ( string.Compare(s, name, true, CultureInfo.InvariantCulture) == 0 )
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns true if SetName already exists.
		/// </summary>
		/// <param name="al"></param>
		/// <param name="parmAttribute"></param>
		/// <returns></returns>
		private static bool InParameterArray(ArrayList al, ParameterAttribute parmAttribute)
		{
			foreach(ParameterAttribute pa in al)
			{
				if ( string.Compare(pa.ParameterSetName, parmAttribute.ParameterSetName, true, CultureInfo.InvariantCulture) == 0 )
					return true;
			}
			return false;
		}

		private static ArrayList ValidateParameterAttributes(ParameterAttribute[] parms)
		{
			ArrayList al = new ArrayList();
			if ( parms == null )
			{
				al.Add(new ParameterAttribute());
				return al;
			}

			// Validate null and dups.
			for(int i=0; i < parms.Length; i++)
			{
				ParameterAttribute pa = parms[i];
				//PosSet ps = posSets[i];
				if ( pa == null )
					continue;
				if ( InParameterArray(al, pa) )
					throw new ArgumentException("Duplicate set name not allowed.");
				al.Add(pa);
			}
			if ( al.Count == 0 )
				al.Add(new ParameterAttribute()); // Add default set.
			return al;
		}

		private static Type ValidateType(Type type)
		{
			if ( type == null )
				throw new ArgumentNullException("Parameter type is null.");
			if ( type.IsAbstract )
				throw new InvalidOperationException("Parameter type is invalid.");
			if ( type.IsInterface || type.IsPointer )
				throw new InvalidOperationException("Parameter type is invalid.");
			
			// If type is an array, the element type must be convertable and comparable.
			if ( type.IsArray )
			{
				Type elType = type.GetElementType();
				if ( ! IsConvertableType(elType) )
					throw new CmdException("Parameter type is array and element type does not implement IComparable and IConvertable.");
			}
			else // The type must be convertable and comparable.
			{
				if ( ! IsConvertableType(type) )
					throw new CmdException("Parameter type must implement IComparable and IConvertable.");
			}
			
			return type;
		}

		/// <summary>
		/// Gets the name of the parameter.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Gets or sets the prompt to use for mandatory parms that are not set.
		/// </summary>
		public string PromptString
		{
			get { return this.promptString; }
			set { this.promptString = value; }
		}

		/// <summary>
		/// Gets or sets the default answer displayed after prompt string.
		/// </summary>
		public string DefaultAnswer
		{
			get { return this.defaultAnswer; }
			set	{ this.defaultAnswer = value; }
		}

		/// <summary>
		/// Gets the type of this parameter.
		/// </summary>
		public Type Type
		{
			get { return this.type; }
		}

		/// <summary>
		/// Get or sets the value object of this parameter.
		/// </summary>
		public object Value
		{
			get
			{
				if ( this.value == null )
				{
					if ( this.memberInfo is PropertyInfo )
					{
						PropertyInfo pi = (PropertyInfo)this.memberInfo;
						return pi.GetValue(this.instance, null);
					}
					else
					{
						FieldInfo fi = (FieldInfo)this.memberInfo;
						return fi.GetValue(this.instance);
					}
				}
				return this.value;
			}
			set
			{
				if ( value == null )
				{
					this.value = null;
				}
				else
				{
					object obj = Convert.ChangeType(value, this.type, CultureInfo.InvariantCulture);
					this.value = obj;
				}
			}
		}

		private void ValidateValue(object obj)
		{
			// Validate Length again.  But this time only for array elements on a string[].
			if ( ValLen != null )
			{
				if ( type.IsArray )
				{
					if ( type.GetElementType() == typeof(string) )
					{
						Array sa = (string[])obj;
						foreach(string s in sa)
						{
							string tmpString = s;
							if ( ValLen.TrimFirst )
								tmpString = tmpString.Trim();
							if ( ! ValLen.IsInRange(tmpString.Length) )
								Throw.ValidationLength(this.Name, ValLen.Min, ValLen.Max);
						}
					}
				}
			}

			Array array = null;
			if ( type.IsArray )
				array = (Array)obj;

			// Validate Count.
			if ( this.ValCount != null )
			{
				if ( ! ValCount.IsInRange(array.Length) )
					Throw.ValidationArrayCount(this.Name, ValCount.Min, ValCount.Max);
			}

            bool inValSet = false;
            string sObj = null;
            bool objIsString = false;
            if ( obj is string )
            {
                sObj = (string)obj;
                objIsString = true;
            }

			// Validate Set (i.e. select list.)
			if ( this.ValSet != null )
			{
				if ( type.IsArray )
				{
					// Verify each element in the array is in the ValSet.
					foreach(object o in array)
					{
                        bool isInSet = false;
                        
                        // Each o must be in the set.
                        if ( o is string )
                        {
                            string os = (string)o;
                            foreach ( string s in valSet )
                            {
                                if ( this.valSetCaseInsensitive )
                                {
                                    if ( os.Equals(s, StringComparison.OrdinalIgnoreCase) )
                                    {
                                        isInSet = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if ( os.Equals(s, StringComparison.Ordinal) )
                                    {
                                        isInSet = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //bool isInSet = false;
                            foreach ( object vo in ValSet )
                            {
                                IComparable ic = (IComparable)o;
                                if ( ic.CompareTo(vo) == 0 )
                                {
                                    isInSet = true;
                                    break;
                                }
                            }
                        }
						if ( ! isInSet )
							Throw.ValidationSet(this.Name, obj.ToString());
					}
				}
				else // Type is not an array, so verify the value is in the ValSet.
				{
					foreach(object o in ValSet)
					{
                        if ( objIsString )
                        {
                            if ( valSetCaseInsensitive )
                            {
                                if ( sObj.Equals((string)o, StringComparison.OrdinalIgnoreCase) )
                                {
                                    inValSet = true;
                                    break;
                                }
                            }
                            else
                            {
                                if ( sObj.Equals((string)o, StringComparison.Ordinal) )
                                {
                                    inValSet = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            IComparable ic = (IComparable)obj;
                            if ( ic.CompareTo(o) == 0 )
                            {
                                inValSet = true;
                                break;
                            }
                        }
					}
					if ( ! inValSet )
						Throw.ValidationSet(this.Name, obj.ToString());
				}
			}

			// Validate Range.
			if ( this.valRange != null )
			{
				if ( type.IsArray )
				{
					// Validate each element in the array is within range.
					foreach(object o in array)
					{
						if ( ! valRange.IsInRange(o) )
							Throw.ValidationRange(this.Name, valRange.Min, valRange.Max);
					}
				}
				else
				{
					if ( ! valRange.IsInRange(obj) )
						Throw.ValidationRange(this.Name, valRange.Min, valRange.Max);
				}
			}

			// Validate Pattern.
			if ( this.valPattern != null )
			{
				Regex rx = new Regex(this.valPattern);
				if ( type.IsArray )
				{
					string[] sa = (string[])obj;
					foreach(string s in sa)
					{
						if ( ! rx.IsMatch(s) )
							Throw.ValidationPattern(this.Name, valPattern);
					}
				}
				else
				{
					if ( ! rx.IsMatch((string)obj) )
						Throw.ValidationPattern(this.Name, valPattern);
				}
			}
		}

		/// <summary>
		/// Returns true if this parameter is optional (i.e. not mandatory).
		/// </summary>
		public bool IsOptional
		{
			get { return ! IsMandatory; }
		}

		/// <summary>
		/// Gets or set a value indicating if this parameter is mandatory.
		/// </summary>
		public bool IsMandatory
		{
			get { return this.mandatory; }
			set { this.mandatory = value; }
		}

		/// <summary>
		/// Gets a value indicating if this is a switch parameter.
		/// </summary>
		public bool IsSwitch
		{
			get { return this.isSwitch; }
			set
			{
				if ( value )
					if ( this.type != typeof(bool) )
						throw new ArgumentException("Parameter type must be Bool to use the Switch type.");
				this.isSwitch = value;
			}
		}

		/// <summary>
		/// Gets or sets the integer range this parameter's value must be within.
		/// </summary>
		public ValidateLengthAttribute ValLen
		{
			get { return this.valLen; }
			set
			{
				if ( value == null )
				{
					this.valLen = null;
					return;
				}

				if ( this.type == typeof(string) || type == typeof(string[]) )
				{
					this.valLen = value;
					return;
				}

				throw new CmdException("ValidationLength only valid on parameters of type System.String and String[].");
			}
		}

		/// <summary>
		/// Gets the regular expression pattern that the string must adher to.
		/// </summary>
		public string ValPattern
		{
			get { return this.valPattern; }
		}

		/// <summary>
		/// Gets or sets the range the parameter value must be within.  This applies only
		/// to Array types.
		/// </summary>
		public IntRange ValCount
		{
			get { return this.valCount; }
			set
			{
				if ( value == null )
				{
					this.valCount = null;
					return;
				}

				if ( ! this.type.IsArray )
					throw new ArgumentException("ValCount only valid on parameters of type System.Array.");
				this.valCount = value;
			}
		}

		/// <summary>
		/// Gets or sets the validation range of values this parameter must be within.
		/// </summary>
		public Range ValRange
		{
			get { return this.valRange; }
			set 
			{
				if ( value == null )
				{
					this.valRange = null;
					return;
				}

				this.valRange = value;
			}
		}

		/// <summary>
		/// Gets or sets the validation set array.  Only valid on [string] parms.
		/// </summary>
		public Array ValSet
		{
			get { return this.valSet; }
			set
			{
				if ( value == null )
				{
					this.valSet = null;
					return;
				}

				Type elType = null;
				if ( type.IsArray )
					elType = type.GetElementType();				
				else
					elType = type;

				foreach(object o in value)
				{
					if ( o == null || o.GetType() != elType )
						throw new CmdException("Each type in ValidationSet must match parameter type.");
				}
				this.valSet = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating if this parameter receives any arguments
		/// that are left over on the command line.
		/// </summary>
		public bool ValueFromRemainingArguments
		{
			get { return this.varList; }
			set
			{
				if ( value )
					if ( this.type != typeof(string[]) )
						throw new ArgumentException("Parameter must be string[] type for VarList.");
				this.varList = value;
			}
		}

		/// <summary>
		/// Gets a value indicating if this parameter has been set by the command line.
		/// </summary>
		public bool BeenSet
		{
			get { return this.beenSet; }
		}

		private static bool IsConvertableType(Type type)
		{
			// Type must be convertible (or have a type converter) and be comparable.
			if ( type.GetInterface("IConvertible", true) != null )
				return true; // Is comparable and convertable.

			TypeConverter tc = TypeDescriptor.GetConverter(type);
			if ( tc == null )
				return false;
			if ( ! tc.CanConvertFrom(typeof(string)) )
				return false;

			return true;
		}

		/// <summary>
		/// This should be called to set the new value.
		/// </summary>
		/// <param name="value"></param>
		internal void SetValue(string value)
		{
			// Validate string Length before any conversions.
			if ( ValLen != null )
			{
				if ( ! type.IsArray )
				{
					if ( ValLen.TrimFirst )
						value = value.Trim();
					if ( ! ValLen.IsInRange(value.Length) )
						Throw.ValidationLength(this.Name, ValLen.Min, ValLen.Max);
				}
			}

			object val = null;
			try
			{
				// Also converts arrays.
				val = Converter.ConvertFromString(value, this.type);
			}
			catch
			{
				Throw.ConversionError(this.Name, this.Type, value);
			}

			// Validate value using rules.
			ValidateValue(val);

			// Set value in the parm class, but also set value in the instance.
			this.value = val;
			if ( this.instance == null )
				throw new CmdException("Instance object is null.");
			if ( this.memberInfo == null )
				throw new CmdException("MemberInfo is null.");
			
			if ( this.memberInfo is PropertyInfo )
			{
				PropertyInfo pi = (PropertyInfo)this.memberInfo;
				pi.SetValue(this.instance, val, null);
			}
			else
			{
				FieldInfo fi = (FieldInfo)this.memberInfo;
				fi.SetValue(this.instance, val);
			}

			this.beenSet = true;
		}

		private static bool IsNullOrEmpty(string s)
		{
			if ( s == null )
				return true;
			if ( s.Length == 0 || string.Compare(s, "null", true, CultureInfo.InvariantCulture) == 0 )
				return true;
			return false;
		}

		private static ParameterBaseAttribute[] GetParameterBaseAttributes(MemberInfo mi)
		{
			if ( mi == null )
				throw new ArgumentNullException("mi");

			// Only add members that have ParameterBaseAttribute(s).
			if ( ! mi.IsDefined(typeof(ParameterBaseAttribute), true) )
				return null;

			ParameterBaseAttribute[] parmAtts = (ParameterBaseAttribute[])mi.GetCustomAttributes(typeof(ParameterBaseAttribute), true);
			return parmAtts;
		}

		internal static Parameter CreateParameter(
			string name,
			Type type,
			object value,
			object instance,
			bool mandatory)
		{
			Parameter p = new Parameter(name, type, value, instance, mandatory);
			return p;
		}

		internal static Parameter CreateParameter(
			string name,
			Type type,
			object value,
			object instance,
			bool mandatory,
			bool isSwitch,
			ValidateLengthAttribute valLen,
			IntRange valCount,
			Range valRange,
			string[] valSet,
			bool varList)
		{
			Parameter p = CreateParameter(name, type, value, instance, mandatory);
			p.IsSwitch = isSwitch;
			p.ValLen = valLen;
			p.ValCount = valCount;
			p.ValRange = valRange;
			p.ValSet = valSet;
			p.ValueFromRemainingArguments = varList;
			return p;
		}

		private static bool IsComparable(Type type)
		{
			if ( type == null )
				return false;
			if ( type.GetInterface("IComparable", true) == null )
				return false;
			return true;
		}

		/// <summary>
		/// Returns an instance of a Parameter object.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="mi"></param>
		/// <param name="parameterAttribute"></param>
		/// <returns></returns>
		internal static Parameter CreateParameter(object instance, MemberInfo mi, ParameterAttribute parameterAttribute)
		{
			if ( instance == null )
				throw new ArgumentNullException("instance");
			if ( mi == null )
				throw new ArgumentNullException("mi");
			if ( parameterAttribute == null )
				throw new ArgumentNullException("parameterAttribute");

			Type type = null;
			if ( mi is PropertyInfo )
			{
				PropertyInfo pi = (PropertyInfo)mi;
				type = pi.PropertyType;
			}
			else
			{
				FieldInfo fi = (FieldInfo)mi;
				type = fi.FieldType;
			}

			Parameter p = new Parameter(mi.Name, type, null, instance, false);
			p.memberInfo = mi;

			ArrayList attrList = new ArrayList();
			ParameterBaseAttribute[] parmAttributes = GetParameterBaseAttributes(mi);
			if ( parmAttributes == null )
				throw new ArgumentException("No ParameterAttributes exist for member.");

			// Handle ParameterAttribute special.
			if ( parameterAttribute.ValueFromRemainingArguments && p.Type != typeof(string[]) )
				throw new CmdException("Parameter must be string[] to use ValueFromRemainingArguments.");
			p.mandatory = parameterAttribute.Mandatory;
			p.setName = parameterAttribute.ParameterSetName;
			p.position = parameterAttribute.Position;
			p.varList = parameterAttribute.ValueFromRemainingArguments;

			foreach(ParameterBaseAttribute pa in parmAttributes)
			{
				switch(pa.GetType().Name)
				{
					case "PromptAttribute":
						PromptAttribute ppsa = (PromptAttribute)pa;
						p.PromptString = ppsa.Prompt;
						p.DefaultAnswer = ppsa.DefaultAnswer;
						attrList.Add(pa);
						break;
					case "ParameterAttribute":
						// Ignore as we handle above.
//						ParameterAttribute pa1 = (ParameterAttribute)pa;
//						p.mandatory = pa1.Mandatory;
//						p.setName = pa1.ParameterSetName;
//						p.position = pa1.Position;
//						if ( pa1.ValueFromRemainingArguments && p.Type != typeof(string[]) )
//							throw new CmdException("Parameter must be string[] to use ValueFromRemainingArguments.");
//						p.varList = pa1.ValueFromRemainingArguments;
//						attrList.Add(pa1);
						break;
					case "HelpAttribute":
						p.help = pa as HelpAttribute;
						attrList.Add(p.help);
						break;
					case "ValidateLengthAttribute":
						if ( type != typeof(string) )
							throw new CmdException("ValidateLength attribute only valid on [string] type.");
						ValidateLengthAttribute vla = (ValidateLengthAttribute)pa;
						p.ValLen = vla;
						attrList.Add(pa);
						break;
					case "ValidateCountAttribute":
						ValidateCountAttribute vca = (ValidateCountAttribute)pa;
						IntRange irvc = new IntRange(vca.Min, vca.Max);
						p.ValCount = irvc;
						attrList.Add(pa);
						break;
					case "ValidateRangeAttribute":
						ValidateRangeAttribute vra = (ValidateRangeAttribute)pa;
						Type elType1;
						if ( type.IsArray )
							elType1 = type.GetElementType();
						else
							elType1 = type;
						if ( ! IsComparable(elType1) )
							throw new CmdException("Type must implement IComparable to use the ValidationRangeAttribute.");
						p.ValRange = new Range(elType1, vra.MinRange.ToString(), vra.MaxRange.ToString());
						attrList.Add(pa);
						break;
					case "ValidateSetAttribute":
						ValidateSetAttribute vsa = (ValidateSetAttribute)pa;
						Type elType2;
						if ( type.IsArray )
							elType2 = type.GetElementType();
						else
							elType2 = type;
						if ( ! IsComparable(elType2) )
							throw new CmdException("Type must implement IComparable to use the ValidationSetAttribute.");
						Type toType = Type.GetType(elType2.ToString()+"[]");
                        // Split string into array of target type.  ValSet will be array.
						Array arr = (Array)Converter.ConvertFromString(vsa.SetString, toType);
						p.ValSet = arr;
                        if ( vsa.CaseInsensitive )
                            p.valSetCaseInsensitive = true;
						attrList.Add(pa);
						break;
					case "ValidatePatternAttribute":
						ValidatePatternAttribute vpa = (ValidatePatternAttribute)pa;
						Type elType;
						if ( p.Type.IsArray )
							elType = type.GetElementType();
						else
							elType = type;
						if ( elType != typeof(string) )
							throw new CmdException("ValidationPattern attribute only valid on [string] type.");
						p.valPattern = vpa.Pattern;
						attrList.Add(pa);
						break;
					case "SwitchAttribute":
						if ( type != typeof(bool) )
							throw new CmdException("SwitchParameter attribute only valid on [bool] type.");
						p.IsSwitch = true;
						attrList.Add(pa);
						break;
					default:
						break;
				}
			}

			return p;
		}

		/// <summary>
		/// Returns true if this parameter is in the named set; otherwise false.
		/// </summary>
		/// <param name="setName"></param>
		/// <returns></returns>
		public bool IsInSet(string setName)
		{
			if ( setName == null )
				setName = "";
			setName = setName.Trim();
			if ( string.Compare(this.setName, setName, true, CultureInfo.InvariantCulture) == 0 )
				return true;
			return false;
		}

		private static string ValidateName(string name)
		{
			if ( name == null )
				throw new ArgumentNullException("name", "name can not be null.");

			name = name.Trim();
			if ( name.Length == 0 )
				throw new ArgumentException("Parameter name must not be zero length.");
			if ( name.StartsWith("-") )
				throw new ArgumentException("Parameter name can not start with the '-' character.");
			
			foreach(Char c in name)
			{
				if ( c == ' ' )
					throw new ArgumentException("Parameter name can not contain spaces.");
				if ( ! Char.IsLetterOrDigit(c) )
					throw new ArgumentException("Parameter name can only contain letters or digits.");
			}
			return name;
		}
	}
}
