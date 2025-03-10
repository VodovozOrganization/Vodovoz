﻿using Edo.Contracts.Messages.Dto;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Settings.Nomenclature;

namespace Edo.Docflow.Factories
{
	public class UniversalTransferDocumentInfoFactory : IUniversalTransferDocumentInfoFactory
	{
		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public UniversalTransferDocumentInfoFactory(
			IUnitOfWorkFactory uowFactory,
			INomenclatureSettings nomenclatureSettings)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
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
				// Не понятно, что нужно сюда добавлять. Посмотрел в имеющемся контейнере. Нигде ТекстИнфТип не встречается
				//AdditionalInformation = GettAdditionalInformation(transferOrder)
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
			new List<PaymentInfo>
			{
				new PaymentInfo
				{
					PaymentNum = transferOrder.Id.ToString(),
					PaymentDate = transferOrder.Date.ToString(_dateFormatString),
				}
			};

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
			var codes = transferOrder.TrueMarkCodes.Select(x => x.TrueMarkCode);

			var codesWithoutGtins = codes.Where(x => string.IsNullOrWhiteSpace(x.GTIN));
			if(codesWithoutGtins.Any())
			{
				var errorMessage = $"Среди переданных кодов имеются коды с незаполненным значением GTIN. Id: {string.Join(", ", codesWithoutGtins)}";
				throw new InvalidOperationException(errorMessage);
			}

			var gtinGroups = codes
				.GroupBy(x => x.GTIN)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach(var gtinGroup in gtinGroups)
			{
				var nomenclature = GetNomenclatureByGtin(uow, gtinGroup.Key);

				if(nomenclature is null)
				{
					var errorMessage = $"Номенклатура с указаннымм значением GTIN не найдена. GTIN: {gtinGroup.Key}";
					throw new InvalidOperationException(errorMessage);
				}

				var productCount = gtinGroup.Value.Count;

				var price = nomenclature.GetPrice(productCount);
				var includeVat = Math.Round(price * nomenclature.VatNumericValue / (1 + nomenclature.VatNumericValue), 2);

				var product = new ProductInfo
				{
					Name = nomenclature.Name,
					IsService = nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId,
					UnitName = nomenclature.Unit.Name,
					OKEI = nomenclature.Unit.OKEI,
					Code = nomenclature.Id.ToString(),
					Count = productCount,
					Price = nomenclature.GetPrice(productCount),
					IncludeVat = includeVat,
					ValueAddedTax = nomenclature.VatNumericValue,
					DiscountMoney = 0,
					TrueMarkCodes = gtinGroup.Value.Select(x => x.RawCode),
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
				.SingleOrDefault();

			return nomenclature;
		}
	}
}
