using Vodovoz.Domain.Orders;
using Vodovoz.Validation;

namespace VodovozBusiness.Factories
{
	/// <summary>
	/// Создатель нужного валидатора по типу онлайн заказа
	/// </summary>
	public interface IOnlineOrderValidatorCreator
	{
		/// <summary>
		/// Создание валидатора онлайн заказа
		/// </summary>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <returns></returns>
		IOnlineOrderValidator Create(OnlineOrder onlineOrder);
	}
}
