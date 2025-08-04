using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Оценки заказов",
		Nominative = "Оценка заказа",
		Prepositional = "Оценке заказа",
		PrepositionalPlural = "Оценках заказов"
	)]
	[HistoryTrace]
	public class OrderRating : PropertyChangedBase, IDomainObject
	{
		private OnlineOrder _onlineOrder;
		private Order _order;
		private DateTime _created;
		private Source _source;
		private OrderRatingStatus _orderRatingStatus;
		private string _comment;
		private Employee _processedByEmployee;
		private int _rating;
		private IList<OrderRatingReason> _orderRatingReasons = new List<OrderRatingReason>();
		
		public virtual int Id { get; set; }
		
		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
		
		[Display(Name = "Онлайн заказ")]
		public virtual OnlineOrder OnlineOrder
		{
			get => _onlineOrder;
			set => SetField(ref _onlineOrder, value);
		}
		
		[Display(Name = "Дата создания")]
		public virtual DateTime Created
		{
			get => _created;
			set => SetField(ref _created, value);
		}
		
		[Display(Name = "Источник оценки")]
		public virtual Source Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}
		
		[Display(Name = "Статус оценки заказа")]
		public virtual OrderRatingStatus OrderRatingStatus
		{
			get => _orderRatingStatus;
			set => SetField(ref _orderRatingStatus, value);
		}
		
		[Display(Name = "Кто обработал оценку")]
		public virtual Employee ProcessedByEmployee
		{
			get => _processedByEmployee;
			set => SetField(ref _processedByEmployee, value);
		}
		
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}
		
		[Display(Name = "Оценка заказа")]
		public virtual int Rating
		{
			get => _rating;
			set => SetField(ref _rating, value);
		}
		
		[Display(Name = "Причины оценки заказа")]
		public virtual IList<OrderRatingReason> OrderRatingReasons
		{
			get => _orderRatingReasons;
			set => SetField(ref _orderRatingReasons, value);
		}

		public virtual void Process(Employee employee)
		{
			OrderRatingStatus = OrderRatingStatus.Processed;
			ProcessedByEmployee = employee;
		}

		public override string ToString()
		{
			if(Id > 0)
			{
				if(Order != null)
				{
					return $"Оценка №{Id} заказа {Order.Id}";
				}

				return OnlineOrder != null ? $"Оценка №{Id} онлайн заказа {OnlineOrder.Id}" : "Оценка неизвестного заказа";
			}
			
			return "Новая оценка";
		}

		public static OrderRating Create(
			Source source,
			int rating,
			string comment,
			int? onlineOrderId,
			int? orderId,
			IEnumerable<int> orderRatingReasonsIds,
			int negativeRating)
		{
			var orderRating = new OrderRating
			{
				Source = source,
				Rating = rating,
				Created = DateTime.Now,
				Comment = comment,
			};

			if(onlineOrderId.HasValue)
			{
				orderRating.OnlineOrder = new OnlineOrder
				{
					Id = onlineOrderId.Value
				};
			}
			
			if(orderId.HasValue)
			{
				orderRating.Order = new Order
				{
					Id = orderId.Value
				};
			}

			orderRating.OrderRatingStatus = orderRating.Rating > negativeRating
				? OrderRatingStatus.Positive
				: OrderRatingStatus.New;

			if(orderRatingReasonsIds != null && orderRatingReasonsIds.Any())
			{
				foreach(var orderRatingReasonId in orderRatingReasonsIds)
				{
					orderRating.OrderRatingReasons.Add(new OrderRatingReason
					{
						Id = orderRatingReasonId
					});
				}
			}

			return orderRating;
		}
	}
}
