using System;

namespace Vodovoz.Application.Errors
{
	/// <summary>
	/// Исключение, выбрасываемое при отсутствии сотрудника - пользователя сервиса для обработки заказа.
	/// </summary>
	public class ServiceUserMissingException : Exception
	{
		public override string Message => "Не удалось найти сотрудника для обработки заказа. Проверьте настройки сервиса.";
	}
}
