using System;
using QS.Extensions.Observable.Collections.List;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Controllers;

namespace Vodovoz
{
	public class OrderItemReturnsNode
	{
		private OrderItem _orderItem;

		public OrderItemReturnsNode(OrderItem item)
		{
			_orderItem = item;
			PromoSetName = _orderItem.PromoSet?.Name;
			DiscountReasons = _orderItem.DiscountReasons;
			IsDiscountReasonsEditable = true;
		}

		public OrderItemReturnsNode(OrderEquipment equipment)
		{
			OrderEquipment = equipment;
			DiscountReasons = OrderEquipment.OrderItem?.DiscountReasons ?? new ObservableList<DiscountReasonBase>();
			IsDiscountReasonsEditable = OrderEquipment.OrderItem != null;
		}

		public OrderItem OrderItem => _orderItem;
		public OrderEquipment OrderEquipment { get; private set; }
		public OrderItem EquipmentOrderItem => OrderEquipment?.OrderItem;

		public IObservableList<DiscountReasonBase> DiscountReasons { get; }

		public string DiscountReasonsNames => string.Join(", ", DiscountReasons.Select(dr => dr.Name));

		public bool IsDiscountReasonsEditable { get; }

		public bool IsEquipment => OrderEquipment != null;

		public bool IsSerialEquipment
		{
			get
			{
				return
					IsEquipment
					&& OrderEquipment.Equipment != null
					&& OrderEquipment.Equipment.Nomenclature.IsSerial;
			}
		}

		public bool IsDelivered
		{
			get => ActualCount > 0;
			set
			{
				if(IsEquipment && IsSerialEquipment)
				{
					ActualCount = value ? 1 : 0;
				}
			}
		}

		public decimal ActualCount
		{
			get
			{
				if(IsEquipment)
				{
					if(IsSerialEquipment)
					{
						return OrderEquipment.Confirmed ? 1 : 0;
					}

					return OrderEquipment.ActualCount ?? 0;
				}

				return _orderItem.ActualCount ?? 0;
			}
			protected set => throw new InvalidOperationException("Нельзя устанавливать фактическое количество из ноды!");
		}

		public Nomenclature Nomenclature
		{
			get
			{
				if(IsEquipment)
				{
					if(IsSerialEquipment)
					{
						return OrderEquipment.Equipment.Nomenclature;
					}

					return OrderEquipment.Nomenclature;
				}

				return _orderItem.Nomenclature;
			}
		}

		public decimal Count => IsEquipment ? 1 : _orderItem.Count;

		public string Name => IsEquipment ? OrderEquipment.NameString : _orderItem.NomenclatureString;

		public bool HasPrice => !IsEquipment || OrderEquipment.OrderItem != null;

		public string ConfirmedComments
		{
			get => IsEquipment ? OrderEquipment.ConfirmedComment : null;
			set
			{
				if(IsEquipment)
				{
					OrderEquipment.ConfirmedComment = value;
				}
			}
		}

		public decimal Price
		{
			get
			{
				if(IsEquipment)
				{
					return OrderEquipment.OrderItem != null ? OrderEquipment.OrderItem.Price : 0;
				}

				return _orderItem.Price;
			}
			protected set => throw new InvalidOperationException($"Нельзя устанавливать цену из ноды! Используйте {nameof(ISaleDiscountController)}");
		}

		public bool IsDiscountInMoney
		{
			get
			{
				if(IsEquipment)
				{
					return OrderEquipment.OrderItem != null && OrderEquipment.OrderItem.IsDiscountInMoney;
				}

				return _orderItem.IsDiscountInMoney;
			}
			set
			{
				if(IsEquipment)
				{
					OrderEquipment.OrderItem.SetIsDiscountInMoney(OrderEquipment.OrderItem != null && value);
				}
				else
				{
					_orderItem.SetIsDiscountInMoney(value);
				}
			}
		}

		public decimal GetDiscount
		{
			get
			{
				if(IsEquipment)
				{
					return OrderEquipment.OrderItem?.GetDiscount ?? 0;
				}

				return _orderItem.GetDiscount;
			}
		}

		public decimal Discount
		{
			get
			{
				if(IsEquipment)
				{
					return OrderEquipment.OrderItem?.Discount ?? 0m;
				}

				return _orderItem.Discount;
			}
		}

		public decimal DiscountMoney
		{
			get
			{
				if(IsEquipment)
				{
					return OrderEquipment.OrderItem?.DiscountMoney ?? 0m;
				}

				return _orderItem.DiscountMoney;
			}
		}

		public decimal Sum => Price * ActualCount - DiscountMoney;
		public string PromoSetName { get; }
	}
}
