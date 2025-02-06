using QS.DomainModel.UoW;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.EntityRepositories.Settings
{
	public interface IOrderOrganizationSettingsRepository
	{
		OnlinePaymentTypeOrganizationSettings GetOnlinePaymentTypeOrganizationSettings(IUnitOfWork uow);
	}
}
