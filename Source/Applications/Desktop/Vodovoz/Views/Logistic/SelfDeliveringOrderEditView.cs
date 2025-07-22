using FluentNHibernate.Data;
using QS.Views.GtkUI;
using QSWidgetLib;
using ReactiveUI;
using System.Collections.Generic;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Logistic;
namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelfDeliveringOrderEditView : TabViewBase<SelfDeliveringOrderEditViewModel>
	{
		public SelfDeliveringOrderEditView(SelfDeliveringOrderEditViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ybuttonSave.BindCommand(ViewModel.SaveCommand);
			ybuttonCancel.BindCommand(ViewModel.CloseCommand);
			
			entityVMEntryClient1.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyAutocompleteSelectorFactory);
			entityVMEntryClient1.Binding.AddBinding(ViewModel.Entity, s => s.Client, w => w.Subject).InitializeFromSource();
			//Проверку сделать?
			//entityVMEntryClient1.CanEditReference = !ViewModel.UserHasOnlyAccessToWarehouseAndComplaints;

			ycheckbuttonPaymentAfterShipment.Binding.AddBinding(ViewModel.Entity, e => e.PayAfterShipment, w => w.Active)
				.InitializeFromSource();
			//Не Работает
			//specialListCmbSelfDeliveryGeoGroup.Binding.AddBinding(ViewModel.Entity, e => e.SelfDeliveryGeoGroup, w => w.SelectedItem)
			//	.InitializeFromSource();

			specialListCmbSelfDeliveryGeoGroup.ItemsList = ViewModel.GetSelfDeliveryGeoGroups();
			specialListCmbSelfDeliveryGeoGroup.Binding
				.AddBinding(ViewModel.Entity, e => e.SelfDeliveryGeoGroup, w => w.SelectedItem)
				.AddBinding(e => e.SelfDelivery, w => w.Visible)
				.InitializeFromSource();

			yentryPaymentType.Binding.AddBinding(ViewModel.Entity, e => e.PaymentType, w => w.Text)
				.InitializeFromSource();

			buttonSelectPaymentType.BindCommand(ViewModel.PaymentTypeCommand);

			// Не Работает
			treeItems.ItemsDataSource = ViewModel.Entity.ObservableOrderItems;
            treeItems.Binding.AddBinding(ViewModel.Entity, 
                e => e.OrderItems,
                w => w.ItemsDataSource)
                .InitializeFromSource();

            // Переделать под yEntryPaymentNumber
            /*yEntryPaymentNumber.ValidationMode = (QS.Widgets.ValidationType)ValidationType.numeric;
			yEntryPaymentNumber.Binding.AddBinding(ViewModel.Entity,
				e => e.OnlinePaymentNumber,
				w => w.Text,
				new NullableIntToStringConverter()).InitializeFromSource();*/
            //ylabelPaymentNumber.Binding.AddBinding(ViewModel, vm => vm.Entity.OnlinePaymentNumber, w => w.Text)
            //	.InitializeFromSource();
        }
	}
}
