using Edo.Contracts.Messages.Dto;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Settings.Nomenclature;

namespace Edo.Docflow.Factories
{
	public class OrderUpdInfoFactory
	{
		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<NomenclatureEntity> _nomenclatureRepository;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public OrderUpdInfoFactory(
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<NomenclatureEntity> nomenclatureRepository,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			INomenclatureSettings nomenclatureSettings
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}

		public UniversalTransferDocumentInfo CreateUniversalTransferDocumentInfo(DocumentEdoTask documentEdoTask)
		{
			return ConvertTransferOrderToUniversalTransferDocumentInfo(documentEdoTask);
		}

		private UniversalTransferDocumentInfo ConvertTransferOrderToUniversalTransferDocumentInfo(DocumentEdoTask documentEdoTask)
		{
			var order = documentEdoTask.OrderEdoRequest.Order;

			var products = GetProducts(documentEdoTask);

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

		private IEnumerable<ProductInfo> GetProducts(DocumentEdoTask documentEdoTask)
		{
			var inventPositions = documentEdoTask.UpdInventPositions;

			var products = new List<ProductInfo>();

			foreach(var inventPosition in inventPositions)
			{
				var orderItem = inventPosition.AssignedOrderItem;
				var nomenclature = orderItem.Nomenclature;
				var isService = nomenclature.Category == NomenclatureCategory.master
					|| nomenclature.Category == NomenclatureCategory.service;

				var codesInfo = new List<ProductCodeInfo>();
				foreach(var code in inventPosition.Codes)
				{
					if(code.GroupCode != null)
					{
						var codeValue = code.GroupCode.IdentificationCode;
						var codeInfo = new ProductCodeInfo
						{
							IsGroup = true,
							IndividualOrGroupCode = codeValue
						};

						var transportCode = _trueMarkCodeRepository.FindParentTransportCode(code.GroupCode);
						if(transportCode != null)
						{
							codeInfo.TransportCode = transportCode.RawCode;
						}

						codesInfo.Add(codeInfo);
						continue;
					}

					if(code.IndividualCode != null)
					{
						var codeValue = code.IndividualCode.IdentificationCode;
						var codeInfo = new ProductCodeInfo
						{
							IsGroup = false,
							IndividualOrGroupCode = codeValue
						};

						var transportCode = _trueMarkCodeRepository.FindParentTransportCode(code.IndividualCode);
						if(transportCode != null)
						{
							codeInfo.TransportCode = transportCode.RawCode;
						}

						codesInfo.Add(codeInfo);
						continue;
					}

					throw new InvalidOperationException("Должен быть обязательно заполнен код в позиции УПД документа");
				}

				if(orderItem.Count != inventPosition.Codes.Sum(x => x.Quantity))
				{
					throw new InvalidOperationException("Количество товара в позиции УПД не совпадает с количеством кодов");
				}

				var product = new ProductInfo
				{
					Name = nomenclature.Name,
					IsService = isService,
					UnitName = nomenclature.Unit.Name,
					OKEI = nomenclature.Unit.OKEI,
					Code = nomenclature.Id.ToString(),
					Count = orderItem.Count,
					Price = orderItem.Price,
					IncludeVat = orderItem.IncludeNDS ?? 0,
					ValueAddedTax = orderItem.ValueAddedTax,
					DiscountMoney = orderItem.DiscountMoney,
					TrueMarkCodes = codesInfo
				};

				products.Add(product);
			}

			return products;
		}
	}
}
