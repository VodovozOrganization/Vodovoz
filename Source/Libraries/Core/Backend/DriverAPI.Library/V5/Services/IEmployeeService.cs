using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.V5.Services
{
	public interface IEmployeeService
	{
		void DisablePushNotifications(ExternalApplicationUser driverAppUser);
		void EnablePushNotifications(ExternalApplicationUser driverAppUser, string token);
		IEnumerable<Employee> GetAllPushNotifiableEmployees();
		Employee GetByAPILogin(string login);
		ExternalApplicationUser GetDriverExternalApplicationUserByFirebaseToken(string recipientToken);
		string GetDriverPushTokenById(int notifyableEmployeeId);
	}
}
