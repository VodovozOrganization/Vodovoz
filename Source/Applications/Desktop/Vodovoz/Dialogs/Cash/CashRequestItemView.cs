using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashRequestItemView : TabViewBase<CashRequestItemViewModel>
	{
		public CashRequestItemView(CashRequestItemViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ydateDate.Binding.AddBinding(
				ViewModel, 
				e => e.Date,
				w => w.Date
			).InitializeFromSource();
			ydateDate.Date = DateTime.Now;

			yentryComment.Binding.AddBinding(
				ViewModel,
				e => e.Comment, 
				w => w.Text
			).InitializeFromSource();

			yspinsum.Adjustment.Upper = 999_000_000d;
			yspinsum.Binding.AddBinding(
				ViewModel, 
				e => e.Sum, 
				w => w.ValueAsDecimal
			).InitializeFromSource();
			
			AccountableEntityviewmodelentry3.SetEntityAutocompleteSelectorFactory(
				ViewModel.EmployeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory());
			
			AccountableEntityviewmodelentry3.Binding.AddBinding(
				ViewModel,
				s => s.AccountableEmployee,
				w => w.Subject
			).InitializeFromSource();

			buttonAccept.Clicked += (sender, args) => ViewModel.AcceptCommand.Execute();
			buttonCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();

			#region Visibility

			yspinsum.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			label1.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;
			
			ydateDate.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			label3.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;
			
			AccountableEntityviewmodelentry3.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			label7.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;
			
			yentryComment.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			label8.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;

			#endregion
		}
	}
}
