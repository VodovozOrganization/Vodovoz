using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "фиксированные цены",
		Nominative = "фиксированная цена")]
	[HistoryTrace]
	public class NomenclatureFixedPrice : NomenclatureFixedPriceEntity, IValidatableObject
	{
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private Nomenclature _nomenclature;

		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual new Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// точка доставки
		/// </summary>
		[Display(Name = "Точка доставки")]
		public virtual new DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Заголовок
		/// </summary>
		[Display(Name = "Заголовок")]
		public virtual new string Title
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

		/// <summary>
		/// Создать фиксированную цену для сотрудника
		/// </summary>
		/// <param name="namedDomainObject"></param>
		/// <returns></returns>
		public static new NomenclatureFixedPrice CreateEmployeeFixedPrice(INamedDomainObject namedDomainObject)
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
