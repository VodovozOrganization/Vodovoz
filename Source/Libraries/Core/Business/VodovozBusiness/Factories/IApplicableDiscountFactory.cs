using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	/// <summary>
	/// Фабрика по созданию классов, реализующих <see cref="IApplicableDiscount"/>
	/// </summary>
	public interface IApplicableDiscountFactory
	{
		/// <summary>
		/// Создание <see cref="ApplicableDiscount"/>
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="orderedCartItem"></param>
		/// <returns></returns>
		IApplicableDiscount CreateApplicableDiscount(
			IUnitOfWork uow,
			IOrderedCartItem orderedCartItem);
	}
}
