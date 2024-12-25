using Edo.Transport.Messages.Dto;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Docflow.Factories
{
	public interface IUniversalTransferDocumentInfoFactory
	{
		UniversalTransferDocumentInfo CreateUniversalTransferDocumentInfo(TransferOrder transferOrder);
	}
}
