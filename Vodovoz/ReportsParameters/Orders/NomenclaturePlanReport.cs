using System;
using QS.Dialog.GtkUI;
using QSReport;

namespace Vodovoz.ReportsParameters.Orders
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class NomenclaturePlanReport : SingleUoWWidgetBase, IParametersWidget
    {
        public NomenclaturePlanReport()
        {
            this.Build();
        }

        public string Title => "Отчёт по мотивации КЦ";

        public event EventHandler<LoadReportEventArgs> LoadReport;
    }
}
