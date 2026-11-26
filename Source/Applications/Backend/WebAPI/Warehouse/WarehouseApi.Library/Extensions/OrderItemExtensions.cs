using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.V1.Dto;
using WarehouseApi.Library.Converters;

namespace WarehouseApi.Library.Extensions
{
	public static class OrderItemExtensions
	{
		public static OrderItemDto ToApiDtoV1(
			this OrderItem orderItem,
			Nomenclature nomenclature,
			SelfDeliveryDocumentItem selfDeliveryDocumentItem,
			ILookup<int, SelfDeliveryDocumentItemTrueMarkProductCode> productCodesByItemId)
		{
			if(orderItem is null)
			{
				throw new ArgumentNullException(nameof(orderItem));
			}

			var orderItemDto = new OrderItemDto
			{
				NomenclatureId = nomenclature.Id,
				Name = nomenclature.Name,
				Gtin = nomenclature.Gtins.Select(x => x.GtinNumber),
				GroupGtins = nomenclature.GroupGtins
					.Select(gg => new GroupGtinDto
					{
						Gtin = gg.GtinNumber,
						Count = gg.CodesCount
					}),
				Quantity = (int)(orderItem.ActualCount ?? orderItem.Count)
			};

			if(selfDeliveryDocumentItem != null)
			{
				var codes = productCodesByItemId[selfDeliveryDocumentItem.Id]
					.OrderBy(x => x.Id)
					.Select((code, index) => new TrueMarkCodeDto
					{
						SequenceNumber = index,
						Code = code.ResultCode?.RawCode ?? code.SourceCode.RawCode,
						Level = WarehouseApiTruemarkCodeLevel.unit,
						ProductGroup = (code.ResultCode?.ProductGroup ?? code.SourceCode.ProductGroup)
					})
					.ToList();

				orderItemDto.Codes.AddRange(codes);
			}

			return orderItemDto;
		}

		public static IEnumerable<OrderItemDto> ToApiDtoV1(
			this IEnumerable<OrderItem> orderItems,
			IEnumerable<Nomenclature> nomenclatures,
			SelfDeliveryDocument selfDeliveryDocument,
			ILookup<int, SelfDeliveryDocumentItemTrueMarkProductCode> productCodesByItemId)
		{
			if(orderItems is null)
			{
				throw new ArgumentNullException(nameof(orderItems));
			}

			return orderItems
				.Select(x => x.ToApiDtoV1(nomenclatures
					.FirstOrDefault(n => n.Id == x.Nomenclature.Id),
					selfDeliveryDocument?.Items?.FirstOrDefault(i => i.OrderItem?.Id == x.Id),
					productCodesByItemId))
				.ToList();
		}


		#region RelatedCodesPopulator

		/// <summary>
		/// Заполняет связанные коды
		/// </summary>
		/// <param name="orderItemDtos"></param>
		/// <param name="unitOfWork"></param>
		/// <param name="trueMarkCodeService"></param>
		/// <param name="trueMarkCodes"></param>
		public static void PopulateRelatedCodes(
			this IEnumerable<OrderItemDto> orderItemDtos,
			IUnitOfWork unitOfWork,
			ITrueMarkCodeService trueMarkCodeService,
			IEnumerable<TrueMarkProductCode> trueMarkCodes)
		{
			foreach(var trueMarkProductCode in trueMarkCodes)
			{
				var resultCode = trueMarkProductCode.ResultCode;

				if(resultCode == null)
				{
					continue;
				}

				if(resultCode.ParentGroupCodeId == null
					&& resultCode.ParentTransportCodeId == null)
				{
					continue;
				}

				var codeToAddInfo = orderItemDtos.FirstOrDefault(x => x.Codes.Select(code => code.Code).Contains(resultCode.RawCode));

				if(codeToAddInfo.Codes.Any(x => x.Parent != null && x.Code == resultCode.RawCode))
				{
					continue;
				}

				var parentCode = trueMarkCodeService.GetParentGroupCode(unitOfWork, resultCode);

				var trueMarkCodeDtos = new List<TrueMarkCodeDto>();

				var allCodes = parentCode.Match(
					transportCode => transportCode.GetAllCodes(),
					groupCode => groupCode.GetAllCodes(),
					identificationCode => new TrueMarkAnyCode[] { identificationCode })
					.ToArray();

				var codesInCurrentOrder = allCodes.Where(x => x.IsTrueMarkIdentificationCode
					&& trueMarkCodes.Any(y =>
						(y.ResultCode != null && y.ResultCode.Id == x.TrueMarkIdentificationCode.Id)
						|| (y.SourceCode != null && y.SourceCode.Id == x.TrueMarkIdentificationCode.Id)))
					.Select(x => x.TrueMarkIdentificationCode)
					.ToArray();

				foreach(var anyCode in allCodes)
				{
					if(anyCode.IsTrueMarkIdentificationCode
						&& !codesInCurrentOrder.Any(x => x.Id == anyCode.TrueMarkIdentificationCode.Id))
					{
						continue;
					}

					trueMarkCodeDtos.Add(
						anyCode.Match(
							PopulateTransportCode(allCodes),
							PopulateGroupCode(allCodes),
							PopulateIdentificationCode(allCodes)));
				}

				codeToAddInfo.Codes.RemoveAll(code => trueMarkCodeDtos.Any(x => x.Code == code.Code));
				codeToAddInfo.Codes.AddRange(trueMarkCodeDtos);
			}
		}

		private static Func<TrueMarkIdentificationCode, TrueMarkCodeDto> PopulateIdentificationCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return identificationCode =>
			{
				string parentRawCode = null;

				if(identificationCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == identificationCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				if(identificationCode.ParentGroupCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkGroupCode
							&& x.TrueMarkGroupCode.Id == identificationCode.ParentGroupCodeId)
						?.TrueMarkGroupCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = identificationCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.unit,
					Parent = parentRawCode,
					ProductGroup = identificationCode.ProductGroup
				};
			};
		}

		private static Func<TrueMarkGroupCode, TrueMarkCodeDto> PopulateGroupCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return groupCode =>
			{
				string parentRawCode = null;

				if(groupCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == groupCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				if(groupCode.ParentGroupCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkGroupCode
							&& x.TrueMarkGroupCode.Id == groupCode.ParentGroupCodeId)
						?.TrueMarkGroupCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = groupCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.group,
					Parent = parentRawCode,
					ProductGroup = groupCode.ProductGroup
				};
			};
		}

		private static Func<TrueMarkTransportCode, TrueMarkCodeDto> PopulateTransportCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return transportCode =>
			{
				string parentRawCode = null;

				if(transportCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == transportCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = transportCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.transport,
					Parent = parentRawCode
				};
			};
		}

		#endregion
	}

}
