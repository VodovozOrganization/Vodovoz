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
using QS.Utilities.Enums;
using NHibernate;
using FluentNHibernate.Utils;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Documents;

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

		#region Конфигурация контролов виджета отчетов
		/// <summary>
		/// Первичная конфигурация одиночного отчета
		/// </summary>
		private void ConfigureSingleReport()
		{
			//Дата по умолчанию
			datepickerSingleReport.Date = DateTime.Today;

			//Время отправления по умолчанию
			timeHourEntrySingleReport.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntrySingleReport.Text = DateTime.Now.Minute.ToString("00.##");

			entryDriverSingleReport.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());

			entryCarSingleReport.SetEntityAutocompleteSelectorFactory(_carJournalFactory.CreateCarAutocompleteSelectorFactory());
		}

		/// <summary>
		/// Первичная конфигурация группового отчета за один день для транспортного отдела
		/// </summary>
		private void ConfigureGroupReportForOneDay()
		{
			//Дата по умолчанию
			datepickerOneDayGroupReport.Date = DateTime.Today;

			// Тип автомобиля
			enumcheckCarTypeOfUseOneDayGroupReport.EnumType = typeof(CarTypeOfUse);
			MakeActiveCheks(new string[]{ CarTypeOfUse.Largus.ToString() }, ref enumcheckCarTypeOfUseOneDayGroupReport);

			// Принадлежность автомобиля
			enumcheckCarOwnTypeOneDayGroupReport.EnumType = typeof(CarOwnType);
			MakeActiveCheks(new string[] { CarOwnType.Company.ToString() }, ref enumcheckCarOwnTypeOneDayGroupReport);

			//Выбор подразделения
			comboDepartmentOneDayGroupReport.SetRenderTextFunc<Subdivision>(x => x.Name);
			comboDepartmentOneDayGroupReport.ItemsList = UoW.GetAll<Subdivision>();
			comboDepartmentOneDayGroupReport.ShowSpecialStateAll = true;

			//Время отправления по умолчанию
			timeHourEntryOneDayGroupReport.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntryOneDayGroupReport.Text = DateTime.Now.Minute.ToString("00.##");
		}

		/// <summary>
		/// Первичная конфигурация группового отчета за выбранный период для бухгалтерии
		/// </summary>
		private void ConfigureGroupReportForPeriod()
		{
			//TODO Выбор организации по умолчанию
			//Период по умолчанию
			datePickerPeriodGroupReport.StartDate = DateTime.Today;
			datePickerPeriodGroupReport.EndDate = DateTime.Today;

			// Тип автомобиля
			enumcheckCarTypeOfUsePeriodGroupReport.EnumType = typeof(CarTypeOfUse);
			MakeActiveCheks(new string[] { CarTypeOfUse.Largus.ToString(), CarTypeOfUse.GAZelle.ToString() }, ref enumcheckCarTypeOfUsePeriodGroupReport);

			// Принадлежность автомобиля
			enumcheckCarOwnTypePeriodGroupReport.EnumType = typeof(CarOwnType);
			MakeActiveCheks(new string[] { CarOwnType.Company.ToString() }, ref enumcheckCarOwnTypePeriodGroupReport);

			//Выбор организации
			entryManufacturesPeriodGroupReport.SetEntityAutocompleteSelectorFactory(
				_organizationJournalFactory.CreateOrganizationAutocompleteSelectorFactory());

			entryManufacturesPeriodGroupReport.SetEntityAutocompleteSelectorFactory(_organizationJournalFactory.CreateOrganizationAutocompleteSelectorFactory());
		}
		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Путевой лист";

		#endregion

		private ReportInfo GetReportInfo(int driverId, int carId, string timeHours, string timeMinnutes, DateTime? date = null)
		{
			return new ReportInfo
			{
				Identifier = "Logistic.WayBillReportGroupPrint",
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

		private IEnumerable<ReportInfo> GetSingleReportInfo()
		{
			var date = datepickerSingleReport.Date;
			var driverId = (entryDriverSingleReport?.Subject as Employee)?.Id ?? -1;
			var carId = (entryCarSingleReport?.Subject as Car)?.Id ?? -1;
			var timeHours = timeHourEntrySingleReport.Text;
			var timeMinutes = timeMinuteEntrySingleReport.Text;
			var needDate = !datepickerSingleReport.IsEmpty;

			return new ReportInfo[] { GetReportInfo(driverId, carId, timeHours, timeMinutes, date) };
		}

		private IEnumerable<ReportInfo> GetGroupReportInfoForOneDay()
		{
			//TODO в запросе нет учета подразделения
			CarTypeOfUse[] selectedCarTypeOfUses = (enumcheckCarTypeOfUseOneDayGroupReport.SelectedValues).Cast<CarTypeOfUse>().ToArray();
			CarOwnType[] selectedCarOwnTypes = (enumcheckCarOwnTypeOneDayGroupReport.SelectedValues).Cast<CarOwnType>().ToArray();

			Car car = null;
			Employee driver = null;
			CarModel carModel = null;
			CarVersion carVersion = null;

			var cars = UoW.Session
				.QueryOver<Car>(() => car)
				.JoinAlias(() => car.CarModel, () => carModel)
				.JoinAlias(() => car.Driver, () => driver)
				.JoinAlias(() => car.CarVersions, () => carVersion)
				.Where(() =>
					!car.IsArchive
					&& driver.Category == EmployeeCategory.driver)
				.WhereRestrictionOn(() => carModel.CarTypeOfUse).IsIn(selectedCarTypeOfUses)
				.WhereRestrictionOn(() => carVersion.CarOwnType).IsIn(selectedCarOwnTypes)
				.List();

			foreach(var c in cars)
			{
				yield return GetReportInfo(c.Driver.Id, c.Id, timeHourEntryOneDayGroupReport.Text, timeMinuteEntryOneDayGroupReport.Text, datepickerOneDayGroupReport.Date);
			}
		}

		private IEnumerable<ReportInfo> GetGroupReportInfoForPeriod()
		{
			CarTypeOfUse[] selectedCarTypeOfUses = (enumcheckCarTypeOfUsePeriodGroupReport.SelectedValues).Cast<CarTypeOfUse>().ToArray();
			CarOwnType[] selectedCarOwnTypes = (enumcheckCarOwnTypePeriodGroupReport.SelectedValues).Cast<CarOwnType>().ToArray();
			DateTime startDate = datePickerPeriodGroupReport.StartDate;
			DateTime endDate = datePickerPeriodGroupReport.EndDate;
			var organizationId = (entryManufacturesPeriodGroupReport?.Subject as Organization)?.Id ?? -1;

			Car car = null;
			Employee driver = null;
			CarModel carModel = null;
			CarVersion carVersion = null;
			Organization organization = null;
			WayBillDocument wayBillDocument = null;

			var cars = UoW.Session
				.QueryOver<Car>(() => car)
				.JoinAlias(() => car.CarModel, () => carModel)
				.JoinAlias(() => car.Driver, () => driver)
				.JoinAlias(() => car.CarVersions, () => carVersion)
				.Where(() =>
					!car.IsArchive
					&& driver.Category == EmployeeCategory.driver)
				.WhereRestrictionOn(() => carModel.CarTypeOfUse).IsIn(selectedCarTypeOfUses)
				.WhereRestrictionOn(() => carVersion.CarOwnType).IsIn(selectedCarOwnTypes)
				.List();

			foreach(var c in cars)
			{
				yield return GetReportInfo(c.Driver.Id, c.Id, "Hours_value", "Minutes_value", DateTime.Now);
			}
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetGroupReportInfoForPeriod().First(), true));
		}

		private static void MakeActiveCheks(string[] valuesToCheck, ref EnumCheckList checkList)
		{
			if(valuesToCheck.Length < 1) return;

			foreach(var check in checkList.Children.Cast<yCheckButton>())
			{
				if(valuesToCheck.Contains(check.Tag.ToString()))
				{
					check.Active = true;
				}
			}
		}
	}
}

