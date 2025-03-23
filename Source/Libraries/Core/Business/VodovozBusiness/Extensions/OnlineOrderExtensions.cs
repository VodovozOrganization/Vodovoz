using Vodovoz.Domain.Orders;
using VodovozBusiness.Models.Orders;

namespace VodovozBusiness.Extensions
{
	public static class OnlineOrderExtensions
	{
		public static OrderOrganizationChoice ToOrderOrganizationChoice(this OnlineOrder onlineOrder) =>
			OrderOrganizationChoice.Create(onlineOrder);
	}
}
