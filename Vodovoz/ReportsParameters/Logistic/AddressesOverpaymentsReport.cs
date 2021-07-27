using System;
using System.Collections.Generic;
using Gamma.Utilities;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AddressesOverpaymentsReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEntityAutocompleteSelectorFactory _driverSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory _officeSelectorFactory;
		private readonly IInteractiveService _interactiveService;

		public AddressesOverpaymentsReport(
			IEntityAutocompleteSelectorFactory driverSelectorFactory,
			IEntityAutocompleteSelectorFactory officeSelectorFactory,
			IInteractiveService interactiveService)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_driverSelectorFactory = driverSelectorFactory ?? throw new ArgumentNullException(nameof(driverSelectorFactory));
			_officeSelectorFactory = officeSelectorFactory ?? throw new ArgumentNullException(nameof(officeSelectorFactory));
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
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

			comboDriverOf.ItemsEnum = typeof(CarTypeOfUse);
			comboDriverOf.ChangedByUser += (sender, args) => OnDriverOfSelected();

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
			return new ReportInfo
			{
				Identifier = "Logistic.AddressesOverpaymentsReport",
				Parameters = new Dictionary<string, object>
				{
					{"start_date", datePicker.StartDateOrNull},
					{"end_date", datePicker.EndDateOrNull?.AddHours(23).AddMinutes(59).AddSeconds(59)},
					{"creation_date", DateTime.Now},
					{"driver_of", comboDriverOf.SelectedItemOrNull},
					{"employee_id", entryDriver.Subject?.GetIdOrNull()},
					{"logistician_id", entryLogistician.Subject?.GetIdOrNull()},
					{"filters", GetSelectedFilters()}
				}
			};
		}

		private string GetSelectedFilters()
		{
			var filters = "Фильтры: водитель: ";
			var empl = entryDriver.GetSubject<Employee>();
			if (empl != null)
			{
				filters += $"{empl.ShortName}";
				filters += $", управляет: {empl.DriverOf.GetEnumTitle()}";
			}
			else
			{
				filters += "все, управляет а/м: ";
				var driver_of = comboDriverOf.SelectedItemOrNull == null
					? "все"
					: ((CarTypeOfUse)comboDriverOf.SelectedItem).GetEnumTitle();
				filters += driver_of;
			}
			var logistician = entryLogistician.GetSubject<Employee>();
			filters += ", логист: ";
			if (logistician != null)
			{
				filters += $"{logistician.ShortName}";
			}
			else
			{
				filters += "все";
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
					  "<b>К</b>: транспорт компании , <b>Н</b>: наемный транспорт, <b>Л</b>: ларгус, <b>Ф</b>: фура, <b>Г</b>: газель.\n\n" +
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
			if(comboDriverOf.SelectedItemOrNull != null)
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

				comboDriverOf.Sensitive = false;
				comboDriverOf.SelectedItemOrNull = empl.DriverOf;
			}
			else
			{
				comboDriverOf.Sensitive = true;
			}
		}
	}
}
