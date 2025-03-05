using Core.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Edo.Contracts.Messages.Dto;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Edo;
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
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<NomenclatureEntity> _nomenclatureRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public OrderUpdInfoFactory(
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<NomenclatureEntity> nomenclatureRepository,
			INomenclatureSettings nomenclatureSettings
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
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
				DocumentId = Guid.NewGuid(),
				Number = order.Id,
				Sum = products.Sum(x => x.Sum),
				Date = order.DeliveryDate.Value,
				Seller = GetSellerInfo(order),
				Customer = GetCustomerInfo(order),
				Consignee = GetConsigneeInfo(order.Client, order.DeliveryPoint),
				DocumentConfirmingShipment = GetDocumentConfirmingShipmentInfo(order),
				BasisShipment = GetBasisShipmentInfo(order.Client, order.Contract),
				Payments = GetPayments(order),
				Products = products,
				AdditionalInformation = GetAdditionalInformation(order, products)
			};

			return document;
		}

		private SellerInfo GetSellerInfo(OrderEntity order) =>
			new SellerInfo { Organization = GetOrganizationInfo(order.Contract.Organization) };

		private CustomerInfo GetCustomerInfo(OrderEntity order) =>
			new CustomerInfo { Organization = GetCustomerOrganizationInfo(order.Client) };

		private ConsigneeInfo GetConsigneeInfo(CounterpartyEntity counterparty, DeliveryPointEntity deliveryPoint)
		{
			var consignee = new ConsigneeInfo
			{
				Organization = GetConsigneeOrganizationInfo(counterparty, deliveryPoint)
			};
			
			if(!string.IsNullOrWhiteSpace(counterparty.CargoReceiver) && counterparty.UseSpecialDocFields)
			{
				consignee.CargoReceiver = counterparty.CargoReceiver;
			}
			
			return consignee;
		}

		private DocumentConfirmingShipmentInfo GetDocumentConfirmingShipmentInfo(OrderEntity order) =>
			new DocumentConfirmingShipmentInfo
			{
				Number = order.Id.ToString(),
				Date = order.DeliveryDate.Value.ToString(_dateFormatString)
			};

		private BasisShipmentInfo GetBasisShipmentInfo(CounterpartyEntity counterparty, CounterpartyContractEntity counterpartyContract)
		{
			var basis = new BasisShipmentInfo();

			if(counterparty.UseSpecialDocFields
			   && !string.IsNullOrWhiteSpace(counterparty.SpecialContractName)
			   && !string.IsNullOrWhiteSpace(counterparty.SpecialContractNumber)
			   && counterparty.SpecialContractDate.HasValue)
			{
				basis.Document = counterparty.SpecialContractName;
				basis.Number = counterparty.SpecialContractNumber;
				basis.Date = counterparty.SpecialContractDate.Value.ToString(_dateFormatString);
				return basis;
			}
			
			if(counterparty.UseSpecialDocFields && !string.IsNullOrWhiteSpace(counterparty.SpecialContractName))
			{
				return basis;
			}

			if(counterpartyContract != null)
			{
				basis.Document = "Договор";
				basis.Number = counterpartyContract.Number;
				basis.Date = counterpartyContract.IssueDate.ToString(_dateFormatString);
			}

			return basis;
		}

		private IEnumerable<PaymentInfo> GetPayments(OrderEntity order) =>
			new List<PaymentInfo>
			{
				new PaymentInfo
				{
					PaymentNum = order.Id.ToString(),
					PaymentDate = order.DeliveryDate.Value.ToString(_dateFormatString),
				}
			};

		private IEnumerable<UpdAdditionalInfo> GetAdditionalInformation(OrderEntity order, IEnumerable<ProductInfo> products)
		{
			var additionalInformation = new List<UpdAdditionalInfo>();
			
			if(products.Any(x => x.TrueMarkCodes.Any())
				&& order.Client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				additionalInformation.Add(new UpdAdditionalInfo
				{
					Id = "СвВыбытияМАРК",
					Value = "1"
				});
			}

			if(order.CounterpartyExternalOrderId != null && order.Client.UseSpecialDocFields)
			{
				additionalInformation.Add(new UpdAdditionalInfo
				{
					Id = "номер_заказа",
					Value = $"N{ order.CounterpartyExternalOrderId }"
				});
			}
			
			return additionalInformation;
		}

		private OrganizationInfo GetOrganizationInfo(OrganizationEntity organization)
		{
			var organizationInfo = new OrganizationInfo
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

			return organizationInfo;
		}
		
		private OrganizationInfo GetCustomerOrganizationInfo(CounterpartyEntity counterparty)
		{
			if(counterparty.PersonType == PersonType.natural)
			{
				throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}
			
			var clientName = counterparty.FullName;
			var clientKpp = counterparty.KPP;

			if(counterparty.UseSpecialDocFields)
			{
				if(!string.IsNullOrWhiteSpace(counterparty.SpecialCustomer))
				{
					clientName = counterparty.SpecialCustomer;
				}
				if(!string.IsNullOrWhiteSpace(counterparty.PayerSpecialKPP))
				{
					clientKpp = counterparty.PayerSpecialKPP;
				}
			}
			
			var organizationInfo = GetCounterpartyOrganizationInfo(
				clientName,
				counterparty.INN,
				clientKpp,
				counterparty.JurAddress,
				counterparty.PersonalAccountIdInEdo);

			return organizationInfo;
		}
		
		private OrganizationInfo GetConsigneeOrganizationInfo(CounterpartyEntity counterparty, DeliveryPointEntity deliveryPoint)
		{
			var address = string.Empty;
			var kpp = string.Empty;
			
			switch(counterparty.CargoReceiverSource)
			{
				case CargoReceiverSource.FromDeliveryPoint:
					address = deliveryPoint != null ? deliveryPoint.ShortAddress : counterparty.JurAddress;
					kpp = deliveryPoint?.KPP ?? counterparty.KPP;
					
					return GetCounterpartyOrganizationInfo(
						counterparty.FullName,
						counterparty.INN,
						kpp,
						address,
						counterparty.PersonalAccountIdInEdo);
				case CargoReceiverSource.Special:
					if(!string.IsNullOrWhiteSpace(counterparty.CargoReceiver) && counterparty.UseSpecialDocFields)
					{
						address = counterparty.CargoReceiver;
						kpp = counterparty.PayerSpecialKPP ?? counterparty.KPP;
						
						return GetCounterpartyOrganizationInfo(
							counterparty.FullName,
							counterparty.INN,
							kpp,
							address,
							counterparty.PersonalAccountIdInEdo);
					}
					return GetCustomerOrganizationInfo(counterparty);
				default:
					return GetCustomerOrganizationInfo(counterparty);
			}
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

				var orderItemsCodes =
					codes
						.Where(x => nomenclature.Gtins.Any(gtin => gtin.GtinNumber == x.GTIN))
						.Select(x => x.IdentificationCode);

				var product = new ProductInfo
				{
					Name = nomenclature.Name,
					IsService =
						nomenclature.Category == NomenclatureCategory.master
						|| nomenclature.Category == NomenclatureCategory.service,
					UnitName = nomenclature.Unit.Name,
					OKEI = nomenclature.Unit.OKEI,
					Code = nomenclature.Id.ToString(),
					Count = orderItem.Count,
					Price = orderItem.Price,
					IncludeVat = orderItem.IncludeNDS ?? 0,
					ValueAddedTax = orderItem.ValueAddedTax,
					DiscountMoney = orderItem.DiscountMoney,
					TrueMarkCodes = orderItemsCodes
				};

				products.Add(product);
			}

			return products;
		}
	}
}
