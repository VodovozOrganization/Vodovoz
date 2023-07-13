using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[ToolboxItem(true)]
	public partial class CashRequestItemView : TabViewBase<CashRequestItemViewModel>
	{
		private const double _maximalSum = 999_000_000d;

		public CashRequestItemView(CashRequestItemViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ydateDate.Binding
				.AddBinding(ViewModel, e => e.Date, w => w.Date)
				.InitializeFromSource();

			ydateDate.Date = DateTime.Now;

			yentryComment.Binding
				.AddBinding(ViewModel, e => e.Comment, w => w.Text)
				.InitializeFromSource();

			yspinsum.Adjustment.Upper = _maximalSum;
			yspinsum.Binding
				.AddBinding(ViewModel, e => e.Sum, w => w.ValueAsDecimal)
				.InitializeFromSource();

			entryEmployee.ViewModel = ViewModel.EmployeeViewModel;

			buttonAccept.Clicked += (sender, args) => ViewModel.AcceptCommand.Execute();
			buttonCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();

			#region Visibility

			yspinsum.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			label1.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;
			
			ydateDate.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			label3.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;
			
			entryEmployee.ViewModel = ViewModel.EmployeeViewModel;

			entryEmployee.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			label7.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;
			
			yentryComment.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNRC_OrRoleCoordinator, w => w.Sensitive).InitializeFromSource();
			label8.Sensitive = ViewModel.CanEditOnlyinStateNRC_OrRoleCoordinator;

			#endregion
		}
	}
}
