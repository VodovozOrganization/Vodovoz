using System;
using fyiReporting.RdlGtkViewer;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using QS.Report;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.DriverTerminal
{
	public partial class DriverTerminalWindow : Gtk.Window, IProgressBarDisplayable
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private ReportViewer reportViewer = null;

		public DriverTerminalWindow() :
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			ClearView();
			entryRouteListNumber.ValidationMode = QSWidgetLib.ValidationType.numeric;
		}

		protected void OnDeleteEvent(object o, Gtk.DeleteEventArgs args)
		{
			Application.Quit();
		}

		private void LoadDocument(RouteListPrintableDocuments docType)
		{
			int rlNumber = GetRouteListNumber();
			if(rlNumber == 0) {
				return;
			}
			ReportInfo document = null;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				RouteList rl = uow.GetById<RouteList>(rlNumber);
				if(rl == null) {
					MessageDialogWorks.RunErrorDialog($"Маршрутный лист с номером {rlNumber} не найден");
					return;
				}
				document = PrintRouteListHelper.GetRDL(rl, docType, uow);
			}

			if(document == null) {
				MessageDialogWorks.RunErrorDialog("Не возможно получить печатную форму документа");
				return;
			}

			if(document.Source != null)
				reportViewer.LoadReport(document.Source, document.GetParametersString(), document.ConnectionString, true);
			else
				reportViewer.LoadReport(document.GetReportUri(), document.GetParametersString(), document.ConnectionString, true);
		}

		private int GetRouteListNumber()
		{
			int result = 0;
			if(!int.TryParse(entryRouteListNumber.Text, out result)) {
				MessageDialogWorks.RunErrorDialog("Неправильный номер маршрутного листа");
			}
			return result;
		}

		protected void OnBtnPrintRouteListClicked(object sender, EventArgs e)
		{
			LoadDocument(RouteListPrintableDocuments.RouteList);
		}

		protected void OnBtnPrintLoadDocumentClicked(object sender, EventArgs e)
		{
			LoadDocument(RouteListPrintableDocuments.LoadSofiyskaya);
		}

		protected void OnBtnPrintRouteMapClicked(object sender, EventArgs e)
		{
			LoadDocument(RouteListPrintableDocuments.RouteMap);
		}

		protected void OnButtonClearClicked(object sender, EventArgs e)
		{
			ClearView();
		}

		private void ClearView()
		{
			if(reportViewer != null) {
				reportViewer.Destroy();
			}
			reportViewer = new ReportViewer();
			foreach(Widget w in hboxViewer.AllChildren) {
				hboxViewer.Remove(w);
			}
			hboxViewer.Add(reportViewer);
			ShowAll();
			entryRouteListNumber.Text = "";
		}

		#region IProgressBarDisplayable implementation

		public void ProgressStart(double maxValue, double minValue = 0, string text = null, double startValue = 0)
		{
			progressStatus.Adjustment = new Adjustment(startValue, minValue, maxValue, 1, 1, 1);
			progressStatus.Text = text;
			progressStatus.Visible = true;
			QSMain.WaitRedraw();
		}

		public void ProgressUpdate(double curValue)
		{
			if(progressStatus == null || progressStatus.Adjustment == null)
				return;
			progressStatus.Adjustment.Value = curValue;
			QSMain.WaitRedraw();
		}

		public void ProgressUpdate(string curText)
		{
			if(progressStatus == null || progressStatus.Adjustment == null)
				return;
			progressStatus.Text = curText;
			QSMain.WaitRedraw();
		}

		public void ProgressAdd(double addValue = 1, string text = null)
		{
			if(progressStatus == null)
				return;
			progressStatus.Adjustment.Value += addValue;
			if(text != null)
				progressStatus.Text = text;
			if(progressStatus.Adjustment.Value > progressStatus.Adjustment.Upper)
				logger.Warn("Значение ({0}) прогресс бара в статусной строке больше максимального ({1})",
							(int)progressStatus.Adjustment.Value,
							(int)progressStatus.Adjustment.Upper
						   );
			QSMain.WaitRedraw();
		}

		public void ProgressClose()
		{
			progressStatus.Text = null;
			progressStatus.Visible = false;
			QSMain.WaitRedraw();
		}

		#endregion
	}
}
