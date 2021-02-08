using System;
using fyiReporting.RdlGtkViewer;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSProjectsLib;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.DriverTerminal
{
	public partial class DriverTerminalWindow : Gtk.Window
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
					MessageDialogHelper.RunErrorDialog($"Маршрутный лист с номером {rlNumber} не найден");
					return;
				}
				document = PrintRouteListHelper.GetRDL(rl, docType, uow);
			}

			if(document == null) {
				MessageDialogHelper.RunErrorDialog("Не возможно получить печатную форму документа");
				return;
			}

			if(document.Source != null)
				reportViewer.LoadReport(document.Source, document.GetParametersString(), document.ConnectionString, true, document.RestrictedOutputPresentationTypes);
			else
				reportViewer.LoadReport(document.GetReportUri(), document.GetParametersString(), document.ConnectionString, true, document.RestrictedOutputPresentationTypes);
		}

		private int GetRouteListNumber()
		{
			int result = 0;
			if(!int.TryParse(entryRouteListNumber.Text, out result)) {
				MessageDialogHelper.RunErrorDialog("Неправильный номер маршрутного листа");
			}
			return result;
		}

		protected void OnBtnPrintRouteListClicked(object sender, EventArgs e)
		{
			LoadDocument(RouteListPrintableDocuments.RouteList);
		}

		protected void OnBtnPrintLoadDocumentClicked(object sender, EventArgs e)
		{
			LoadDocument(RouteListPrintableDocuments.LoadDocument);
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
	}
}
