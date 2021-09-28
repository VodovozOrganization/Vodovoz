using Bitrix.DTO;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace BitrixIntegration.Processors
{
	public interface IProductProcessor
	{
		void ProcessProducts(IUnitOfWork uow, Deal deal, Order order);
	}
}