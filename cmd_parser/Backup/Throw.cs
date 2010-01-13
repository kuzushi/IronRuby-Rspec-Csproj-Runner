using System;
using System.Globalization;

namespace CmdParser
{
	/// <summary>
	/// Static helpers to throw various exceptions for the program.
	/// </summary>
	internal sealed class Throw
	{
		private Throw()
		{
		}

		public static void MandatoryError()
		{
			string s = "Missing mandatory parameter(s).";
			throw new CmdException(s);
		}

		public static void DuplicateParmError(string parmName)
		{
			string s = "Duplicate parameter name [{0}]";
			throw new CmdException(string.Format(CultureInfo.InvariantCulture, s, parmName));
		}

		public static void AmbiguousParameter(string parmName)
		{
			string s = "Parameter [-{0}] not unique. Try adding more of the name.";
			throw new CmdException(string.Format(CultureInfo.InvariantCulture, s, parmName));
		}

		public static void ParameterMismatch()
		{
			string s = "Parameter mismatch. Can not mix parameters across sets.";
			throw new CmdException(s);
		}

		public static void ValidationPattern(string parmName, string pattern)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, "Parameter [{0}] did not match pattern [{1}].", parmName, pattern);
			throw new CmdException(msg);
		}
		
		public static void ValidationRange(string parmName, object min, object max)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, "Parameter [{0}] is not in range [{1}-{2}].", parmName, min.ToString(), max.ToString());
			throw new CmdException(msg);
		}

		public static void ValidationSet(string parmName, string value)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, "Value [{0}] on parameter [{1}] is not in the validation set.", value, parmName);
			throw new CmdException(msg);
		}

		public static void ValidationArrayCount(string parmName, int min, int max)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, "Parameter [{0}] must have array count between [{1}-{2}].", parmName, min, max);
			throw new CmdException(msg);
		}

		public static void ValidationLength(string parmName, int min, int max)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, "Parameter [{0}] must have length between [{1}-{2}].", parmName, min, max);
			throw new CmdException(msg);
		}

		public static void InvalidParmValue(string parmName, string value)
		{
			string s = "Parameter [{0}] has invalid value [{1}].";
			throw new CmdException(string.Format(CultureInfo.InvariantCulture, s, parmName, value));
		}

		public static void InvalidParmName(string parmName)
		{
			string s = "Invalid parameter name [{0}].";
			throw new CmdException(string.Format(CultureInfo.InvariantCulture, s, parmName));
		}

		public static void ConversionError(string parmName, Type destType, string inData)
		{
			string s = "Parameter [{0}] could not be converted to [{1}].\nInvalid data: '{2}'";
			string tName = destType.Name;
			throw new CmdException(string.Format(CultureInfo.InvariantCulture, s, parmName, tName, inData));
		}
	}
}
