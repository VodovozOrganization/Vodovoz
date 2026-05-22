using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders;
using CustomerOrders.Contracts.V5.Orders.Templates;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.V5.Factories
{
	/// <summary>
	/// Фабрика для создания онлайн заказа из данных ИПЗ
	/// </summary>
	public interface IOnlineOrderFactoryV5
	{
		/// <summary>
		/// Создание сущности онлайн заказ из данных, пришедших с ИПЗ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="creatingOnlineOrder">Данные из ИПЗ</param>
		/// <param name="fastDeliveryScheduleId">Идентификатор быстрой доставки(доставка за час)</param>
		/// <param name="selfDeliveryDiscountReasonId">Идентификатор основания скидки за самовывоз</param>
		/// <returns></returns>
		OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow, ICreatingOnlineOrder creatingOnlineOrder, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);
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
