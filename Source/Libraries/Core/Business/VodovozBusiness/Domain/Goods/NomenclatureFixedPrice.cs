using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "фиксированные цены",
		Nominative = "фиксированная цена")]
	[HistoryTrace]
	public class NomenclatureFixedPrice : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private Nomenclature _nomenclature;
		private decimal _price;
		private int _minCount;
		private bool _isEmployeeFixedPrice;

		public virtual int Id { get; set; }

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		[Display(Name = "Минимальное количество")]
		public virtual int MinCount
		{
			get => _minCount;
			set => SetField(ref _minCount, value);
		}
		
		[Display(Name = "Фикса сотрудника")]
		public virtual bool IsEmployeeFixedPrice
		{
			get => _isEmployeeFixedPrice;
			set => SetField(ref _isEmployeeFixedPrice, value);
		}

		public virtual string Title
		{
			get
			{
				if(Counterparty != null)
				{
					return $"Фикса клиента №{Counterparty.Id} {Counterparty.Name}";
				}

				return DeliveryPoint != null ? $"Фикса точки доставки №{DeliveryPoint.Id} {DeliveryPoint.CompiledAddress}" : $"Фикса №{Id}";
			}
		}

		public static NomenclatureFixedPrice CreateEmployeeFixedPrice(INamedDomainObject namedDomainObject)
		{
			return new NomenclatureFixedPrice
			{
				Nomenclature = new Nomenclature
				{
					Id = namedDomainObject.Id,
					Name = namedDomainObject.Name
				},
				IsEmployeeFixedPrice = true
			};
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Price <= 0)
			{
				yield return new ValidationResult($"Фиксированная цена для {Nomenclature.Name} должна быть больше нуля",
					new[] { nameof(Price) });
			}

			if(MinCount < 1)
			{
				yield return new ValidationResult(
					$"Значение минимального количества бутылей для {Nomenclature.Name} должно быть больше нуля",
					new[] { nameof(MinCount) });
			}
		}
	}
}
