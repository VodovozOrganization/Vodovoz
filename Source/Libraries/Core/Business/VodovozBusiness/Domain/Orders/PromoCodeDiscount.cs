using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Промокоды",
		Nominative = "Промокод",
		GenitivePlural = "Промокодов")]
	public class PromoCodeDiscount : DiscountReasonBase
	{
		public const int OrderMinSumLimit = 1_000_000_000;
		private const int _promoCodeNameLimit = 15;
		
		private string _promoCodeName;
		private bool _isOneTimePromoCode;
		private decimal _orderMinSum;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private TimeSpan? _startTime;
		private TimeSpan? _endTime;
		
		/// <summary>
		/// Промокод
		/// </summary>
		[Display(Name = "Промокод")]
		public virtual string PromoCodeName
		{
			get => _promoCodeName;
			set => SetField(ref _promoCodeName, value);
		}
		
		/// <summary>
		/// Одноразовый промокод
		/// </summary>
		[Display(Name = "Одноразовый промокод")]
		public virtual bool IsOneTimePromoCode
		{
			get => _isOneTimePromoCode;
			set => SetField(ref _isOneTimePromoCode, value);
		}
		
		/// <summary>
		/// Минимальная сумма заказа для применения промокода
		/// </summary>
		[Display(Name = "Минимальная сумма заказа")]
		public virtual decimal OrderMinSum
		{
			get => _orderMinSum;
			set => SetField(ref _orderMinSum, value);
		}
		
		/// <summary>
		/// Начальная дата действия промокода
		/// </summary>
		[Display(Name = "Начальная дата действия промокода")]
		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}
		
		/// <summary>
		/// Конечная дата действия промокода
		/// </summary>
		[Display(Name = "Конечная дата действия промокода")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}
		
		/// <summary>
		/// Начальное время действия промокода
		/// </summary>
		[Display(Name = "Начальное время действия промокода")]
		public virtual TimeSpan? StartTime
		{
			get => _startTime;
			set => SetField(ref _startTime, value);
		}
		
		/// <summary>
		/// Конечное время действия промокода
		/// </summary>
		[Display(Name = "Конечное время действия промокода")]
		public virtual TimeSpan? EndTime
		{
			get => _endTime;
			set => SetField(ref _endTime, value);
		}
		
		public override DiscountReasonType DiscountReasonType => DiscountReasonType.PromoCode;

		public virtual bool HasPromoCodeDurationTime => _startTime.HasValue || _endTime.HasValue;
		public virtual bool HasOrderMinSum => OrderMinSum > 0;
		
		public virtual string StartTimePromoCodeString => StartTime.HasValue
			? $"{StartTime.Value:hh\\:mm}"
			: string.Empty;
		
		public virtual string EndTimePromoCodeString => EndTime.HasValue
			? $"{EndTime.Value:hh\\:mm}"
			: string.Empty;
		
		public virtual void ResetOrderMinSum()
		{
			OrderMinSum = 0;
		}
		
		public virtual void ResetTimeDuration()
		{
			StartTime = null;
			EndTime = null;
		}

		//TODO надо сделать отдельный валидатор под сущность
		public virtual IEnumerable<ValidationResult> Validate()
		{
			if(string.IsNullOrEmpty(PromoCodeName))
			{
				yield return new ValidationResult("Название промокода должно быть заполнено", new[] { nameof(PromoCodeName) });
			}

			if(PromoCodeName?.Length > _promoCodeNameLimit)
			{
				yield return new ValidationResult(
					$"Превышена длина названия промокода на {PromoCodeName.Length}-{_promoCodeNameLimit}",
					new[] { nameof(PromoCodeName) });
			}

			if(!StartDate.HasValue)
			{
				yield return new ValidationResult(
					"Не заполнена начальная дата действия промокода",
					new[] { nameof(StartDate) });
			}

			if(!EndDate.HasValue)
			{
				yield return new ValidationResult(
					"Не заполнена конечная дата действия промокода",
					new[] { nameof(EndDate) });
			}

			/*using(var uow =
				validationContext.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("Проверка промокода на дубли"))
			{

				if(discountRepository.ExistsPromoCodeWithName(uow, Id, PromoCodeName, out var duplicatePromoCode))
				{
					var archived = duplicatePromoCode.IsArchive ? "архивный" : null;
					yield return new ValidationResult(
						$"Уже есть созданный {archived} промокод {duplicatePromoCode.Id} {duplicatePromoCode.Name}",
						new[] { nameof(PromoCodeName) });
				}
			}*/
		}
		
		public static PromoCodeDiscount Create(DiscountReasonBase copyingDiscount)
		{
			var newDiscount = new PromoCodeDiscount();
			newDiscount.Copy(copyingDiscount);
			
			return newDiscount;
		}
	}
}
