using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.Contacts
{
	public interface IPhoneService
	{
		/// <summary>
		/// Получение телефона ВТ
		/// </summary>
		/// <returns></returns>
		string GetCourierDispatcherPhone();

		/// <summary>
		/// Получение телефона курьера по телефону для связи сегодняшних заказов
		/// </summary>
		/// <param name="counterpartyPhoneNumber"></param>
		/// <returns></returns>
		Result<string> GetCourierPhonesByTodayOrderContactPhone(string counterpartyPhoneNumber);
	}
}
