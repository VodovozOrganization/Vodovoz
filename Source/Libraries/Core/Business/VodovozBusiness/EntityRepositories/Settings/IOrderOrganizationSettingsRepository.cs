using QS.DomainModel.UoW;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.EntityRepositories.Settings
{
	public interface IOrderOrganizationSettingsRepository
	{
		OnlinePaymentTypeOrganizationSettings GetOnlinePaymentTypeOrganizationSettings(IUnitOfWork uow);
	}
}
