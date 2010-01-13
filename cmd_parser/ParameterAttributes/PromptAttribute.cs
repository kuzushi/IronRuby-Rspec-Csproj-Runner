using System;

namespace CmdParser
{
	/// <summary>
	/// Summary description for ParsingPromptStringAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=false)]
	public sealed class PromptAttribute : ParameterBaseAttribute
	{
		private string prompt;
		private string defaultAnswer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="prompt"></param>
		public PromptAttribute(string prompt) : this(prompt, null)
		{
		}

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="prompt"></param>
		/// <param name="defaultAnswer"></param>
		public PromptAttribute(string prompt, string defaultAnswer)
		{
			if ( prompt == null )
				throw new ArgumentNullException("prompt");
			this.prompt = prompt;
			this.defaultAnswer = defaultAnswer;
		}

		/// <summary>
		/// Gets the prompt string to display to user for mandatory parameters that are not supplied.
		/// </summary>
		public string Prompt
		{
			get { return this.prompt; }
		}

		/// <summary>
		/// Gets the default answer to be supplied at the command line.
		/// </summary>
		public string DefaultAnswer
		{
			get { return this.defaultAnswer; }
		}
	}
}
