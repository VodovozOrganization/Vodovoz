using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashRequestItemView : TabViewBase<CashRequestItemViewModel>
	{
		public CashRequestItemView(CashRequestItemViewModel viewModel) : base(viewModel)
		{
			this.Build();
			this.Configure();
		}


		private void Configure()
		{
			ydateDate.Binding.AddBinding(
				ViewModel.Entity, 
				e => e.Date,
				w => w.Date
			).InitializeFromSource();
			ydateDate.Date = DateTime.Now;

			yentryComment.Binding.AddBinding(
				ViewModel.Entity,
				e => e.Comment, 
				w => w.Text
			).InitializeFromSource();
		
			yspinsum.Binding.AddBinding(
				ViewModel.Entity, 
				e => e.Sum, 
				w => w.ValueAsDecimal
			).InitializeFromSource();
			
			AccountableEntityviewmodelentry3.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() =>
					{
						var employeeFilter = new EmployeeFilterViewModel{
							Status = EmployeeStatus.IsWorking,
						};
						return new EmployeesJournalViewModel(
							employeeFilter,
							UnitOfWorkFactory.GetDefaultFactory, 
							ServicesConfig.CommonServices);
					})
			);
			
			AccountableEntityviewmodelentry3.Binding.AddBinding(
				ViewModel.Entity,
				s => s.AccountableEmployee,
				w => w.Subject
			).InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveCommand.Execute();
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false, CloseSource.Cancel);};

			
			
			#region Visibility
			// TODO gavr сенсативити чето неправильная, или правильная а у тя не та роль при проверке
			// yspinsum.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			// label1.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;
			//
			// ydateDate.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			// label3.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;
			//
			// AccountableEntityviewmodelentry3.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			// label7.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;
			//
			// yentryComment.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			// label8.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;

			#endregion
		}
	}
}
