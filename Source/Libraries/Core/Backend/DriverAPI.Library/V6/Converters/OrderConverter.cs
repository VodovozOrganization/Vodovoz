using DriverApi.Contracts.V6;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Services.TrueMark;

namespace DriverAPI.Library.V6.Converters
{
	/// <summary>
	/// Конвертер заказа
	/// </summary>
	public class OrderConverter
	{
		private readonly IUnitOfWork _uow;
		private readonly DeliveryPointConverter _deliveryPointConverter;
		private readonly SmsPaymentStatusConverter _smsPaymentConverter;
		private readonly PaymentTypeConverter _paymentTypeConverter;
		private readonly SignatureTypeConverter _signatureTypeConverter;
		private readonly QrPaymentConverter _qrPaymentConverter;
		private readonly IOrderRepository _orderRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly ICounterpartyEdoAccountController _edoAccountController;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="deliveryPointConverter"></param>
		/// <param name="smsPaymentConverter"></param>
		/// <param name="paymentTypeConverter"></param>
		/// <param name="signatureTypeConverter"></param>
		/// <param name="qrPaymentConverter"></param>
		/// <param name="orderRepository"></param>
		/// <param name="trueMarkWaterCodeService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public OrderConverter(
			IUnitOfWork uow,
			DeliveryPointConverter deliveryPointConverter,
			SmsPaymentStatusConverter smsPaymentConverter,
			PaymentTypeConverter paymentTypeConverter,
			SignatureTypeConverter signatureTypeConverter,
			QrPaymentConverter qrPaymentConverter,
			IOrderRepository orderRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			ICounterpartyEdoAccountController edoAccountController)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
			_smsPaymentConverter = smsPaymentConverter ?? throw new ArgumentNullException(nameof(smsPaymentConverter));
			_paymentTypeConverter = paymentTypeConverter ?? throw new ArgumentNullException(nameof(paymentTypeConverter));
			_signatureTypeConverter = signatureTypeConverter ?? throw new ArgumentNullException(nameof(signatureTypeConverter));
			_qrPaymentConverter = qrPaymentConverter ?? throw new ArgumentNullException(nameof(qrPaymentConverter));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_edoAccountController = edoAccountController ?? throw new ArgumentNullException(nameof(edoAccountController));
		}

		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="vodovozOrder">Заказ ДВ</param>
		/// <param name="routeListItem">Адрес маршрутного листа</param>
		/// <param name="smsPaymentStatus">Статус оплаты по смс</param>
		/// <param name="qrPaymentDtoStatus">Статус оплаты по QR-коду</param>
		/// <returns></returns>
		public OrderDto ConvertToAPIOrder(
			Order vodovozOrder,
			RouteListItem routeListItem,
			SmsPaymentStatus? smsPaymentStatus,
			FastPaymentStatus? qrPaymentDtoStatus)
		{
			var pairOfSplitedLists = SplitDeliveryItems(vodovozOrder.OrderEquipments);

			var apiOrder = new OrderDto
			{
				OrderId = vodovozOrder.Id,
				SmsPaymentStatus = _smsPaymentConverter.ConvertToAPIPaymentStatus(smsPaymentStatus),
				QRPaymentStatus = _qrPaymentConverter.ConvertToAPIPaymentStatus(qrPaymentDtoStatus),
				DeliveryTime = vodovozOrder.TimeDelivered?.ToString("HH:mm:ss"),
				FullBottleCount = vodovozOrder.Total19LBottlesToDeliver,
				EmptyBottlesToReturn = (vodovozOrder.BottlesReturn ?? 0) + vodovozOrder.BottlesByStockCount,
				BottlesByStockActualCount = vodovozOrder.BottlesByStockActualCount,
				Counterparty = vodovozOrder.Client.FullName,
				PhoneNumbers = CreatePhoneList(vodovozOrder),
				PaymentType = _paymentTypeConverter.ConvertToAPIPaymentType(vodovozOrder.PaymentType, qrPaymentDtoStatus == FastPaymentStatus.Performed, vodovozOrder.PaymentByTerminalSource),
				Address = _deliveryPointConverter.ExtractAPIAddressFromDeliveryPoint(vodovozOrder.DeliveryPoint),
				OrderSum = vodovozOrder.OrderSum,
				OrderSaleItems = PrepareSaleItemsList(vodovozOrder.OrderItems, routeListItem),
				OrderDeliveryItems = pairOfSplitedLists.orderDeliveryItems,
				OrderReceptionItems = pairOfSplitedLists.orderReceptionItems,
				IsFastDelivery = vodovozOrder.IsFastDelivery,
				ContactlessDelivery = vodovozOrder.ContactlessDelivery,
				AddedToRouteListTime = routeListItem.CreationDate.ToString("dd.MM.yyyyTHH:mm:ss"),
				CallBeforeArrivalMinutes = vodovozOrder.CallBeforeArrivalMinutes,
				Trifle = vodovozOrder.Trifle ?? 0,
				SignatureType = _signatureTypeConverter.ConvertToApiSignatureType(vodovozOrder.SignatureType),
				WaitUntilTime = vodovozOrder.WaitUntilTime,
				OrderType = GetOrderType(vodovozOrder)
			};

			if(vodovozOrder.DontArriveBeforeInterval)
			{
				var sb = new StringBuilder();

				if(!string.IsNullOrWhiteSpace(vodovozOrder.Comment))
				{
					sb.AppendLine(vodovozOrder.Comment.TrimEnd('\r', '\n'));
				}

				sb.AppendLine(Order.DontArriveBeforeIntervalString);
				apiOrder.OrderComment = sb.ToString();
			}
			else
			{
				apiOrder.OrderComment = vodovozOrder.Comment;
			}

			return apiOrder;
		}

		private OrderReasonForLeavingDtoType GetOrderType(
			Order vodovozOrder)
		{
			if(vodovozOrder.IsNeedIndividualSetOnLoad(_edoAccountController) || vodovozOrder.IsNeedIndividualSetOnLoadForTender)
			{
				return OrderReasonForLeavingDtoType.Distributing;
			}

			if(vodovozOrder.IsOrderForResale || vodovozOrder.IsOrderForTender)
			{
				return OrderReasonForLeavingDtoType.ForResale;
			}

			return OrderReasonForLeavingDtoType.ForPersonal;
		}

		private IEnumerable<PhoneDto> CreatePhoneList(Order vodovozOrder)
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.RussiaOnlyShort);

			var deliveryPointPhones = vodovozOrder.DeliveryPoint.Phones
				.Where(p => !p.IsArchive)
				.GroupBy(p => p.DigitsNumber)
				.Select(x => new PhoneDto
				{
					Number = phoneFormatter.FormatString(x.First().DigitsNumber),
					PhoneType = PhoneDtoType.DeliveryPoint,
					Info = vodovozOrder.DeliveryPoint.ShortAddress
				})
				.ToList();

			var counterpartyPhones = vodovozOrder.Client.Phones
				.Where(p => !p.IsArchive)
				.GroupBy(p => p.DigitsNumber)
				.Select(x => new PhoneDto
				{
					Number = phoneFormatter.FormatString(x.First().DigitsNumber),
					PhoneType = PhoneDtoType.Counterparty,
					Info = vodovozOrder.DeliveryPoint.ShortAddress
				})
				.ToList();

			var allPhones = deliveryPointPhones.Concat(counterpartyPhones);

			var orderContactPhone = vodovozOrder.ContactPhone;

			if(orderContactPhone == null)
			{
				return allPhones;
			}

			var foundContactPhone = allPhones.FirstOrDefault(p => p.Number == phoneFormatter.FormatString(orderContactPhone.DigitsNumber));

			if(foundContactPhone == null)
			{
				return allPhones;
			}

			var resultPhoneList = new List<PhoneDto>
			{
				foundContactPhone
			};

			foreach(var phone in allPhones)
			{
				if(!resultPhoneList.Contains(phone))
				{
					resultPhoneList.Add(phone);
				}
			}

			return resultPhoneList;
		}

		private (IEnumerable<OrderDeliveryItemDto> orderDeliveryItems, IEnumerable<OrderReceptionItemDto> orderReceptionItems)
			SplitDeliveryItems(IEnumerable<OrderEquipment> orderEquipment)
		{
			var deliveryItems = new List<OrderDeliveryItemDto>();
			var receptionItems = new List<OrderReceptionItemDto>();

			foreach(var transferItem in orderEquipment)
			{
				if(transferItem.Direction == Direction.Deliver)
				{
					deliveryItems.Add(ConvertToAPIOrderDeliveryItem(transferItem));
				}
				else if(transferItem.Direction == Direction.PickUp)
				{
					receptionItems.Add(ConvertToAPIOrderReceptionItem(transferItem));
				}
			}

			return (deliveryItems, receptionItems);
		}

		private IEnumerable<OrderSaleItemDto> PrepareSaleItemsList(
			IEnumerable<OrderItem> orderItems,
			RouteListItem routeListItem)
		{
			var result = new List<OrderSaleItemDto>();

			foreach(var saleItem in orderItems)
			{
				result.Add(ConvertToAPIOrderSaleItem(saleItem, routeListItem));
			}

			return result;
		}

		private OrderSaleItemDto ConvertToAPIOrderSaleItem(
			OrderItem saleItem,
			RouteListItem routeListItem)
		{
			var result = new OrderSaleItemDto
			{
				OrderSaleItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count,
				NeedScanCode = saleItem.Nomenclature.IsAccountableInTrueMark,
				OrderItemPrice = saleItem.Price,
				TotalOrderItemPrice = saleItem.ActualSum,
				IsBottleStock = saleItem.Order.IsBottleStock && saleItem.DiscountByStock > 0,
				IsDiscountInMoney = saleItem.IsDiscountInMoney,
				Discount = saleItem.IsDiscountInMoney ? saleItem.DiscountMoney : saleItem.Discount,
				DiscountReason = saleItem.DiscountReason?.Name,
				CapColor = saleItem.Nomenclature.BottleCapColor,
				IsNeedAdditionalControl = saleItem.Nomenclature.ProductGroup?.IsNeedAdditionalControl ?? false,
				Gtin = saleItem.Nomenclature.Gtins.Select(x => x.GtinNumber).ToList(),
				GroupGtins = saleItem.Nomenclature.GroupGtins.Select(x => new GroupGtinDto { Gtin = x.GtinNumber, Count = x.CodesCount }).ToList(),
				Codes = GetOrderItemCodes(saleItem, routeListItem)
			};

			if(saleItem.Nomenclature.TareVolume != null)
			{
				switch(saleItem.Nomenclature.TareVolume)
				{
					case TareVolume.Vol19L:
						result.TareVolume = 19;
						break;
					case TareVolume.Vol6L:
						result.TareVolume = 6;
						break;
					case TareVolume.Vol1500ml:
						result.TareVolume = 1.5m;
						break;
					case TareVolume.Vol600ml:
						result.TareVolume = 0.6m;
						break;
					case TareVolume.Vol500ml:
						result.TareVolume = 0.5m;
						break;
				}
			}

			return result;
		}

		private IEnumerable<TrueMarkCodeDto> GetOrderItemCodes(
			OrderItem saleItem,
			RouteListItem routeListItem)
		{
			var codes = Enumerable.Empty<TrueMarkCodeDto>();

			if(!saleItem.IsTrueMarkCodesMustBeAdded)
			{
				return codes;
			}

			var sequenceNumber = 0;

			var addedTrueMarkWaterCodes =
				saleItem.IsTrueMarkCodesMustBeAddedInWarehouse(_edoAccountController)
				? GetCodesAddedInWarehouse(saleItem)
				: GetCodesAddedByDriver(saleItem, routeListItem);

			var trueMarkCodes = new List<TrueMarkAnyCode>();

			foreach(var trueMarkWaterCode in addedTrueMarkWaterCodes)
			{
				if(trueMarkCodes.Any(x => x.Match(
					transportCode => transportCode.RawCode == trueMarkWaterCode.RawCode,
					groupCode => groupCode.RawCode == trueMarkWaterCode.RawCode,
					waterCode => waterCode.RawCode == trueMarkWaterCode.RawCode)))
				{
					continue;
				}

				if(trueMarkWaterCode.ParentWaterGroupCodeId == null
					&& trueMarkWaterCode.ParentTransportCodeId == null)
				{
					trueMarkCodes.Add(trueMarkWaterCode);
					continue;
				}

				var parentCode = _trueMarkWaterCodeService.GetParentGroupCode(_uow, trueMarkWaterCode);

				trueMarkCodes.AddRange(
					parentCode.Match(
						transportCode => transportCode.GetAllCodes(),
						groupCode => groupCode.GetAllCodes(),
						_ => throw new InvalidOperationException("Не может быть найден родитель являющийся индивидуальным кодом, что-то пошло не так")));
			}

			codes = trueMarkCodes
				.Select(x => ConvertToApiTrueMarkCode(x, sequenceNumber++, trueMarkCodes))
				.ToList();

			return codes;
		}

		private IEnumerable<TrueMarkWaterIdentificationCode> GetCodesAddedInWarehouse(OrderItem saleItem)
		{
			var skipCodesCount = (int?)saleItem.Order.OrderItems
								.Where(x => x.Nomenclature.Id == saleItem.Nomenclature.Id
									&& x.Id < saleItem.Id)
								.Sum(x => x.ActualCount ?? x.Count) ?? 0;

			var takeCodesCount = (int)(saleItem.ActualCount ?? saleItem.Count);

			var nomenclatureGtins = saleItem.Nomenclature.Gtins.Select(x => x.GtinNumber).ToList();

			var waterCodes = _orderRepository.GetTrueMarkCodesAddedInWarehouseToOrderByOrderId(_uow, saleItem.Order.Id)
				.Where(x => nomenclatureGtins.Contains(x.Gtin));

			var codes = waterCodes
				.Skip(skipCodesCount)
				.Take(takeCodesCount)
				.ToList();

			return codes;
		}

		private IEnumerable<TrueMarkWaterIdentificationCode> GetCodesAddedByDriver(OrderItem saleItem, RouteListItem routeListItem)
		{
			var codes = new List<TrueMarkWaterIdentificationCode>();

			var productCodesByOrderItems = _orderRepository.GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(_uow, saleItem.Id);

			if(productCodesByOrderItems is null || !productCodesByOrderItems.Any())
			{
				return codes;
			}

			var orderItemCodesIds = productCodesByOrderItems
				.Where(x => x.OrderItemId == saleItem.Id)
				.Select(x => x.TrueMarkProductCodeId);

			codes = routeListItem.TrueMarkCodes
				.Where(x => orderItemCodesIds.Contains(x.Id))
				.Where(x => x.SourceCode != null || x.ResultCode != null)
				.Select(x => x.SourceCode ?? x.ResultCode)
				.ToList();

			return codes;
		}

		private TrueMarkCodeDto ConvertToApiTrueMarkCode(TrueMarkAnyCode trueMarkCode, int sequenceNumber, IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return trueMarkCode.Match(
				transportCode => new TrueMarkCodeDto
				{
					SequenceNumber = sequenceNumber,
					Level = DriverApiTruemarkCodeLevel.transport,
					Code = transportCode.RawCode,
					Parent = transportCode.ParentTransportCodeId != null
						? allCodes
							.FirstOrDefault(x => x.IsTrueMarkTransportCode && x.TrueMarkTransportCode.Id == transportCode.ParentTransportCodeId)
							?.TrueMarkTransportCode.RawCode
						: null
				},
				groupCode => new TrueMarkCodeDto
				{
					SequenceNumber = sequenceNumber,
					Level = DriverApiTruemarkCodeLevel.group,
					Code = groupCode.RawCode,
					Parent = groupCode.ParentTransportCodeId != null
						? allCodes
							.FirstOrDefault(x => x.IsTrueMarkTransportCode && x.TrueMarkTransportCode.Id == groupCode.ParentTransportCodeId)
							?.TrueMarkTransportCode.RawCode
						: groupCode.ParentWaterGroupCodeId != null
							? allCodes
								.FirstOrDefault(x => x.IsTrueMarkWaterGroupCode && x.TrueMarkWaterGroupCode.Id == groupCode.ParentWaterGroupCodeId)
								?.TrueMarkWaterGroupCode.RawCode
							: null
				},
				waterCode => new TrueMarkCodeDto
				{
					SequenceNumber = sequenceNumber,
					Level = DriverApiTruemarkCodeLevel.unit,
					Code = waterCode.RawCode,
					Parent = waterCode.ParentTransportCodeId != null
						? allCodes
							.FirstOrDefault(x => x.IsTrueMarkTransportCode && x.TrueMarkTransportCode.Id == waterCode.ParentTransportCodeId)
							?.TrueMarkTransportCode.RawCode
						: waterCode.ParentWaterGroupCodeId != null
							? allCodes
								.FirstOrDefault(x => x.IsTrueMarkWaterGroupCode && x.TrueMarkWaterGroupCode.Id == waterCode.ParentWaterGroupCodeId)
								?.TrueMarkWaterGroupCode.RawCode
							: null
				});
		}

		private OrderDeliveryItemDto ConvertToAPIOrderDeliveryItem(OrderEquipment saleItem)
		{
			var result = new OrderDeliveryItemDto()
			{
				OrderDeliveryItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count
			};

			return result;
		}

		private OrderReceptionItemDto ConvertToAPIOrderReceptionItem(OrderEquipment saleItem)
		{
			var result = new OrderReceptionItemDto()
			{
				OrderReceptionItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count
			};

			return result;
		}

		/// <summary>
		/// Конвертация данных строки заказа и кодов ЧЗ в DTO с данными по номенклатуре и кодам ЧЗ
		/// </summary>
		/// <param name="saleItem">Строка заказа</param>
		/// <param name="routeListItem">Строка маршрутного листа</param>
		/// <returns></returns>
		public NomenclatureTrueMarkCodesDto ConvertOrderItemTrueMarkCodesDataToDto(
			OrderItem saleItem,
			RouteListItem routeListItem)
		{
			var result = new NomenclatureTrueMarkCodesDto
			{
				OrderSaleItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Gtin = saleItem.Nomenclature.Gtins.Select(x => x.GtinNumber).ToList(),
				GroupGtins = saleItem.Nomenclature.GroupGtins.Select(x => new GroupGtinDto { Gtin = x.GtinNumber, Count = x.CodesCount }).ToList(),
				Quantity = saleItem.ActualCount ?? saleItem.Count,
				Codes = GetOrderItemCodes(saleItem, routeListItem)
			};
			return result;
		}
	}
}
