using System;
using System.Collections.Generic;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Journal.Search.Criterion;
using QS.Project.Services;
using QS.Validation.GtkUI;
using QSOrmProject;
using Vodovoz.Additions.Store;
using Vodovoz.Core;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.JournalViewModels;
using Vodovoz.PermissionExtensions;
using Vodovoz.SearchViewModels;

namespace Vodovoz
{
	public partial class IncomingInvoiceDlg : QS.Dialog.Gtk.EntityDialogBase<IncomingInvoice>
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		public IncomingInvoiceDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<IncomingInvoice>();
			Entity.Author = EmployeeSingletonRepository.GetInstance().GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			Entity.Warehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.IncomingInvoiceCreate);

			ConfigureDlg();
		}

		public IncomingInvoiceDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<IncomingInvoice>(id);

			ConfigureDlg();
		}

		public IncomingInvoiceDlg(IncomingInvoice sub) : this(sub.Id)
		{
		}

		bool canCreate;
		bool canEdit;

		void ConfigureDlg()
		{
			canEdit = Entity.Id != 0 && StoreDocumentHelper.CanEditDocument(WarehousePermissions.IncomingInvoiceEdit, Entity.Warehouse);
			if(Entity.Id != 0 && Entity.TimeStamp < DateTime.Today) {
				var permissionValidator = new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), EmployeeSingletonRepository.GetInstance());
				canEdit &= permissionValidator.Validate(typeof(IncomingInvoice), UserSingletonRepository.GetInstance().GetCurrentUser(UoW).Id, nameof(RetroactivelyClosePermission));
			}

			canCreate = Entity.Id == 0 && !StoreDocumentHelper.CheckCreateDocument(WarehousePermissions.IncomingInvoiceCreate, Entity.Warehouse);
			if(!canCreate && Entity.Id == 0) {
				FailInitialize = true;
				return;
			}

			if(!canEdit && Entity.Id != 0)
				MessageDialogHelper.RunWarningDialog("У вас нет прав на изменение этого документа.");

			if(!canCreate && !canEdit)
				HasChanges = false;

			entryInvoiceNumber.IsEditable = entryWaybillNumber.IsEditable = ytextviewComment.Editable
				= entityVMEntryClient.IsEditable = lstWarehouse.Sensitive = canEdit || canCreate;
			incominginvoiceitemsview1.Sensitive = canEdit || canCreate;

			entryInvoiceNumber.Binding.AddBinding(Entity, e => e.InvoiceNumber, w => w.Text).InitializeFromSource();
			entryWaybillNumber.Binding.AddBinding(Entity, e => e.WaybillNumber, w => w.Text).InitializeFromSource();
			labelTimeStamp.Binding.AddBinding(Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource();

			lstWarehouse.SetRenderTextFunc<Warehouse>(w => w.Name);
			lstWarehouse.ItemsList = StoreDocumentHelper.GetRestrictedWarehouseQuery(Entity.Id > 0 ? WarehousePermissions.IncomingInvoiceEdit : WarehousePermissions.IncomingInvoiceCreate)
														.GetExecutableQueryOver(UoW.Session)
														.List()
														;
			lstWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.SelectedItem).InitializeFromSource();

			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.SetAndRefilterAtOnce(x => x.RestrictIncludeArhive = false);
			entityVMEntryClient.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel, CriterionSearchModel>(
					ServicesConfig.CommonServices,
					CriterionSearchFactory.GetMultipleEntryCriterionSearchViewModel()
				)
			);
			entityVMEntryClient.Binding.AddBinding(Entity, s => s.Contractor, w => w.Subject).InitializeFromSource();
			entityVMEntryClient.CanEditReference = true;

			incominginvoiceitemsview1.DocumentUoW = UoWGeneric;
			ytextviewComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			Entity.CanEdit = canEdit;
			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date) {
				ytextviewComment.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				entryInvoiceNumber.Sensitive = false;
				entryWaybillNumber.Sensitive = false;
				lstWarehouse.Sensitive = false;
				entityVMEntryClient.Sensitive = false;
				incominginvoiceitemsview1.Sensitive = false;
				buttonSave.Sensitive = false;
			} else {
				Entity.CanEdit = true;
			}
			btnPrint.Clicked += BtnPrint_Clicked;
			EntitySaved += (sender, e) => {
				//после сохранения, если нет прав на редактирование, засериваем
				if(!canEdit) {
					ytextviewComment.Sensitive = false;
					entryInvoiceNumber.Sensitive = false;
					entryWaybillNumber.Sensitive = false;
					lstWarehouse.Sensitive = false;
					entityVMEntryClient.Sensitive = false;
					incominginvoiceitemsview1.Sensitive = false;
					buttonSave.Sensitive = false;
				}
			};
		}

		public override bool Save()
		{
			if(!Entity.CanEdit)
				return false;

			var documentHasChanges = UoWGeneric.HasChanges;

			var valid = new QSValidator<IncomingInvoice>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = EmployeeSingletonRepository.GetInstance().GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info("Сохраняем входящую накладную...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			if(documentHasChanges && MessageDialogHelper.RunQuestionDialog("Документ был изменён. Распечатать актуальный документ?"))
				Print();
			return true;
		}

		protected void BtnPrint_Clicked(object sender, EventArgs e)
		{
			if(!UoWGeneric.HasChanges || CommonDialogs.SaveBeforePrint(typeof(IncomingInvoice), "входящей накладной") && Save())
				Print();
		}

		void Print()
		{
			var currentEmployee = EmployeeSingletonRepository.GetInstance().GetEmployeeForCurrentUser(UoW);
			var reportInfo = new QS.Report.ReportInfo {
				Title = Entity.Title,
				Identifier = "Store.IncomingInvoice",
				Parameters = new Dictionary<string, object> {
						{ "document_id",  Entity.Id },
						{ "printed_by_id",  currentEmployee?.Id ?? 0 }
					}
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo)
			);
		}
	}
}