using System;
using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	/// <summary>
	/// Фабрика фильтра "Водители" и "Районы" отчета по выполненным адресам и районам
	/// </summary>
	public interface IIncludeExcludeCompletedAddressesReportFilterFactory
	{
		/// <summary>
		/// Создает фильтр с разделами "Водители" и "Районы", элементы которых берутся из переданных источников
		/// </summary>
		/// <param name="getDrivers">Источник водителей периода (ключ - id водителя, заголовок - ФИО)</param>
		/// <param name="getDistricts">Источник районов периода (ключ - название района, заголовок - название)</param>
		IncludeExludeFiltersViewModel CreateCompletedAddressesReportIncludeExcludeFilter(
			Func<IEnumerable<(string Key, string Title)>> getDrivers,
			Func<IEnumerable<(string Key, string Title)>> getDistricts);
	}
}
