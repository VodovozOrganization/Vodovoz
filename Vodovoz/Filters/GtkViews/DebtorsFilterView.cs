using System;
using QS.Views.GtkUI;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DebtorsFilterView : FilterViewBase<DebtorsJournalFilterViewModel>
	{
		public DebtorsFilterView(DebtorsJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entryreferenceClient.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyVM);
			entryreferenceDeliveryPoint.RepresentationModel = ViewModel.DeliveryPointVM;
			entityviewmodelentryNomenclature.SetEntityAutocompleteSelectorFactory(ViewModel.NomenclatureVM);

			yvalidatedentryDebtTo.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryDebtFrom.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryBottlesTo.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryBottlesFrom.ValidationMode = QSWidgetLib.ValidationType.numeric;

			yenumcomboboxOPF.ItemsEnum = typeof(PersonType);

			ycomboboxReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			ycomboboxReason.ItemsList = ViewModel.UoW?.Session.QueryOver<DiscountReason>().List();

			entryreferenceClient.Binding.AddBinding(ViewModel, x => x.Client, x => x.Subject).InitializeFromSource();
			entryreferenceDeliveryPoint.Binding.AddBinding(ViewModel, x => x.Address, x => x.Subject).InitializeFromSource();
			entityviewmodelentryNomenclature.Binding.AddBinding(ViewModel, x => x.LastOrderNomenclature, x => x.Subject).InitializeFromSource();
			yvalidatedentryDebtTo.Binding.AddBinding(ViewModel, x => x.DebtBottlesTo, x => x.Text, new IntToStringConverter()).InitializeFromSource();
			yvalidatedentryDebtFrom.Binding.AddBinding(ViewModel, x => x.DebtBottlesFrom, x => x.Text, new IntToStringConverter()).InitializeFromSource();
			yvalidatedentryBottlesTo.Binding.AddBinding(ViewModel, x => x.LastOrderBottlesTo, x => x.Text, new IntToStringConverter()).InitializeFromSource();
			yvalidatedentryBottlesFrom.Binding.AddBinding(ViewModel, x => x.LastOrderBottlesFrom, x => x.Text, new IntToStringConverter()).InitializeFromSource();
			yenumcomboboxOPF.Binding.AddBinding(ViewModel, x => x.OPF, x => x.SelectedItemOrNull).InitializeFromSource();
			ycomboboxReason.Binding.AddBinding(ViewModel, x => x.DiscountReason, x => x.SelectedItem).InitializeFromSource();
			ydateperiodpickerLastOrder.Binding.AddBinding(ViewModel, x => x.StartDate, x => x.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerLastOrder.Binding.AddBinding(ViewModel, x => x.EndDate, x => x.EndDateOrNull).InitializeFromSource();
			ycheckbuttonHideActive.Binding.AddBinding(ViewModel, x => x.HideActiveCounterparty, x => x.Active).InitializeFromSource();
			ycheckbuttonHideActive.Binding.AddBinding(ViewModel, x => x.ShowHideActiveCheck, x => x.Visible).InitializeFromSource();
		}

		protected void OnEntryreferenceClientChanged(object sender, EventArgs e)
		{
			if(ViewModel?.Address?.Counterparty?.Id != ViewModel.Client?.Id)
				ViewModel.Address = null;
			if(ViewModel.DeliveryPointVM.JournalFilter is DeliveryPointFilter clientFilter)
				clientFilter.Client = ViewModel.Client;
		}
	}
}
