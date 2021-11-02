using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "основания скидок",
		Nominative = "основание скидки")]
	[EntityPermission]
	public class DiscountReason : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }
		private string _name;
		private bool _isArchive;
		private DiscountValueType _valueType;
		private decimal _value;
		private IList<DiscountNomenclatureGroup> _productGroups = new List<DiscountNomenclatureGroup>();

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
		public virtual DiscountValueType ValueType
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

		public virtual IList<DiscountNomenclatureGroup> ProductGroups
		{
			get => _productGroups;
			set => SetField(ref _productGroups, value);
		}

		GenericObservableList<DiscountNomenclatureGroup> _observableDiscountNomenclatureGroups;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DiscountNomenclatureGroup> ObservableDiscountNomenclatureGroups
		{
			get
			{
				if(_observableDiscountNomenclatureGroups == null)
				{
					_observableDiscountNomenclatureGroups = new GenericObservableList<DiscountNomenclatureGroup>(ProductGroups);
				}

				return _observableDiscountNomenclatureGroups;
			}
		}

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

			if(Name?.Length > 45)
			{
				yield return new ValidationResult($"Превышена длина названия ({Name.Length}/45)", new[] { nameof(Name) });
			}
			if(ValueType == DiscountValueType.Percents && Value > 100)
			{
				yield return new ValidationResult($"Размер скидки в процентах больше 100", new[] { nameof(Value) });
			}
		}

		public virtual void AddProductGroup(ProductGroup productGroup)
		{
			DiscountNomenclatureGroup discountNomenclatureGroup = new DiscountNomenclatureGroup()
			{
				ProductGroup = productGroup,
				DiscountReason = this
			};

			if(!ObservableDiscountNomenclatureGroups.Contains(discountNomenclatureGroup))
			{
				ObservableDiscountNomenclatureGroups.Add(discountNomenclatureGroup);
			}
			
		}

		public virtual void RemoveGroup(DiscountNomenclatureGroup group)
		{
			if(ObservableDiscountNomenclatureGroups.Contains(group))
			{
				ObservableDiscountNomenclatureGroups.Remove(group);
			}
		}
	}

	public enum DiscountValueType
	{
		[Display(Name = "₽")]
		Roubles,
		[Display(Name = "%")]
		Percents
	}

	public class DiscountValueTypeStringType : NHibernate.Type.EnumStringType
	{
		public DiscountValueTypeStringType() : base(typeof(DiscountValueType)) { }
	}
}
