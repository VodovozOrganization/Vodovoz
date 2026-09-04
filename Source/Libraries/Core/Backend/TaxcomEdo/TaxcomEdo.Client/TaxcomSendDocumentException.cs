using System;

namespace TaxcomEdo.Client
{
	public class TaxcomSendDocumentException : Exception
	{
		public TaxcomSendDocumentException()
		{
		}

		public TaxcomSendDocumentException(string message) : base(message)
		{
		}

		public TaxcomSendDocumentException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
