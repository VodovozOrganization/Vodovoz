using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Orders;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Orders
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class EShopSalesReport : SingleUoWWidgetBase, IParametersWidget
    {
		private readonly ReportFactory _reportFactory;

		public EShopSalesReport(ReportFactory reportFactory)
        {
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
            this.Build();
            UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
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

            enumchecklistOrderStatus.SelectAll();

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
                {"start_date", datePeriodPicker.StartDateOrNull.Value.Date},
                {"end_date", datePeriodPicker.EndDateOrNull.Value.Date.AddDays(1).AddMilliseconds(-1)},
                {"e_shop_id", (ycomboboxEShopId.SelectedItem as OnlineStore).Id},
                {"creation_timestamp", DateTime.Now},
                {"order_statuses", enumchecklistOrderStatus.SelectedValues},
                {"order_statuses_rus", string.Join(", ", enumchecklistOrderStatus.SelectedValuesList.Select(x => x.GetEnumTitle()))}
            };

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Orders.EShopSalesReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
        }

        void OnUpdate(bool hide = false) {

            if (enumchecklistOrderStatus.SelectedValuesList.Count > 0)
            {
                LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
            }
            else
            {
                MessageDialogHelper.RunInfoDialog("Список статусов не может быть пустым");
            }
        }

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
