using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
	public class NomenclatureGroupPricingPriceModel : PropertyChangedBase, IValidatableObject
	{
		private readonly DateTime _date;
		private readonly INomenclatureCostPriceModel _nomenclatureCostPriceModel;
		private readonly INomenclatureInnerDeliveryPriceModel _nomenclatureInnerDeliveryPriceModel;
		private readonly bool _canCreateCostPrice;
		private readonly bool _canCreateInnerDeliveryPrice;
		private decimal? _costPrice;
		private decimal? _innerDeliveryPrice;

		public NomenclatureGroupPricingPriceModel(
			DateTime date,
			Nomenclature nomenclature,
			INomenclatureCostPriceModel nomenclatureCostPriceModel,
			INomenclatureInnerDeliveryPriceModel nomenclatureInnerDeliveryPriceModel)
		{
			_date = date;
			Nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));
			_nomenclatureCostPriceModel = nomenclatureCostPriceModel ?? throw new ArgumentNullException(nameof(nomenclatureCostPriceModel));
			_nomenclatureInnerDeliveryPriceModel = nomenclatureInnerDeliveryPriceModel ?? throw new ArgumentNullException(nameof(nomenclatureInnerDeliveryPriceModel));

			_canCreateInnerDeliveryPrice = nomenclatureInnerDeliveryPriceModel.CanCreatePrice(Nomenclature, _date);
			_canCreateCostPrice = nomenclatureCostPriceModel.CanCreatePrice(Nomenclature, _date);
		}

		public Nomenclature Nomenclature { get; }

		public bool IsValidCostPrice => !CostPrice.HasValue || (CostPrice.HasValue && _canCreateCostPrice);

		public decimal? CostPrice
		{
			get => _costPrice;
			set
			{
				if(SetField(ref _costPrice, value))
				{
					OnPropertyChanged(nameof(IsValidCostPrice));
				}
			}
		}

		public bool IsValidInnerDeliveryPrice => !InnerDeliveryPrice.HasValue || (InnerDeliveryPrice.HasValue && _canCreateInnerDeliveryPrice);

		public decimal? InnerDeliveryPrice
		{
			get => _innerDeliveryPrice;
			set
			{
				if(SetField(ref _innerDeliveryPrice, value))
				{
					OnPropertyChanged(nameof(IsValidInnerDeliveryPrice));
				}
			}
		}

		public void CreatePrices()
		{
			var valid = Validate(new ValidationContext(this));
			if(valid.Any())
			{
				throw new InvalidOperationException($"Невозможно создать цены. Проверить причины можно вызовом валидации.");
			}

			CreateCostPrice();
			CreateInnerDeliveryPrice();
		}

		private void CreateCostPrice()
		{
			if(CostPrice == null)
			{
				return;
			}

			_nomenclatureCostPriceModel.CreatePrice(Nomenclature, _date, CostPrice.Value);
		}

		private void CreateInnerDeliveryPrice()
		{
			if(InnerDeliveryPrice == null)
			{
				return;
			}

			var newPrice = _nomenclatureInnerDeliveryPriceModel.CreatePrice(Nomenclature, _date);
			newPrice.Price = InnerDeliveryPrice.Value;
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!IsValidCostPrice)
			{
				yield return new ValidationResult($"Невозможно создать цены для {Nomenclature.Name}, так как на эту дату ({_date}) уже имеется цена себестоимости");
			}

			if(!IsValidInnerDeliveryPrice)
			{
				yield return new ValidationResult($"Невозможно создать цены для {Nomenclature.Name}, так как на эту дату ({_date}) уже имеется цена доставки на склад");
			}
		}
	}
}
