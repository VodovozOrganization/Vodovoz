using System;
using System.Collections.Generic;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Project.Services;
using QS.Report;
using QS.Validation.GtkUI;
using QSOrmProject;
using QSReport;
using Vodovoz.Additions.Store;
using Vodovoz.Core;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class MovementDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<MovementDocument>
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		public MovementDocumentDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<MovementDocument>();

			Entity.Author = Entity.ResponsiblePerson = EmployeeSingletonRepository.GetInstance().GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.FromWarehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.MovementEdit);
			Entity.ToWarehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.MovementEdit);

			ConfigureDlg();
		}

		public MovementDocumentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<MovementDocument>(id);
			ConfigureDlg();
		}

		public MovementDocumentDlg(MovementDocument sub) : this(sub.Id)
		{
		}

		void ConfigureDlg()
		{
			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.MovementEdit, Entity.FromWarehouse, Entity.ToWarehouse)) {
				FailInitialize = true;
				return;
			}

			textComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			labelTimeStamp.Binding.AddBinding(Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource();

			referenceWarehouseTo.ItemsQuery = StoreDocumentHelper.GetWarehouseQuery();
			referenceWarehouseTo.Binding.AddBinding(Entity, e => e.ToWarehouse, w => w.Subject).InitializeFromSource();
			referenceWarehouseFrom.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissions.MovementEdit);
			referenceWarehouseFrom.Binding.AddBinding(Entity, e => e.FromWarehouse, w => w.Subject).InitializeFromSource();
			repEntryEmployee.RepresentationModel = new EmployeesVM();
			repEntryEmployee.Binding.AddBinding(Entity, e => e.ResponsiblePerson, w => w.Subject).InitializeFromSource();

			yentryrefWagon.SubjectType = typeof(MovementWagon);
			yentryrefWagon.Binding.AddBinding(Entity, e => e.MovementWagon, w => w.Subject).InitializeFromSource();

			ylabelTransportationStatus.Binding.AddBinding(Entity, e => e.TransportationDescription, w => w.LabelProp).InitializeFromSource();

			MovementDocumentCategory[] filteredDoctypeList = { MovementDocumentCategory.InnerTransfer };
			object[] MovementDocumentList = Array.ConvertAll(filteredDoctypeList, x => (object)x);
			enumMovementType.ItemsEnum = typeof(MovementDocumentCategory);
			enumMovementType.AddEnumToHideList(MovementDocumentList);
			enumMovementType.Binding.AddBinding(Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();
			if(Entity.Id == 0) {
				Entity.Category = MovementDocumentCategory.Transportation;
				OnEnumMovementTypeChanged(null, null);
			}

			moveingNomenclaturesView.DocumentUoW = UoWGeneric;

			UpdateAcessibility();

			var permmissionValidator = new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), EmployeeSingletonRepository.GetInstance());
			Entity.CanEdit = permmissionValidator.Validate(typeof(MovementDocument), UserSingletonRepository.GetInstance().GetCurrentUser(UoW).Id, nameof(RetroactivelyClosePermission));
			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date) {
				enumMovementType.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				yentryrefWagon.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				referenceWarehouseTo.Sensitive = false;
				referenceWarehouseFrom.Sensitive = false;
				repEntryEmployee.Sensitive = false;
				textComment.Sensitive = false;
				buttonDelivered.Sensitive = false;
				moveingNomenclaturesView.Sensitive = false;

				buttonSave.Sensitive = false;
			} else {
				Entity.CanEdit = true;
			}
		}

		void UpdateAcessibility()
		{
			bool canEditOldDocument = ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(
				"can_edit_delivered_goods_transfer_documents",
				ServicesConfig.CommonServices.UserService.CurrentUserId
			);
			if(Entity.TransportationStatus == TransportationStatus.Delivered && !canEditOldDocument) {
				HasChanges = false;
				tableCommon.Sensitive = false;
				hbxSenderAddressee.Sensitive = false;
				moveingNomenclaturesView.Sensitive = false;
				buttonSave.Sensitive = false;
				return;
			}

			var editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.MovementEdit, Entity.FromWarehouse, Entity.ToWarehouse);
			enumMovementType.Sensitive = repEntryEmployee.IsEditable = referenceWarehouseTo.Sensitive
				 = yentryrefWagon.IsEditable = textComment.Sensitive = editing;
			moveingNomenclaturesView.Sensitive = editing;

			referenceWarehouseFrom.IsEditable = StoreDocumentHelper.CanEditDocument(WarehousePermissions.MovementEdit, Entity.FromWarehouse);

			buttonDelivered.Sensitive = Entity.TransportationStatus == TransportationStatus.Submerged
				&& CurrentPermissions.Warehouse[WarehousePermissions.MovementEdit, Entity.ToWarehouse];
		}

		public override bool Save()
		{
			if(!Entity.CanEdit)
				return false;

			var valid = new QSValidator<MovementDocument>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info("Сохраняем документ перемещения...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}

		protected void OnEnumMovementTypeChanged(object sender, EventArgs e)
		{
			var selected = Entity.Category;
			referenceWarehouseTo.Visible = referenceWarehouseFrom.Visible = labelStockFrom.Visible = labelStockTo.Visible
				= (selected == MovementDocumentCategory.InnerTransfer || selected == MovementDocumentCategory.Transportation);

			//Траспортировка
			labelWagon.Visible = hboxTransportation.Visible = yentryrefWagon.Visible = labelTransportationTitle.Visible
				= selected == MovementDocumentCategory.Transportation;
		}

		protected void OnButtonDeliveredClicked(object sender, EventArgs e)
		{
			buttonDelivered.Sensitive = false;
			Entity.TransportationCompleted();
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(!UoWGeneric.HasChanges || CommonDialogs.SaveBeforePrint(typeof(MovementDocument), "документа") && Save()) {
				var doc = new MovementDocumentRdl(Entity);
				if(doc is IPrintableRDLDocument)
					TabParent.AddTab(DocumentPrinter.GetPreviewTab(doc as IPrintableRDLDocument), this, false);
			}
		}
	}

	public class MovementDocumentRdl : IPrintableRDLDocument
	{
		public string Title { get; set; } = "Документ перемещения";

		public MovementDocument Document { get; set; }

		public Dictionary<object, object> Parameters { get; set; }

		public PrinterType PrintType { get; set; } = PrinterType.RDL;

		public DocumentOrientation Orientation { get; set; } = DocumentOrientation.Portrait;

		public int CopiesToPrint { get; set; } = 1;

		public string Name { get; set; } = "Документ перемещения";

		public ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = Document.Title,
				Identifier = "Documents.MovementOperationDocucment",
				Parameters = new Dictionary<string, object>
				{
					{ "documentId" , Document.Id} ,
					{ "date" , Document.TimeStamp.ToString("dd/MM/yyyy")}
				}
			};
		}
		public MovementDocumentRdl(MovementDocument document) => Document = document;
	}
}