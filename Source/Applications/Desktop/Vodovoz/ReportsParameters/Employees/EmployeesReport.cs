using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ReportsParameters.Employees
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class EmployeesReport : SingleUoWWidgetBase, IParametersWidget
    {
		private readonly IInteractiveService _interactiveService;

		public EmployeesReport(IInteractiveService interactiveService)
        {
            this.Build();
            UoW = UnitOfWorkFactory.CreateWithoutRoot();
            Configure();
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

        public string Title => "Отчет по сотрудникам";
        public event EventHandler<LoadReportEventArgs> LoadReport;

        void Configure()
        {
            buttonInfo.Clicked += ShowInfoWindow;
            buttonRun.Clicked += ButtonRunOnClicked;
            buttonRun.Sensitive = false;

            enumCategory.ItemsEnum = typeof(EmployeeCategory);
            enumCategory.AddEnumToHideList(new Enum[] {EmployeeCategory.office});
            enumCategory.ChangedByUser += (sender, args) => { buttonRun.Sensitive = true; };
        }

        private void ButtonRunOnClicked(object sender, EventArgs e)
        {
            LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo()));
        }

        ReportInfo GetReportInfo()
        {
            return new ReportInfo
            {
                Identifier = "Employees.EmployeesReport",
                Parameters = new Dictionary<string, object>
                {
                    {"report_date", DateTime.Now},
                    {"selected_filters", GetSelectedFilters()},
                    {"empl_category", enumCategory.SelectedItem},
                    {"empl_status", GetStatuses()},
                    {"creation_date", creationPicker.StartDateOrNull},
                    {"creation_date_end", creationPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)},

                    {"first_work_day", firstWorkingDayPicker.StartDateOrNull},
                    {"first_work_day_end", firstWorkingDayPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)},

                    {"date_hired", hiredPicker.StartDateOrNull},
                    {"date_hired_end", hiredPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)},

                    {"date_fired", firedPicker.StartDateOrNull},
                    {"date_fired_end", firedPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)},

                    {"date_calculation", calculationPicker.StartDateOrNull},
                    {"date_calculation_end", calculationPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)},

                    {"first_rl", firstRLPicker.StartDateOrNull},
                    {"first_rl_end", firstRLPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)},

                    {"last_rl", lastRLPicker.StartDateOrNull},
                    {"last_rl_end", lastRLPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)},
                }
            };
        }

        string GetSelectedFilters()
        {
            var result = "Фильтры: категория: ";
            result += (EmployeeCategory)enumCategory.SelectedItem == EmployeeCategory.driver ? "водители" : "экспедиторы";
            result += ", статусы: ";
            result += checkIsWorking.Active ? "работает, " : "";
            result += checkIsFired.Active ? "уволен, " : "";
            result += checkOnMaternityLeave.Active ? "в декрете, " : "";
            result += checkOnCalculation.Active ? "на расчете, " : "";
            result += creationPicker.StartDateOrNull != null ? $"дата создания: с {creationPicker.StartDateOrNull} по {creationPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)}, " : "";
            result += firstWorkingDayPicker.StartDateOrNull != null ? $"дата первого рабочего дня: с {firstWorkingDayPicker.StartDateOrNull} по {firstWorkingDayPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)}, " : "";
            result += hiredPicker.StartDateOrNull != null ? $"дата приема на работу: с {hiredPicker.StartDateOrNull} по {hiredPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)}, " : "";
            result += firedPicker.StartDateOrNull != null ? $"дата увольнения: с {firedPicker.StartDateOrNull} по {firedPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)}, " : "";
            result += calculationPicker.StartDateOrNull != null ? $"дата расчета: с {calculationPicker.StartDateOrNull} по {calculationPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)}, " : "";
            result += firstRLPicker.StartDateOrNull != null ? $"дата первого МЛ: с {firstRLPicker.StartDateOrNull} по {firstRLPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)}, " : "";
            result += lastRLPicker.StartDateOrNull != null ? $"дата последнего МЛ: с {lastRLPicker.StartDateOrNull} по {lastRLPicker.EndDateOrNull?.AddHours(23).AddMinutes(59)}, " : "";
            return result.TrimEnd(',', ' ');
        }

        string GetStatuses()
        {
            var result = checkIsFired.Active ? "IsFired," : "";
            result += checkIsWorking.Active ? "IsWorking," : "";
            result += checkOnMaternityLeave.Active ? "OnMaternityLeave," : "";
            result += checkOnCalculation.Active ? "OnCalculation" : "";
            if (result.Length == 0)
                result = "IsFired,IsWorking,OnMaternityLeave,OnCalculation";
            return result.TrimEnd(',');
        }

        void ShowInfoWindow(object sender, EventArgs args)
        {
            var info = "Фильтры отчета:\n" +
                       "<b>Категория</b>: выбор категории сотрудника\n" +
                       "<b>Фильтр статусов</b>: позволяет выбрать несколько статусов. Если не выбран ни один статус - в таблицу попадут сотрудники с любыми статусами\n" +
                       "<b>Фильтры периодов</b>: не обязательны для выбора и не конфликтуют между собой. " +
                       "<b>Периоды по дате расчета и увольнения</b>: учитываются, если выбраны соответствующие статусы\n" +
                       "<b>В отчет не попадают</b>: водители управляющие фурой компании, являющиеся разовыми, являющиеся мастерами\n";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
        }
    }
}
