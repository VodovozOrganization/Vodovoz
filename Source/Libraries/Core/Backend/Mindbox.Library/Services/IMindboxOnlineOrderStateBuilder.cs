namespace Mindbox.Library.Services
{
	/// <summary>
	/// Состояние онлайн-заказа для синхронизации с Mindbox
	/// </summary>
	public interface IMindboxOnlineOrderStateBuilder
	{
		/// <summary>
		/// Собирает текущее состояние заказа
		/// </summary>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <returns>Текущее состояние заказа для сравнения со снимком</returns>
		MindboxOnlineOrderState BuildState(Vodovoz.Domain.Orders.OnlineOrder onlineOrder);

		/// <summary>
		/// Возвращает статус позиций заказа по настройкам маппинга
		/// </summary>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <returns>Статус позиций в терминах Mindbox</returns>
		string ResolveStatus(Vodovoz.Domain.Orders.OnlineOrder onlineOrder);

		/// <summary>
		/// Собирает запрос создания/обновления состава
		/// </summary>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <param name="state">Состояние заказа для отправки</param>
		/// <returns>Запрос операции CreateAuthorizedLoyaltyOrder</returns>
		Contracts.MindboxLoyaltyOrderRequest BuildCreateRequest(
			Vodovoz.Domain.Orders.OnlineOrder onlineOrder,
			MindboxOnlineOrderState state);

		/// <summary>
		/// Собирает запрос смены статуса позиций
		/// </summary>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <param name="state">Текущее состояние заказа</param>
		/// <param name="changedLines">Позиции, у которых изменился статус</param>
		/// <param name="newStatus">Новый статус позиций</param>
		/// <param name="updateAllLines">Признак смены статуса всех позиций заказа</param>
		/// <returns>Запрос операции изменения статуса позиций</returns>
		Contracts.MindboxOrderLinesStatusRequest BuildStatusRequest(
			Vodovoz.Domain.Orders.OnlineOrder onlineOrder,
			MindboxOnlineOrderState state,
			System.Collections.Generic.IReadOnlyCollection<MindboxOnlineOrderStateLine> changedLines,
			string newStatus,
			bool updateAllLines);
	}
}
