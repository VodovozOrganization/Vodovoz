namespace Vodovoz.ViewModels.Dialogs.Mango.Talks
{
	/// <summary>
	/// Сообщения, показываемые пользователю при переводе звонка на водителя
	/// </summary>
	public static class DriverCallForwardingMessages
	{
		/// <summary>
		/// Заголовок окон и сообщений перевода звонка на водителя
		/// </summary>
		public const string DialogTitle = "Перевод звонка на водителя";

		/// <summary>
		/// Номер звонящего не привязан ни к одному контрагенту
		/// </summary>
		public const string CounterpartyNotFound = "По данному номеру не зарегистрирован контрагент";

		/// <summary>
		/// У контрагента нет заказов в пути, то есть переводить звонок не на кого
		/// </summary>
		public const string NoOrdersOnTheWay = "У контрагента нет заказов, закрепленных за водителем";

		/// <summary>
		/// Разговор уже завершён, переводить нечего
		/// </summary>
		public const string NoActiveTalk = "Нет активного разговора, перевести звонок невозможно";
	}
}
