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
		private decimal? _costPurchasePrice;
		private decimal? _innerDeliveryPrice;
		private decimal? _activeCostPurchasePrice;
		private decimal? _activeInnerDeliveryPrice;

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

			LoadPrices();
		}

		public Nomenclature Nomenclature { get; }

		private void LoadPrices()
		{
			var costPrice = _nomenclatureCostPurchasePriceModel.GetPrice(_date, Nomenclature);
			if(costPrice != null)
			{
				_activeCostPurchasePrice = costPrice.PurchasePrice;
			}

			var innerDeliveryPrice = _nomenclatureInnerDeliveryPriceModel.GetPrice(_date, Nomenclature);
			if(innerDeliveryPrice != null)
			{
				_activeInnerDeliveryPrice = innerDeliveryPrice.Price;
			}
		}

		public bool IsValidCostPurchasePrice => !CostPurchasePrice.HasValue || !_activeCostPurchasePrice.HasValue;

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

		public bool IsValidInnerDeliveryPrice => !InnerDeliveryPrice.HasValue || !_activeInnerDeliveryPrice.HasValue;

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
