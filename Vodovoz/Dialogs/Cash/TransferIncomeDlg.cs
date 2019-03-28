using System;
using Gamma.Utilities;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TransferIncomeDlg : EntityDialogBase<Income>
	{
		public TransferIncomeDlg()
		{
			this.Build();
			throw new InvalidOperationException($"Для данного диалога невозможно создание новой сущности");
		}

		public TransferIncomeDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income>(id);
			if(Entity.TypeDocument != IncomeInvoiceDocumentType.IncomeTransferDocument) {
				throw new InvalidOperationException($"Диалог доступен только для документа типа {nameof(IncomeInvoiceDocumentType.IncomeTransferDocument)}");
			}
			ConfigureDlg();
		}
		public TransferIncomeDlg(Income income) : this(income.Id) { }

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
