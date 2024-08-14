using System;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersRatingsJournalFilterView : FilterViewBase<OrdersRatingsJournalFilterViewModel>
	{
		public OrdersRatingsJournalFilterView(OrdersRatingsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			entryOrderRatingId.KeyReleaseEvent += UpdateFilter;
			entryOrderRatingId.Binding
				.AddBinding(ViewModel, vm => vm.OrderRatingId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			
			orderRatingCmb.ShowSpecialStateAll = true;
			orderRatingCmb.ItemsEnum = typeof(OrderRatingStatus);
			orderRatingCmb.Binding
				.AddBinding(ViewModel, vm => vm.OrderRatingStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			sourceCmb.ShowSpecialStateAll = true;
			sourceCmb.ItemsEnum = typeof(Source);
			sourceCmb.Binding
				.AddBinding(ViewModel, vm => vm.OrderRatingSource, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			entryOnlineOrder.KeyReleaseEvent += UpdateFilter;
			entryOnlineOrder.Binding
				.AddBinding(ViewModel, vm => vm.OnlineOrderId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			
			entryOrder.KeyReleaseEvent += UpdateFilter;
			entryOrder.Binding
				.AddBinding(ViewModel, vm => vm.OrderId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			
			creationDateRangePicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			creationDateRangePicker.PeriodChangedByUser += OnDateEventPeriodChangedByUser;

			entryOrderRatingReason.KeyReleaseEvent += UpdateFilter;
			entryOrderRatingReason.Binding
				.AddBinding(ViewModel, vm => vm.OrderRatingReason, w => w.Text)
				.InitializeFromSource();
			
			enumCmbRatingCriterion.ItemsEnum = typeof(ComparisonSings);
			enumCmbRatingCriterion.Binding
				.AddBinding(ViewModel, vm => vm.RatingCriterion, w => w.SelectedItem)
				.InitializeFromSource();
			
			entryRating.Binding
				.AddBinding(ViewModel, vm => vm.OrderRatingValue, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
		}
		
		private void OnDateEventPeriodChangedByUser(object sender, EventArgs e)
		{
			ViewModel.Update();
		}

		private void UpdateFilter(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Return)
			{
				ViewModel.Update();
			}
		}

		public override void Destroy()
		{
			entryOrderRatingId.KeyReleaseEvent -= UpdateFilter;
			entryOnlineOrder.KeyReleaseEvent -= UpdateFilter;
			entryOrder.KeyReleaseEvent -= UpdateFilter;
			creationDateRangePicker.PeriodChangedByUser -= OnDateEventPeriodChangedByUser;
			entryOrderRatingReason.KeyReleaseEvent -= UpdateFilter;

			base.Destroy();
		}
	}
}
