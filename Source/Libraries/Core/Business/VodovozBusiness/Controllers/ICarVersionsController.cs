using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Controllers
{
	public interface ICarVersionsController
	{
		void AddNewVersion(CarVersion newCarVersion, DateTime startDate);

		void AddNewVersionOnMinimumPossibleDate(CarVersion newCarVersion);

		void CreateAndAddVersion(DateTime? startDate = null);

		void ChangeVersionStartDate(CarVersion version, DateTime newStartDate);

		/// <summary>
		/// Возвращает список доступных принадлежностей для версии, основываясь на более старой версии отностиельно переданной
		/// </summary>
		/// <param name="version">
		///		Если не равна null, то версия обязательно должна быть добавлена в коллекцию сущности.<br/>
		///		Если равна null, то подбирает доступные принадлежности как для новой версии
		/// </param>
		/// <returns>Список доступных принадлежностей</returns>
		IList<CarOwnType> GetAvailableCarOwnTypesForVersion(CarVersion version = null);

		bool IsValidDateForNewCarVersion(DateTime dateTime);

		bool IsValidDateForVersionStartDateChange(CarVersion version, DateTime newStartDate);

		/// <summary>
		/// Возвращает список всех МЛ, затронутых добавлением/изменением даты версий авто
		/// </summary>
		IList<RouteList> GetAllAffectedRouteLists(IUnitOfWork uow);
	}
}
