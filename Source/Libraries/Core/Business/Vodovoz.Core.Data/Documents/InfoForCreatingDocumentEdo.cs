using System;

namespace Vodovoz.Core.Data.Documents
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
