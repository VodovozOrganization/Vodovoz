using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Validation.GtkUI;
using Vodovoz.Additions.Store;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Domain.Permissions;
using Vodovoz.PermissionExtensions;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Dialogs.DocumentDialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShiftChangeWarehouseDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<ShiftChangeWarehouseDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public ShiftChangeWarehouseDocumentDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<ShiftChangeWarehouseDocument>();
			Entity.Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			if(UoW.IsNew)
				Entity.Warehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.ShiftChangeCreate);
			if(!UoW.IsNew)
				Entity.Warehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.ShiftChangeEdit);

			ConfigureDlg();
		}

		public ShiftChangeWarehouseDocumentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ShiftChangeWarehouseDocument>(id);
			ConfigureDlg();
		}

		public ShiftChangeWarehouseDocumentDlg(ShiftChangeWarehouseDocument sub) : this (sub.Id)
		{
		}

		bool canCreate;
		bool canEdit;

		public bool CanSave => canCreate || canEdit;

		void ConfigureDlg()
		{
			canEdit = !UoW.IsNew && StoreDocumentHelper.CanEditDocument(WarehousePermissions.ShiftChangeEdit, Entity.Warehouse);

			if(Entity.Id != 0 && Entity.TimeStamp < DateTime.Today) {
				var permmissionValidator = new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), EmployeeSingletonRepository.GetInstance(), UserSingletonRepository.GetInstance());
				canEdit &= permmissionValidator.Validate(typeof(ShiftChangeWarehouseDocument), UserSingletonRepository.GetInstance().GetCurrentUser(UoW).Id, nameof(RetroactivelyClosePermission));
			}

			canCreate = UoW.IsNew && !StoreDocumentHelper.CheckCreateDocument(WarehousePermissions.ShiftChangeCreate, Entity.Warehouse);

			if(!canCreate && UoW.IsNew){
				FailInitialize = true;
				return;
			}

			if(!canEdit && !UoW.IsNew)
				MessageDialogHelper.RunWarningDialog("У вас нет прав на изменение этого документа.");

			ydatepickerDocDate.Sensitive = yentryrefWarehouse.IsEditable = ytextviewCommnet.Editable = canEdit || canCreate;
			shiftchangewarehousedocumentitemsview1.Sensitive = canEdit || canCreate;
			ydatepickerDocDate.Binding.AddBinding(Entity, e => e.TimeStamp, w => w.Date).InitializeFromSource();
			if(UoW.IsNew)
				yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissions.ShiftChangeCreate);
			if(!UoW.IsNew)
				yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissions.ShiftChangeEdit);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();

			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			string errorMessage = "Не установлены единицы измерения у следующих номенклатур :" + Environment.NewLine;
			int wrongNomenclatures = 0;
			foreach(var item in UoWGeneric.Root.Items) {
				if(item.Nomenclature.Unit == null) {
					errorMessage += string.Format("Номер: {0}. Название: {1}{2}",
						item.Nomenclature.Id, item.Nomenclature.Name, Environment.NewLine);
					wrongNomenclatures++;
				}
			}
			if(wrongNomenclatures > 0) {
				MessageDialogHelper.RunErrorDialog(errorMessage);
				FailInitialize = true;
				return;
			}

			shiftchangewarehousedocumentitemsview1.DocumentUoW = UoWGeneric;
		}

		public override bool Save()
		{
			if(!CanSave)
				return false;

			var valid = new QSValidator<ShiftChangeWarehouseDocument>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info("Сохраняем акт списания...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(ShiftChangeWarehouseDocument), "акта передачи склада"))
				Save();

			var reportInfo = new QS.Report.ReportInfo {
				Title = String.Format("Акт передачи склада №{0} от {1:d}", Entity.Id, Entity.TimeStamp),
				Identifier = "Store.ShiftChangeWarehouse",
				Parameters = new Dictionary<string, object> {
					{ "document_id",  Entity.Id }
				}
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo)
			);
		}

		protected void OnYentryrefWarehouseBeforeChangeByUser(object sender, EntryReferenceBeforeChangeEventArgs e)
		{
			if(Entity.Warehouse != null && Entity.Items.Count > 0) {
				if(MessageDialogHelper.RunQuestionDialog("При изменении склада табличная часть документа будет очищена. Продолжить?"))
					Entity.ObservableItems.Clear();
				else
					e.CanChange = false;
			}
		}
	}
}
