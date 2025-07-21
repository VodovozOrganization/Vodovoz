using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Models.Orders;

namespace VodovozBusiness.Extensions
{
	public static class OnlineOrderExtensions
	{
		public static OrderOrganizationChoice ToOrderOrganizationChoice(
			this OnlineOrder onlineOrder,
			IUnitOfWork uow,
			IOrderSettings orderSettings) =>
			OrderOrganizationChoice.Create(uow, orderSettings, onlineOrder);
	}
}
