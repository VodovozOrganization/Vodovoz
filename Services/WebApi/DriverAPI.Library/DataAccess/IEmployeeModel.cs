using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.DataAccess
{
	public interface IEmployeeModel
	{
		void DisablePushNotifications(Employee driver);
		void EnablePushNotifications(Employee driver, string token);
		Employee GetByAPILogin(string login);
	}
}