using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.Services.Logistics
{
	/// <summary>
	/// Класс для проверки водителя
	/// </summary>
	public interface IDriverChecker
	{
		/// <summary>
		/// Проверка водителя на большое количество незакрытых МЛ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="driver">Водитель</param>
		/// <param name="routeListId">Идентификатор МЛ</param>
		/// <returns></returns>
		bool IsDriversDebtInPermittedRangeVerification(
			IUnitOfWork uow,
			Employee driver,
			int routeListId);
	}
}
