using BitrixNotificationsSend.Contracts.Dto;
using Vodovoz.Core.Domain.Orders;

namespace BitrixNotificationsSend.Library.Factories
{
	/// <summary>
	/// Фабрика создания данных о последнем сервисном заказе клиента для отправки в Битрикс24
	/// </summary>
	public interface ILastServiceOrderDtoFactory
	{
		/// <summary>
		/// Создание данных о последнем сервисном заказе клиента
		/// из сохранённых в базе данных данных о последнем сервисном заказе
		/// </summary>
		/// <param name="lastServiceOrder">Сохранённые данные о последнем сервисном заказе</param>
		/// <returns>Данные о последнем сервисном заказе для отправки в Битрикс24</returns>
		LastServiceOrderDto CreateLastServiceOrderDto(LastServiceOrder lastServiceOrder);
	}
}
