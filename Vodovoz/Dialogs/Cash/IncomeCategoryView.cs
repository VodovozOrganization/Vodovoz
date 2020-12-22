using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeCategoryView : TabViewBase<IncomeCategoryViewModel>
	{
		public IncomeCategoryView(IncomeCategoryViewModel viewModel) : base(viewModel)
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

			#region ParentEntityviewmodelentry
			ParentEntityviewmodelentry.SetEntityAutocompleteSelectorFactory(ViewModel.IncomeCategoryAutocompleteSelectorFactory);
			ParentEntityviewmodelentry.Binding.AddBinding(ViewModel.Entity, s => s.Parent, w => w.Subject).InitializeFromSource();
			ParentEntityviewmodelentry.CanEditReference = true;
			#endregion

			#region SubdivisionEntityviewmodelentry
			//Это создается тут, а не в ExpenseCategoryViewModel потому что EmployeesJournalViewModel и EmployeeFilterViewModel нет в ViewModels
			var employeeSelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory
					<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices);

			var filter = new SubdivisionFilterViewModel() { SubdivisionType = SubdivisionType.Default };

			SubdivisionEntityviewmodelentry.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(typeof(Subdivision), () => new SubdivisionsJournalViewModel(
					filter,
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					employeeSelectorFactory
				))
			);
			SubdivisionEntityviewmodelentry.Binding.AddBinding(ViewModel.Entity, s => s.Subdivision, w => w.Subject).InitializeFromSource();
			#endregion

			ycheckArchived.Binding.AddBinding(ViewModel, e => e.IsArchive, w => w.Active).InitializeFromSource();

			yenumTypeDocument.ItemsEnum = typeof(IncomeInvoiceDocumentType);
			yenumTypeDocument.Binding.AddBinding(ViewModel.Entity, e => e.IncomeDocumentType, w => w.SelectedItem).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false, QS.Navigation.CloseSource.Cancel); };
		}
	}
}
