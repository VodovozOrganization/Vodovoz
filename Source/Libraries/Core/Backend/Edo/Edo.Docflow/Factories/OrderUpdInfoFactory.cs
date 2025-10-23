using Edo.Contracts.Messages.Dto;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Docflow.Factories
{
	public class OrderUpdInfoFactory : IDisposable
	{
		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly ICounterpartyEdoAccountEntityController _edoAccountEntityController;

		public OrderUpdInfoFactory(
			IUnitOfWork uow,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			ICounterpartyEdoAccountEntityController edoAccountEntityController
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_edoAccountEntityController =
				edoAccountEntityController ?? throw new ArgumentNullException(nameof(edoAccountEntityController));
		}

		public async Task<UniversalTransferDocumentInfo> CreateUniversalTransferDocumentInfo(
			DocumentEdoTask documentEdoTask, 
			IEnumerable<PaymentEntity> payments,
			CancellationToken cancellationToken
			)
		{
			var order = documentEdoTask.OrderEdoRequest.Order;

			// предзагрузка для ускорения
			var productCodes = await _uow.Session.QueryOver<TrueMarkProductCode>()
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Where(x => x.CustomerEdoRequest.Id == documentEdoTask.OrderEdoRequest.Id)
				.ListAsync();

			var sourceCodes = productCodes
				.Where(x => x.SourceCode != null)
				.Select(x => x.SourceCode);

			var resultCodes = productCodes
				.Where(x => x.ResultCode != null)
				.Select(x => x.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await _trueMarkCodeRepository.PreloadCodes(codesToPreload, cancellationToken);

			var products = await GetProducts(documentEdoTask, cancellationToken);
			var edoAccount =
				_edoAccountEntityController.GetDefaultCounterpartyEdoAccountByOrganizationId(order.Client, order.Contract.Organization.Id);

			var document = new UniversalTransferDocumentInfo
			{
				DocumentId = Guid.NewGuid(),
				Number = order.Id,
				Sum = products.Sum(x => x.Sum),
				Date = order.DeliveryDate.Value,
				Seller = GetSellerInfo(order),
				Customer = GetCustomerInfo(order, edoAccount),
				Consignee = GetConsigneeInfo(order.Client, edoAccount, order.DeliveryPoint),
				DocumentConfirmingShipment = GetDocumentConfirmingShipmentInfo(order),
				GovContract = string.IsNullOrWhiteSpace(order.Client.GovContract) ? null : order.Client.GovContract,
				BasisShipment = GetBasisShipmentInfo(order.Client, order.Contract),
				Payments = GetPayments(payments),
				Products = products,
				AdditionalInformation = GetAdditionalInformation(order, products)
			};

			return document;
		}

		private SellerInfo GetSellerInfo(OrderEntity order) =>
			new SellerInfo { Organization = GetOrganizationInfo(order.Contract.Organization) };

		private CustomerInfo GetCustomerInfo(OrderEntity order, CounterpartyEdoAccountEntity edoAccount) =>
			new CustomerInfo { Organization = GetCustomerOrganizationInfo(order.Client, edoAccount) };

		private ConsigneeInfo GetConsigneeInfo(
			CounterpartyEntity counterparty, CounterpartyEdoAccountEntity edoAccount, DeliveryPointEntity deliveryPoint)
		{
			var consignee = new ConsigneeInfo
			{
				Organization = GetConsigneeOrganizationInfo(counterparty, edoAccount, deliveryPoint)
			};
			
			if(!string.IsNullOrWhiteSpace(counterparty.CargoReceiver) && counterparty.UseSpecialDocFields)
			{
				consignee.CargoReceiver = counterparty.CargoReceiver;
			}
			
			return consignee;
		}

		private DocumentConfirmingShipmentInfo GetDocumentConfirmingShipmentInfo(OrderEntity order)
		{
			var documentConfirmingShipmentInfo = new DocumentConfirmingShipmentInfo();

			if(order.Client.UseSpecialDocFields
				&& !string.IsNullOrWhiteSpace(order.Client.SpecialContractName)
				&& !string.IsNullOrWhiteSpace(order.Client.SpecialContractNumber)
				&& order.Client.SpecialContractDate.HasValue)
			{
				documentConfirmingShipmentInfo.Document = order.Client.SpecialContractName;
				documentConfirmingShipmentInfo.Number = order.Client.SpecialContractNumber;
				documentConfirmingShipmentInfo.Date = $"{order.Client.SpecialContractDate.Value:dd.MM.yyyy}";
				return documentConfirmingShipmentInfo;
			}

			if(order.Client.UseSpecialDocFields && !string.IsNullOrWhiteSpace(order.Client.SpecialContractName))
			{
				documentConfirmingShipmentInfo.Document = "Без документа-основания";
				return documentConfirmingShipmentInfo;
			}

			if(order.Contract != null)
			{
				documentConfirmingShipmentInfo.Document = "Договор";
				documentConfirmingShipmentInfo.Number = order.Contract.Number;
				documentConfirmingShipmentInfo.Date = $"{order.Contract.IssueDate:dd.MM.yyyy}";
			}
			else
			{
				documentConfirmingShipmentInfo.Document = "Без документа-основания";
			}

			return documentConfirmingShipmentInfo;
		}

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

		private IEnumerable<PaymentInfo> GetPayments(IEnumerable<PaymentEntity> payments)
		{
			return payments.Select(payment => new PaymentInfo
				{
					PaymentNum = payment.PaymentNum.ToString(),
					PaymentDate = payment.Date.ToString(_dateFormatString),
				}).ToList();
		}

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

			if(!string.IsNullOrWhiteSpace(order.CounterpartyExternalOrderId) && order.Client.UseSpecialDocFields)
			{
				additionalInformation.Add(new UpdAdditionalInfo
				{
					Id = "номер_заказа",
					Value = order.Client.SpecialNomenclatures.Any() ? $"{order.CounterpartyExternalOrderId}" : $"N{order.CounterpartyExternalOrderId}"
				});
			}

			additionalInformation.Add(new UpdAdditionalInfo
			{
				Id = "номер_отгрузки",
				Value = $"{order.Id}"
			});

			if(order.DeliveryDate.HasValue)
			{
				additionalInformation.Add(new UpdAdditionalInfo
				{
					Id = "дата_заказа",
					Value = $"{order.DeliveryDate.Value:dd.MM.yyyy}"
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
				EdoAccountId = organization.TaxcomEdoSettings.EdoAccount,
			};

			return organizationInfo;
		}
		
		private OrganizationInfo GetCustomerOrganizationInfo(CounterpartyEntity counterparty, CounterpartyEdoAccountEntity edoAccount)
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
				edoAccount.PersonalAccountIdInEdo);

			return organizationInfo;
		}
		
		private OrganizationInfo GetConsigneeOrganizationInfo(
			CounterpartyEntity counterparty,
			CounterpartyEdoAccountEntity edoAccount,
			DeliveryPointEntity deliveryPoint)
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
						edoAccount.PersonalAccountIdInEdo);
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
							edoAccount.PersonalAccountIdInEdo);
					}
					return GetCustomerOrganizationInfo(counterparty, edoAccount);
				default:
					return GetCustomerOrganizationInfo(counterparty, edoAccount);
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

		private async Task<IEnumerable<ProductInfo>> GetProducts(
			DocumentEdoTask documentEdoTask, 
			CancellationToken cancellationToken
			)
		{
			var inventPositions = documentEdoTask.UpdInventPositions;

			var client = documentEdoTask.OrderEdoRequest.Order.Client;
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

						var transportCode = await _trueMarkCodeRepository.FindParentTransportCode(code.GroupCode, cancellationToken);
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

						var transportCode = await _trueMarkCodeRepository.FindParentTransportCode(code.IndividualCode, cancellationToken);
						if(transportCode != null)
						{
							codeInfo.TransportCode = transportCode.RawCode;
						}

						codesInfo.Add(codeInfo);
						continue;
					}

					throw new InvalidOperationException("Должен быть обязательно заполнен код в позиции УПД документа");
				}

				var product = new ProductInfo
				{
					Name = nomenclature.OfficialName,
					IsService = isService,
					UnitName = nomenclature.Unit.Name,
					OKEI = nomenclature.Unit.OKEI,
					Code = nomenclature.Id.ToString(),
					Count = orderItem.CurrentCount,
					Price = orderItem.Price,
					IncludeVat = orderItem.IncludeNDS ?? 0,
					ValueAddedTax = orderItem.ValueAddedTax,
					DiscountMoney = orderItem.DiscountMoney,
					TrueMarkCodes = codesInfo,
					EconomicLifeFacts = new List<ProductEconomicLifeFactsInfo>()
				};

				var clientSpecialNomenclature = client.SpecialNomenclatures
					.Where(x => x.Nomenclature.Id == nomenclature.Id)
					.FirstOrDefault();

				if(client.UseSpecialDocFields && clientSpecialNomenclature != null)
				{
					var productEconomicLifeFacts = new List<ProductEconomicLifeFactsInfo>();

					var productEconomicLifeFact = new ProductEconomicLifeFactsInfo
					{
						Id = "код_материала",
						Value = clientSpecialNomenclature.SpecialId.ToString()
					};

					productEconomicLifeFacts.Add(productEconomicLifeFact);

					product.EconomicLifeFacts = productEconomicLifeFacts;
				}

				products.Add(product);
			}

			return products;
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
