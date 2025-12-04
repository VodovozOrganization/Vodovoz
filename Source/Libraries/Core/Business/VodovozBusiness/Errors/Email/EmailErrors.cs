using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Email
{
	public static partial class EmailErrors

	{
		public static Error MissingEmailForRequiredMailType =>
			new Error(typeof(EmailErrors),
				nameof(MissingEmailForRequiredMailType),
				"Отсутствует адрес для отправки email");

		public static Error MissingDocumentForSending =>
			new Error(typeof(EmailErrors),
				nameof(MissingDocumentForSending),
				"Отсутствует документ для отправки по email");
	}
}
