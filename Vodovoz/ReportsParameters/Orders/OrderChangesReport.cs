using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Organizations;
using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using System.Linq;

namespace Vodovoz.ReportsParameters.Orders
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class OrderChangesReport : Gtk.Bin, IParametersWidget
    {
        private List<SelectedChangeTypeNode> changeTypes = new List<SelectedChangeTypeNode>();

        public OrderChangesReport()
        {
            this.Build();
            Configure();
        }

        private void Configure()
        {
            buttonCreateReport.Clicked += OnButtonCreateReportClicked;
            ydatepickerDateFrom.Date = DateTime.Now.AddDays(-7);
            ydatepickerDateFrom.DateChanged += OnDateChanged;
            yentryreferenceOrganization.SubjectType = typeof(Organization);
            yentryreferenceOrganization.Changed += (sender, e) => UpdateSensitivity();
            ytreeviewChangeTypes.ColumnsConfig = FluentColumnsConfig<SelectedChangeTypeNode>.Create()
                .AddColumn("✓").AddToggleRenderer(x => x.Selected)
                .AddColumn("Тип").AddTextRenderer(x => x.Title)
                .Finish();

            AddChangeType("Фактическое кол-во товара", "ActualCount");
            AddChangeType("Цена товара", "Price");
            AddChangeType("Добавление/Удаление товаров", "OrderItemsCount");
            AddChangeType("Тип оплаты заказа", "PaymentType");

            ytreeviewChangeTypes.ItemsDataSource = changeTypes;
        }

        private void AddChangeType(string title, string value)
        {
            var changeType = new SelectedChangeTypeNode();
            changeType.Title = title;
            changeType.Value = value;
            changeType.PropertyChanged += (sender, e) => UpdateSensitivity();
            changeType.Selected = true;
            changeTypes.Add(changeType);
        }

        #region IParametersWidget implementation

        public string Title => "Отчет по изменениям заказа при доставке";

        public event EventHandler<LoadReportEventArgs> LoadReport;

        #endregion IParametersWidget implementation

        private ReportInfo GetReportInfo()
        {
            var ordganizationId = ((Organization)yentryreferenceOrganization.Subject).Id;
            var selectedChangeTypes = string.Join(",", changeTypes.Where(x => x.Selected).Select(x => x.Value));
            var selectedChangeTypesTitles = string.Join(", ", changeTypes.Where(x => x.Selected).Select(x => x.Title)); 

            return new ReportInfo
            {
                Identifier = "Orders.OrderChangesReport",
                UseUserVariables = true,
                Parameters = new Dictionary<string, object>
                {
                    { "date_from", ydatepickerDateFrom.Date },
                    { "organization_id", ordganizationId },
                    { "change_types", selectedChangeTypes },
                    { "change_types_rus", selectedChangeTypesTitles }
                }
            };
        }

        private void OnButtonCreateReportClicked(object sender, EventArgs e)
        {
            if (ydatepickerDateFrom.DateOrNull == null
                || (ydatepickerDateFrom.DateOrNull != null && ydatepickerDateFrom.Date >= DateTime.Now)
                || yentryreferenceOrganization.Subject == null
                || !changeTypes.Any(x => x.Selected)
                ) {
                return;
            }

            var reportInfo = GetReportInfo();
            LoadReport?.Invoke(this, new LoadReportEventArgs(reportInfo));
        }

        private void UpdateSensitivity()
        {
            bool hasValidDate = ydatepickerDateFrom.DateOrNull != null && ydatepickerDateFrom.Date < DateTime.Now;
            bool hasOrganization = yentryreferenceOrganization.Subject != null;
            bool hasChangeTypes = changeTypes.Any(x => x.Selected);
            buttonCreateReport.Sensitive = hasValidDate && hasOrganization && hasChangeTypes;
        }

        private void UpdatePeriodMessage()
        {
            if(ydatepickerDateFrom.DateOrNull == null) {
                ylabelDateWarning.Visible = false;
                return;
            }

            var period = DateTime.Now - ydatepickerDateFrom.Date;
            ylabelDateWarning.Visible = period.Days > 14;
        }

        private void OnDateChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
            UpdatePeriodMessage();
        }
    }

    public class SelectedChangeTypeNode : PropertyChangedBase
    {
        private bool selected;
        public virtual bool Selected
        {
            get => selected;
            set => SetField(ref selected, value);
        }

        public string Title { get; set; }

        public string Value { get; set; }
    }
}
