using System;
using Gtk;
using NLog;
using QSOrmProject;
using QSOrmProject.UpdateNotification;
using QSTDI;
using Vodovoz.Domain.Documents;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class WarehouseDocumentsView : TdiTabBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		IUnitOfWork uow;

		public WarehouseDocumentsView ()
		{
			this.Build ();
			this.TabName = "Журнал документов";
			tableDocuments.RepresentationModel = new DocumentsVM ();
			hboxFilter.Add (tableDocuments.RepresentationModel.RepresentationFilter as Widget);
			(tableDocuments.RepresentationModel.RepresentationFilter as Widget).Show ();
			tableDocuments.RepresentationModel.UpdateNodes ();
			uow = tableDocuments.RepresentationModel.UoW;
			tableDocuments.Selection.Changed += OnSelectionChanged;
			buttonEdit.Sensitive = buttonDelete.Sensitive = false;
		}

		void OnRefObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			tableDocuments.RepresentationModel.UpdateNodes ();

		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive = tableDocuments.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			Document document;

			DocumentType type = (DocumentType)e.ItemEnum;	
			switch (type) {
			case DocumentType.IncomingInvoice:
				document = new IncomingInvoice ();
				break;
			case DocumentType.IncomingWater:
				document = new IncomingWater ();
				break;
			case DocumentType.MovementDocument:
				document = new MovementDocument ();
				break;
			case DocumentType.WriteoffDocument:
				document = new WriteoffDocument ();
				break;
			case DocumentType.CarLoadDocument:
				TabParent.AddTab(new ReadyForShipmentView(), this, false);
				return;
			case DocumentType.CarUnloadDocument:
				TabParent.AddTab(new ReadyForReceptionView(), this, false);
				return;
			default:
				throw new NotSupportedException ("Тип документа не поддерживается.");
			}
			if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
				return;
			TabParent.AddTab (OrmMain.CreateObjectDialog (document.GetType ()), this);
		}

		protected void OnTableDocumentsRowActivated (object o, RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			if (tableDocuments.GetSelectedObjects ().GetLength (0) > 0) {
				int id = (tableDocuments.GetSelectedObjects () [0] as ViewModel.DocumentVMNode).Id;
				DocumentType DocType = (tableDocuments.GetSelectedObjects () [0] as ViewModel.DocumentVMNode).DocTypeEnum;
				ITdiDialog dlg;
				switch (DocType) {
				case DocumentType.IncomingInvoice:
					dlg = new IncomingInvoiceDlg (id);
					break;
				case DocumentType.IncomingWater:
					dlg = new IncomingWaterDlg (id);
					break;
				case DocumentType.MovementDocument: 
					dlg = new MovementDocumentDlg (id);
					break;
				case DocumentType.WriteoffDocument:
					dlg = new WriteoffDocumentDlg (id);
					break;
					case DocumentType.CarLoadDocument:
						var doc = uow.GetById<CarLoadDocument>(id);
						var reportInfo = new QSReport.ReportInfo {
							Title = doc.Title,
							Identifier = "Store.CarLoadDoc",
							Parameters = new System.Collections.Generic.Dictionary<string, object> {
								{ "id",  id }
							}
						};

						TabParent.AddTab (new QSReport.ReportViewDlg (reportInfo) , this);
						return;
					case DocumentType.CarUnloadDocument:
						var unloadDoc = uow.GetById<CarUnloadDocument>(id);
						var unloadReportInfo = new QSReport.ReportInfo {
							Title = unloadDoc.Title,
							Identifier = "Store.CarUnloadDoc",
							Parameters = new System.Collections.Generic.Dictionary<string, object> {
								{ "id",  id }
							}
						};

						TabParent.AddTab (new QSReport.ReportViewDlg (unloadReportInfo) , this);
						return;
				default:
					return;
				}
				TabParent.AddTab (dlg, this);
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			var item = tableDocuments.GetSelectedObject<ViewModel.DocumentVMNode>();
			if(OrmMain.DeleteObject (Document.GetDocClass(item.DocTypeEnum), item.Id))
				tableDocuments.RepresentationModel.UpdateNodes ();
		}

		protected void OnButtonFilterToggled (object sender, EventArgs e)
		{
			hboxFilter.Visible = buttonFilter.Active;
		}
	}
}

