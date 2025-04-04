﻿using Edo.Contracts.Messages.Dto;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Nomenclature;

namespace Edo.Docflow.Factories
{
	public class TransferOrderUpdInfoFactory
	{
		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly IEdoTransferSettings _edoTransferSettings;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public TransferOrderUpdInfoFactory(
			IUnitOfWorkFactory uowFactory,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			IEdoTransferSettings edoTransferSettings,
			INomenclatureSettings nomenclatureSettings)
		{
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}

		public UniversalTransferDocumentInfo CreateUniversalTransferDocumentInfo(IUnitOfWork uow, TransferOrder transferOrder)
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

			return ConvertTransferOrderToUniversalTransferDocumentInfo(uow, transferOrder);
		}

		private UniversalTransferDocumentInfo ConvertTransferOrderToUniversalTransferDocumentInfo(IUnitOfWork uow, TransferOrder transferOrder)
		{
			var products = GetProducts(uow, transferOrder);

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
				EdoAccountId = organization.TaxcomEdoAccountId,
			};

			return oganizationInfo;
		}

		private IEnumerable<ProductInfo> GetProducts(IUnitOfWork uow, TransferOrder transferOrder)
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
						transportCode = _trueMarkCodeRepository.FindParentTransportCode(code.GroupCode);
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
						transportCode = _trueMarkCodeRepository.FindParentTransportCode(code.IndividualCode);
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
				var includeVat = Math.Round(sum * nomenclature.VatNumericValue / (1 + nomenclature.VatNumericValue), 2);

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
					ValueAddedTax = nomenclature.VatNumericValue,
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
	}
}
