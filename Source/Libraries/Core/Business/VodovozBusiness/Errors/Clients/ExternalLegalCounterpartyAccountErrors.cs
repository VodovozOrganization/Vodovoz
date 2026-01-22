using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.Clients
{
	public static class ExternalLegalCounterpartyAccountErrors
	{
		/// <summary>
		/// Контрагент не найден
		/// </summary>
		public static Error EmailNotFound =>
			new Error(
				typeof(ExternalLegalCounterpartyAccountErrors),
				nameof(EmailNotFound),
				"Не найдена почта у аккаунта. Дальнейшие действия невозможны");
	}
}
