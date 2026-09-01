using Vodovoz.Core.Domain.Interfaces.Sale;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Контракт для расчета итоговой скидки в деньгах
	/// </summary>
	public interface ICalculatingTotalMoneyDiscount : ICurrentRawPrice, IDiscountReasons
	{
		/// <summary>
		/// Персональная скидка
		/// </summary>
		PersonalDiscount PersonalDiscount { get; }
	}
}
