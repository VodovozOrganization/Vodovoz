using System.Collections.Generic;
using CustomerOrdersApi.Library.V5.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.Factories.V5
{
	public interface IOnlineOrderFactoryV5
	{
		OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow, CreatingOnlineOrder creatingOnlineOrder, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);

		/// <summary>
		/// Создание сущности шаблона автозаказа из ИПЗ
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="creatingOnlineOrder">Созданный онлайн заказ</param>
		/// <param name="creatingTemplate">Данные для шаблона</param>
		/// <returns></returns>
		(
			OnlineOrderTemplate OrderTemplate,
			IEnumerable<OnlineOrderTemplateProduct> OrderTemplateProducts,
			IEnumerable<OnlineOrderTemplateWeekday> OrderTemplateWeekDays
		)
			CreateOnlineOrderTemplate(IUnitOfWork uow, OnlineOrder creatingOnlineOrder, CreatingOrderTemplate creatingTemplate);
	}
}
