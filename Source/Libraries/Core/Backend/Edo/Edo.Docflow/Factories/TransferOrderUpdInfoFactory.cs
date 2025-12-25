using Edo.Contracts.Messages.Dto;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Nomenclature;

namespace Edo.Docflow.Factories
{
	public class TransferOrderUpdInfoFactory : IDisposable
	{
		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly IEdoTransferSettings _edoTransferSettings;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public TransferOrderUpdInfoFactory(
			IUnitOfWork uow,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			IEdoTransferSettings edoTransferSettings,
			INomenclatureSettings nomenclatureSettings)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}

		public async Task<UniversalTransferDocumentInfo> CreateUniversalTransferDocumentInfo(
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

			if(transferOrder.Date == default)
			{
				throw new InvalidOperationException("В заказе перемещения товаров не указана дата");
			}

			if(transferOrder.Id == 0)
			{
				throw new InvalidOperationException("При заполнении данных в УПД необходимо, чтобы заказ перемещения товаров был предварительно сохранен");
			}

			var transferOrderCodes = await _uow.Session.QueryOver<TransferOrderTrueMarkCode>()
				.Fetch(SelectMode.Fetch, x => x.Nomenclature)
				.Fetch(SelectMode.Fetch, x => x.Nomenclature.Unit)
				.Fetch(SelectMode.Fetch, x => x.IndividualCode)
				.Fetch(SelectMode.Fetch, x => x.IndividualCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.GroupCode)
				.Where(x => x.TransferOrder.Id == transferOrder.Id)
				.ListAsync(cancellationToken);

			var preloadCodes = transferOrderCodes
				.Where(x => x.IndividualCode != null)
				.Select(x => x.IndividualCode);
			await _trueMarkCodeRepository.PreloadCodes(preloadCodes, cancellationToken);

			return await ConvertTransferOrderToUniversalTransferDocumentInfo(transferOrder, cancellationToken);
		}

		private async Task<UniversalTransferDocumentInfo> ConvertTransferOrderToUniversalTransferDocumentInfo(
			TransferOrder transferOrder,
			CancellationToken cancellationToken
			)
		{
			var products = await GetProducts(transferOrder, cancellationToken);

			var document = new UniversalTransferDocumentInfo
			{
				DocumentId = Guid.NewGuid(),
				Number = transferOrder.Id,
				Sum = products.Sum(x => x.Sum),
				Date = transferOrder.Date,
				Seller = GetSellerInfo(transferOrder),
				Customer = GetCustomerInfo(transferOrder),
				Consignee = GetConsigneeInfo(transferOrder),
				DocumentConfirmingShipment = GetDocumentConfirmingShipmentInfo(transferOrder),
				BasisShipment = GetBasisShipmentInfo(transferOrder),
				Payments = GetPayments(transferOrder),
				Products = products,
				AdditionalInformation = GettAdditionalInformation(transferOrder)
			};

			return document;
		}

		private SellerInfo GetSellerInfo(TransferOrder transferOrder) =>
			new SellerInfo { Organization = GetOrganizationInfo(transferOrder.Seller) };

		private CustomerInfo GetCustomerInfo(TransferOrder transferOrder) =>
			new CustomerInfo { Organization = GetOrganizationInfo(transferOrder.Customer) };

		private ConsigneeInfo GetConsigneeInfo(TransferOrder transferOrder) =>
			new ConsigneeInfo { Organization = GetOrganizationInfo(transferOrder.Customer) };

		private DocumentConfirmingShipmentInfo GetDocumentConfirmingShipmentInfo(TransferOrder transferOrder) =>
			new DocumentConfirmingShipmentInfo
			{
				Number = transferOrder.Id.ToString(),
				Date = transferOrder.Date.ToString(_dateFormatString)
			};

		private BasisShipmentInfo GetBasisShipmentInfo(TransferOrder transferOrder) =>
			new BasisShipmentInfo
			{
				Number = transferOrder.Id.ToString(),
				Date = transferOrder.Date.ToString(_dateFormatString)
			};

		private IEnumerable<PaymentInfo> GetPayments(TransferOrder transferOrder) =>
			new List<PaymentInfo>();

		private IEnumerable<UpdAdditionalInfo> GettAdditionalInformation(TransferOrder transferOrder) =>
			new List<UpdAdditionalInfo>();

		private OrganizationInfo GetOrganizationInfo(OrganizationEntity organization)
		{
			var oganizationInfo = new OrganizationInfo
			{
				Name = organization.Name,
				Address = new AddressInfo
				{
					Address = organization.ActiveOrganizationVersion.JurAddress,
				},
				Inn = organization.INN,
				Kpp = organization.KPP,
				EdoAccountId = organization.TaxcomEdoSettings.EdoAccount,
			};

			return oganizationInfo;
		}

		private async Task<IEnumerable<ProductInfo>> GetProducts(TransferOrder transferOrder, CancellationToken cancellationToken)
		{
			var products = new List<ProductInfo>();

			var itemsByNomenclature = transferOrder.Items.GroupBy(x => x.Nomenclature);

			foreach(var codesByNomenclature in itemsByNomenclature)
			{
				var nomenclature = codesByNomenclature.Key;

				var quantity = codesByNomenclature.Sum(x => x.Quantity);

				var productCodes = new List<ProductCodeInfo>();

				foreach(var code in codesByNomenclature)
				{
					var productCode = new ProductCodeInfo();

					TrueMarkTransportCode transportCode = null;

					if(code.GroupCode != null)
					{
						transportCode = await _trueMarkCodeRepository.FindParentTransportCode(code.GroupCode, cancellationToken);
						if(transportCode != null)
						{
							productCode.TransportCode = transportCode.RawCode;
						}
						else
						{
							productCode.IndividualOrGroupCode = code.GroupCode.IdentificationCode;
							productCode.IsGroup = true;
						}
					}
					else
					{
						transportCode = await _trueMarkCodeRepository.FindParentTransportCode(code.IndividualCode, cancellationToken);
						if(transportCode != null)
						{
							productCode.TransportCode = transportCode.RawCode;
						}
						else
						{
							productCode.IndividualOrGroupCode = code.IndividualCode.IdentificationCode;
							productCode.IsGroup = false;
						}
					}

					productCodes.Add(productCode);
				}

				var price = nomenclature.GetPurchasePriceOnDate(DateTime.Now);
				if(price == 0m)
				{
					price = nomenclature.GetPrice(quantity);
				}
				else
				{
					var additionalPercent = _edoTransferSettings.AdditionalPurchasePricePrecentForTransfer;
					price *= 1 + additionalPercent / 100;
				}

				var sum = price * quantity;
				
				var vatRateVersion = transferOrder.Seller != null && transferOrder.Seller.IsUsnMode 
					? transferOrder.Seller.GetActualVatRateVersion(transferOrder.Date)
					: nomenclature.GetActualVatRateVersion(transferOrder.Date);
				
				if(vatRateVersion == null)
				{
					throw new InvalidOperationException($"У товара #{nomenclature.Id} отсутствует версия НДС на дату трансфера заказа #{transferOrder.Date}");
				}
				
				var includeVat = vatRateVersion.VatRate.VatRateValue != 0 
					? Math.Round(sum * vatRateVersion.VatRate.VatNumericValue / (1 + vatRateVersion.VatRate.VatNumericValue), 2) 
					: 0;
				var valueAddedTax = vatRateVersion.VatRate.VatRateValue != 0 
					? vatRateVersion.VatRate.VatNumericValue 
					: (decimal?)null;

				var product = new ProductInfo
				{
					Name = nomenclature.OfficialName,
					IsService = nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId,
					UnitName = nomenclature.Unit.Name,
					OKEI = nomenclature.Unit.OKEI,
					Code = nomenclature.Id.ToString(),
					Count = quantity,
					Price = price,
					IncludeVat = includeVat,
					ValueAddedTax = valueAddedTax,
					DiscountMoney = 0,
					TrueMarkCodes = productCodes
				};

				products.Add(product);
			}

			return products;
		}

		private NomenclatureEntity GetNomenclatureByGtin(IUnitOfWork uow, string gtin)
		{
			GtinEntity gtinAlias = null;

			var nomenclature = uow.Session.QueryOver<NomenclatureEntity>()
				.Left.JoinAlias(x => x.Gtins, () => gtinAlias)
				.Where(() => gtinAlias.GtinNumber == gtin)
				.List().FirstOrDefault();

			return nomenclature;
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
