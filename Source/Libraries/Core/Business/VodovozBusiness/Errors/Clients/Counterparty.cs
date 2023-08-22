namespace Vodovoz.Errors.Clients
{
	public static partial class Counterparty
	{
		/// <summary>
		/// Поставки закрыты
		/// </summary>
		public static Error DeliveriesClosed =>
			new Error(
				typeof(Counterparty),
				nameof(Counterparty),
				"Поставки закрыты");

		/// <summary>
		/// Контрагент не является юридическим лицом
		/// </summary>
		public static Error IsNotДegalУntity =>
			new Error(
				typeof(Counterparty),
				nameof(IsNotДegalУntity),
				"Контрагент не является юридическим лицом");

		/// <summary>
		/// У контрагента не указан ИНН
		/// </summary>
		public static Error HasNoInn =>
			new Error(
				typeof(Counterparty),
				nameof(HasNoInn),
				"У контрагента не указан ИНН");
	}
}
