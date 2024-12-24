using Core.Infrastructure;
using Edo.Transport.Messages.Dto;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Settings.Nomenclature;

namespace Edo.Docflow.Factories
{
	public class OrderUpdInfoFactory
	{
		private const string _dateFormatString = "dd.MM.yyyy";

		private readonly IUnitOfWork _uow;
		private readonly IGenericRepository<NomenclatureEntity> _nomenclatureRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public OrderUpdInfoFactory(
			IUnitOfWork uow,
			IGenericRepository<NomenclatureEntity> nomenclatureRepository,
			INomenclatureSettings nomenclatureSettings
			)
		{
			_uow = uow ?? throw new System.ArgumentNullException(nameof(uow));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}

		public UniversalTransferDocumentInfo CreateUniversalTransferDocumentInfo(OrderEntity order, IEnumerable<TrueMarkWaterIdentificationCode> codes)
		{
			return ConvertTransferOrderToUniversalTransferDocumentInfo(order, codes);
		}

		private UniversalTransferDocumentInfo ConvertTransferOrderToUniversalTransferDocumentInfo(OrderEntity order, IEnumerable<TrueMarkWaterIdentificationCode> codes)
		{
			var products = GetProducts(order, codes);

			var document = new UniversalTransferDocumentInfo
			{
				Number = order.Id,
				Sum = products.Sum(x => x.Sum),
				Date = order.DeliveryDate.Value,
				Seller = GetSellerInfo(order),
				Customer = GetCustomerInfo(order),
				Consignee = GetConsigneeInfo(order),
				DocumentConfirmingShipment = GetDocumentConfirmingShipmentInfo(order),
				BasisShipment = GetBasisShipmentInfo(order),
				Payments = GetPayments(order),
				Products = products,
				AdditionalInformation = GettAdditionalInformation(order)
			};

			return document;
		}

		private SellerInfo GetSellerInfo(OrderEntity order) =>
			new SellerInfo { Organization = GetOrganizationInfo(order.Contract.Organization) };

		private CustomerInfo GetCustomerInfo(OrderEntity order) =>
			new CustomerInfo { Organization = GetCounterpartyInfo(order.Client) };

		private ConsigneeInfo GetConsigneeInfo(OrderEntity order) =>
			new ConsigneeInfo { Organization = GetCounterpartyInfo(order.Client) };

		private DocumentConfirmingShipmentInfo GetDocumentConfirmingShipmentInfo(OrderEntity order) =>
			new DocumentConfirmingShipmentInfo
			{
				Number = order.Id.ToString(),
				Date = order.DeliveryDate.Value.ToString(_dateFormatString)
			};

		private BasisShipmentInfo GetBasisShipmentInfo(OrderEntity order) =>
			new BasisShipmentInfo
			{
				Number = order.Id.ToString(),
				Date = order.DeliveryDate.Value.ToString(_dateFormatString)
			};

		private IEnumerable<PaymentInfo> GetPayments(OrderEntity order) =>
			new List<PaymentInfo>
			{
				new PaymentInfo
				{
					PaymentNum = order.Id.ToString(),
					PaymentDate = order.DeliveryDate.Value.ToString(_dateFormatString),
				}
			};

		private IEnumerable<UpdAdditionalInfo> GettAdditionalInformation(OrderEntity order) =>
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

		private OrganizationInfo GetCounterpartyInfo(CounterpartyEntity counterparty)
		{
			var oganizationInfo = new OrganizationInfo
			{
				Name = counterparty.FullName,
				Address = new AddressInfo
				{
					Address = counterparty.JurAddress,
				},
				Inn = counterparty.INN,
				Kpp = counterparty.KPP,
				EdoAccountId = counterparty.PersonalAccountIdInEdo,
			};

			return oganizationInfo;
		}

		private IEnumerable<ProductInfo> GetProducts(OrderEntity order, IEnumerable<TrueMarkWaterIdentificationCode> codes)
		{
			var products = new List<ProductInfo>();

			var hasCodesWithoutGtins = codes.Any(x => x.GTIN.IsNullOrWhiteSpace());
			if(hasCodesWithoutGtins)
			{
				var errorMessage = $"Среди переданных кодов имеются коды с незаполненным значением GTIN. Id: {string.Join(", ", hasCodesWithoutGtins)}";
				throw new InvalidOperationException(errorMessage);
			}

			foreach(var orderItem in order.OrderItems)
			{
				var nomenclature = orderItem.Nomenclature;

				var orderItemsCodes = codes.Where(x => x.GTIN == nomenclature.Gtin).Select(x => x.FullCode);

				var product = new ProductInfo
				{
					Name = nomenclature.Name,
					IsService = nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId,
					UnitName = nomenclature.Unit.Name,
					OKEI = nomenclature.Unit.OKEI,
					Code = nomenclature.Id.ToString(),
					Count = orderItem.Count,
					Price = orderItem.Price,
					IncludeVat = orderItem.IncludeNDS ?? 0,
					ValueAddedTax = orderItem.ValueAddedTax,
					DiscountMoney = orderItem.DiscountMoney,
					TrueMarkCodes = orderItemsCodes,
				};

				products.Add(product);
			}

			return products;
		}

		private NomenclatureEntity GetNomenclatureByGtin(string gtin) =>
			_nomenclatureRepository.Get(_uow, x => x.Gtin == gtin).FirstOrDefault();
	}
}
