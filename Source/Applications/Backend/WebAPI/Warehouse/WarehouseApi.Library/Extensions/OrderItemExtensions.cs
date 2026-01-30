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
using VodovozBusiness.Domain.Documents;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Library.Extensions
{
	public static class OrderItemExtensions
	{
		public static OrderItemDto ToApiDtoV1(this OrderItem orderItem, Nomenclature nomenclature, SelfDeliveryDocumentItem selfDeliveryDocumentItem)
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
				Quantity = (int)orderItem.ActualCount
			};

			var codes = selfDeliveryDocumentItem.TrueMarkProductCodes
				.Select((code, index) => new TrueMarkCodeDto
				{
					SequenceNumber = index,
					Code = code.SourceCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.unit
				})
				.ToList();

			orderItemDto.Codes.AddRange(codes);

			return orderItemDto;
		}

		public static IEnumerable<OrderItemDto> ToApiDtoV1(this IEnumerable<OrderItem> orderItems, IEnumerable<Nomenclature> nomenclatures, SelfDeliveryDocument selfDeliveryDocument)
		{
			if(orderItems is null)
			{
				throw new ArgumentNullException(nameof(orderItems));
			}

			return orderItems
				.Select(x => x.ToApiDtoV1(nomenclatures
					.FirstOrDefault(n => n.Id == x.Nomenclature.Id),
					selfDeliveryDocument.Items.FirstOrDefault(i => i.OrderItem?.Id == x.Id)))
				.ToList();
		}


		#region RelatedCodesPopulator

		/// <summary>
		/// Заполняет связанные коды
		/// </summary>
		/// <param name="orderItemDtos"></param>
		/// <param name="unitOfWork"></param>
		/// <param name="trueMarkWaterCodeService"></param>
		/// <param name="trueMarkCodes"></param>
		public static void PopulateRelatedCodes(
			this IEnumerable<OrderItemDto> orderItemDtos,
			IUnitOfWork unitOfWork,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			IEnumerable<TrueMarkProductCode> trueMarkCodes)
		{
			foreach(var trueMarkProductCode in trueMarkCodes)
			{
				if(trueMarkProductCode.ResultCode == null)
				{
					continue;
				}

				if(trueMarkProductCode.ResultCode.ParentWaterGroupCodeId == null
					&& trueMarkProductCode.ResultCode.ParentTransportCodeId == null)
				{
					continue;
				}

				var codeToAddInfo = orderItemDtos.FirstOrDefault(x => x.Codes.Select(code => code.Code).Contains(trueMarkProductCode.ResultCode.RawCode));

				if(codeToAddInfo.Codes.Any(x => x.Parent != null && x.Code == trueMarkProductCode.ResultCode.RawCode))
				{
					continue;
				}

				var parentCode = trueMarkWaterCodeService.GetParentGroupCode(unitOfWork, trueMarkProductCode.ResultCode);

				var trueMarkCodeDtos = new List<TrueMarkCodeDto>();

				var allCodes = parentCode.Match(
					transportCode => transportCode.GetAllCodes(),
					groupCode => groupCode.GetAllCodes(),
					waterCode => new TrueMarkAnyCode[] { waterCode })
					.ToArray();

				var codesInCurrentOrder = allCodes.Where(x => x.IsTrueMarkWaterIdentificationCode
					&& trueMarkCodes.Any(y =>
						(y.ResultCode != null && y.ResultCode.Id == x.TrueMarkWaterIdentificationCode.Id)
						|| (y.SourceCode != null && y.SourceCode.Id == x.TrueMarkWaterIdentificationCode.Id)))
					.Select(x => x.TrueMarkWaterIdentificationCode)
					.ToArray();

				foreach(var anyCode in allCodes)
				{
					if(anyCode.IsTrueMarkWaterIdentificationCode
						&& !codesInCurrentOrder.Any(x => x.Id == anyCode.TrueMarkWaterIdentificationCode.Id))
					{
						continue;
					}

					trueMarkCodeDtos.Add(
						anyCode.Match(
							PopulateTransportCode(allCodes),
							PopulateGroupCode(allCodes),
							PopulateWaterCode(allCodes)));
				}

				codeToAddInfo.Codes.RemoveAll(code => trueMarkCodeDtos.Any(x => x.Code == code.Code));
				codeToAddInfo.Codes.AddRange(trueMarkCodeDtos);
			}
		}

		private static Func<TrueMarkWaterIdentificationCode, TrueMarkCodeDto> PopulateWaterCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return waterCode =>
			{
				string parentRawCode = null;

				if(waterCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == waterCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				if(waterCode.ParentWaterGroupCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkWaterGroupCode
							&& x.TrueMarkWaterGroupCode.Id == waterCode.ParentWaterGroupCodeId)
						?.TrueMarkWaterGroupCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = waterCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.unit,
					Parent = parentRawCode,
				};
			};
		}

		private static Func<TrueMarkWaterGroupCode, TrueMarkCodeDto> PopulateGroupCode(IEnumerable<TrueMarkAnyCode> allCodes)
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

				if(groupCode.ParentWaterGroupCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkWaterGroupCode
							&& x.TrueMarkWaterGroupCode.Id == groupCode.ParentWaterGroupCodeId)
						?.TrueMarkWaterGroupCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = groupCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.group,
					Parent = parentRawCode
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
