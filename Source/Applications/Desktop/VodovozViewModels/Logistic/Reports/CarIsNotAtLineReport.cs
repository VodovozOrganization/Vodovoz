using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Errors;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	[Appellative(Nominative = "Отчёт по простою")]
	public partial class CarIsNotAtLineReport : IClosedXmlReport
	{
		private const string _defaultDateTimeFormat = "dd.MM.yyyy";

		private CarIsNotAtLineReport(
			DateTime date,
			int countDays,
			IEnumerable<(int Id, string Title)> includedEvents,
			IEnumerable<(int Id, string Title)> excludedEvents)
		{
			Date = date;
			CountDays = countDays;
			IncludedEvents = includedEvents;
			ExcludedEvents = excludedEvents;
		}

		#region Параметры отчета

		public DateTime CreatedAt { get; }

		public DateTime Date { get; }

		public int CountDays { get; }

		public IEnumerable<(int Id, string Title)> IncludedEvents { get; }

		public IEnumerable<(int Id, string Title)> ExcludedEvents { get; }

		#endregion Параметры отчета

		#region Строки отчета

		public IEnumerable<Row> Rows { get; set; }

		public IEnumerable<CarTransferRow> CarTransferRows { get; set; }

		public IEnumerable<CarReceptionRow> CarReceptionRows { get; set; }

		public string TemplatePath => throw new NotImplementedException();

		#endregion Строки отчета

		public static Result<CarIsNotAtLineReport> Generate(
			IUnitOfWork unitOfWork,
			IGenericRepository<RouteList> routeListRepository,
			IGenericRepository<CarEvent> carEventRepository,
			IGenericRepository<Car> carRepository,
			DateTime date,
			int countDays,
			IEnumerable<(int Id, string Title)> includedEvents,
			IEnumerable<(int Id, string Title)> excludedEvents)
		{
			var carsHasRouteListInDays = routeListRepository
				.GetValue(
					unitOfWork,
					rl => rl.Car.Id,
					rl => rl.Date >= date.Add(TimeSpan.FromDays(-countDays)))
				.Distinct()
				.ToArray();

			var carsWithoutRouteLists = carRepository
				.GetValue(unitOfWork, c => c, c => !carsHasRouteListInDays.Contains(c.Id));



			return new CarIsNotAtLineReport(date, countDays, includedEvents, excludedEvents);
		}
	}
}
