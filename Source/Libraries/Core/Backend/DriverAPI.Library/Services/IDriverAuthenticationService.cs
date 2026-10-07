using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.Services
{
	/// <summary>
	/// Проверка доступа сотрудника к водительскому приложению.
	/// </summary>
	public interface IDriverAuthenticationService
	{
		/// <summary>
		/// Проверяет статус сотрудника после проверки учётных данных.
		/// </summary>
		/// <param name="login">Логин водительского приложения.</param>
		/// <returns>Отказ для уволенного сотрудника, иначе успешный результат.</returns>
		Result ValidateEmployeeAccess(string login);
	}
}
