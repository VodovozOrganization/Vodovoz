using System;
using QS.DomainModel.UoW;
using QS.Views.GtkUI;
using QSOrmProject;
using QSWidgetLib;
using Vodovoz.Domain.BusinessTasks;
using Vodovoz.ViewModels.BusinessTasks;
using Vodovoz.Views.Comments;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Views.BusinessTasks
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentTaskView : TabViewBase<PaymentTaskViewModel>
	{
		readonly IUnitOfWork uow;

		public PaymentTaskView(PaymentTaskViewModel viewModel) : base(viewModel)
		{
			this.Build();
			uow = ViewModel.UoW;
			Configure();
		}

		private void Configure()
		{
			buttonSave.Clicked += (sender, e) => ViewModel.SaveCommand.Execute();
			buttonCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();

			labelCreator.Binding.AddFuncBinding(ViewModel.Entity,
												vm => vm.TaskCreator == null ? "" : vm.TaskCreator.ShortName,
												v => v.Text).InitializeFromSource();

			taskStateComboBox.ItemsEnum = typeof(BusinessTaskStatus);
			taskStateComboBox.Binding.AddBinding(ViewModel.Entity, vm => vm.TaskState, w => w.SelectedItemOrNull).InitializeFromSource();
			paymentStatusComboBox.ItemsEnum = typeof(OrderPaymentStatus);
			paymentStatusComboBox.Binding.AddBinding(ViewModel.Entity, vm => vm.PaymentStatus, w => w.SelectedItemOrNull).InitializeFromSource();
			isTaskCompleteButton.Binding.AddBinding(ViewModel.Entity, s => s.IsTaskComplete, w => w.Active).InitializeFromSource();
			isTaskCompleteButton.Label += ViewModel.Entity.CompleteDate?.ToString("dd / MM / yyyy  HH:mm");
			deadlineYdatepicker.Binding.AddBinding(ViewModel.Entity, vm => vm.EndActivePeriod, w => w.Date).InitializeFromSource();
			//datepickerPaymentDate.Binding.AddBinding(ViewModel.Entity, vm => vm.Date, w => w.Date).InitializeFromSource();
			//orderSumEntry.ValidationMode = ValidationType.numeric;
			//currentPayEntry.Binding.AddBinding(ViewModel.Entity, vm => vm.OrderPositiveSum, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();
			//lastPaymentsEntry.Binding.AddBinding(ViewModel.Entity, vm => vm.OrderPositiveSum, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();
			//orderSumEntry.Binding.AddBinding(ViewModel.Entity, vm => vm.OrderPositiveSum, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();
			//textViewPaymentPurpose.Binding.AddBinding(ViewModel.Entity, vm => vm.PaymentPurpose, w => w.Buffer.Text).InitializeFromSource();

			employeeViewModelEntry.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			employeeViewModelEntry.CanEditReference = true;
			employeeViewModelEntry.Binding.AddBinding(ViewModel.Entity, vm => vm.AssignedEmployee, v => v.Subject).InitializeFromSource();

			counterpartyViewModelEntry.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			counterpartyViewModelEntry.CanEditReference = true;
			counterpartyViewModelEntry.Binding.AddBinding(ViewModel.Entity, vm => vm.Counterparty, w => w.Subject).InitializeFromSource();
			//counterpartyViewModelEntry.ChangedByUser += ViewModel.OnCounterpartyViewModelEntryChangedByUser;

			orderViewModelEntry.SetEntityAutocompleteSelectorFactory(ViewModel.OrderSelectorFactory);
			orderViewModelEntry.CanEditReference = true;
			orderViewModelEntry.Binding.AddBinding(ViewModel.Entity, vm => vm.Order, w => w.Subject).InitializeFromSource();

			/*
			subdivisionVMEntry.SetEntityAutocompleteSelectorFactory(ViewModel.SubdivisionSelectorFactory);
			subdivisionVMEntry.CanEditReference = true;
			subdivisionVMEntry.Binding.AddBinding(ViewModel.Entity, vm => vm.Subdivision, w => w.Subject).InitializeFromSource();
			*/

			var docCommentView = new DocumentCommentView(ViewModel.Entity, ViewModel.employeeRepository, uow);
			vboxComments.Add(docCommentView);
			docCommentView.Show();
		}
	}
}
