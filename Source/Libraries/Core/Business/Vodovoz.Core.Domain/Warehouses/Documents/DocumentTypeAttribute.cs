using System;

namespace Vodovoz.Core.Domain.Warehouses.Documents
{
	public class DocumentTypeAttribute : Attribute
	{
		public DocumentType Type { get; set; }

		public DocumentTypeAttribute(DocumentType type)
		{
			Type = type;
		}
	}
}
