using System;
using System.Linq;
using Gamma.ColumnConfig;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QS.Widgets.GtkUI;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.JournalViewModels;

namespace Vodovoz.ReportsParameters.Orders
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class NomenclaturePlanReport : SingleUoWWidgetBase, IParametersWidget
    {
        public NomenclaturePlanReport()
        {
            this.Build();
            Configure();
        }

        private void Configure()
        {
            UoW = UnitOfWorkFactory.CreateWithoutRoot();
            buttonCreateReport.Clicked += OnButtonCreateReportClicked;
            dateperiodReportDate.StartDate = DateTime.Today.AddDays(-1);
            dateperiodReportDate.EndDate = DateTime.Today.AddDays(-1);
            yenumcomboboxNomenclatureCategory.ItemsEnum = typeof(NomenclatureCategory);



            ytreeviewChangeTypes.ColumnsConfig = FluentColumnsConfig<SelectedChangeTypeNode>.Create()
                .AddColumn("✓").AddToggleRenderer(x => x.Selected)
                .AddColumn("Тип").AddTextRenderer(x => x.Title)
                .Finish();

            AddChangeType("Фактическое кол-во товара", "ActualCount");
            AddChangeType("Цена товара", "Price");
            AddChangeType("Добавление/Удаление товаров", "OrderItemsCount");
            AddChangeType("Тип оплаты заказа", "PaymentType");

            ytreeviewChangeTypes.ItemsDataSource = changeTypes;
            //entityentrySubdivision = new EntityViewModelEntry();
            //entityentrySubdivision.SetEntityAutocompleteSelectorFactory(
            //    new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(typeof(Subdivision), () => {
            //        var filter = new SubdivisionFilterViewModel();
            //        filter.SubdivisionType = SubdivisionType.Logistic;
            //        IEntityAutocompleteSelectorFactory employeeSelectorFactory =
            //            new DefaultEntityAutocompleteSelectorFactory
            //                <Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices);
            //        return new SubdivisionsJournalViewModel(
            //            filter,
            //            UnitOfWorkFactory.GetDefaultFactory,
            //            ServicesConfig.CommonServices,
            //            employeeSelectorFactory
            //        );
            //    })
            //);
        }

        private void OnButtonCreateReportClicked(object sender, EventArgs e)
        {
            //if (dateperiodpicker.StartDateOrNull == null
            //    || (dateperiodpicker.StartDateOrNull != null && dateperiodpicker.StartDate >= DateTime.Now)
            //    || comboOrganization.SelectedItem == null
            //    || (!changeTypes.Any(x => x.Selected) && !issueTypes.Any(x => x.Selected))
            //) {
            //    return;
            //}

            var reportInfo = GetReportInfo();
            LoadReport?.Invoke(this, new LoadReportEventArgs(reportInfo));
        }

        private ReportInfo GetReportInfo()
        {
            //var ordganizationId = ((Organization)comboOrganization.SelectedItem).Id;
            //var selectedChangeTypes = string.Join(",", changeTypes.Where(x => x.Selected).Select(x => x.Value));
            //var selectedIssueTypes = changeTypes.Any(x => x.Selected && x.Value == "PaymentType") ? string.Empty : string.Join(",", issueTypes.Where(x => x.Selected).Select(x => x.Value));
            //var selectedChangeTypesTitles = string.Join(", ", changeTypes.Where(x => x.Selected).Select(x => x.Title));
            //var selectedIssueTypesTitles = changeTypes.Any(x => x.Selected && x.Value == "PaymentType") ? string.Empty : string.Join(", ", issueTypes.Where(x => x.Selected).Select(x => x.Title));

            //var parameters = new Dictionary<string, object>
            //{
            //    { "start_date", dateperiodpicker.StartDate },
            //    { "end_date", dateperiodpicker.EndDate },
            //    { "organization_id", ordganizationId },
            //    { "change_types", selectedChangeTypes },
            //    { "change_types_rus", selectedChangeTypesTitles },
            //    { "issue_types", selectedIssueTypes },
            //    { "issue_types_rus", selectedIssueTypesTitles }
            //};

            return new ReportInfo
            {
                Identifier = "Orders.OrderChangesReport",
                UseUserVariables = true,
                //Parameters = parameters
            };
        }

        public string Title => "Отчёт по мотивации КЦ";

        public event EventHandler<LoadReportEventArgs> LoadReport;
    }
}
