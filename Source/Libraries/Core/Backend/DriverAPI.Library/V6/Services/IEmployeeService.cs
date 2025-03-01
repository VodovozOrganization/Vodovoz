using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.V6.Services
{
	/// <summary>
	/// Интерфейс для работы с сотрудниками.
	/// </summary>
	public interface IEmployeeService
	{
		/// <summary>
		/// Отключает push-уведомления для пользователя внешнего приложения.
		/// </summary>
		/// <param name="driverAppUser">Пользователь внешнего приложения.</param>
		void DisablePushNotifications(ExternalApplicationUser driverAppUser);

		/// <summary>
		/// Включает push-уведомления для пользователя внешнего приложения.
		/// </summary>
		/// <param name="driverAppUser">Пользователь внешнего приложения.</param>
		/// <param name="token">Токен для push-уведомлений.</param>
		void EnablePushNotifications(ExternalApplicationUser driverAppUser, string token);

		/// <summary>
		/// Получает всех сотрудников, которым можно отправлять push-уведомления.
		/// </summary>
		/// <returns>Список сотрудников.</returns>
		IEnumerable<Employee> GetAllPushNotifiableEmployees();

		/// <summary>
		/// Получает сотрудника по логину для API.
		/// </summary>
		/// <param name="login">Логин сотрудника.</param>
		/// <returns>Сотрудник.</returns>
		Employee GetByAPILogin(string login);

		/// <summary>
		/// Получает пользователя внешнего приложения по токену Firebase.
		/// </summary>
		/// <param name="recipientToken">Токен получателя.</param>
		/// <returns>Пользователь внешнего приложения.</returns>
		ExternalApplicationUser GetDriverExternalApplicationUserByFirebaseToken(string recipientToken);

		/// <summary>
		/// Получает токен push-уведомлений водителя по его идентификатору.
		/// </summary>
		/// <param name="notifyableEmployeeId">Идентификатор сотрудника.</param>
		/// <returns>Токен push-уведомлений.</returns>
		string GetDriverPushTokenById(int notifyableEmployeeId);
	}
}
