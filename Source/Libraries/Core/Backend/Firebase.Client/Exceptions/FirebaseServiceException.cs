using System;
using System.Net;

namespace Firebase.Client.Exceptions
{
	public class FirebaseServiceException : Exception
	{
		public FirebaseServiceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public FirebaseServiceException(
			HttpStatusCode statusCode,
			string reasonPhrase)
			: base($"Firebase ответил: {Enum.GetName(typeof(HttpStatusCode), statusCode)} {reasonPhrase}")
		{
		}
	}
}
