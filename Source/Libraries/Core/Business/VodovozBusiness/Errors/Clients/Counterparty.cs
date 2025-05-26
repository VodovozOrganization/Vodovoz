using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Clients
{
	public static partial class Counterparty
	{
		/// <summary>
		/// Контрагент не найден
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(Counterparty),
				nameof(NotFound),
				"Контрагент не найден");

		/// <summary>
		/// Поставки закрыты
		/// </summary>
		public static Error DeliveriesClosed =>
			new Error(
				typeof(Counterparty),
				nameof(DeliveriesClosed),
				"Поставки закрыты");

		/// <summary>
		/// Контрагент не является юридическим лицом
		/// </summary>
		public static Error IsNotLegalEntity =>
			new Error(
				typeof(Counterparty),
				nameof(IsNotLegalEntity),
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
