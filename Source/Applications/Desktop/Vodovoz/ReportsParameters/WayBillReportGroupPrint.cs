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
using MoreLinq.Extensions;
using Vodovoz.Core.Permissions;
using System.Linq.Dynamic.Core;
using QS.Dialog;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ReportsParameters
{
	public partial class WayBillReportGroupPrint : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICarJournalFactory _carJournalFactory;
		private readonly IOrganizationJournalFactory _organizationJournalFactory;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IInteractiveService _interactiveService;
		private Func<ReportInfo> _selectedReport;
		private List<Subdivision> _availableSubdivisionsForOneDayGroupReport;

		public WayBillReportGroupPrint(
				IEmployeeJournalFactory employeeJournalFactory, 
				ICarJournalFactory carJournalFactory, 
				IOrganizationJournalFactory organizationJournalFactory, 
				IInteractiveService interactiveService,
				ISubdivisionRepository subdivisionRepository)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_carJournalFactory = carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory));
			_organizationJournalFactory = organizationJournalFactory ?? throw new ArgumentNullException(nameof(organizationJournalFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));

			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			ConfigureSingleReport();
			ConfigureGroupReportForOneDay();

			frameSingleReport.Sensitive = true;
			frameOneDayGroupReport.Sensitive = false;

			_selectedReport = () => GetSingleReportInfo();

			ybuttonCreateReport.Clicked += OnButtonCreateRepotClicked;
			buttonInfoSingleReport.Clicked += OnButtonInfoSingleReportClicked;
			buttonInfoOneDayGroupReport.Clicked += OnButtonInfoOneDayGroupReportClicked;

			enumcheckCarTypeOfUseOneDayGroupReport.CheckStateChanged += EnumcheckCarTypeOfUseOneDayGroupReport_CheckStateChanged;
			enumcheckCarOwnTypeOneDayGroupReport.CheckStateChanged += EnumcheckCarOwnTypeOneDayGroupReport_CheckStateChanged;
		}

		#region Конфигурация элементов управления виджета отчетов
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

			//Выбор водителя
			entityDriverSingleReport.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());

			//Выбор автомобиля
			entityCarSingleReport.SetEntityAutocompleteSelectorFactory(_carJournalFactory.CreateCarAutocompleteSelectorFactory());

			yradiobuttonSingleReport.Clicked += OnRadiobuttonSingleReportClicked;
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
			SetChekBoxesInActive(new string[]{ CarTypeOfUse.Largus.ToString() }, ref enumcheckCarTypeOfUseOneDayGroupReport);

			// Принадлежность автомобиля
			enumcheckCarOwnTypeOneDayGroupReport.EnumType = typeof(CarOwnType);
			SetChekBoxesInActive(new string[] { CarOwnType.Company.ToString() }, ref enumcheckCarOwnTypeOneDayGroupReport);

			//Выбор подразделения
			comboSubdivisionsOneDayGroupReport.SetRenderTextFunc<Subdivision>(x => x.Name);
			_availableSubdivisionsForOneDayGroupReport = GetAvailableSubdivisionsListInAccordingWithCarParameters();
			comboSubdivisionsOneDayGroupReport.ItemsList = _availableSubdivisionsForOneDayGroupReport;
			comboSubdivisionsOneDayGroupReport.ShowSpecialStateAll = _availableSubdivisionsForOneDayGroupReport.Count() > 0;

			//Время отправления по умолчанию
			timeHourEntryOneDayGroupReport.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntryOneDayGroupReport.Text = DateTime.Now.Minute.ToString("00.##");

			yradiobuttonOneDayGroupReport.Clicked += OnRadiobuttonOneDayGroupReportClicked;
		}
		#endregion

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
					{ "driver_id", (entityDriverSingleReport?.Subject as Employee)?.Id ?? -1 },
					{ "car_id", (entityCarSingleReport?.Subject as Car)?.Id ?? -1 },
					{ "time", timeHourEntrySingleReport.Text + ":" + timeMinuteEntrySingleReport.Text },
					{ "need_date", !datepickerSingleReport.IsEmpty }
				}
			};
		}

		private ReportInfo GetGroupReportInfoForOneDay()
		{
			int[] subdivisionIds = (comboSubdivisionsOneDayGroupReport.SelectedItem as Subdivision) != null
				? new[] { (comboSubdivisionsOneDayGroupReport.SelectedItem as Subdivision).Id }
				: _availableSubdivisionsForOneDayGroupReport.Select(s=>s.Id).ToArray();

			var carTypesOfUse = enumcheckCarTypeOfUseOneDayGroupReport.SelectedValues.ToArray();
			var carOwnTypes = enumcheckCarOwnTypeOneDayGroupReport.SelectedValues.ToArray();

			return new ReportInfo
			{
				Identifier = "Logistic.WayBillReportOneDayGroupPrint",
				Parameters = new Dictionary<string, object>
				{
					{ "date", datepickerOneDayGroupReport.Date },
					{ "auto_types", carTypesOfUse.Any() ? carTypesOfUse : new[] { (object)0 } },
					{ "owner_types", carOwnTypes.Any() ? carOwnTypes : new[] { (object)0 } },
					{ "subdivisions", subdivisionIds },
					{ "time", timeHourEntryOneDayGroupReport.Text + ":" + timeMinuteEntryOneDayGroupReport.Text },
					{ "need_date", !datepickerOneDayGroupReport.IsEmpty }
				}
			};
		}

		private List<Subdivision> GetAvailableSubdivisionsListInAccordingWithCarParameters()
		{

			var selectedCarTypeOfUses = (enumcheckCarTypeOfUseOneDayGroupReport.SelectedValues).Cast<CarTypeOfUse>().ToArray();
			var selectedCarOwnTypes = (enumcheckCarOwnTypeOneDayGroupReport.SelectedValues).Cast<CarOwnType>().ToArray();

			return _subdivisionRepository.GetAvailableSubdivisionsInAccordingWithCarTypeAndOwner(UoW, selectedCarTypeOfUses, selectedCarOwnTypes).ToList();
		}

		private static void SetChekBoxesInActive(string[] valuesToCheck, ref EnumCheckList checkList)
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

		protected void OnButtonInfoSingleReportClicked(object sender, EventArgs e)
		{
			var info =
				"Формируется один путевой лист с данными из соответсвующих полей:" +
				$"\n\t'Дата' - дата выезда из гаража" +
				$"\n\t'Водитель' - информация о водителе" +
				$"\n\t'Автомобиль' - информация об автомобиле" +
				$"\n\t'Время' - время выезда из гаража" +
				$"\nДанные поля не являются обязательными к заполнению. При оставлении полей незаполненными (пустыми)," +
				$"\nв путевых листах будут отсутствовать соответствующие значения.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчетом");
		}

		protected void OnButtonInfoOneDayGroupReportClicked(object sender, EventArgs e)
		{
			var info =
				"<b>1.</b> Формируется множество путевых листов для автомобилей, согласно установленным фильтрам" +
				$"\nпо типу и принадлежности автомобиля, а также выбранному значению подразделения." +
				$"\n" +
				$"\n<b>2.</b> При выборе пункта 'Все' в списке подразделений в выборку попадут автомобили," +
				$"\nудовлетворяющие условиям остальных фильтров, из всех подразделений." +
				$"\n" +
				$"\n<b>3.</b> Список подразделений обновляется при каждом изменении фильтра параметров автомобиля." +
				$"\nОтсутствие данных в списке подразделений означает, что автомобили, удовлетворяющие заданным" +
				$"\nусловиям не найдены ни в одном из подразделений." +
				$"\n" +
				$"\n<b>4.</b> Во всех путевых листах указываются одинаковые данные из следующих полей:" +
				$"\n\t'Дата' - дата выезда из гаража" +
				$"\n\t'Время' - время выезда из гаража" +
				$"\nДанные поля не являются обязательными к заполнению. При оставлении полей незаполненными (пустыми)," +
				$"\nв путевых листах будут отсутствовать соответствующие значения." +
				$"\n" +
				$"\n<b>5.</b> В выборку попадают только неархивные автомобили, имеющие \"привязанных\" водителей." +
				$"\nДанные водителя в каждый путевой лист вносятся автоматически.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчетом");
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			if (yradiobuttonOneDayGroupReport.Active && 
				(_availableSubdivisionsForOneDayGroupReport.Count < 1 
				|| enumcheckCarOwnTypeOneDayGroupReport.SelectedValues.Count() < 1
				|| enumcheckCarTypeOfUseOneDayGroupReport.SelectedValues.Count() < 1))
			{
				MessageDialogHelper.RunErrorDialog("Недостаточно данных для отчета");
				return;
			}
			LoadReport?.Invoke(this, new LoadReportEventArgs(_selectedReport.Invoke(), true));
		}

		protected void OnRadiobuttonSingleReportClicked(object sender, EventArgs e)
		{
			_selectedReport = () => GetSingleReportInfo();
			frameSingleReport.Sensitive = true;
			frameOneDayGroupReport.Sensitive = false;
		}

		protected void OnRadiobuttonOneDayGroupReportClicked(object sender, EventArgs e)
		{
			_selectedReport = () => GetGroupReportInfoForOneDay();
			frameSingleReport.Sensitive = false;
			frameOneDayGroupReport.Sensitive = true;
		}

		private void EnumcheckCarTypeOfUseOneDayGroupReport_CheckStateChanged(object sender, CheckStateChangedEventArgs e)
		{
			_availableSubdivisionsForOneDayGroupReport = GetAvailableSubdivisionsListInAccordingWithCarParameters();
			comboSubdivisionsOneDayGroupReport.ItemsList = _availableSubdivisionsForOneDayGroupReport;
			comboSubdivisionsOneDayGroupReport.ShowSpecialStateAll = _availableSubdivisionsForOneDayGroupReport.Count() > 0;
		}

		private void EnumcheckCarOwnTypeOneDayGroupReport_CheckStateChanged(object sender, CheckStateChangedEventArgs e)
		{
			_availableSubdivisionsForOneDayGroupReport = GetAvailableSubdivisionsListInAccordingWithCarParameters();
			comboSubdivisionsOneDayGroupReport.ItemsList = _availableSubdivisionsForOneDayGroupReport;
			comboSubdivisionsOneDayGroupReport.ShowSpecialStateAll = _availableSubdivisionsForOneDayGroupReport.Count() > 0;
		}
	}
}

