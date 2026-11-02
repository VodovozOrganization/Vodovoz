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
		/// <param name="getDrivers">Источник водителей периода (id водителя, заголовок - ФИО)</param>
		/// <param name="getDistricts">Источник районов периода (id района, для адресов без района - null, заголовок - название)</param>
		IncludeExludeFiltersViewModel CreateCompletedAddressesReportIncludeExcludeFilter(
			Func<IEnumerable<(int Id, string Title)>> getDrivers,
			Func<IEnumerable<(int? Id, string Title)>> getDistricts);
	}
}
