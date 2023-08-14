using QS.Views.GtkUI;
using QS.Widgets;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class DebtorsFilterView : FilterViewBase<DebtorsJournalFilterViewModel>
	{
		public DebtorsFilterView(DebtorsJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			entryreferenceClient.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entityVMEntryDeliveryPoint.SetEntityAutocompleteSelectorFactory(ViewModel.DeliveryPointSelectorFactory);
			entityviewmodelentryNomenclature.SetEntityAutocompleteSelectorFactory(ViewModel.NomenclatureSelectorFactory);

			yvalidatedentryDebtTo.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryDebtFrom.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryBottlesTo.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryBottlesFrom.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryDeliveryPointsTo.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryDeliveryPointsFrom.ValidationMode = QSWidgetLib.ValidationType.numeric;

			yenumcomboboxOPF.ItemsEnum = typeof(PersonType);
			yenumcomboboxHasTask.ItemsEnum = typeof(DebtorsTaskStatus);

			ycomboboxReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			ycomboboxReason.ItemsList = ViewModel.UoW?.Session.QueryOver<DiscountReason>().List();

			entryreferenceClient.Binding
				.AddBinding(ViewModel, x => x.Client, x => x.Subject)
				.InitializeFromSource();

			entityVMEntryDeliveryPoint.Binding
				.AddBinding(ViewModel, x => x.Address, x => x.Subject)
				.InitializeFromSource();

			entityviewmodelentryNomenclature.Binding
				.AddBinding(ViewModel, x => x.LastOrderNomenclature, x => x.Subject)
				.InitializeFromSource();

			yvalidatedentryDebtTo.Binding
				.AddBinding(ViewModel, x => x.DebtBottlesTo, x => x.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			yvalidatedentryDebtFrom.Binding
				.AddBinding(ViewModel, x => x.DebtBottlesFrom, x => x.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			yvalidatedentryBottlesTo.Binding
				.AddBinding(ViewModel, x => x.LastOrderBottlesTo, x => x.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			yvalidatedentryBottlesFrom.Binding
				.AddBinding(ViewModel, x => x.LastOrderBottlesFrom, x => x.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			yvalidatedentryDeliveryPointsTo.Binding
				.AddBinding(ViewModel, x => x.DeliveryPointsTo, x => x.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			yvalidatedentryDeliveryPointsFrom.Binding
				.AddBinding(ViewModel, x => x.DeliveryPointsFrom, x => x.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			yenumcomboboxOPF.Binding
				.AddBinding(ViewModel, x => x.OPF, x => x.SelectedItemOrNull)
				.InitializeFromSource();

			ycomboboxReason.Binding
				.AddBinding(ViewModel, x => x.DiscountReason, x => x.SelectedItem)
				.InitializeFromSource();

			ydateperiodpickerLastOrder.Binding
				.AddSource(ViewModel)
				.AddBinding(x => x.StartDate, x => x.StartDateOrNull)
				.AddBinding(x => x.EndDate, x => x.EndDateOrNull)
				.InitializeFromSource();

			ycheckbuttonHideActive.Binding
				.AddSource(ViewModel)
				.AddBinding(x => x.HideActiveCounterparty, x => x.Active)
				.AddFuncBinding(
					x => x.ShowHideActiveCheck && !x.ShowCancellationCounterparty && !x.ShowSuspendedCounterparty,
					x => x.Visible)
				.InitializeFromSource();

			nullablecheckOneOrder.RenderMode = RenderMode.Icon;
			nullablecheckOneOrder.Binding
				.AddBinding(ViewModel, vm => vm.WithOneOrder, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonHideWithoutEmail.Binding
				.AddSource(ViewModel)
				.AddBinding(x => x.HideWithoutEmail, x => x.Active)
				.InitializeFromSource();

			ycheckbuttonHideWithoutFixedPrices.Binding
				.AddSource(ViewModel)
				.AddBinding(x => x.HideWithoutFixedPrices, x => x.Active)
				.InitializeFromSource();

			ycheckbuttonShowSuspended.Binding
				.AddSource(ViewModel)
				.AddBinding(x => x.ShowSuspendedCounterparty, x => x.Active)
				.AddFuncBinding(x => !x.ShowCancellationCounterparty && !x.HideActiveCounterparty, x => x.Visible)
				.InitializeFromSource();

			ycheckbuttonShowCancellation.Binding
				.AddSource(ViewModel)
				.AddBinding(x => x.ShowCancellationCounterparty, x => x.Active)
				.AddFuncBinding(x => !x.ShowSuspendedCounterparty && !x.HideActiveCounterparty, x => x.Visible)
				.InitializeFromSource();

			listDeliveryPointCategories.ItemsList = ViewModel.DeliveryPointCategories;
			listDeliveryPointCategories.Binding
				.AddBinding(ViewModel, vm => vm.SelectedDeliveryPointCategory, w => w.SelectedItem)
				.InitializeFromSource();

			yenumcomboboxHasTask.Binding
				.AddBinding(ViewModel, x => x.DebtorsTaskStatus, x => x.SelectedItemOrNull)
				.InitializeFromSource();
		}

		protected void OnEntryreferenceClientChanged(object sender, EventArgs e)
		{
			if(ViewModel?.Address?.Counterparty?.Id != ViewModel?.Client?.Id)
			{
				ViewModel.Address = null;
			}

			if(ViewModel?.DeliveryPointJournalFilterViewModel != null)
			{
				ViewModel.DeliveryPointJournalFilterViewModel.Counterparty = ViewModel.Client;
			}
		}
	}
}
