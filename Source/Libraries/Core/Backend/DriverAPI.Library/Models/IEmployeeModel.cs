using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.Models
{
	public interface IEmployeeModel
	{
		void DisablePushNotifications(Employee driver);
		void EnablePushNotifications(Employee driver, string token);
		IEnumerable<Employee> GetAllPushNotifiableEmployees();
		Employee GetByAPILogin(string login);
	}
}
