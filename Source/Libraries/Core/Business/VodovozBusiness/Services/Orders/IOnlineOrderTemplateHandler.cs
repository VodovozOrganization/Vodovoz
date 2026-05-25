using System.Threading.Tasks;
using CustomerOrders.Contracts.V5.Orders.Templates;
using QS.DomainModel.UoW;

namespace VodovozBusiness.Services.Orders
{
	public interface IOnlineOrderTemplateHandler
	{
		Task<OrderTemplateInfoDto> GetFreshOnlineOrderTemplateDataAsync(IUnitOfWork uow, int templateId);
		OrderTemplatesDto GetOnlineOrdersTemplatesList(IUnitOfWork uow, int counterpartyId, int skip, int take);
	}
}
