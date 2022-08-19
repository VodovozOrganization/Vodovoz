using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Factories;

namespace Vodovoz.Models
{
	public class GroupNomenclaturePricesModel : IValidatableObject
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly INomenclaturePricesRepository _nomenclaturePricesRepository;
		private readonly GroupNomenclaturePriceModelFactory _groupNomenclaturePriceModelFactory;
		private IUnitOfWork _uow;

		public GroupNomenclaturePricesModel(
			IUnitOfWorkFactory uowFactory, 
			INomenclaturePricesRepository nomenclaturePricesRepository, 
			GroupNomenclaturePriceModelFactory groupNomenclaturePriceModelFactory
		)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_nomenclaturePricesRepository = nomenclaturePricesRepository ?? throw new ArgumentNullException(nameof(nomenclaturePricesRepository));
			_groupNomenclaturePriceModelFactory = groupNomenclaturePriceModelFactory ?? throw new ArgumentNullException(nameof(groupNomenclaturePriceModelFactory));
		}

		public IEnumerable<GroupNomenclaturePriceModel> PriceModels { get; private set; }

		public void LoadPrices(DateTime dateTime)
		{
			DiscardChanges();
			_uow = _uowFactory.CreateWithoutRoot();

			var nomenclatures = _nomenclaturePricesRepository.GetNomenclaturesForGroupPricing(_uow);

			var models = new List<GroupNomenclaturePriceModel>();
			foreach (var nomenclature in nomenclatures)
			{
				var model = _groupNomenclaturePriceModelFactory.CreateModel(dateTime, nomenclature);
				models.Add(model);
			}
			PriceModels = models;
		}

		public void SavePrices()
		{
			if(_uow == null)
			{
				return;
			}

			var validationResults = Validate(new ValidationContext(this));
			if(validationResults.Any())
			{
				throw new ValidationException("Невозможно сохранить. По результатам валидации имеются не устранные проблемы. Перед запуском сохранения необходимо проверять валидацию.");
			}

			foreach(var priceModel in PriceModels)
			{
				priceModel.CreatePrices();
			}
			_uow.Commit();
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach(var priceModel in PriceModels)
			{
				foreach(var validationResult in priceModel.Validate(validationContext))
				{
					yield return validationResult;
				}
			}
		}

		private void DiscardChanges()
		{
			PriceModels = Enumerable.Empty<GroupNomenclaturePriceModel>();
			_uow?.Dispose();
			_uow = null;
		}
	}

	public class GroupNomenclaturePriceModel : PropertyChangedBase, IValidatableObject
	{
		private readonly DateTime _date;
		private readonly NomenclatureCostPurchasePriceModel _nomenclatureCostPurchasePriceModel;
		private readonly NomenclatureInnerDeliveryPriceModel _nomenclatureInnerDeliveryPriceModel;
		private decimal? _costPurchasePrice;
		private decimal? _innerDeliveryPrice;
		private decimal? _activeCostPurchasePrice;
		private decimal? _activeInnerDeliveryPrice;

		public GroupNomenclaturePriceModel(
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

			var newPrice = _nomenclatureCostPurchasePriceModel.CreatePrice(Nomenclature, _date);
			newPrice.PurchasePrice = CostPurchasePrice.Value;
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
			if(_activeCostPurchasePrice.HasValue)
			{
				yield return new ValidationResult($"Невозможно создать цены для {Nomenclature.Name}, так как на эту дату ({_date}) уже имеется цена закупки или себестоимости");
			}

			if(_activeInnerDeliveryPrice.HasValue)
			{
				yield return new ValidationResult($"Невозможно создать цены для {Nomenclature.Name}, так как на эту дату ({_date}) уже имеется цена доставки на склад");
			}
		}
	}
}
