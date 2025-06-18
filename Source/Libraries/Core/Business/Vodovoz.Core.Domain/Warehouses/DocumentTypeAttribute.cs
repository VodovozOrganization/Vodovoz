using System;
using Vodovoz.Core.Domain.Warehouses.Documents;

namespace Vodovoz.Core.Domain.Warehouses
{
	/// <summary>
	/// Аттрибут для указания типа документа склада.
	/// </summary>
	public class DocumentTypeAttribute : Attribute
	{
		public DocumentType Type { get; set; }

		public DocumentTypeAttribute(DocumentType type)
		{
			Type = type;
		}
	}
}
