using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Services.Logistics
{
	/// <summary>
	/// Сервис для работы со стоп-листами водителей.
	/// </summary>
	public interface IDriverStopListService
	{
		/// <summary>Блокирует водителя до ближайшей полуночи, прекращая действующие снятия. Фиксацию выполняет вызывающий код.</summary>
		/// <param name="uow">Единица работы для сохранения изменений.</param>
		/// <param name="driver">Водитель.</param>
		void AddDriverToStopList(IUnitOfWork uow, Employee driver);

		/// <summary>Проверяет статус по правилам журнала с учётом ручной блокировки и временного снятия.</summary>
		/// <param name="uow">Единица работы.</param>
		/// <param name="driver">Водитель; отсутствие водителя не блокирует выбор.</param>
		/// <param name="routeListId">Маршрутный лист, исключаемый из расчёта долгов.</param>
		/// <returns>Водитель находится в стоп-листе.</returns>
		bool IsDriverInStopList(IUnitOfWork uow, Employee driver, int routeListId);
	}
}
