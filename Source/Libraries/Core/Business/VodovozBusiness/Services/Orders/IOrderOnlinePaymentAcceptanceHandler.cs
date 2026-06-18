using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	/// <summary>
	/// Контракт обработчика принятия онлайн оплаты
	/// </summary>
	public interface IOrderOnlinePaymentAcceptanceHandler
	{
		/// <summary>
		/// Принятие онлайн оплаты заказом 
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="orders">Список заказов</param>
		/// <param name="paymentNumber">Номер оплаты</param>
		/// <param name="paymentType">Форма оплаты заказа</param>
		/// <param name="paymentFrom">Источник оплаты</param>
		void AcceptOnlinePayment(
			IUnitOfWork uow,
			IEnumerable<Order> orders,
			int paymentNumber,
			PaymentType paymentType,
			PaymentFrom paymentFrom);
	}
}
