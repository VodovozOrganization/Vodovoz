using Edo.Contracts.Messages.Dto;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Docflow.Factories
{
	public interface IUniversalTransferDocumentInfoFactory
	{
		UniversalTransferDocumentInfo CreateUniversalTransferDocumentInfo(IUnitOfWork uow, TransferOrder transferOrder);
	}
}
