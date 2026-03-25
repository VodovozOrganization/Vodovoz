using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.Errors
{
	public static class ServiceEmployee
	{
		public static Error MissingServiceUser =>
			new Error(
				typeof(ServiceEmployee),
				nameof(MissingServiceUser),
				"Не удалось найти сотрудника для обработки заказа. Проверьте настройки сервиса.");
	}
}
