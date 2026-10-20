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
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;


namespace Edo.Docflow.Factories
{
	public class OrderUpdInfoFactory : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;

		public OrderUpdInfoFactory(
			IUnitOfWork uow,
			ITrueMarkCodeRepository trueMarkCodeRepository
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
		}

		public async Task<UniversalTransferDocumentInfo> CreateUniversalTransferDocumentInfo(
			DocumentEdoTask documentEdoTask, 
			IEnumerable<PaymentEntity> payments,
			CancellationToken cancellationToken
			)
		{
			var upd = documentEdoTask.Upd;

			// предзагрузка для ускорения
			var productCodes = await _uow.Session.QueryOver<TrueMarkProductCode>()
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Where(x => x.CustomerEdoRequest.Id == documentEdoTask.FormalEdoRequest.Id)
				.ListAsync(cancellationToken);
			
			var sourceCodes = productCodes
				.Where(x => x.SourceCode != null)
				.Select(x => x.SourceCode);

			var resultCodes = productCodes
				.Where(x => x.ResultCode != null)
				.Select(x => x.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await _trueMarkCodeRepository.PreloadCodes(codesToPreload, cancellationToken);

			var products = await GetProducts(upd, cancellationToken);

			var document = new UniversalTransferDocumentInfo
			{
				DocumentId = upd.Guid,
				StringNumber = upd.Number,
				Sum = upd.Sum,
				Date = upd.Date,
				Seller = new SellerInfo 
				{ 
					Organization = GetOrganizationInfo(upd.Seller) 
				},
				Shipper = new ShipperInfo
				{
					Organization = GetOrganizationInfo(upd.Shipper)
				},
				Customer = new CustomerInfo 
				{ 
					Organization = GetOrganizationInfo(upd.Customer) 
				},
				Consignee = new ConsigneeInfo 
				{ 
					Organization = GetOrganizationInfo(upd.Consignee),
					CargoReceiver = upd.SpecialConsignee
				},
				DocumentConfirmingShipment = new DocumentConfirmingShipmentInfo 
				{ 
					Document = upd.ShippingDocument,
					Number = upd.ShippingDocumentNumber,
					Date = upd.ShippingDocumentDate
				},
				GovContract = upd.GovContract,
				BasisShipment = new BasisShipmentInfo 
				{ 
					Document = upd.ShippingReasonDocument,
					Number = upd.ShippingReasonDocumentNumber,
					Date = upd.ShippingReasonDocumentDate
				},
				Payments = upd.Payments.Select(x => new PaymentInfo 
				{ 
					PaymentNum = x.PaymentNum,
					PaymentDate = x.PaymentDate
				}),
				Products = products,
				AdditionalInformation = GetAdditionalInformation(upd, products)
			};

			return document;
		}

		private IEnumerable<UpdAdditionalInfo> GetAdditionalInformation(EdoUpdDocument upd, IEnumerable<ProductInfo> products)
		{
			var additionalInformation = new List<UpdAdditionalInfo>();

			if(products.Any(x => x.TrueMarkCodes.Any()) && upd.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				additionalInformation.Add(new UpdAdditionalInfo
				{
					Id = "СвВыбытияМАРК",
					Value = "1"
				});
			}

			if(!string.IsNullOrWhiteSpace(upd.CustomerExternalOrderId))
			{
				additionalInformation.Add(new UpdAdditionalInfo
				{
					Id = "номер_заказа",
					Value = upd.CustomerExternalOrderId
				});
			}

			additionalInformation.Add(new UpdAdditionalInfo
			{
				Id = "номер_отгрузки",
				Value = $"{upd.OrderId}"
			});

			additionalInformation.Add(new UpdAdditionalInfo
			{
				Id = "дата_заказа",
				Value = $"{upd.Date:dd.MM.yyyy}"
			});

			return additionalInformation;
		}


		private OrganizationInfo GetOrganizationInfo(UpdOrganization organization)
		{
			if(organization == null)
			{
				return null;
			}

			var organizationInfo = new OrganizationInfo
			{
				Name = organization.Name,
				Address = new AddressInfo
				{
					CountryCode = organization.CountryCode,
					CountryName = organization.CountryName,
					Address = organization.Address
				},
				Inn = organization.Inn,
				Kpp = organization.Kpp,
				OGRN = organization.Ogrn,
				OGRNDate = organization.OgrnDate,
				EdoAccountId = organization.EdoAccountId,
				IsWithoutVat = organization.IsWithoutVat
			};

			return organizationInfo;
		}

		private async Task<IEnumerable<ProductInfo>> GetProducts(EdoUpdDocument upd, CancellationToken cancellationToken)
		{
			var products = new List<ProductInfo>();

			foreach(var inventPosition in upd.InventPositions)
			{
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
					Name = inventPosition.Name,
					IsService = inventPosition.IsService,
					UnitName = inventPosition.UnitName,
					OKEI = inventPosition.OKEI,
					Code = inventPosition.NomenclatureId.ToString(),
					Count = inventPosition.Count,
					Price = inventPosition.Price,
					IncludeVat = inventPosition.IncludeVat,
					ValueAddedTax = inventPosition.ValueAddedTax,
					DiscountMoney = inventPosition.DiscountMoney,
					TrueMarkCodes = codesInfo
				};

				var economicLifeFacts = new List<ProductEconomicLifeFactsInfo>();

				if(inventPosition.SpecialCustomerNomenclatureId.HasValue)
				{
					economicLifeFacts.Add(new ProductEconomicLifeFactsInfo
					{
						Id = "код_материала",
						Value = inventPosition.SpecialCustomerNomenclatureId.Value.ToString()
					});
				}

				product.EconomicLifeFacts = economicLifeFacts;

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
