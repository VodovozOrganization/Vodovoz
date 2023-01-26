//using System;
//namespace Vodovoz.ReportsParameters
//{
//	[System.ComponentModel.ToolboxItem(true)]
//	public partial class WayBillReportGroupPrint : Gtk.Bin
//	{
//		public WayBillReportGroupPrint()
//		{
//			this.Build();
//		}
//	}
//}

using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.Domain.Employees;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using System.Linq;
using NHibernate.Criterion;
using Vodovoz.Domain.Goods;
using NHibernate.Util;

namespace Vodovoz.ReportsParameters
{
	public partial class WayBillReportGroupPrint : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICarJournalFactory _carJournalFactory;
		private readonly IOrganizationJournalFactory _organizationJournalFactory;

		public WayBillReportGroupPrint(IEmployeeJournalFactory employeeJournalFactory, ICarJournalFactory carJournalFactory, IOrganizationJournalFactory organizationJournalFactory)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_carJournalFactory = carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory));
			_organizationJournalFactory = organizationJournalFactory ?? throw new ArgumentNullException(nameof(organizationJournalFactory));

			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureSingleReport();
			ConfigureGroupReportForOneDay();
			ConfigureGroupReportForPeriod();
		}

		private void ConfigureSingleReport()
		{
			datepickerSingleReport.Date = DateTime.Today;
			timeHourEntrySingleReport.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntrySingleReport.Text = DateTime.Now.Minute.ToString("00.##");

			entryDriverSingleReport.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());

			entryCarSingleReport.SetEntityAutocompleteSelectorFactory(_carJournalFactory.CreateCarAutocompleteSelectorFactory());
		}

		private void ConfigureGroupReportForOneDay()
		{
			//Дата по умолчанию
			datepickerOneDayGroupReport.Date = DateTime.Today;

			// Тип автомобиля
			enumcheckCarTypeOfUseOneDayGroupReport.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUseOneDayGroupReport.SelectAll();

			// Принадлежность автомобиля
			enumcheckCarOwnTypeOneDayGroupReport.EnumType = typeof(CarOwnType);
			enumcheckCarOwnTypeOneDayGroupReport.SelectAll();

			//Выбор подразделения
			comboDepartmentOneDayGroupReport.SetRenderTextFunc<Subdivision>(x => x.Name);
			comboDepartmentOneDayGroupReport.ItemsList = UoW.GetAll<Subdivision>();
			comboDepartmentOneDayGroupReport.ShowSpecialStateAll = true;

			//Время отправления по умолчанию
			timeHourEntryOneDayGroupReport.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntryOneDayGroupReport.Text = DateTime.Now.Minute.ToString("00.##");
		}

		private void ConfigureGroupReportForPeriod()
		{
			//Период по умолчанию
			datePickerPeriodGroupReport.StartDate = DateTime.Today;
			datePickerPeriodGroupReport.EndDate = DateTime.Today;

			// Тип автомобиля
			enumcheckCarTypeOfUsePeriodGroupReport.EnumType = typeof(CarOwnType);
			enumcheckCarTypeOfUsePeriodGroupReport.SelectAll();

			// Принадлежность автомобиля
			enumcheckCarOwnTypePeriodGroupReport.EnumType = typeof(CarTypeOfUse);
			enumcheckCarOwnTypePeriodGroupReport.SelectAll();

			//Выбор организации
			entryManufacturesPeriodGroupReport.SetEntityAutocompleteSelectorFactory(
				_organizationJournalFactory.CreateOrganizationAutocompleteSelectorFactory());

			entryManufacturesPeriodGroupReport.SetEntityAutocompleteSelectorFactory(_organizationJournalFactory.CreateOrganizationAutocompleteSelectorFactory());
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Путевой лист";

		#endregion

		private ReportInfo GetSingleReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "Logistic.WayBillReport",
				Parameters = new Dictionary<string, object>
				{
					{ "date", datepickerSingleReport.Date },
					{ "driver_id", (entryDriverSingleReport?.Subject as Employee)?.Id ?? -1 },
					{ "car_id", (entryCarSingleReport?.Subject as Car)?.Id ?? -1 },
					{ "time", timeHourEntrySingleReport.Text + ":" + timeMinuteEntrySingleReport.Text },
					{ "need_date", !datepickerSingleReport.IsEmpty }
				}
			};
		}

		private ReportInfo GetReportInfo(int driverId, int carId, string timeHours, string timeMinnutes, DateTime? date = null)
		{
			return new ReportInfo
			{
				Identifier = "Logistic.WayBillReport",
				Parameters = new Dictionary<string, object>
				{
					{ "date", date },
					{ "driver_id", driverId },
					{ "car_id", carId },
					{ "time", timeHours + ":" + timeMinnutes },
					{ "need_date", date != null }
				}
			};
		}

		private IEnumerable<ReportInfo> GetGroupReportInfoForOneDay()
		{
			var types = (enumcheckCarTypeOfUseOneDayGroupReport.SelectedValues).ToArray();
			
			var cars = UoW.GetAll<Car>()
				.Where(c => !c.IsArchive)
				.ToArray<Car>();
			var type = cars[0].CarModel.CarTypeOfUse;
			var cont2 = types.Contains(cars[0].CarModel.CarTypeOfUse);
			var cont3 = types.Any(t => (CarTypeOfUse)t == cars[0].CarModel.CarTypeOfUse);

			var drivers = UoW.GetAll<Employee>()
				.Where(e=>e.Category == EmployeeCategory.driver)
				.ToArray<Employee>();

			var carDriver = cars.Join(drivers,
					(c) => c.Id,
					(d) => d.Id,
					(c, d) => new { carId = c.Id, driverId = d.Id })
				.ToArray();

			foreach (var car in carDriver)
			{
				yield return GetReportInfo(car.driverId, car.carId, timeHourEntryOneDayGroupReport.Text, timeMinuteEntryOneDayGroupReport.Text, datepickerOneDayGroupReport.Date);
			}
		}

		private ReportInfo GetSingleReportInfo(Dictionary<string, object> parameters)
		{
			return new ReportInfo
			{
				Identifier = "Logistic.WayBillReport",
				Parameters = parameters
			};
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetGroupReportInfoForOneDay().First(), true));
		}
	}
}

