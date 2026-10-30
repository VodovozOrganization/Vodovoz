using System;
using CustomerOrders.Contracts.V8.Sale.Templates;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Factories.DeliverySchedules
{
	public interface ITemplateDeliveryScheduleDescriptionFactory
	{
		/// <summary>
		/// Создание описания параметров доставки
		/// </summary>
		/// <param name="deliverySchedule">Параметры доставки</param>
		/// <param name="fromInterval">С какого времени</param>
		/// <param name="toInterval">По какое</param>
		/// <param name="withStartedAt">Включая строчку "начиная с"</param>
		/// <returns></returns>
		string Create(
			TemplateDeliveryScheduleBaseDto deliverySchedule,
			TimeSpan fromInterval,
			TimeSpan toInterval,
			bool withStartedAt = true
			);

		/// <summary>
		/// Создание описания параметров доставки
		/// </summary>
		/// <param name="deliveryScheduleData">Параметры доставки из шаблона</param>
		/// <param name="fromInterval">С какого времени</param>
		/// <param name="toInterval">По какое</param>
		/// <returns></returns>
		string Create(
			TemplateDeliveryScheduleData deliveryScheduleData,
			TimeSpan fromInterval,
			TimeSpan toInterval);
	}
}
