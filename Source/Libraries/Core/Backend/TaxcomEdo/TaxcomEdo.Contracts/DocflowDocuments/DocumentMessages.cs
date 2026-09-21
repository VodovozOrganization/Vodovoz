using System;
using System.Collections.Generic;
using System.Text;

namespace TaxcomEdo.Contracts.DocflowDocuments
{
	public class DocumentWithMessage
	{
		public DocumentWithMessageType DocumentType { get; set; }
		public string Message { get; set; }
	}
}
