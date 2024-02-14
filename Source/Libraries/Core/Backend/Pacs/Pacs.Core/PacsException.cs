using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Pacs.Core
{
	/// <summary>
	/// Иключение, возникающее при работе СКУД
	/// </summary>
	public class PacsException : Exception
	{
		public PacsException()
		{
		}

		public PacsException(string message) : base(message)
		{
		}

		public PacsException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected PacsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
