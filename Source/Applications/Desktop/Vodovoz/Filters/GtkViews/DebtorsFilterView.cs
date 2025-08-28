using QS.Views.GtkUI;
using QS.Widgets;
using System;
using System.ComponentModel;
using Gtk;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Key = Gdk.Key;

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

			entryNomenclature.ViewModel = ViewModel.NomenclatureViewModel;

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

			entrySalesManager.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ManagerSelectorFactory, w => w.EntitySelectorAutocompleteFactory)
				.AddBinding(vm => vm.SalesManager, w => w.Subject)
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

			ycheckbuttonHideExcludedFromAutoCalls.Binding
				.AddBinding(ViewModel, vm => vm.HideExcludeFromAutoCalls, w => w.Active)
				.InitializeFromSource();
			
			yvalidatedentryFixPriceFrom.Binding
            	.AddBinding(ViewModel, x => x.FixPriceFrom, x => x.Text, new NullableDecimalToStringConverter())
            	.InitializeFromSource();

			yvalidatedentryFixPriceFrom.KeyReleaseEvent += OnKeyReleased;

			yvalidatedentryFixPriceTo.Binding
            	.AddBinding(ViewModel, x => x.FixPriceTo, x => x.Text, new NullableDecimalToStringConverter())
            	.InitializeFromSource();
			
			yvalidatedentryFixPriceTo.KeyReleaseEvent += OnKeyReleased;
		}

		private void OnKeyReleased(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
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

		public override void Dispose()
		{
			yvalidatedentryFixPriceFrom.KeyReleaseEvent -= OnKeyReleased;
			yvalidatedentryFixPriceTo.KeyReleaseEvent -= OnKeyReleased;
			
			base.Dispose();
		}
	}
}
