using Vodovoz.Errors;

namespace Vodovoz.Application.Contacts
{
	public interface IPhoneService
	{
		string GetCourierDispatcherPhone();
		Result<string> GetCourierPhonesByTodayOrderContactPhone(string counterpartyPhoneNumber);
	}
}
