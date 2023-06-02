using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Factories;

namespace Vodovoz.Models
{
	public class NomenclatureGroupPricingModel : IValidatableObject, INomenclatureGroupPricingModel
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly INomenclaturePricesRepository _nomenclaturePricesRepository;
		private readonly INomenclatureGroupPricingPriceModelFactory _nomenclatureGroupPricingPriceModelFactory;
		private IUnitOfWork _uow;

		public NomenclatureGroupPricingModel(
			IUnitOfWorkFactory uowFactory,
			INomenclaturePricesRepository nomenclaturePricesRepository,
			INomenclatureGroupPricingPriceModelFactory nomenclatureGroupPricingPriceModelFactory
		)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_nomenclaturePricesRepository = nomenclaturePricesRepository ?? throw new ArgumentNullException(nameof(nomenclaturePricesRepository));
			_nomenclatureGroupPricingPriceModelFactory = nomenclatureGroupPricingPriceModelFactory ?? throw new ArgumentNullException(nameof(nomenclatureGroupPricingPriceModelFactory));
		}

		public IEnumerable<NomenclatureGroupPricingPriceModel> PriceModels { get; private set; }

		public void LoadPrices(DateTime dateTime)
		{
			DiscardChanges();
			_uow = _uowFactory.CreateWithoutRoot();

			var nomenclatures = _nomenclaturePricesRepository.GetNomenclaturesForGroupPricing(_uow);

			var models = new List<NomenclatureGroupPricingPriceModel>();
			foreach(var nomenclature in nomenclatures)
			{
				var model = _nomenclatureGroupPricingPriceModelFactory.CreateModel(dateTime, nomenclature);
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
				throw new ValidationException("Невозможно сохранить. По результатам валидации имеются не устраненные проблемы. Перед запуском сохранения необходимо проверять валидацию.");
			}

			foreach(var priceModel in PriceModels)
			{
				priceModel.CreatePrices();
				_uow.Save(priceModel.Nomenclature);
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
			PriceModels = Enumerable.Empty<NomenclatureGroupPricingPriceModel>();
			_uow?.Dispose();
			_uow = null;
		}
	}
}
