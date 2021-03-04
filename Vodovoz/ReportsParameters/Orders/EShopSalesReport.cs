using System;
using System.Collections.Generic;
using System.Data.Bindings;
using System.Linq;
using Gamma.ColumnConfig;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ReportsParameters.Orders
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class EShopSalesReport : SingleUoWWidgetBase, IParametersWidget
    {
        private List<OrderStatusSelectableNode> orderStatuses;
        public List<OrderStatusSelectableNode> OrderStatuses
        {
            get => orderStatuses;
            set => orderStatuses = value;
        }

        public EShopSalesReport()
        {
            this.Build();
            UoW = UnitOfWorkFactory.CreateWithoutRoot();
            Configure();
        }

        private void Configure()
        {
            buttonRun.Clicked += (sender, args) => OnUpdate();

            datePeriodPicker.StartDate = DateTime.Today;
            datePeriodPicker.EndDate = DateTime.Today;

            datePeriodPicker.PeriodChangedByUser += OnDatePeriodPickerPeriodChanged;

            EShopsLoad();

            foreach (var orderStatus in Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>())
            {
                OrderStatuses.Add(new OrderStatusSelectableNode(orderStatus));
            }

            ytreeviewOrderStatuses.ColumnsConfig = FluentColumnsConfig<OrderStatusSelectableNode>.Create()
                .AddColumn("").AddToggleRenderer(x => x.Selected)
                .AddColumn("Статус").AddTextRenderer(x => x.Title)
                .Finish();

            ytreeviewOrderStatuses.ItemsDataSource = OrderStatuses;

            buttonRun.Sensitive = true;
        }

        private void EShopsLoad()
        {
            List<OnlineStore> eShops = new List<OnlineStore>();

            var onlineStores = UoW.Session.QueryOver<OnlineStore>().List();

            eShops.Add(new OnlineStore() { Id = -1, Name = "Все" });

            eShops.AddRange(onlineStores);

            ycomboboxEShopId.SetRenderTextFunc<OnlineStore>(x => x.Name);
            ycomboboxEShopId.ItemsList = eShops;
        }

        #region IParametersWidget implementation

        public string Title => "Отчет по продажам ИМ";

        public event EventHandler<LoadReportEventArgs> LoadReport;

        #endregion

        private ReportInfo GetReportInfo()
        {
            var statuses = string.Join(",", OrderStatuses.Where(x => x.Selected).Select(x => x.Title));

            var parameters = new Dictionary<string, object>
            {
                {"start_date", datePeriodPicker.StartDateOrNull.Value},
                {"end_date", datePeriodPicker.EndDateOrNull.Value.AddHours(23).AddMinutes(59).AddSeconds(59)},
                {"e_shop_id", (ycomboboxEShopId.SelectedItem as OnlineStore).Id},
                {"creation_timestamp", DateTime.Now},
                {"order_statuses", statuses}
            };

            return new ReportInfo
            {
                Identifier = "Orders.EShopSalesReport",
                Parameters = parameters
            };
        }

        void OnUpdate(bool hide = false) =>
            LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

        protected void OnDatePeriodPickerPeriodChanged(object sender, EventArgs e)
        {
            SetSensitivity();
        }

        private void SetSensitivity()
        {
            var datePeriodSelected = datePeriodPicker.EndDateOrNull.HasValue && datePeriodPicker.StartDateOrNull.HasValue;
            buttonRun.Sensitive = datePeriodSelected;
        }

        public class OrderStatusSelectableNode : PropertyChangedBase
        {
            private bool selected;
            public virtual bool Selected
            {
                get => selected;
                set => SetField(ref selected, value);
            }

            public OrderStatus OrderStatus { get; }

            public string Title => OrderStatus.GetEnumTitle();

            public OrderStatusSelectableNode(OrderStatus orderStatus)
            {
                OrderStatus = orderStatus;
            }
        }
    }
}
