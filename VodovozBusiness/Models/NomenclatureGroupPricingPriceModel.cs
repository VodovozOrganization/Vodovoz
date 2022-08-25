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
		private readonly NomenclatureCostPurchasePriceModel _nomenclatureCostPurchasePriceModel;
		private readonly NomenclatureInnerDeliveryPriceModel _nomenclatureInnerDeliveryPriceModel;
		private readonly bool _canCreateCostPurchasePrice;
		private readonly bool _canCreateInnerDeliveryPrice;
		private decimal? _costPurchasePrice;
		private decimal? _innerDeliveryPrice;

		public NomenclatureGroupPricingPriceModel(
			DateTime date,
			Nomenclature nomenclature,
			NomenclatureCostPurchasePriceModel nomenclatureCostPurchasePriceModel,
			NomenclatureInnerDeliveryPriceModel nomenclatureInnerDeliveryPriceModel)
		{
			_date = date;
			Nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));
			_nomenclatureCostPurchasePriceModel = nomenclatureCostPurchasePriceModel ?? throw new ArgumentNullException(nameof(nomenclatureCostPurchasePriceModel));
			_nomenclatureInnerDeliveryPriceModel = nomenclatureInnerDeliveryPriceModel ?? throw new ArgumentNullException(nameof(nomenclatureInnerDeliveryPriceModel));

			_canCreateCostPurchasePrice = nomenclatureCostPurchasePriceModel.CanCreatePrice(Nomenclature, _date);
			_canCreateInnerDeliveryPrice = nomenclatureInnerDeliveryPriceModel.CanCreatePrice(Nomenclature, _date);
		}

		public Nomenclature Nomenclature { get; }

		public bool IsValidCostPurchasePrice => !CostPurchasePrice.HasValue || (CostPurchasePrice.HasValue && _canCreateCostPurchasePrice);

		public decimal? CostPurchasePrice
		{
			get => _costPurchasePrice;
			set
			{
				if(SetField(ref _costPurchasePrice, value))
				{
					OnPropertyChanged(nameof(IsValidCostPurchasePrice));
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
			if(CostPurchasePrice == null)
			{
				return;
			}

			var newPrice = _nomenclatureCostPurchasePriceModel.CreatePrice(Nomenclature, _date, CostPurchasePrice.Value);
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
			if(!IsValidCostPurchasePrice)
			{
				yield return new ValidationResult($"Невозможно создать цены для {Nomenclature.Name}, так как на эту дату ({_date}) уже имеется цена закупки или себестоимости");
			}

			if(!IsValidInnerDeliveryPrice)
			{
				yield return new ValidationResult($"Невозможно создать цены для {Nomenclature.Name}, так как на эту дату ({_date}) уже имеется цена доставки на склад");
			}
		}
	}
}
