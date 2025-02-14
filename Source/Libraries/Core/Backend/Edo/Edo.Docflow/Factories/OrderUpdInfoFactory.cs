using Core.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Edo.Contracts.Messages.Dto;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Docflow.Factories
{
	public class OrderUpdInfoFactory
	{
		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<NomenclatureEntity> _nomenclatureRepository;
		private readonly IGenericRepository<OrderUpdOperation> _orderUpdOperationRepository;

		public OrderUpdInfoFactory(
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<NomenclatureEntity> nomenclatureRepository,
			IGenericRepository<OrderUpdOperation> orderUpdOperationRepository
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_orderUpdOperationRepository = orderUpdOperationRepository ?? throw new ArgumentNullException(nameof(orderUpdOperationRepository));
		}

		public UniversalTransferDocumentInfo CreateUniversalTransferDocumentInfo(OrderEntity order, IEnumerable<TrueMarkWaterIdentificationCode> codes)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(nameof(OrderUpdInfoFactory)))
			{
				var orderUpdOperation = _orderUpdOperationRepository.Get(uow, x => x.OrderId == order.Id).FirstOrDefault();

				return ConvertTransferOrderToUniversalTransferDocumentInfo(orderUpdOperation, codes);
			}
		}

		private UniversalTransferDocumentInfo ConvertTransferOrderToUniversalTransferDocumentInfo(
			OrderUpdOperation orderUpdOperation,
			IEnumerable<TrueMarkWaterIdentificationCode> codes)
		{
			if(orderUpdOperation is null)
			{
				throw new ArgumentNullException(nameof(orderUpdOperation));
			}

			var products = GetProducts(orderUpdOperation, codes);

			var document = new UniversalTransferDocumentInfo
			{
				DocumentId = Guid.NewGuid(),
				Number = orderUpdOperation.OrderId,
				Sum = products.Sum(x => x.Sum),
				Date = orderUpdOperation.OrderDeliveryDate,
				Seller = GetSellerInfo(orderUpdOperation),
				Customer = GetCustomerInfo(orderUpdOperation),
				Consignee = GetConsigneeInfo(orderUpdOperation),
				DocumentConfirmingShipment = GetDocumentConfirmingShipmentInfo(orderUpdOperation),
				BasisShipment = GetBasisShipmentInfo(orderUpdOperation),
				Payments = GetPayments(orderUpdOperation),
				Products = products,
				AdditionalInformation = GetAdditionalInformation(orderUpdOperation)
			};

			return document;
		}

		private SellerInfo GetSellerInfo(OrderUpdOperation orderUpdOperation) =>
			new SellerInfo { Organization = GetOrganizationInfo(orderUpdOperation) };

		private CustomerInfo GetCustomerInfo(OrderUpdOperation orderUpdOperation) =>
			new CustomerInfo { Organization = GetCustomerOrganizationInfo(orderUpdOperation) };

		private ConsigneeInfo GetConsigneeInfo(OrderUpdOperation orderUpdOperation)
		{
			var consignee = new ConsigneeInfo
			{
				Organization = GetConsigneeOrganizationInfo(orderUpdOperation)
			};

			consignee.CargoReceiver = orderUpdOperation.ConsigneeAddress;

			return consignee;
		}

		private DocumentConfirmingShipmentInfo GetDocumentConfirmingShipmentInfo(OrderUpdOperation orderUpdOperation) =>
			new DocumentConfirmingShipmentInfo
			{
				Number = orderUpdOperation.OrderId.ToString(),
				Date = orderUpdOperation.OrderDeliveryDate.ToString(_dateFormatString)
			};

		private BasisShipmentInfo GetBasisShipmentInfo(OrderUpdOperation orderUpdOperation)
		{
			var basis = new BasisShipmentInfo
			{
				Document = orderUpdOperation.ClientContractDocumentName,
				Number = orderUpdOperation.ClientContractNumber,
				Date = orderUpdOperation.ClientContractDate.ToString(_dateFormatString)
			};

			return basis;
		}

		private IEnumerable<PaymentInfo> GetPayments(OrderUpdOperation orderUpdOperation)
		{
			var payments = new List<PaymentInfo>();

			foreach(var orderUpdOperationPayment in orderUpdOperation.Payments)
			{
				var payment = new PaymentInfo
				{
					PaymentNum = orderUpdOperationPayment.PaymentNum,
					PaymentDate = orderUpdOperationPayment.PaymentDate.ToString(_dateFormatString)
				};
				payments.Add(payment);
			}

			if(!payments.Any())
			{
				payments.Add(new PaymentInfo
				{
					PaymentNum = orderUpdOperation.OrderId.ToString(),
					PaymentDate = orderUpdOperation.OrderDeliveryDate.ToString(_dateFormatString),
				});
			}

			return payments;
		}

		private IEnumerable<UpdAdditionalInfo> GetAdditionalInformation(OrderUpdOperation orderUpdOperation)
		{
			var additionalInformation = new List<UpdAdditionalInfo>();

			if(orderUpdOperation.IsOrderForOwnNeeds)
			{
				additionalInformation.Add(new UpdAdditionalInfo
				{
					Id = "СвВыбытияМАРК",
					Value = "1"
				});
			}

			if(orderUpdOperation.CounterpartyExternalOrderId.HasValue)
			{
				additionalInformation.Add(new UpdAdditionalInfo
				{
					Id = "номер_заказа",
					Value = $"N{orderUpdOperation.CounterpartyExternalOrderId}"
				});
			}

			return additionalInformation;
		}

		private OrganizationInfo GetOrganizationInfo(OrderUpdOperation orderUpdOperation)
		{
			var organizationInfo = new OrganizationInfo
			{
				Name = orderUpdOperation.OrganizationName,
				Address = new AddressInfo
				{
					Address = orderUpdOperation.OrganizationAddress,
				},
				Inn = orderUpdOperation.OrganizationInn,
				Kpp = orderUpdOperation.OrganizationKpp,
				EdoAccountId = orderUpdOperation.OrganizationTaxcomEdoAccountId,
			};

			return organizationInfo;
		}

		private OrganizationInfo GetCustomerOrganizationInfo(OrderUpdOperation orderUpdOperation)
		{
			var organizationInfo = GetCounterpartyOrganizationInfo(
				orderUpdOperation.ClientName,
				orderUpdOperation.ClientInn,
				orderUpdOperation.ClientKpp,
				orderUpdOperation.ClientAddress,
				orderUpdOperation.ClientPersonalAccountIdInEdo);

			return organizationInfo;
		}

		private OrganizationInfo GetConsigneeOrganizationInfo(OrderUpdOperation orderUpdOperation)
		{
			var organizationInfo = GetCounterpartyOrganizationInfo(
				orderUpdOperation.ConsigneeName,
				orderUpdOperation.ConsigneeInn,
				orderUpdOperation.ConsigneeKpp,
				orderUpdOperation.ConsigneeAddress,
				orderUpdOperation.ClientPersonalAccountIdInEdo);

			return organizationInfo;
		}

		private OrganizationInfo GetCounterpartyOrganizationInfo(
			string name,
			string inn,
			string kpp,
			string address,
			string accountEdo)
		{
			return new OrganizationInfo
			{
				Name = name,
				Address = new AddressInfo
				{
					Address = address,
				},
				Inn = inn,
				Kpp = kpp,
				EdoAccountId = accountEdo,
			};
		}

		private IEnumerable<ProductInfo> GetProducts(OrderUpdOperation orderUpdOperation, IEnumerable<TrueMarkWaterIdentificationCode> codes)
		{
			var products = new List<ProductInfo>();

			var hasCodesWithoutGtins = codes.Any(x => x.GTIN.IsNullOrWhiteSpace());
			if(hasCodesWithoutGtins)
			{
				var errorMessage = $"Среди переданных кодов имеются коды с незаполненным значением GTIN. Id: {string.Join(", ", hasCodesWithoutGtins)}";
				throw new InvalidOperationException(errorMessage);
			}

			foreach(var updOperationProduct in orderUpdOperation.Goods)
			{
				var orderItemsCodes =
					codes
						.Where(x => x.GTIN == updOperationProduct.Gtin)
						.Select(x => x.ConvertToIdentificationCode());

				var product = new ProductInfo
				{
					Name = updOperationProduct.NomenclatureName,
					IsService = updOperationProduct.IsService,
					UnitName = updOperationProduct.MeasurementUnitName,
					OKEI = updOperationProduct.OKEI,
					Code = updOperationProduct.NomenclatureId.ToString(),
					Count = updOperationProduct.Count,
					Price = updOperationProduct.ItemPrice,
					IncludeVat = updOperationProduct.IncludeVat,
					ValueAddedTax = updOperationProduct.ValueAddedTax,
					DiscountMoney = updOperationProduct.ItemDiscountMoney,
					TrueMarkCodes = orderItemsCodes
				};

				products.Add(product);
			}

			return products;
		}

		private NomenclatureEntity GetNomenclatureByGtin(IUnitOfWork uow, string gtin) =>
			_nomenclatureRepository.Get(uow, x => x.Gtin == gtin).FirstOrDefault();

		private NomenclatureEntity GetNomenclatureById(IUnitOfWork uow, int nomenclatureId) =>
			_nomenclatureRepository.Get(uow, x => x.Id == nomenclatureId).FirstOrDefault();
	}
}
