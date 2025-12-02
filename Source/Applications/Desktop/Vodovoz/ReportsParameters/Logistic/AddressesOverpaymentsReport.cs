using Gamma.Utilities;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.TempAdapters;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class AddressesOverpaymentsReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEntityAutocompleteSelectorFactory _driverSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory _officeSelectorFactory;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IInteractiveService _interactiveService;

		public AddressesOverpaymentsReport(
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IInteractiveService interactiveService)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
				
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_driverSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			_officeSelectorFactory = _employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			Configure();
		}

		public string Title => "Отчет по переплатам за адрес";
		public event EventHandler<LoadReportEventArgs> LoadReport;

		private void Configure()
		{
			buttonInfo.Clicked += ShowInfoWindow;
			buttonRun.Clicked += OnButtonRunClicked;
			buttonRun.Sensitive = false;
			datePicker.StartDateChanged += (sender, e) => { buttonRun.Sensitive = true; };

			comboDriverOfCarTypeOfUse.ItemsEnum = typeof(CarTypeOfUse);
			comboDriverOfCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Loader);
			comboDriverOfCarTypeOfUse.ChangedByUser += (sender, args) => OnDriverOfSelected();

			comboDriverOfCarOwnType.ItemsEnum = typeof(CarOwnType);
			comboDriverOfCarOwnType.ChangedByUser += (sender, args) => OnDriverOfSelected();

			entryDriver.SetEntityAutocompleteSelectorFactory(_driverSelectorFactory);
			entryDriver.Changed += (sender, args) => OnEmployeeSelected();

			entryLogistician.SetEntityAutocompleteSelectorFactory(_officeSelectorFactory);
		}

		private void OnButtonRunClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo()));
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", datePicker.StartDateOrNull },
				{ "end_date", datePicker.EndDateOrNull?.AddHours(23).AddMinutes(59).AddSeconds(59) },
				{ "creation_date", DateTime.Now },
				{ "driver_of_car_type_of_use", comboDriverOfCarTypeOfUse.SelectedItemOrNull },
				{ "driver_of_car_own_type", comboDriverOfCarOwnType.SelectedItemOrNull },
				{ "employee_id", entryDriver.Subject?.GetIdOrNull() },
				{ "logistician_id", entryLogistician.Subject?.GetIdOrNull() },
				{ "filters", GetSelectedFilters() }
			};

			var reportInfo = _reportInfoFactory.Create("Logistic.AddressesOverpaymentsReport", Title, parameters);

			return reportInfo;
		}

		private string GetSelectedFilters()
		{
			var filters = "Фильтры:";
			filters += "\n\tВодитель: ";
			var empl = entryDriver.GetSubject<Employee>();
			if(empl != null)
			{
				filters += $"{empl.ShortName}";
				filters += "\n\t";
				filters +=
					$"Управляет а/м типа: {empl.DriverOfCarTypeOfUse.GetEnumTitle()}, принадлежности: {empl.DriverOfCarOwnType.GetEnumTitle()}";
			}
			else
			{
				filters += "все";
				filters += "\n\t";
				filters += "Управляют а/м типа: " +
				(
					comboDriverOfCarTypeOfUse.SelectedItemOrNull == null
						? "все"
						: ((CarTypeOfUse)comboDriverOfCarTypeOfUse.SelectedItem).GetEnumTitle()
				);
				filters += ", принадлежности: " +
				(
					comboDriverOfCarOwnType.SelectedItemOrNull == null
						? "все"
						: ((CarOwnType)comboDriverOfCarOwnType.SelectedItem).GetEnumTitle()
				);
			}

			var logistician = entryLogistician.GetSubject<Employee>();
			if(logistician != null)
			{
				filters += "\n\t";
				filters += $"Логист: {logistician.ShortName}";
			}
			return filters;
		}

		private void ShowInfoWindow(object sender, EventArgs args)
		{
			var info = "Особенности отчета:\n" +
				"<b>Ситуация 1.</b> У водителя не было закрепленных районов доставки на некоторую дату, и он закрыл какой-либо МЛ " +
				"в этот день.\n" +
				"З/П по адресам этого МЛ начислится как за чужой район, и заказы появятся в данном отчете, в \"закрепленных " +
				"районах\" будет пусто.\n" +
				"В тот же день водителю добавили районы доставки, в таком случае (у водителя их еще не было), районы активируются " +
				"сразу \n" +
				"в 00 часов 00 минут того же дня, а не на следующий день. Итог - после повторной генерации отчета \n" +
				"у заказов появятся \"закрепленные районы\", которые, к тому же, могут совпасть с \"районом доставки\".\n" +
				"<b>Ситуация 2.</b> У некоторого водителя ставка за свой и чужой районы на момент выполнения МЛ может совпадать.\n" +
				"В отчет попадают заказы, у которых переплата больше нуля.\n" +
				"Затем водителю сделали разницу в ставках за районы. Отчет отобразит заказы, если после изменения ставок ЗП за МЛ " +
				"будет пересчитана.\n\n" +
				"Сокращения отчета:\n" +
				"<b>КТС</b>: категория транспортного средства. \n\tСокращения столбца: " +
				"<b>К</b>: транспорт компании , <b>Н</b>: наемный транспорт, <b>Л</b>: ларгус, <b>Ф</b>: фура, <b>Г</b>: газель, <b>Т</b>: Форд Транзит Мини.\n\n" +
				"Столбцы отчета:\n" +
				"<b>№</b>: порядковый номер\n" +
				"<b>№ МЛ</b>: номер маршрутного листа\n" +
				"<b>№ заказа</b>: номер заказа маршрутного листа\n" +
				"<b>ФИО водителя</b>: фамилия имя отчество водителя\n" +
				"<b>ФИО логиста</b>: фамилия имя отчество логиста, создавшего МЛ\n" +
				"<b>КТС</b>: вид транспорта, которым управляет водитель\n" +
				"<b>Подразделение</b>: подразделение водителя\n" +
				"<b>Адрес</b>: адрес в чужом для водителя районе\n" +
				"<b>Переплата</b>: разница между ставкой за 1 адрес в чужом районе и ставкой за свой район\n" +
				"<b>Район адреса</b>\n" +
				"<b>Закрепленные районы</b>: закрепленные за водителем районы на дату МЛ\n" +
				"<b>Комментарий</b>: комментарий к адресу из диалога Разбор МЛ";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		private void OnDriverOfSelected()
		{
			if(comboDriverOfCarOwnType.SelectedItemOrNull != null || comboDriverOfCarTypeOfUse.SelectedItemOrNull != null)
			{
				entryDriver.Sensitive = false;
				entryDriver.Subject = null;
			}
			else
			{
				entryDriver.Sensitive = true;
			}
		}

		private void OnEmployeeSelected()
		{
			if(entryDriver.Subject is Employee empl)
			{
				if(empl.Category != EmployeeCategory.driver)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, "Можно выбрать только водителя");
					entryDriver.Subject = null;
					return;
				}

				comboDriverOfCarOwnType.Sensitive = false;
				comboDriverOfCarOwnType.SelectedItemOrNull = empl.DriverOfCarOwnType;

				comboDriverOfCarTypeOfUse.Sensitive = false;
				comboDriverOfCarTypeOfUse.SelectedItemOrNull = empl.DriverOfCarTypeOfUse;
			}
			else
			{
				comboDriverOfCarOwnType.Sensitive = true;
				comboDriverOfCarTypeOfUse.Sensitive = true;
			}
		}
	}
}
