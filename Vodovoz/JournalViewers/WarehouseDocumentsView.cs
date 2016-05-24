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
			DocumentType type = (DocumentType)e.ItemEnum;	
			switch (type) {
				case DocumentType.IncomingInvoice:
				case DocumentType.IncomingWater:
				case DocumentType.MovementDocument:
				case DocumentType.WriteoffDocument:
				case DocumentType.InventoryDocument:
				case DocumentType.RegradingOfGoodsDocument:
				case DocumentType.SelfDeliveryDocument:
					TabParent.OpenTab(
						OrmMain.GenerateDialogHashName(Document.GetDocClass(type), 0),
						() => OrmMain.CreateObjectDialog(Document.GetDocClass(type)),
						this);
					break;
				case DocumentType.CarLoadDocument:
					TabParent.OpenTab(
						TdiTabBase.GenerateHashName(typeof(ReadyForShipmentView)),
						() => new ReadyForShipmentView(), this
					);
					break;
				case DocumentType.CarUnloadDocument:
					TabParent.OpenTab(
						TdiTabBase.GenerateHashName(typeof(ReadyForReceptionView)),
						() => new ReadyForReceptionView(), this
					);
					break;
			default:
				throw new NotSupportedException ("Тип документа не поддерживается.");
			}
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

				switch (DocType) {
					case DocumentType.IncomingInvoice:
						TabParent.OpenTab(
							OrmMain.GenerateDialogHashName<IncomingInvoice>(id),
							() => new IncomingInvoiceDlg (id),
							this);
					break;
				case DocumentType.IncomingWater:
						TabParent.OpenTab(
							OrmMain.GenerateDialogHashName<IncomingWater>(id),
							() => new IncomingWaterDlg (id),
							this);
					break;
				case DocumentType.MovementDocument: 
						TabParent.OpenTab(
							OrmMain.GenerateDialogHashName<MovementDocument>(id),
							() => new MovementDocumentDlg (id),
							this);
					break;
				case DocumentType.WriteoffDocument:
						TabParent.OpenTab(
							OrmMain.GenerateDialogHashName<WriteoffDocument>(id),
							() => new WriteoffDocumentDlg (id),
							this);
					break;
					case DocumentType.InventoryDocument:
						TabParent.OpenTab(
							OrmMain.GenerateDialogHashName<InventoryDocument>(id),
							() => new InventoryDocumentDlg (id),
							this);
						break;
					case DocumentType.RegradingOfGoodsDocument:
						TabParent.OpenTab(
							OrmMain.GenerateDialogHashName<RegradingOfGoodsDocument>(id),
							() => new RegradingOfGoodsDocumentDlg (id),
							this);
						break;
					case DocumentType.SelfDeliveryDocument:
						TabParent.OpenTab(
							OrmMain.GenerateDialogHashName<SelfDeliveryDocument>(id),
							() => new SelfDeliveryDocumentDlg (id),
							this);
						break;
					case DocumentType.CarLoadDocument:
						var doc = uow.GetById<CarLoadDocument>(id);
						var reportInfo = new QSReport.ReportInfo
						{
							Title = doc.Title,
							Identifier = "Store.CarLoadDoc",
							Parameters = new System.Collections.Generic.Dictionary<string, object>
							{
								{ "id",  id }
							}
						};
						TabParent.OpenTab(
							QSReport.ReportViewDlg.GenerateHashName(reportInfo),
							() => new QSReport.ReportViewDlg(reportInfo),
							this);
						break;
					case DocumentType.CarUnloadDocument:
						var unloadDoc = uow.GetById<CarUnloadDocument>(id);
						var unloadReportInfo = new QSReport.ReportInfo
						{
							Title = unloadDoc.Title,
							Identifier = "Store.CarUnloadDoc",
							Parameters = new System.Collections.Generic.Dictionary<string, object>
							{
								{ "id",  id }
							}
						};

						TabParent.OpenTab(
							QSReport.ReportViewDlg.GenerateHashName(unloadReportInfo),
							() => new QSReport.ReportViewDlg(unloadReportInfo),
							this);
						break;
				default:
					throw new NotSupportedException ("Тип документа не поддерживается.");
				}
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

