using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Receipts;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public interface IFiscalOrderSnapshotBuilder
	{
		FiscalOrderSnapshot BuildPreviousSnapshot(IUnitOfWork uow, int orderId);

		FiscalOrderSnapshot BuildFromOrder(Order order);
	}
}
