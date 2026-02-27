using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.DiscountReasons;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "основания скидок",
		Nominative = "основание скидки",
		GenitivePlural = "оснований скидок")]
	[EntityPermission]
	[HistoryTrace]
	public class DiscountReason : PropertyChangedBase, IDomainObject, IValidatableObject, INamed, IArchivable
	{
		public const int PromoCodeOrderMinSumLimit = 1_000_000_000;
		
		private const int _percentsLimit = 100;
		private const int _nameLimit = 45;
		private const int _promoCodeNameLimit = 15;
		private string _name;
		private bool _isArchive;
		private bool _isPremiumDiscount;
		private bool _isPresent;
		private DiscountUnits _valueType;
		private decimal _value;
		private IList<DiscountReasonNomenclatureCategory> _nomenclatureCategories = new List<DiscountReasonNomenclatureCategory>();
		private IList<Nomenclature> _nomenclatures = new List<Nomenclature>();
		private IList<ProductGroup> _productGroups = new List<ProductGroup>();
		private GenericObservableList<DiscountReasonNomenclatureCategory> _observableNomenclatureCategories;
		private GenericObservableList<Nomenclature> _observableNomenclatures;
		private GenericObservableList<ProductGroup> _observableProductGroups;
		private bool _isPromoCode;
		private string _promoCodeName;
		private bool _isOneTimePromoCode;
		private decimal _promoCodeOrderMinSum;
		private DateTime? _startDatePromoCode;
		private DateTime? _endDatePromoCode;
		private TimeSpan? _startTimePromoCode;
		private TimeSpan? _endTimePromoCode;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "В архиве")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Тип значения скидки")]
		public virtual DiscountUnits ValueType
		{
			get => _valueType;
			set => SetField(ref _valueType, value);
		}

		[Display(Name = "Значение скидки")]
		public virtual decimal Value
		{
			get => _value;
			set => SetField(ref _value, value);
		}
		
		[Display(Name = "Премиальная скидка?")]
		public virtual bool IsPremiumDiscount
		{
			get => _isPremiumDiscount;
			set => SetField(ref _isPremiumDiscount, value);
		}

		[Display(Name = "Подарок?")]
		public virtual bool IsPresent
		{
			get => _isPresent;
			set => SetField(ref _isPresent, value);
		}

		public virtual IList<DiscountReasonNomenclatureCategory> NomenclatureCategories
		{
			get => _nomenclatureCategories;
			set => SetField(ref _nomenclatureCategories, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DiscountReasonNomenclatureCategory> ObservableNomenclatureCategories =>
			_observableNomenclatureCategories ??
			(_observableNomenclatureCategories = new GenericObservableList<DiscountReasonNomenclatureCategory>(NomenclatureCategories));
		
		public virtual IList<Nomenclature> Nomenclatures
		{
			get => _nomenclatures;
			set => SetField(ref _nomenclatures, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Nomenclature> ObservableNomenclatures =>
			_observableNomenclatures ??
			(_observableNomenclatures = new GenericObservableList<Nomenclature>(Nomenclatures));

		public virtual IList<ProductGroup> ProductGroups
		{
			get => _productGroups;
			set => SetField(ref _productGroups, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ProductGroup> ObservableProductGroups =>
			_observableProductGroups ??
			(_observableProductGroups = new GenericObservableList<ProductGroup>(ProductGroups));
		
		/// <summary>
		/// Основание скидки - Промокод
		/// </summary>
		[Display(Name = "Основание скидки - Промокод")]
		public virtual bool IsPromoCode
		{
			get => _isPromoCode;
			set => SetField(ref _isPromoCode, value);
		}
		
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
		public virtual decimal PromoCodeOrderMinSum
		{
			get => _promoCodeOrderMinSum;
			set => SetField(ref _promoCodeOrderMinSum, value);
		}
		
		/// <summary>
		/// Начальная дата действия промокода
		/// </summary>
		[Display(Name = "Начальная дата действия промокода")]
		public virtual DateTime? StartDatePromoCode
		{
			get => _startDatePromoCode;
			set => SetField(ref _startDatePromoCode, value);
		}
		
		/// <summary>
		/// Конечная дата действия промокода
		/// </summary>
		[Display(Name = "Конечная дата действия промокода")]
		public virtual DateTime? EndDatePromoCode
		{
			get => _endDatePromoCode;
			set => SetField(ref _endDatePromoCode, value);
		}
		
		/// <summary>
		/// Начальное время действия промокода
		/// </summary>
		[Display(Name = "Начальное время действия промокода")]
		public virtual TimeSpan? StartTimePromoCode
		{
			get => _startTimePromoCode;
			set => SetField(ref _startTimePromoCode, value);
		}
		
		/// <summary>
		/// Конечное время действия промокода
		/// </summary>
		[Display(Name = "Конечное время действия промокода")]
		public virtual TimeSpan? EndTimePromoCode
		{
			get => _endTimePromoCode;
			set => SetField(ref _endTimePromoCode, value);
		}
		
		public virtual bool HasPromoCodeDurationTime => _startTimePromoCode.HasValue || _endTimePromoCode.HasValue;
		public virtual bool HasOrderMinSum => PromoCodeOrderMinSum > 0;
		public virtual string StartTimePromoCodeString => StartTimePromoCode.HasValue
			? $"{StartTimePromoCode.Value:hh\\:mm}"
			: string.Empty;
		public virtual string EndTimePromoCodeString => EndTimePromoCode.HasValue
			? $"{EndTimePromoCode.Value:hh\\:mm}"
			: string.Empty;
		
		public virtual void AddProductGroup(ProductGroup productGroup)
		{
			if(!ObservableProductGroups.Contains(productGroup))
			{
				ObservableProductGroups.Add(productGroup);
			}
		}

		public virtual void RemoveProductGroup(ProductGroup productGroup)
		{
			if(ObservableProductGroups.Contains(productGroup))
			{
				ObservableProductGroups.Remove(productGroup);
			}
		}
		
		public virtual void AddNomenclature(Nomenclature nomenclature)
		{
			if(!ObservableNomenclatures.Contains(nomenclature))
			{
				ObservableNomenclatures.Add(nomenclature);
			}
		}

		public virtual void RemoveNomenclature(Nomenclature nomenclature)
		{
			if(ObservableNomenclatures.Contains(nomenclature))
			{
				ObservableNomenclatures.Remove(nomenclature);
			}
		}
		
		public virtual void UpdateNomenclatureCategories(SelectableNomenclatureCategoryNode selectedCategory)
		{
			if(selectedCategory.IsSelected)
			{
				AddNomenclatureCategory(selectedCategory);
			}
			else
			{
				RemoveNomenclatureCategory(selectedCategory);
			}
		}
		
		public virtual void ResetOrderMinSum()
		{
			PromoCodeOrderMinSum = 0;
		}
		
		public virtual void ResetTimeDuration()
		{
			StartTimePromoCode = null;
			EndTimePromoCode = null;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Id == 0 && IsArchive)
			{
				yield return new ValidationResult("Нельзя создать новое архивное основание", new[] { nameof(IsArchive) });
			}
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Название скидки должно быть заполнено", new[] { nameof(Name) });
			}
			if(Name?.Length > _nameLimit)
			{
				yield return new ValidationResult($"Превышена длина названия скидки ({Name.Length}/{_nameLimit})", new[] { nameof(Name) });
			}
			if(Value == 0)
			{
				yield return new ValidationResult("Размер скидки не может быть равен 0", new[] { nameof(Value) });
			}
			if(ValueType == DiscountUnits.percent && Value > _percentsLimit)
			{
				yield return new ValidationResult($"Размер скидки в процентах больше {_percentsLimit}", new[] { nameof(Value) });
			}
			
			var discountRepository = validationContext.GetRequiredService<IDiscountReasonRepository>();

			if(IsPromoCode)
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

				if(!StartDatePromoCode.HasValue)
				{
					yield return new ValidationResult(
						"Не заполнена начальная дата действия промокода",
						new[] { nameof(StartDatePromoCode) });
				}

				if(!EndDatePromoCode.HasValue)
				{
					yield return new ValidationResult(
						"Не заполнена конечная дата действия промокода",
						new[] { nameof(EndDatePromoCode) });
				}

				using(var uow =
				      validationContext.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("Проверка промокода на дубли"))
				{

					if(discountRepository.ExistsPromoCodeWithName(uow, Id, PromoCodeName, out var duplicatePromoCode))
					{
						var archived = duplicatePromoCode.IsArchive ? "архивный" : null;
						yield return new ValidationResult(
							$"Уже есть созданный {archived} промокод {duplicatePromoCode.Id} {duplicatePromoCode.Name}",
							new[] { nameof(PromoCodeName) });
					}
				}
			}
			else
			{
				using(var uow =
				      validationContext.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("Проверка основания скидки на дубли"))
				{
					if(discountRepository.ExistsActiveDiscountReasonWithName(
						   uow, Id, Name, out var activeDiscountReasonWithSameName))
					{
						yield return new ValidationResult(
							"Уже существует основание для скидки с таким названием.\n" +
							$"Код: {activeDiscountReasonWithSameName.Id}\n" +
							$"Название: {activeDiscountReasonWithSameName.Name}",
							new[] { nameof(Name) });
					}
				}
			}
		}

		private void AddNomenclatureCategory(SelectableNomenclatureCategoryNode selectedCategory)
		{
			if(!NomenclatureCategories.Contains(selectedCategory.DiscountReasonNomenclatureCategory))
			{
				NomenclatureCategories.Add(selectedCategory.DiscountReasonNomenclatureCategory);
			}
		}

		private void RemoveNomenclatureCategory(SelectableNomenclatureCategoryNode selectedCategory)
		{
			if(NomenclatureCategories.Contains(selectedCategory.DiscountReasonNomenclatureCategory))
			{
				NomenclatureCategories.Remove(selectedCategory.DiscountReasonNomenclatureCategory);
			}
		}
	}
}
