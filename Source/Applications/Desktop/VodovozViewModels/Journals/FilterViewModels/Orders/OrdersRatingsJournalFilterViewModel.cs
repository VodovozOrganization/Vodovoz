using System;
using QS.Project.Filter;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class OrdersRatingsJournalFilterViewModel : FilterViewModelBase<OrdersRatingsJournalFilterViewModel>
	{
		private OrderRatingStatus? _orderRatingStatus;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Core.Domain.Clients.Source? _orderRatingSource;
		private int? _orderRatingId;
		private int? _onlineOrderId;
		private int? _orderId;
		private string _orderRatingReason;
		private int? _orderRatingValue;
		private ComparisonSings _ratingCriterion = ComparisonSings.LessOrEqual;

		public OrderRatingStatus? OrderRatingStatus
		{
			get => _orderRatingStatus;
			set => UpdateFilterField(ref _orderRatingStatus, value);
		}
		
		public DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}
		
		public DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}
		
		public Core.Domain.Clients.Source? OrderRatingSource
		{
			get => _orderRatingSource;
			set => UpdateFilterField(ref _orderRatingSource, value);
		}
		
		public int? OrderRatingId
		{
			get => _orderRatingId;
			set => _orderRatingId = value;
		}

		public int? OnlineOrderId
		{
			get => _onlineOrderId;
			set => _onlineOrderId = value;
		}
		
		public int? OrderId
		{
			get => _orderId;
			set => _orderId = value;
		}

		public string OrderRatingReason
		{
			get => _orderRatingReason;
			set => _orderRatingReason = value;
		}
		
		public int? OrderRatingValue
		{
			get => _orderRatingValue;
			set => _orderRatingValue = value;
		}
		
		public ComparisonSings RatingCriterion
		{
			get => _ratingCriterion;
			set => UpdateFilterField(ref _ratingCriterion, value);
		}
	}
}
