using Gamma.Utilities;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Services;
using QSOrmProject;
using QSOrmProject.UpdateNotification;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Warehouses;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz
{
	public partial class WarehouseDocumentsView : QS.Dialog.Gtk.TdiTabBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		IUnitOfWork uow;

		public WarehouseDocumentsView ()
		{
			this.Build ();
			this.TabName = "Журнал складских документов";
			tableDocuments.RepresentationModel = new DocumentsVM ();
			hboxFilter.Add (tableDocuments.RepresentationModel.RepresentationFilter as Widget);
			(tableDocuments.RepresentationModel.RepresentationFilter as Widget).Show ();
			uow = tableDocuments.RepresentationModel.UoW;
			tableDocuments.Selection.Changed += OnSelectionChanged;
			buttonEdit.Sensitive = buttonDelete.Sensitive = false;
			buttonAdd.ItemsEnum = typeof(DocumentType);
			buttonAdd.SetVisibility(DocumentType.DeliveryDocument, false);

			CurrentWarehousePermissions warehousePermissions = new CurrentWarehousePermissions();
			var allPermissions = warehousePermissions.WarehousePermissions;
			foreach(DocumentType doctype in Enum.GetValues(typeof(DocumentType))) {
				if(allPermissions.Any(x => x.WarehousePermissionType.GetAttributes<DocumentTypeAttribute>()
					.Any(at => at.Type.Equals(doctype))))
				{
					continue;
				}

				buttonAdd.SetSensitive(doctype, false);
			}
		}

		void OnRefObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			tableDocuments.RepresentationModel.UpdateNodes ();
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonEdit.Sensitive = false;
			buttonDelete.Sensitive = false;

			bool isSelected = tableDocuments.Selection.CountSelectedRows() > 0;
			var storeDocument = new StoreDocumentHelper(new UserSettingsGetter());
			if(isSelected) {
				var node = tableDocuments.GetSelectedObject<DocumentVMNode>();
				if(node.DocTypeEnum == DocumentType.ShiftChangeDocument) {
					var doc = uow.GetById<ShiftChangeWarehouseDocument>(node.Id);
					isSelected = isSelected && storeDocument.CanEditDocument(WarehousePermissionsType.ShiftChangeEdit, doc.Warehouse);
				}

				var item = tableDocuments.GetSelectedObject<DocumentVMNode>();
				var permissionResult = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(Document.GetDocClass(item.DocTypeEnum), ServicesConfig.UserService.CurrentUserId);

				buttonDelete.Sensitive = permissionResult.CanDelete;
				buttonEdit.Sensitive = permissionResult.CanUpdate;
			}
		}

		protected void OnButtonAddEnumItemClicked (object sender, QS.Widgets.EnumItemClickedEventArgs e)
		{
			DocumentType type = (DocumentType)e.ItemEnum;
			switch(type) {
				case DocumentType.MovementDocument:
					Startup.MainWin.NavigationManager
						.OpenViewModelOnTdi<MovementDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
					break;
				case DocumentType.IncomingInvoice:
					Startup.MainWin.NavigationManager
						.OpenViewModelOnTdi<IncomingInvoiceViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
					break;
				case DocumentType.WriteoffDocument:
					Startup.MainWin.NavigationManager.OpenViewModelOnTdi<WriteOffDocumentViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForCreate());
					break;
				case DocumentType.InventoryDocument:
					Startup.MainWin.NavigationManager.OpenViewModelOnTdi<Vodovoz.ViewModels.ViewModels.Warehouses.InventoryDocumentViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForCreate());
					break;
				case DocumentType.ShiftChangeDocument:
					Startup.MainWin.NavigationManager.OpenViewModelOnTdi<ShiftChangeResidueDocumentViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForCreate());
					break;
				case DocumentType.IncomingWater:
				case DocumentType.SelfDeliveryDocument:
				case DocumentType.CarLoadDocument:
				case DocumentType.CarUnloadDocument:
				case DocumentType.RegradingOfGoodsDocument:
				default:
					TabParent.OpenTab(
						DialogHelper.GenerateDialogHashName(Document.GetDocClass(type), 0),
						() => OrmMain.CreateObjectDialog(Document.GetDocClass(type)),
						this
					);
					break;
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
						Startup.MainWin.NavigationManager
							.OpenViewModelOnTdi<IncomingInvoiceViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(id));
						break;
					case DocumentType.IncomingWater:
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<IncomingWater>(id),
							() => new IncomingWaterDlg (id),
							this);
						break;
					case DocumentType.MovementDocument:
						Startup.MainWin.NavigationManager
							.OpenViewModelOnTdi<MovementDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(id));
						break;
					case DocumentType.DriverTerminalGiveout:
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<DriverAttachedTerminalGiveoutDocument>(id),
							() => new DriverAttachedTerminalViewModel(
								EntityUoWBuilder.ForOpen(id),
								UnitOfWorkFactory.GetDefaultFactory,
								ServicesConfig.CommonServices
							),
							this
						);
						break;
					case DocumentType.DriverTerminalReturn:
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<DriverAttachedTerminalReturnDocument>(id),
							() => new DriverAttachedTerminalViewModel(
								EntityUoWBuilder.ForOpen(id),
								UnitOfWorkFactory.GetDefaultFactory,
								ServicesConfig.CommonServices
							),
							this
						);
						break;
					case DocumentType.WriteoffDocument:
						Startup.MainWin.NavigationManager
							.OpenViewModelOnTdi<WriteOffDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(id));
						break;
					case DocumentType.InventoryDocument:
						Startup.MainWin.NavigationManager.OpenViewModelOnTdi<Vodovoz.ViewModels.ViewModels.Warehouses.InventoryDocumentViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForOpen(id));
						break;
					case DocumentType.ShiftChangeDocument:
						Startup.MainWin.NavigationManager
							.OpenViewModelOnTdi<ShiftChangeResidueDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(id));
						break;
					case DocumentType.RegradingOfGoodsDocument:
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<RegradingOfGoodsDocument>(id),
							() => new RegradingOfGoodsDocumentDlg (id),
							this);
						break;
					case DocumentType.SelfDeliveryDocument:
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<SelfDeliveryDocument>(id),
							() => new SelfDeliveryDocumentDlg (id),
							this);
						break;
					case DocumentType.CarLoadDocument:
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<CarLoadDocument>(id),
							() => new CarLoadDocumentDlg (id),
							this);
						break;
					case DocumentType.CarUnloadDocument:
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<CarUnloadDocument>(id),
							() => new CarUnloadDocumentDlg (id),
							this);
						break;
				default:
					throw new NotSupportedException ("Тип документа не поддерживается.");
				}
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			var item = tableDocuments.GetSelectedObject<DocumentVMNode>();
			var permissionResult = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(Document.GetDocClass(item.DocTypeEnum), ServicesConfig.UserService.CurrentUserId);

			if(!permissionResult.CanDelete) {
				return;
			}

			if(OrmMain.DeleteObject (Document.GetDocClass(item.DocTypeEnum), item.Id))
				tableDocuments.RepresentationModel.UpdateNodes ();
		}

		protected void OnButtonFilterToggled (object sender, EventArgs e)
		{
			hboxFilter.Visible = buttonFilter.Active;
		}

		protected void OnSearchentity1TextChanged(object sender, EventArgs e)
		{
			tableDocuments.SearchHighlightText = searchentity1.Text;
			tableDocuments.RepresentationModel.SearchString = searchentity1.Text;
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			tableDocuments.RepresentationModel.UpdateNodes ();
		}

		public override void Destroy()
		{
			uow?.Dispose();
			base.Destroy();
		}
	}
}
