using System;
using fyiReporting.RdlGtkViewer;
using Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using NLog;

namespace Vodovoz.DriverTerminal
{
	public partial class DriverTerminalWindow : Gtk.Window, IProgressBarDisplayable
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public DriverTerminalWindow() :
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
		}

		protected void OnDeleteEvent(object o, Gtk.DeleteEventArgs args)
		{
			Application.Quit();
		}

		private void LoadDocument(int rlNumber, RouteListPrintableDocuments docType)
		{
			ReportInfo document = null;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				RouteList rl = uow.GetById<RouteList>(rlNumber);
				if(rl == null) {
					return;
				}
				document = PrintRouteListHelper.GetRDL(rl, docType, uow);
			}

			if(document == null) {
				MessageDialogWorks.RunErrorDialog("Не возможно получить печатную форму документа");
				return;
			}

			if(document.Source != null)
				reportviewer.LoadReport(document.Source, document.GetParametersString(), document.ConnectionString, true);
			else
				reportviewer.LoadReport(document.GetReportUri(), document.GetParametersString(), document.ConnectionString, true);
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
			LoadDocument(GetRouteListNumber(), RouteListPrintableDocuments.RouteList);
		}

		protected void OnBtnPrintLoadDocumentClicked(object sender, EventArgs e)
		{
			LoadDocument(GetRouteListNumber(), RouteListPrintableDocuments.LoadSofiyskaya);
		}

		protected void OnBtnPrintRouteMapClicked(object sender, EventArgs e)
		{
			LoadDocument(GetRouteListNumber(), RouteListPrintableDocuments.RouteMap);
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
