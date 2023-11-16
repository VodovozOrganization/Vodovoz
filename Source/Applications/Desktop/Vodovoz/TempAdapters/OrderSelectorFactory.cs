using Autofac;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
	public class OrderSelectorFactory : IOrderSelectorFactory
	{
		private readonly ILifetimeScope _lifetimeScope;
		private OrderJournalFilterViewModel _orderJournalFilter;

		public OrderSelectorFactory(
			ILifetimeScope lifetimeScope,
			OrderJournalFilterViewModel orderFilter = null)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_orderJournalFilter = orderFilter;
		}

		public IEntitySelector CreateOrderSelectorForDocument(bool IsOnlineStoreOrders, IEnumerable<OrderStatus> orderStatuses)
		{
			var orderFilter = new OrderForMovDocJournalFilterViewModel();
			orderFilter.IsOnlineStoreOrders = IsOnlineStoreOrders;
			orderFilter.OrderStatuses = orderStatuses;
			
			var viewModel =	 _lifetimeScope.Resolve<OrderForMovDocJournalViewModel>(new TypedParameter(typeof(OrderForMovDocJournalFilterViewModel), orderFilter));

			viewModel.SelectionMode = JournalSelectionMode.Multiple;

			return viewModel;
		}

		public IEntityAutocompleteSelectorFactory CreateOrderAutocompleteSelectorFactory(OrderJournalFilterViewModel filterViewModel = null)
		{
			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() => CreateOrderJournalViewModel(filterViewModel)
			);
		}

		public IEntityAutocompleteSelectorFactory CreateCashSelfDeliveryOrderAutocompleteSelector()
		{
			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					Action<OrderJournalFilterViewModel> filterConfig = (filter) =>
					{
						filter.RestrictStatus = OrderStatus.WaitForPayment;
						filter.AllowPaymentTypes = new[] { PaymentType.Cash };
						filter.RestrictOnlySelfDelivery = true;
						filter.RestrictWithoutSelfDelivery = false;
						filter.RestrictHideService = true;
						filter.RestrictOnlyService = false;
					};

					return _lifetimeScope.Resolve<OrderJournalViewModel>(new TypedParameter(typeof(Action<OrderJournalFilterViewModel>), filterConfig));
				});
		}

		public IEntityAutocompleteSelectorFactory CreateSelfDeliveryDocumentOrderAutocompleteSelector()
		{
			Action<OrderJournalFilterViewModel> filterConfig = (filter) =>
			{
				filter.RestrictOnlySelfDelivery = true;
				filter.RestrictStatus = OrderStatus.OnLoading;
			};

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					return _lifetimeScope.Resolve<OrderJournalViewModel>(new TypedParameter(typeof(Action<OrderJournalFilterViewModel>), filterConfig));
				});
		}

		public OrderJournalViewModel CreateOrderJournalViewModel(OrderJournalFilterViewModel filterViewModel = null)
		{
			_orderJournalFilter = filterViewModel ?? _orderJournalFilter ?? _lifetimeScope.Resolve<OrderJournalFilterViewModel>();

			return _lifetimeScope.Resolve<OrderJournalViewModel>(new TypedParameter(typeof(OrderJournalFilterViewModel), _orderJournalFilter));
		}
	}
}
