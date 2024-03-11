using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.V4.Models
{
	public interface IEmployeeModel
	{
		void DisablePushNotifications(ExternalApplicationUser driverAppUser);
		void EnablePushNotifications(ExternalApplicationUser driverAppUser, string token);
		IEnumerable<Employee> GetAllPushNotifiableEmployees();
		Employee GetByAPILogin(string login);
	}
}
