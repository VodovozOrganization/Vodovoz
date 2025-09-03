using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Clients
{
	public static partial class CounterpartyErrors
	{
		/// <summary>
		/// Контрагент не найден
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(CounterpartyErrors),
				nameof(NotFound),
				"Контрагент не найден");

		/// <summary>
		/// Поставки закрыты
		/// </summary>
		public static Error DeliveriesClosed =>
			new Error(
				typeof(CounterpartyErrors),
				nameof(DeliveriesClosed),
				"Поставки закрыты");

		/// <summary>
		/// Контрагент не является юридическим лицом
		/// </summary>
		public static Error IsNotLegalEntity =>
			new Error(
				typeof(CounterpartyErrors),
				nameof(IsNotLegalEntity),
				"Контрагент не является юридическим лицом");

		/// <summary>
		/// У контрагента не указан ИНН
		/// </summary>
		public static Error HasNoInn =>
			new Error(
				typeof(CounterpartyErrors),
				nameof(HasNoInn),
				"У контрагента не указан ИНН");
	}
}
