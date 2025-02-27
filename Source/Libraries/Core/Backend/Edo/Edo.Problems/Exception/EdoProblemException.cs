using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Edo.Problems.Exception
{
	public abstract class EdoProblemException : System.Exception
	{
		public EdoProblemException()
		{
		}

		public EdoProblemException(string message) : base(message)
		{
		}

		public EdoProblemException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		protected EdoProblemException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
