using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule;
using VodovozBusiness.Nodes;

namespace Vodovoz.ViewModels.Services.DriverSchedule
{
	public interface IDriverScheduleService
	{
		/// <summary>
		/// Загружает данные для отображения графика водителей
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="selectedSubdivisionIds"></param>
		/// <param name="selectedCarOwnTypes"></param>
		/// <param name="selectedCarTypeOfUse"></param>
		/// <param name="canEditAfter13"></param>
		/// <param name="availableCarEventTypes"></param>
		/// <returns></returns>
		IEnumerable<DriverScheduleRow> LoadScheduleData(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			int[] selectedSubdivisionIds,
			CarOwnType[] selectedCarOwnTypes,
			CarTypeOfUse[] selectedCarTypeOfUse,
			bool canEditAfter13,
			List<CarEventType> availableCarEventTypes);

		/// <summary>
		/// Сохраняет изменения в графике водителей
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="changedRows"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="currentUserId"></param>
		void SaveScheduleChanges(
			IUnitOfWork uow,
			IEnumerable<DriverScheduleRow> changedRows,
			DateTime startDate,
			DateTime endDate,
			int currentUserId);

		/// <summary>
		/// Экспортирует график водителей в Excel
		/// </summary>
		/// <param name="scheduleRows"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		byte[] ExportToExcel(
			IEnumerable<DriverScheduleRow> scheduleRows,
			DateTime startDate,
			DateTime endDate);

		/// <summary>
		/// Получает список подразделений для фильтра
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="carTypeOfUse"></param>
		/// <returns></returns>
		IEnumerable<SubdivisionNode> GetSubdivisions(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			CarTypeOfUse[] carTypeOfUse);

		/// <summary>
		/// Форматирует указанную дату в строку заданного формата.
		/// </summary>
		/// <param name="date">Дата для форматирования</param>
		/// <returns>
		/// Строка в формате: "Сокращенное название дня недели на русском, ДД.ММ.ГГГГ". 
		/// Пример: "Пн, 01.01.2001"
		/// </returns>
		string GetShortDayString(DateTime date);
	}
}
