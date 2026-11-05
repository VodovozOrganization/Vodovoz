using Edo.Contracts.Messages.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;

namespace Edo.Docflow.Factories
{
	/// <summary>
	/// Подготавливает данные отправки трансфера из сохранённого документа УПД.
	/// </summary>
	public class TransferOrderUpdInfoFactory
	{
		/// <summary>
		/// Преобразует сохранённые значения УПД трансфера в данные для Такском.
		/// </summary>
		/// <param name="transferTask">Задача трансфера с сохранённым УПД.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Данные сохранённого УПД для отправки.</returns>
		public Task<UniversalTransferDocumentInfo> CreateUniversalTransferDocumentInfo(
			TransferEdoTask transferTask, CancellationToken cancellationToken)
		{
			var upd = transferTask.Upd;
			if(upd == null)
			{
				throw new NotSupportedException($"В задаче трансфера {transferTask.Id} отсутствует сохранённый УПД.");
			}

			var products = upd.InventPositions.Select(position =>
				CreateProductInfo(position, position.Codes.Select(code => new ProductCodeInfo
				{
					TransportCode = code.TransportCode,
					IsGroup = string.IsNullOrEmpty(code.TransportCode) && code.GroupCode != null,
					IndividualOrGroupCode = string.IsNullOrEmpty(code.TransportCode) ? code.IdentificationCode : null
				}).ToList())).ToList();

			return Task.FromResult(CreateDocumentInfo(upd, products, new List<UpdAdditionalInfo>()));
		}

		private static UniversalTransferDocumentInfo CreateDocumentInfo(
			EdoUpdDocument upd, IEnumerable<ProductInfo> products, IEnumerable<UpdAdditionalInfo> additionalInformation)
		{
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
				AdditionalInformation = additionalInformation
			};

			return document;
		}

		private static OrganizationInfo GetOrganizationInfo(UpdOrganization organization)
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

		/// <summary>Преобразует товарную позицию и подготовленные коды.</summary>
		/// <param name="inventPosition">Сохранённая товарная позиция.</param>
		/// <param name="codes">Коды в представлении соответствующего сценария.</param>
		/// <returns>Товарная позиция для отправки.</returns>
		private static ProductInfo CreateProductInfo(UpdInventPosition inventPosition, IEnumerable<ProductCodeInfo> codes)
		{
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
				TrueMarkCodes = codes
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

			return product;
		}

	}
}
