using Bitrix;
using Bitrix.DTO;
using NLog;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Common;
using Vodovoz.EntityRepositories.Goods;

namespace BitrixIntegration.Processors
{
	public class ProductProcessor : IProductProcessor
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IBitrixClient _bitrixClient;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IProductGroupProcessor _productGroupProcessor;
		private readonly IMeasurementUnitsRepository _measurementUnitsRepository;

		public ProductProcessor(IBitrixClient bitrixClient,
			INomenclatureRepository nomenclatureRepository,
			IProductGroupProcessor productGroupProcessor,
			IMeasurementUnitsRepository measurementUnitsRepository)
		{
			_bitrixClient = bitrixClient ?? throw new ArgumentNullException(nameof(bitrixClient));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_productGroupProcessor = productGroupProcessor ?? throw new ArgumentNullException(nameof(productGroupProcessor));
			_measurementUnitsRepository = measurementUnitsRepository ?? throw new ArgumentNullException(nameof(measurementUnitsRepository));
		}

		public void ProcessProducts(IUnitOfWork uow, Deal deal, Order order)
		{
			var dealProductItems = _bitrixClient.GetProductsForDeal(deal.Id).GetAwaiter().GetResult();
			foreach(var dealProductItem in dealProductItems)
			{
				var product = _bitrixClient.GetProduct(dealProductItem.ProductId).GetAwaiter().GetResult()
				              ?? throw new InvalidOperationException($"Не удалось загрузить товар битрикса ({dealProductItem.ProductId})");

				if(product.IsOurProduct)
				{
					ProcessOurProduct(uow, deal, order, dealProductItem, product);
				}
				else
				{
					ProcessOnlineStoreProduct(uow, deal, order, dealProductItem);
				}
			}
		}

		private void ProcessOurProduct(IUnitOfWork uow, Deal deal, Order order, DealProductItem dealProductItem, Product product)
		{
			Nomenclature nomenclature = GetNomenclatureForOurProduct(uow, product);
			if(nomenclature == null)
			{
				throw new InvalidOperationException($"Не найдена номенклатура для добавления нашего товара из битрикса. " +
					$"Id номенклатуры в битриксе {product.NomenclatureInfo?.NomenclatureId}");
			}
			decimal discount = Math.Abs(nomenclature.GetPrice(1) - dealProductItem.Price);
			order.AddNomenclature(nomenclature, dealProductItem.Count, discount, true);
		}

		private Nomenclature GetNomenclatureForOurProduct(IUnitOfWork uow, Product product)
		{
			if(product.NomenclatureInfo == null)
			{
				throw new InvalidOperationException($"Попытка загрузить номенклатуру для не соответствующего продукта " +
													$"(Для продукта {product.Id} ({product.Name}) не заполнено поле " +
													$"{nameof(product.NomenclatureInfo)})");
			}

			Nomenclature nomenclature = uow.GetById<Nomenclature>(product.NomenclatureInfo.NomenclatureId);
			if(nomenclature == null)
			{
				_logger.Info($"Для нашего продукта {product.Id} ({product.Name}) не удалось найти номенклатуру по " +
				             $"{nameof(product.NomenclatureInfo.NomenclatureId)}");
			}
			else
			{
				_logger.Info($"Для нашего продукта {product.Id} ({product.Name}) найдена номенклатура по " +
				             $"{nameof(product.NomenclatureInfo.NomenclatureId)} {nomenclature.Id} ({nomenclature.Name})");
			}
			return nomenclature;
		}

		private void ProcessOnlineStoreProduct(IUnitOfWork uow, Deal deal, Order order, DealProductItem dealProductItem)
		{
			decimal discount = 0M;
			bool isDiscountInMoney = false;
			bool dealHasPromo = !string.IsNullOrEmpty(deal.Promocode);

			Nomenclature nomenclature = GetNomenclatureForOnlineStoreProduct(uow, dealProductItem);
			if(nomenclature == null)
			{
				nomenclature = CreateOnlineStoreNomenclature(uow, dealProductItem);
				nomenclature.UpdatePrice(dealProductItem.Price, (int)dealProductItem.Count);
			}
			else
			{
				if(dealHasPromo)
				{
					discount = Math.Abs(nomenclature.GetPrice(1) - dealProductItem.Price);
					isDiscountInMoney = true;
				}
				else
				{
					nomenclature.UpdatePrice(dealProductItem.Price, (int)dealProductItem.Count);
				}
			}

			order.AddNomenclature(nomenclature, dealProductItem.Count, discount, isDiscountInMoney);
		}

		private Nomenclature GetNomenclatureForOnlineStoreProduct(IUnitOfWork uow, DealProductItem product)
		{
			Nomenclature nomenclature;
			if(MatchNomenclatureByBitrixId(uow, product.ProductId, out nomenclature))
			{
				_logger.Info($"Для продукта ИМ {product.ProductId} ({product.ProductName}) найдена номенклатура по " +
				             $"bitrix_id {nomenclature.BitrixId} ({nomenclature.Name})");
			}
			else if(MatchNomenclatureByName(uow, product.ProductName, out nomenclature))
			{
				_logger.Info($"Для продукта ИМ {product.ProductId} ({product.ProductName}) найдена номенклатура по " +
				             $"имени {nomenclature.BitrixId} ({nomenclature.Name})");
			}

			if(nomenclature == null)
			{
				_logger.Info($"Для продукта ИМ {product.ProductId} ({product.ProductName}) не удалось найти соответствующую номенклатуру");
			}
			return nomenclature;
		}

		private bool MatchNomenclatureByBitrixId(IUnitOfWork uow, uint productId, out Nomenclature outNomenclature)
		{
			Nomenclature nomenclature = _nomenclatureRepository.GetNomenclatureByBitrixId(uow, productId);

			if(nomenclature == null)
			{
				outNomenclature = null;
				_logger.Info($"Не удалось сопоставить Nommenclature по BitrixId: {productId}");

				return false;
			}

			outNomenclature = nomenclature;
			_logger.Info($"Сопоставление ТМЦ: {outNomenclature.Id} по BitrixId: {productId} прошло успешно");
			return true;
		}

		private bool MatchNomenclatureByName(IUnitOfWork uow, string productName, out Nomenclature outNomenclature)
		{
			outNomenclature = _nomenclatureRepository.GetNomenclatureByName(uow, productName);
			if(outNomenclature == null)
			{
				_logger.Warn($"Номенклатура не найдена по названию {productName}");
				return false;
			}
			return true;
		}

		private Nomenclature CreateOnlineStoreNomenclature(IUnitOfWork uow, DealProductItem dealProductItem)
		{
			//Если нет такой группы то создаем группу
			var group = _productGroupProcessor.ProcessProductGroup(uow, dealProductItem);
			var measurementUnit = _measurementUnitsRepository.GetUnitsByBitrix(uow, dealProductItem.MeasureName);
			if(measurementUnit == null)
			{
				throw new InvalidOperationException(
					$"Не удалось найти единицу измерения в доставке воды для {dealProductItem.MeasureName}");
			}
			var nomenclature = new Nomenclature()
			{
				Name = dealProductItem.ProductName,
				OfficialName = dealProductItem.ProductName,
				Description = dealProductItem.ProductDescription ?? "",
				CreateDate = DateTime.Now,
				Category = NomenclatureCategory.additional,
				BitrixId = dealProductItem.ProductId,
				VAT = VAT.Vat20,
				OnlineStoreExternalId = "3",
				Unit = measurementUnit,
				ProductGroup = group
			};
			return nomenclature;
		}
	}
}
