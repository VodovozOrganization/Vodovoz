using System;
using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseCategoryView : TabViewBase<ExpenseCategoryViewModel>
	{
		public ExpenseCategoryView(ExpenseCategoryViewModel expenseCategoryViewModel) : base(expenseCategoryViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, (widget) => widget.Text)
				.InitializeFromSource();

			yentryParent.SubjectType = typeof(ExpenseCategory);
			yentryParent.Binding.AddBinding(ViewModel.Entity, e => e.Parent, w => w.Subject).InitializeFromSource();

			yenumTypeDocument.ItemsEnum = typeof(ExpenseInvoiceDocumentType);
			yenumTypeDocument.Binding.AddBinding(ViewModel.Entity, e => e.ExpenseDocumentType, w => w.SelectedItem).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false, QS.Navigation.CloseSource.Cancel); };
		}
	}
}
