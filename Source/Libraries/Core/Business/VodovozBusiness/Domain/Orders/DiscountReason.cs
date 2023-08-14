using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "основания скидок",
		Nominative = "основание скидки")]
	[EntityPermission]
	[HistoryTrace]
	public class DiscountReason : PropertyChangedBase, IDomainObject, IValidatableObject, INamed, IArchivable
	{
		private const int _percentsLimit = 100;
		private const int _nameLimit = 45;
		private string _name;
		private bool _isArchive;
		private bool _isPremiumDiscount;
		private DiscountUnits _valueType;
		private decimal _value;
		private IList<DiscountReasonNomenclatureCategory> _nomenclatureCategories = new List<DiscountReasonNomenclatureCategory>();
		private IList<Nomenclature> _nomenclatures = new List<Nomenclature>();
		private IList<ProductGroup> _productGroups = new List<ProductGroup>();
		private GenericObservableList<DiscountReasonNomenclatureCategory> _observableNomenclatureCategories;
		private GenericObservableList<Nomenclature> _observableNomenclatures;
		private GenericObservableList<ProductGroup> _observableProductGroups;

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

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Id == 0 && IsArchive)
			{
				yield return new ValidationResult("Нельзя создать новое архивное основание", new[] { nameof(IsArchive) });
			}
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено", new[] { nameof(Name) });
			}
			if(Name?.Length > _nameLimit)
			{
				yield return new ValidationResult($"Превышена длина названия ({Name.Length}/{_nameLimit})", new[] { nameof(Name) });
			}
			if(Value == 0)
			{
				yield return new ValidationResult("Размер скидки не может быть равен 0", new[] { nameof(Value) });
			}
			if(ValueType == DiscountUnits.percent && Value > _percentsLimit)
			{
				yield return new ValidationResult($"Размер скидки в процентах больше {_percentsLimit}", new[] { nameof(Value) });
			}
		}

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

	public class DiscountUnitTypeStringType : NHibernate.Type.EnumStringType
	{
		public DiscountUnitTypeStringType() : base(typeof(DiscountUnits)) { }
	}
}
