using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Validation
{
	/// <summary>
	/// Валидатор онлайн заказа
	/// </summary>
	public interface IOnlineOrderValidator
	{
		/// <summary>
		/// Проверка оналйн заказа на соответствие
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="checkPerformedOrders">Проверять оформленные заказы из онлайна</param>
		/// <returns></returns>
		Result Validate(IUnitOfWork uow, bool checkPerformedOrders = false);
	}
}
