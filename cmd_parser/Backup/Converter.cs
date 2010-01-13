using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CmdParser
{
	/// <summary>
	/// Summary description for Converter.
	/// </summary>
	internal sealed class Converter
	{
		private static Regex splitRx = new Regex(@",\s*"); // split on comma plus 0 or more white.
		private static readonly Hashtable scTypes = new Hashtable(); // Special Convertable Types.

		private Converter()
		{
		}

		/// <summary>
		/// Returns the object from the string representation using the type converter. 
		/// </summary>
		/// <param name="objString"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public static object ConvertFromString(string objString, Type destinationType)
		{
			Type type = destinationType;
			if ( type == null )
				throw new ArgumentNullException("destinationType");
			if ( objString == null )
				throw new ArgumentNullException("objString");
			
			Type elType;
			if ( type.IsArray )
				elType = type.GetElementType();
			else
				elType = type;

			if ( ! CanConvertFromString(elType) )
				throw new CmdException("Can not convert string to destination type.");
			
			TypeConverter tc = GetTypeConverter(elType);
			if ( tc == null )
				throw new CmdException("No type converter supplied for type.");

			// Convert to object from string using TypeConverter.
			//IConvertible ic = (IConvertible)elType.GetInterface("IConvertible", true);
			try
			{
				if ( type.IsArray )
				{
					string[] sa = Parameters.SplitQuoted(objString, ", ");
					ArrayList al = new ArrayList();
					foreach(string s in sa)
					{
						string es = s.Trim();
						try
						{
							al.Add(tc.ConvertFromString(es));
						}
						catch
						{
							throw new CmdException("Could not convert element in array to type.");
						}
					}
					return (Array)al.ToArray(elType);
				}
				else
				{
					return tc.ConvertFromString(objString);
				}
			}
			catch
			{
				throw new CmdException("Could not convert string to type.");
			}
		}

		private static TypeConverter GetTypeConverter(Type type)
		{
			TypeConverter tc = scTypes[type] as TypeConverter;
			if ( tc == null )
				tc = TypeDescriptor.GetConverter(type);
			return tc;
		}

		public static bool CanConvertFromString(Type type)
		{
			if ( type == null )
				return false;
			
			// Allow any special types and types that have string converters.
			Type elType;
			if ( type.IsArray )
				elType = type.GetElementType();
			else
				elType = type;

			// Test type has a string converter.
			if ( TypeDescriptor.GetConverter(elType).CanConvertFrom(typeof(string)) )
				return true;

			// Test special types we allow.
			foreach(Type t in scTypes.Keys)
			{
				if ( elType == t )
					return true;
			}

			return false;
		}
	}
}
