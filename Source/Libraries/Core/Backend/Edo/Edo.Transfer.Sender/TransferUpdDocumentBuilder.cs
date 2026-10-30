using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Dto;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Nomenclature;

namespace Edo.Transfer.Sender
{
	/// <summary>
	/// Формирует сохраняемый УПД по подготовленному заказу перемещения товаров.
	/// </summary>
	public class TransferUpdDocumentBuilder
	{
		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly IEdoTransferSettings _edoTransferSettings;
		private readonly INomenclatureSettings _nomenclatureSettings;

		/// <summary>
		/// Создаёт построитель УПД трансфера.
		/// </summary>
		/// <param name="trueMarkCodeRepository">Репозиторий кодов маркировки.</param>
		/// <param name="edoTransferSettings">Настройки расчёта цены трансфера.</param>
		/// <param name="nomenclatureSettings">Настройки номенклатуры.</param>
		public TransferUpdDocumentBuilder(
			ITrueMarkCodeRepository trueMarkCodeRepository,
			IEdoTransferSettings edoTransferSettings,
			INomenclatureSettings nomenclatureSettings)
		{
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}

		/// <summary>
		/// Фиксирует реквизиты, цены и коды маркировки УПД трансфера.
		/// </summary>
		/// <param name="transferOrder">Подготовленный заказ перемещения товаров.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Документ УПД для сохранения в задаче трансфера.</returns>
		public async Task<EdoUpdDocument> BuildAsync(
			TransferOrder transferOrder,
			CancellationToken cancellationToken
			)
		{
			if(transferOrder is null)
			{
				throw new ArgumentNullException(nameof(transferOrder));
			}

			if(transferOrder.Seller is null)
			{
				throw new InvalidOperationException("В заказе перемещения товаров не указан продавец");
			}

			if(transferOrder.Customer is null)
			{
				throw new InvalidOperationException("В заказе перемещения товаров не указан покупатель");
			}

			if(transferOrder.TransferDocument is null && transferOrder.Date >= new DateTime(2026, 1, 1))
			{
				throw new InvalidOperationException("В заказе перемещения товаров не указан счетчик документов УПД для 2026 года и позже");
			}

			if(transferOrder.Date == default)
			{
				throw new InvalidOperationException("В заказе перемещения товаров не указана дата");
			}

			if(transferOrder.Id == 0)
			{
				throw new InvalidOperationException("При заполнении данных в УПД необходимо, чтобы заказ перемещения товаров был предварительно сохранен");
			}

			var preloadCodes = transferOrder.Items
				.Where(x => x.IndividualCode != null)
				.Select(x => x.IndividualCode);
			await _trueMarkCodeRepository.PreloadCodes(preloadCodes, cancellationToken);

			return await CreateDocumentAsync(transferOrder, cancellationToken);
		}

		private async Task<EdoUpdDocument> CreateDocumentAsync(
			TransferOrder transferOrder,
			CancellationToken cancellationToken
			)
		{
			var products = await GetProducts(transferOrder, cancellationToken);
			
			var document = new EdoUpdDocument
			{
				Guid = Guid.NewGuid(),
				Number = DocumentNumberBuilder.Build(transferOrder, transferOrder.TransferDocument),
				Sum = products.Sum(x => x.Sum),
				Date = transferOrder.Date,
				Seller = GetOrganizationInfo(transferOrder.Seller),
				Customer = GetOrganizationInfo(transferOrder.Customer),
				Consignee = GetOrganizationInfo(transferOrder.Customer),
				ShippingDocument = new DocumentConfirmingShipmentInfo().Document,
				ShippingDocumentNumber = transferOrder.Id.ToString(),
				ShippingDocumentDate = transferOrder.Date.ToString(_dateFormatString),
				ShippingReasonDocument = new BasisShipmentInfo().Document,
				ShippingReasonDocumentNumber = transferOrder.Id.ToString(),
				ShippingReasonDocumentDate = transferOrder.Date.ToString(_dateFormatString),
				InventPositions = products
			};

			return document;
		}
		
		private UpdOrganization GetOrganizationInfo(OrganizationEntity organization)
		{
			var address = new AddressInfo { Address = organization.ActiveOrganizationVersion.JurAddress };
			var oganizationInfo = new UpdOrganization
			{
				Name = organization.Name,
				CountryCode = address.CountryCode,
				CountryName = address.CountryName,
				Address = address.Address,
				Inn = organization.INN,
				Kpp = organization.KPP,
				Ogrn = organization.OGRN,
				OgrnDate = organization.OGRNDate,
				EdoAccountId = organization.TaxcomEdoSettings.EdoAccount,
			};

			return oganizationInfo;
		}

		private async Task<IList<UpdInventPosition>> GetProducts(TransferOrder transferOrder, CancellationToken cancellationToken)
		{
			var products = new List<UpdInventPosition>();

			var itemsByNomenclature = transferOrder.Items.GroupBy(x => x.Nomenclature);

			foreach(var codesByNomenclature in itemsByNomenclature)
			{
				var nomenclature = codesByNomenclature.Key;

				var quantity = codesByNomenclature.Sum(x => x.Quantity);

				var productCodes = new List<UpdProductCode>();

				foreach(var code in codesByNomenclature)
				{
					var productCode = new UpdProductCode
					{
						IndividualCode = code.IndividualCode,
						GroupCode = code.GroupCode,
						IdentificationCode = code.GroupCode?.IdentificationCode ?? code.IndividualCode?.IdentificationCode,
						Quantity = code.Quantity
					};

					var transportCode = code.GroupCode != null
						? await _trueMarkCodeRepository.FindParentTransportCode(code.GroupCode, cancellationToken)
						: await _trueMarkCodeRepository.FindParentTransportCode(code.IndividualCode, cancellationToken);
					productCode.TransportCode = transportCode?.RawCode;

					productCodes.Add(productCode);
				}

				var purchasePrice = nomenclature.GetPurchasePriceOnDate(DateTime.Now);
				var costPrice = nomenclature.GetCostPriceOnDate(DateTime.Now);
				var newestPrice = purchasePrice?.StartDate > costPrice?.StartDate 
					? purchasePrice?.PurchasePrice 
					: costPrice?.CostPrice;
				var additionalPercent = _edoTransferSettings.AdditionalPurchasePricePrecentForTransfer;
				var price = 0m;

				if(newestPrice == null || newestPrice == 0m)
				{
					price = nomenclature.GetPrice(quantity).Price;
					price *= 1 - ((decimal)additionalPercent) / 100;
				}
				else
				{
					price = newestPrice.Value;
					price *= 1 + ((decimal)additionalPercent) / 100;
				}

				var sum = price * quantity;
				
				var vatRateVersion = nomenclature.GetEffectiveVatRateVersion(transferOrder.Seller, transferOrder.Date);
				
				if(vatRateVersion == null)
				{
					throw new InvalidOperationException($"У товара #{nomenclature.Id} отсутствует версия " +
						$"НДС на дату трансфера заказа #{transferOrder.Date}");
				}
				
				var includeVat = vatRateVersion.VatRate.VatRateValue != 0 
					? Math.Round(sum * vatRateVersion.VatRate.VatNumericValue / (1 + vatRateVersion.VatRate.VatNumericValue), 2) 
					: 0;
				var valueAddedTax = vatRateVersion.VatRate.VatRateValue != 0 
					? vatRateVersion.VatRate.VatNumericValue 
					: (decimal?)null;

				var product = new UpdInventPosition
				{
					Name = nomenclature.OfficialName,
					IsService = nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId,
					UnitName = nomenclature.Unit.Name,
					OKEI = nomenclature.Unit.OKEI,
					NomenclatureId = nomenclature.Id,
					Count = quantity,
					Price = price,
					IncludeVat = includeVat,
					ValueAddedTax = valueAddedTax,
					DiscountMoney = 0,
					Codes = productCodes
				};

				products.Add(product);
			}

			return products;
		}

	}
}
