using System;
using System.Net;

namespace Firebase.Client.Exceptions
{
	public class FirebaseCloudMessagingClientServiceException : Exception
	{
		public FirebaseCloudMessagingClientServiceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public FirebaseCloudMessagingClientServiceException(
			HttpStatusCode statusCode,
			string reasonPhrase)
			: base($"Firebase ответил: {Enum.GetName(typeof(HttpStatusCode), statusCode)} {reasonPhrase}")
		{
		}
	}
}
