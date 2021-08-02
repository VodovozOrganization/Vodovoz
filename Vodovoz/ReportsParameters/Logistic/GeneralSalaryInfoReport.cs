using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.CommonEnums;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ReportsParameters.Logistic
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class GeneralSalaryInfoReport : SingleUoWWidgetBase, IParametersWidget
    {
        public GeneralSalaryInfoReport(
            IEntityAutocompleteSelectorFactory employeeSelectorFactory,
            IInteractiveService interactiveService)
        {
            _interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
            this.employeeSelectorFactory = employeeSelectorFactory ??
                                           throw new ArgumentNullException(nameof(employeeSelectorFactory));
            this.Build();
            UoW = UnitOfWorkFactory.CreateWithoutRoot();
            Configure();
        }

        public string Title => "Основная информация по ЗП";
        public event EventHandler<LoadReportEventArgs> LoadReport;

        private readonly IEntityAutocompleteSelectorFactory employeeSelectorFactory;
        private readonly IInteractiveService _interactiveService;

        private void Configure()
        {
            buttonInfo.Clicked += ShowInfoWindow;

            buttonRun.Sensitive = true;
            buttonRun.Clicked += OnButtonRunClicked;

            comboMonth.ItemsEnum = typeof(Month);
            comboMonth.SelectedItem = (Month) DateTime.Today.Month;

            comboYear.DefaultFirst = true;
            comboYear.ItemsList = Enumerable.Range(DateTime.Now.AddYears(-10).Year, 11).Reverse();

            comboCategory.ItemsEnum = typeof(EmployeeCategory);
            comboCategory.AddEnumToHideList(new Enum[] {EmployeeCategory.office});

            comboDriverOf.ItemsEnum = typeof(CarTypeOfUse);
            comboDriverOf.AddEnumToHideList(new Enum[] {CarTypeOfUse.CompanyTruck});
            comboDriverOf.ChangedByUser += (sender, args) => OnDriverOfSelected();

            entryEmployee.SetEntityAutocompleteSelectorFactory(employeeSelectorFactory);
            entryEmployee.CanEditReference = true;
            entryEmployee.Changed += (sender, args) => OnEmployeeSelected();
        }

        private void OnButtonRunClicked(object sender, EventArgs e)
        {
            LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo()));
        }

        private ReportInfo GetReportInfo()
        {
            var selectedYear = (int) comboYear.SelectedItem;
            var selectedMonth = (int) comboMonth.SelectedItem;
            var creationDate = DateTime.Now;
            var endDate = creationDate.Month == selectedMonth && creationDate.Year == selectedYear
                ? new DateTime(creationDate.Year, creationDate.Month, creationDate.Day, 23,59,59)
                : new DateTime(selectedYear, selectedMonth, DateTime.DaysInMonth(selectedYear, selectedMonth), 23, 59, 59);

            return new ReportInfo
            {
                Identifier = "Logistic.GeneralSalaryInfoReport",
                Parameters = new Dictionary<string, object>
                {
                    {"start_date", new DateTime(selectedYear, selectedMonth, 1)},
                    {"end_date", endDate},
                    {"creation_date", creationDate},
                    {"driver_of", comboDriverOf.SelectedItemOrNull},
                    {"employee_category", comboCategory.SelectedItemOrNull},
                    {"employee_id", entryEmployee.Subject?.GetIdOrNull()},
                    {"filters", GetSelectedFilters()}
                }
            };
        }

        private string GetSelectedFilters()
        {
            var empl = entryEmployee.GetSubject<Employee>();
            var filters = "Фильтры: ";
            if (empl == null)
            {
                var driver_of = comboDriverOf.SelectedItemOrNull == null
                    ? "все, кроме фур"
                    : ((CarTypeOfUse) comboDriverOf.SelectedItem).GetEnumTitle();
                var category = comboCategory.SelectedItemOrNull == null
                    ? "водители и экспедиторы"
                    : ((EmployeeCategory) comboCategory.SelectedItem).GetEnumTitle();
                filters += "сотрудник: все, категория: ";
                filters += category;
                filters += ", тип авто: ";
                filters += driver_of;
            }
            else
            {
                filters += $"сотрудник: {empl.ShortName}, категория: {empl.Category.GetEnumTitle()}";
                if (empl.Category != EmployeeCategory.forwarder)
                    filters += $", управляет: {empl.DriverOf.GetEnumTitle()}";
            }

            return filters;
        }

        private void ShowInfoWindow(object sender, EventArgs e)
        {
            var info = "Сокращения отчета:\n" +
                       "<b>КТС</b>: категория транспортного средства. \n\tСокращения столбца: " +
                       "<b>К</b>: транспорт компании , <b>Н</b>: наемный транспорт, <b>Р</b>: раскатный, <b>Л</b>: ларгус, <b>Г</b>: газель.\n" +
                       "Столбцы отчета:\n" +
                       "<b>№</b>: порядковый номер\n" +
                       "<b>Код</b>: код сотрудника\n" +
                       "<b>ФИО</b>: фамилия имя отчество сотрудника\n" +
                       "<b>Категория</b>: категория сотрудника (Водитель, Экспедитор)\n" +
                       "<b>Управляет</b>: вид транспорта, которым управляет сотрудник ЕСЛИ сотрудник - водитель\n" +
                       "<b>Работает с сотрудником</b>: поле \"Экспедитор по умолчанию\" для водителей\n" +
                            "Если же сотрудник - экспедитор, то к какому водителю он приставлен по умолчанию. Если поле не заполнено, то пустота\n" +
                       "<b>Кол-во рабочих дней</b>: общее количество отработанных дней водителем, <b>Рабочий день</b> - день, в котором есть хотя бы один закрытый МЛ\n" +
                       "<b>Кол-во адресов</b>: общее количество адресов, которые объехал водитель (заказов в закрытых МЛ ) фактическое (не перенес, не отменен, не опоздали)\n" +
                       "<b>Адресов в среднем за день</b>: Кол-во адресов / Кол-во рабочих дней (округлены до целого)\n" +
                       "<b>Кол-во полных бутылей</b>: общее количество перевезенных полных бутылей фактическое\n" +
                       "<b>Полных бутылей в среднем за день</b>: общее количество перевезенных полных бутылей / количество рабочих дней (округлены до целого)\n" +
                       "<b>Кол-во пустых бутылей</b>: общее количество перевезенных пустых бутылей фактическое\n" +
                       "<b>Пустых бутылей в среднем за день</b>: общее количество перевезенных пустых бутылей / количество рабочих дней (округлены до целого)\n" +
                       "<b>ЗП за месяц</b>: фактическая ЗП за месяц\n" +
                       "<b>ЗП за месяц среднее за день</b>: ЗП за месяц / кол- во рабочих дней\n" +
                       "<b>Расчетная ЗП за 22 р.д.</b>: с ЗП за месяц среднее в день * 22\n";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		private void OnDriverOfSelected()
        {
            if (comboDriverOf.SelectedItemOrNull != null)
            {
                comboCategory.Sensitive = false;
                comboCategory.SelectedItem = EmployeeCategory.driver;
            }
            else
            {
                comboCategory.Sensitive = true;
            }
        }

        private void OnEmployeeSelected()
        {
            if (entryEmployee.Subject is Employee empl)
            {
                if (empl.Category == EmployeeCategory.office)
                {
                    _interactiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя выбрать офисного сотрудника");
                    entryEmployee.Subject = null;
                    return;
                }

                if (empl.DriverOf == CarTypeOfUse.CompanyTruck)
                {
                    _interactiveService.ShowMessage(ImportanceLevel.Warning,
                        "Нельзя выбрать водителя, управляющего фурой");
                    entryEmployee.Subject = null;
                    return;
                }

                comboDriverOf.Sensitive = false;
                comboDriverOf.SelectedItemOrNull = empl.DriverOf;
                comboCategory.Sensitive = false;
                comboCategory.SelectedItem = empl.Category;
            }
            else
            {
                comboDriverOf.Sensitive = true;
                comboCategory.Sensitive = true;
            }
        }
    }
}