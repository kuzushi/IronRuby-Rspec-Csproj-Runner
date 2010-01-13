using System;
using System.Runtime.Serialization;

namespace CmdParser
{
	/// <summary>
	/// Represents a Command Exception.
	/// </summary>
	[Serializable]
	public class CmdException : Exception
	{
		/// <summary>
		/// 
		/// </summary>
		public CmdException()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public CmdException(string message) : base(message)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected CmdException(SerializationInfo info, StreamingContext context): base(info, context)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="inner"></param>
		public CmdException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}
