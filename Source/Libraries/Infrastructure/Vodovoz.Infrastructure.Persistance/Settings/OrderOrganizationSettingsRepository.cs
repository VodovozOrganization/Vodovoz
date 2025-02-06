using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Settings;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Infrastructure.Persistance.Settings
{
	public class OrderOrganizationSettingsRepository : IOrderOrganizationSettingsRepository
	{
		public OnlinePaymentTypeOrganizationSettings GetOnlinePaymentTypeOrganizationSettings(IUnitOfWork uow)
		{
			return
				(from settings in uow.Session.Query<OnlinePaymentTypeOrganizationSettings>()
					select settings).SingleOrDefault();
		}
	}
}
