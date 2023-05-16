using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseCategoryView : TabViewBase<ExpenseCategoryViewModel>
	{
		public ExpenseCategoryView(ExpenseCategoryViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, (widget) => widget.Text)
				.InitializeFromSource();

			yentryNumbering.Binding
				.AddBinding(ViewModel.Entity, e => e.Numbering, (widget) => widget.Text)
				.InitializeFromSource();

			#region SubdivisionEntityviewmodelentry

			SubdivisionEntityviewmodelentry.SetEntityAutocompleteSelectorFactory(ViewModel.SubdivisionAutocompleteSelectorFactory);
			SubdivisionEntityviewmodelentry.Binding.AddBinding(ViewModel.Entity, s => s.Subdivision, w => w.Subject).InitializeFromSource();

			#endregion

			ycheckArchived.Binding.AddBinding(ViewModel, e => e.IsArchive, w => w.Active).InitializeFromSource();

			yenumTypeDocument.ItemsEnum = typeof(ExpenseInvoiceDocumentType);
			yenumTypeDocument.Binding.AddBinding(ViewModel.Entity, e => e.ExpenseDocumentType, w => w.SelectedItem).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };
		}
	}
}
