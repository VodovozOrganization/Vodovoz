using System;

namespace TaxcomEdo.Contracts.Documents
{
	public abstract class InfoForCreatingDocumentEdo
	{
		protected InfoForCreatingDocumentEdo()
		{
			MainDocumentId = Guid.NewGuid();
		}
		
		public Guid MainDocumentId { get; }
	}
}
