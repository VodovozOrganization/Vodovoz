using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.Dialogs.Email;
using Vodovoz.Infrastructure.Converters;

namespace Vodovoz.Views.Orders.OrdersWithoutShipment
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderWithoutShipmentForDebtView : TabViewBase<OrderWithoutShipmentForDebtViewModel>
	{
		public OrderWithoutShipmentForDebtView(OrderWithoutShipmentForDebtViewModel viewModel) : base(viewModel)
		{
			this.Build();
			
			Configure();
		}

		private void Configure()
		{
			btnCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();
			ybtnOpenBill.Clicked += (sender, e) => ViewModel.OpenBillCommand.Execute();

			ylabelOrderNum.Binding.AddBinding(ViewModel.Entity, e => e.Id, w => w.Text, new IntToStringConverter()).InitializeFromSource();
			yentryDebtName.Binding.AddBinding(ViewModel.Entity, e => e.DebtName, w => w.Text).InitializeFromSource();
			yspinbtnDebtSum.Binding.AddBinding(ViewModel.Entity, e => e.DebtSum, v => v.ValueAsDecimal).InitializeFromSource();
			ylabelOrderDate.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text).InitializeFromSource();
			ylabelOrderAuthor.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text).InitializeFromSource();
			yCheckBtnHideSignature.Binding.AddBinding(ViewModel.Entity, e => e.HideSignature, w => w.Active).InitializeFromSource();

			entityViewModelEntryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory());

			entityViewModelEntryCounterparty.Changed += ViewModel.OnEntityViewModelEntryChanged;

			entityViewModelEntryCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.Client, w => w.Subject).InitializeFromSource();
			entityViewModelEntryCounterparty.Binding.AddFuncBinding(ViewModel, vm => !vm.IsDocumentSent, w => w.Sensitive).InitializeFromSource();
			entityViewModelEntryCounterparty.CanEditReference = true;
			
			var sendEmailView = new SendDocumentByEmailView(ViewModel.SendDocViewModel);
			hbox7.Add(sendEmailView);
			sendEmailView.Show();

			ViewModel.OpenCounterpartyJournal += entityViewModelEntryCounterparty.OpenSelectDialog;
		}
		
		public override void Destroy()
		{
			entityViewModelEntryCounterparty.Changed -= ViewModel.OnEntityViewModelEntryChanged;
			
			base.Destroy();
		}
	}
}
