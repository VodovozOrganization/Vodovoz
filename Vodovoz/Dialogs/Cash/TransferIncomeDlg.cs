using System;
using Gamma.Utilities;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using QSProjectsLib;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TransferIncomeDlg : EntityDialogBase<Income>
	{
		private bool canEdit = true;

		public TransferIncomeDlg()
		{
			this.Build();
			throw new InvalidOperationException($"Для данного диалога невозможно создание новой сущности");
		}

		public TransferIncomeDlg(int id, IPermissionService permissionService)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income>(id);
			if(Entity.TypeDocument != IncomeInvoiceDocumentType.IncomeTransferDocument) {
				throw new InvalidOperationException($"Диалог доступен только для документа типа {nameof(IncomeInvoiceDocumentType.IncomeTransferDocument)}");
			}
			var userPermission = permissionService.ValidateUserPermission(typeof(Income), ServicesConfig.UserService.CurrentUserId);
			if(!userPermission.CanRead) {
				MessageDialogHelper.RunErrorDialog("Отсутствуют права на просмотр приходного ордера");
				FailInitialize = true;
				return;
			}
			canEdit = userPermission.CanUpdate;
			ConfigureDlg();
		}
		public TransferIncomeDlg(Income income, IPermissionService permissionService) : this(income.Id,permissionService) { }

		private void ConfigureDlg()
		{
			HasChanges = false;
			ylabelDate.Binding.AddFuncBinding(Entity, e => e.Date.ToShortDateString(), w => w.LabelProp).InitializeFromSource();
			ylabelCashier.Binding.AddFuncBinding(Entity, e => e.Casher.GetPersonNameWithInitials(), w => w.LabelProp).InitializeFromSource();
			ylabelTypeOperation.Binding.AddFuncBinding(Entity, e => e.TypeOperation.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			ylabelIncomeCategory.Binding.AddFuncBinding(Entity, e => e.IncomeCategory.Name, w => w.LabelProp).InitializeFromSource();
			ylabelCashSubdivions.Binding.AddFuncBinding(Entity, e => e.RelatedToSubdivision.Name, w => w.LabelProp).InitializeFromSource();
			ylabelTransferDocument.Binding.AddFuncBinding(Entity, e => e.CashTransferDocument.Title, w => w.LabelProp).InitializeFromSource();
			ylabelSum.Binding.AddFuncBinding(Entity, e => e.Money.ToShortCurrencyString(), w => w.LabelProp).InitializeFromSource();
			ylabelDescription.Binding.AddFuncBinding(Entity, e => e.Description, w => w.LabelProp).InitializeFromSource();
		}

		public override bool Save()
		{
			OnCloseTab(false);
			return true;
		}

		protected void OnButtonCloseClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}
