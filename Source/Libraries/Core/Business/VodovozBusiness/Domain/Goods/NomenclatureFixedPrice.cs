﻿using System.Collections.Generic;
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
		public virtual int Id { get; set; }

        private Counterparty counterparty;
        [Display(Name = "Контрагент")]
        public virtual Counterparty Counterparty {
            get => counterparty;
            set => SetField(ref counterparty, value);
        }

        private DeliveryPoint deliveryPoint;
        [Display(Name = "Точка доставки")]
        public virtual DeliveryPoint DeliveryPoint {
            get => deliveryPoint;
            set => SetField(ref deliveryPoint, value);
        }

        private Nomenclature nomenclature;
        [Display(Name = "Номенклатура")]
        public virtual Nomenclature Nomenclature {
            get => nomenclature;
            set => SetField(ref nomenclature, value);
        }

        private decimal price;
        [Display(Name = "Цена")]
        public virtual decimal Price {
            get => price;
            set => SetField(ref price, value);
		}

		private int _minCount;
		[Display(Name = "Минимальное количество")]
		public virtual int MinCount
		{
			get => _minCount;
			set => SetField(ref _minCount, value);
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

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Price <= 0) {
                yield return new ValidationResult($"Фиксированная цена для {Nomenclature.Name} должна быть больше нуля", new []{ nameof(Price) });
            }
        }
    }
}
