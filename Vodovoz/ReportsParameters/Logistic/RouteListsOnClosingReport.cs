using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListsOnClosingReport : Gtk.Bin, IParametersWidget
	{
		public RouteListsOnClosingReport()
		{
			this.Build();
			ConfigureChkBtns();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по незакрытым МЛ";

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			parameters.Add("todayRloff", chkRemTodayRLs.Active);
			parameters.Add("RemTruckRLs", chkRemTruckRLs.Active);
			parameters.Add("RemServiceRLs", chkRemServiceRLs.Active);
			parameters.Add("RemMercRLs", chkRemMercRLs.Active);

			return new ReportInfo
			{
				Identifier = "Logistic.RouteListOnClosing",
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		#region Настройка смены названий кнопок
		Dictionary<yCheckButton, string> chkLabels = new Dictionary<yCheckButton, string>();

		void ConfigureChkBtns()
		{
			AddChkBtnToListForRenaming(chkRemTodayRLs);
			AddChkBtnToListForRenaming(chkRemTruckRLs);
			AddChkBtnToListForRenaming(chkRemServiceRLs);
			AddChkBtnToListForRenaming(chkRemMercRLs);
		}

		void AddChkBtnToListForRenaming(yCheckButton btn) { 
			chkLabels.Add(btn, btn.Label);
			btn.Toggled += OnChkBtnToggled;
			SetChkBtnTitle(btn);
		}

		void SetChkBtnTitle(yCheckButton btn){
			if(!chkLabels.ContainsKey(btn))
				throw new NotImplementedException(String.Format("Переключатель {0} - {1} не найден в словаре", btn.Name, btn.Label));
			btn.Label = String.Format("{0} {1}", btn.Active ? "Показать" : "Скрыть", chkLabels[btn]);
		}

		protected void OnChkBtnToggled(object sender, EventArgs e)
		{
			SetChkBtnTitle((yCheckButton)sender);
		}
		#endregion
	}
}
