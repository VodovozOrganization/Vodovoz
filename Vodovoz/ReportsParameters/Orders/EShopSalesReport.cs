using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ReportsParameters.Orders
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class EShopSalesReport : SingleUoWWidgetBase, IParametersWidget
    {
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

            enumchecklistOrderStatus.EnumType = typeof(OrderStatus);

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

            var parameters = new Dictionary<string, object>
            {
                {"start_date", datePeriodPicker.StartDateOrNull.Value},
                {"end_date", datePeriodPicker.EndDateOrNull.Value.AddHours(23).AddMinutes(59).AddSeconds(59)},
                {"e_shop_id", (ycomboboxEShopId.SelectedItem as OnlineStore).Id},
                {"creation_timestamp", DateTime.Now},
                {"order_statuses", enumchecklistOrderStatus.SelectedValues}
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
    }
}
