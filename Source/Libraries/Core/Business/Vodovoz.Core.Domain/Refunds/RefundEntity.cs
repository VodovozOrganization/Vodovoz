using Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Repositories;

namespace Vodovoz.Core.Domain.Refunds
{
	public class RefundEntity : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private OrderEntity _order;
		private OrderEntity _orderOnline;
		private DateTime _date;
		public virtual int Id { get; set; }

		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Онлайн заказ")]
		public virtual OrderEntity OrderOnline
		{
			get => _orderOnline;
			set => SetField(ref _orderOnline, value);
		}

		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			var profitCategoryRepository = validationContext.GetRequiredService<IGenericRepository<ProfitCategory>>();

			if(Order == null)
			{
				yield return new ValidationResult("Заказ должен быть заполнен");
			}
			if(OrderOnline == null)
			{
				yield return new ValidationResult("Онлайн заказ должен быть заполнен");
			}
			if(Date == DateTime.MinValue)
			{
				yield return new ValidationResult("Дата возврата должна быть заполнена");
			}
		}
	}
}
