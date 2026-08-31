using Vodovoz.Core.Data.InfoMessages;

namespace CustomerOrdersApi.Library.V4.Factories
{
	/// <summary>
	/// Фабрика для создания информационных сообщений, отображаемых в ИПЗ
	/// </summary>
	public interface IInfoMessageFactory
	{
		/// <summary>
		/// Создает информационное сообщение о необходимости оплаты заказа
		/// </summary>
		/// <returns>Сообщение с таймером обратного отсчета, отображаемое в верхней части карточки заказа</returns>
		InfoMessage CreateNeedPayOrderInfoMessage();

		/// <summary>
		/// Создает информационное сообщение о неоплаченном заказе
		/// </summary>
		/// <returns>Сообщение о том, что заказ не был оплачен и менеджер свяжется с клиентом</returns>
		InfoMessage CreateNotPaidOrderInfoMessage();
	}
}
