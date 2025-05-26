using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Email
{
	public static partial class Email
	{
		public static Error MissingEmailForRequiredMailType =>
			new Error(typeof(Email),
				nameof(MissingEmailForRequiredMailType),
				"Отсутствует адрес для отправки email");

		public static Error MissingDocumentForSending =>
			new Error(typeof(Email),
				nameof(MissingDocumentForSending),
				"Отсутствует документ для отправки по email");
	}
}
