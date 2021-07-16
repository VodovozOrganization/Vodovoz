using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.Models
{
	public interface IEmployeeModel
	{
		void DisablePushNotifications(Employee driver);
		void EnablePushNotifications(Employee driver, string token);
		Employee GetByAPILogin(string login);
	}
}