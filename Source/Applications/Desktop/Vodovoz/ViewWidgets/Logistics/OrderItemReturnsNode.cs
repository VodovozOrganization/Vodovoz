using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz
{
	public class OrderItemReturnsNode
	{
		private OrderItem _orderItem;
		private OrderEquipment _orderEquipment;

		public OrderItem OrderItem => _orderItem;

		public OrderItemReturnsNode(OrderItem item)
		{
			_orderItem = item;
			PromoSetName = _orderItem.PromoSet?.Name;
		}

		public OrderItemReturnsNode(OrderEquipment equipment)
		{
			_orderEquipment = equipment;
		}

		public bool IsEquipment => _orderEquipment != null;

		public bool IsSerialEquipment
		{
			get
			{
				return
					IsEquipment
					&& _orderEquipment.Equipment != null
					&& _orderEquipment.Equipment.Nomenclature.IsSerial;
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
						return _orderEquipment.Confirmed ? 1 : 0;
					}

					return _orderEquipment.ActualCount ?? 0;
				}

				return _orderItem.ActualCount ?? 0;
			}
			set
			{
				if(IsEquipment)
				{
					if(IsSerialEquipment)
					{
						_orderEquipment.ActualCount = value > 0 ? 1 : 0;
					}

					_orderEquipment.ActualCount = (int?) value;
				}
				else
				{
					_orderItem.SetActualCountWithPreserveOrRestoreDiscount(value);
				}
			}
		}

		public Nomenclature Nomenclature
		{
			get
			{
				if(IsEquipment)
				{
					if(IsSerialEquipment)
					{
						return _orderEquipment.Equipment.Nomenclature;
					}

					return _orderEquipment.Nomenclature;
				}

				return _orderItem.Nomenclature;
			}
		}

		public decimal Count => IsEquipment ? 1 : _orderItem.Count;

		public string Name => IsEquipment ? _orderEquipment.NameString : _orderItem.NomenclatureString;

		public bool HasPrice => !IsEquipment || _orderEquipment.OrderItem != null;

		public string ConfirmedComments
		{
			get => IsEquipment ? _orderEquipment.ConfirmedComment : null;
			set
			{
				if(IsEquipment)
				{
					_orderEquipment.ConfirmedComment = value;
				}
			}
		}

		public decimal Price
		{
			get
			{
				if(IsEquipment)
				{
					return _orderEquipment.OrderItem != null ? _orderEquipment.OrderItem.Price : 0;
				}

				return _orderItem.Price;
			}
			set
			{
				if(IsEquipment)
				{
					if(_orderEquipment.OrderItem != null)
					{
						_orderEquipment.OrderItem.SetPrice(value);
					}
				}
				else
				{
					_orderItem.SetPrice(value);
				}
			}
		}

		public bool IsDiscountInMoney
		{
			get
			{
				if(IsEquipment)
				{
					return _orderEquipment.OrderItem != null && _orderEquipment.OrderItem.IsDiscountInMoney;
				}

				return _orderItem.IsDiscountInMoney;
			}

			set
			{
				if(IsEquipment)
				{
					_orderEquipment.OrderItem.SetIsDiscountInMoney(_orderEquipment.OrderItem != null && value);
				}
				else
				{
					_orderItem.SetIsDiscountInMoney(value);
				}
			}
		}

		public decimal ManualChangingDiscount
		{
			get
			{
				if(IsEquipment)
				{
					return _orderEquipment.OrderItem != null ? _orderEquipment.OrderItem.ManualChangingDiscount : 0;
				}

				return _orderItem.ManualChangingDiscount;
			}

			set
			{
				if(IsEquipment)
				{
					if(_orderEquipment.OrderItem != null)
					{
						_orderEquipment.OrderItem.SetManualChangingDiscount(value);
					}
				}
				else
				{
					_orderItem.SetManualChangingDiscount(value);
				}
			}
		}

		public decimal Discount
		{
			get
			{
				if(IsEquipment)
				{
					return _orderEquipment.OrderItem != null ? _orderEquipment.OrderItem.Discount : 0m;
				}

				return _orderItem.Discount;
			}
			set
			{
				if(IsEquipment)
				{
					if(_orderEquipment.OrderItem != null)
					{
						_orderEquipment.OrderItem.SetDiscount(value);
					}
				}
				else
				{
					_orderItem.SetDiscount(value);
				}
			}
		}

		public decimal DiscountMoney
		{
			get
			{
				if(IsEquipment)
				{
					return _orderEquipment.OrderItem != null ? _orderEquipment.OrderItem.DiscountMoney : 0m;
				}

				return _orderItem.DiscountMoney;
			}
		}

		public DiscountReason DiscountReason
		{
			get => IsEquipment ? _orderEquipment.OrderItem?.DiscountReason : _orderItem.DiscountReason;
			set
			{
				if(IsEquipment)
				{
					if(_orderEquipment.OrderItem != null)
					{
						_orderEquipment.OrderItem.DiscountReason = value;
					}
				}
				else
				{
					_orderItem.DiscountReason = value;
				}
			}
		}

		public decimal Sum => Price * ActualCount - DiscountMoney;
		public string PromoSetName { get; }
	}
}
