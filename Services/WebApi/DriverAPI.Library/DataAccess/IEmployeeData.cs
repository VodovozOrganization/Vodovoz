using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.DataAccess
{
	public interface IEmployeeData
	{
		void DisablePushNotifications(Employee driver);
		void EnablePushNotifications(Employee driver, string token);
		Employee GetByAPILogin(string login);
	}
}