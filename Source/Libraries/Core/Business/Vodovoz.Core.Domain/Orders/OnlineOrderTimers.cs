using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Настройки временных интервалов для онлайн заказа
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "таймеры онлайн заказов",
		Nominative = "таймеры онлайн заказа",
		Prepositional = "таймерах онлайн заказа",
		PrepositionalPlural = "таймерах онлайн заказов"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class OnlineOrderTimers : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private TimeSpan _payTimeWithoutFastDelivery;
		private TimeSpan _timeForTransferToManualProcessingWithoutFastDelivery;
		private TimeSpan _payTimeWithFastDelivery;
		private TimeSpan _timeForTransferToManualProcessingWithFastDelivery;
		
		public virtual int Id { get; set; }

		/// <summary>
		/// Время для оплаты заказа клиентом без доставки за час
		/// </summary>
		[Display(Name = "Время для оплаты заказа клиентом без доставки за час")]
		public virtual TimeSpan PayTimeWithoutFastDelivery
		{
			get => _payTimeWithoutFastDelivery;
			set => SetField(ref _payTimeWithoutFastDelivery, value);
		}

		/// <summary>
		/// Время для переноса заказа на ручную обработку без доставки за час
		/// </summary>
		[Display(Name = "Время для переноса заказа на ручную обработку без доставки за час")]
		public virtual TimeSpan TimeForTransferToManualProcessingWithoutFastDelivery
		{
			get => _timeForTransferToManualProcessingWithoutFastDelivery;
			set => SetField(ref _timeForTransferToManualProcessingWithoutFastDelivery, value);
		}

		/// <summary>
		/// Время для оплаты заказа клиентом с доставкой за час
		/// </summary>
		[Display(Name = "Время для оплаты заказа клиентом с доставкой за час")]
		public virtual TimeSpan PayTimeWithFastDelivery
		{
			get => _payTimeWithFastDelivery;
			set => SetField(ref _payTimeWithFastDelivery, value);
		}
		
		/// <summary>
		/// Время для переноса заказа на ручную обработку с доставкой за час
		/// </summary>
		[Display(Name = "Время для переноса заказа на ручную обработку с доставкой за час")]
		public virtual TimeSpan TimeForTransferToManualProcessingWithFastDelivery
		{
			get => _timeForTransferToManualProcessingWithFastDelivery;
			set => SetField(ref _timeForTransferToManualProcessingWithFastDelivery, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(TimeForTransferToManualProcessingWithoutFastDelivery <= PayTimeWithoutFastDelivery)
			{
				yield return new ValidationResult(
					"Время для переноса заказа на ручную обработку без доставки за час не может быть равным" +
					" или меньше времени для оплаты заказа клиентом без доставки за час");
			}
			
			if(TimeForTransferToManualProcessingWithFastDelivery <= PayTimeWithFastDelivery)
			{
				yield return new ValidationResult(
					"Время для переноса заказа на ручную обработку с доставкой за час не может быть равным" +
					" или меньше времени для оплаты заказа клиентом с доставкой за час");
			}
		}
	}
}
