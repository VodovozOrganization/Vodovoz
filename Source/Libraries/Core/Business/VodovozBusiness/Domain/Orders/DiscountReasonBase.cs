using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.DiscountReasons;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "основания скидок",
		Nominative = "основание скидки",
		GenitivePlural = "оснований скидок")]
	[EntityPermission]
	[HistoryTrace]
	public abstract class DiscountReasonBase : PropertyChangedBase, IDomainObject, IValidatableObject, INamed, IArchivable
	{
		public const string DiscountReasonTypeColumn = "discount_reason_type";
		
		private const int _percentsLimit = 100;
		private const int _nameLimit = 45;
		private string _name;
		private bool _isArchive;
		private bool _isPremiumDiscount;
		private bool _isPresent;
		private DiscountUnits _valueType;
		private decimal _value;
		private IObservableList<DiscountReasonNomenclatureCategory> _nomenclatureCategories =
			new ObservableList<DiscountReasonNomenclatureCategory>();
		private IObservableList<Nomenclature> _nomenclatures = new ObservableList<Nomenclature>();
		private IObservableList<ProductGroup> _productGroups = new ObservableList<ProductGroup>();
		private IObservableList<PromotionalSet> _promoSets = new ObservableList<PromotionalSet>();
		private IObservableList<DiscountApplicability> _discountApplicabilities = new ObservableList<DiscountApplicability>();

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
			set => SetField(ref this._value, value);
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

		public virtual IObservableList<DiscountReasonNomenclatureCategory> NomenclatureCategories
		{
			get => _nomenclatureCategories;
			set => SetField(ref _nomenclatureCategories, value);
		}

		public virtual IObservableList<Nomenclature> Nomenclatures
		{
			get => _nomenclatures;
			set => SetField(ref _nomenclatures, value);
		}

		public virtual IObservableList<ProductGroup> ProductGroups
		{
			get => _productGroups;
			set => SetField(ref _productGroups, value);
		}

		public virtual IObservableList<DiscountApplicability> DiscountApplicabilities
		{
			get => _discountApplicabilities;
			set => SetField(ref _discountApplicabilities, value);
		}

		public virtual IObservableList<PromotionalSet> PromoSets
		{
			get => _promoSets;
			set => SetField(ref _promoSets, value);
		}

		public abstract DiscountReasonType DiscountReasonType { get; }
		
		public virtual void AddProductGroups(IEnumerable<INamedDomainObject> productGroups)
		{
			foreach(var productGroup in productGroups)
			{
				AddProductGroup(
					new ProductGroup
					{
						Id = productGroup.Id,
						Name = productGroup.Name
					});
			}
		}

		public virtual void RemoveProductGroups(IEnumerable<object> productGroups)
		{
			foreach(var productGroup in productGroups)
			{
				RemoveProductGroup(productGroup as ProductGroup);
			}
		}
		
		public virtual void AddNomenclatures(IEnumerable<INamedDomainObject> nomenclatures)
		{
			foreach(var nomenclature in nomenclatures)
			{
				AddNomenclature(
					new Nomenclature
					{
						Id = nomenclature.Id,
						Name = nomenclature.Name
					});
			}
		}
		
		public virtual void RemoveNomenclatures(IEnumerable<object> nomenclatures)
		{
			foreach(var nomenclature in nomenclatures)
			{
				RemoveNomenclature(nomenclature as Nomenclature);
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
		
		public virtual void PromoSetsClear()
		{
			PromoSets.Clear();
		}

		public virtual void UpdateDiscountApplicabilities(IDictionary<DiscountType, UseDiscountType?> discountTypeUses)
		{
			var countUses = discountTypeUses.Count;
			
			foreach(var discountApplicability in DiscountApplicabilities)
			{
				if(discountTypeUses.TryGetValue(discountApplicability.DiscountType, out var discountTypeUse))
				{
					discountApplicability.UseDiscountType = discountTypeUse.Value;
					discountTypeUses.Remove(discountApplicability.DiscountType);
				}
				else
				{
					throw new InvalidOperationException("Что-то пошло не так. Не должно быть расхождений в типах применимости скидки");
				}
			}

			foreach(var keyPairValue in discountTypeUses)
			{
				DiscountApplicabilities.Add(DiscountApplicability.Create(keyPairValue.Key, keyPairValue.Value.Value, this));
			}

			if(DiscountApplicabilities.Count != countUses)
			{
				throw new InvalidOperationException("Число применимостей по типам должно совпадать с пришедшим значением!");
			}
		}

		protected virtual void Copy(
			DiscountReasonBase copyingDiscount,
			IObservableList<PromotionalSet> promoSets = null)
		{
			Id = copyingDiscount.Id;
			_name = copyingDiscount.Name;
			_isArchive = copyingDiscount.IsArchive;
			_isPremiumDiscount = copyingDiscount.IsPremiumDiscount;
			_isPresent = copyingDiscount.IsPresent;
			_valueType = copyingDiscount.ValueType;
			_value = copyingDiscount.Value;
			_nomenclatureCategories = new ObservableList<DiscountReasonNomenclatureCategory>(copyingDiscount.NomenclatureCategories);
			_nomenclatures = new ObservableList<Nomenclature>(copyingDiscount.Nomenclatures);
			_productGroups = new ObservableList<ProductGroup>(copyingDiscount.ProductGroups);
			_promoSets = promoSets ?? new ObservableList<PromotionalSet>(copyingDiscount.PromoSets);
			_discountApplicabilities = new ObservableList<DiscountApplicability>();
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

			if(Value == 0)
			{
				yield return new ValidationResult("Размер скидки не может быть равен 0", new[] { nameof(Value) });
			}
			if(ValueType == DiscountUnits.percent && Value > _percentsLimit)
			{
				yield return new ValidationResult($"Размер скидки в процентах больше {_percentsLimit}", new[] { nameof(Value) });
			}
			
			var discountRepository = validationContext.GetRequiredService<IDiscountReasonRepository>();

			if(DiscountReasonType != DiscountReasonType.PromoCode)
			{
				if(Name?.Length > _nameLimit)
				{
					var difference = Name.Length - _nameLimit;
					yield return new ValidationResult($"Превышена длина названия скидки на {difference}", new[] { nameof(Name) });
				}
				
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
		
		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? "Новое основание скидки"
				: Name;
		}
		
		private void AddNomenclature(Nomenclature nomenclature)
		{
			var foundNomenclature = Nomenclatures.SingleOrDefault(x => x.Id == nomenclature.Id);
			if(foundNomenclature is null)
			{
				Nomenclatures.Add(nomenclature);
			}
		}
		
		private void RemoveNomenclature(Nomenclature nomenclature)
		{
			if(Nomenclatures.Contains(nomenclature))
			{
				Nomenclatures.Remove(nomenclature);
			}
		}
		
		private void AddProductGroup(ProductGroup productGroup)
		{
			var foundProductGroup = ProductGroups.SingleOrDefault(x => x.Id == productGroup.Id);
			if(foundProductGroup is null)
			{
				ProductGroups.Add(productGroup);
			}
		}

		private void RemoveProductGroup(ProductGroup productGroup)
		{
			if(ProductGroups.Contains(productGroup))
			{
				ProductGroups.Remove(productGroup);
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
