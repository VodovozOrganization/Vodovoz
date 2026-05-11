using QS.Extensions.Observable.Collections.List;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz
{
	public class OrderItemReturnsNode
	{
		private OrderItem _orderItem;
		private OrderEquipment _orderEquipment;

		public OrderItemReturnsNode(OrderItem item)
		{
			_orderItem = item;
			PromoSetName = _orderItem.PromoSet?.Name;
			DiscountReasons = _orderItem.DiscountReasons;
			IsDiscountReasonsEditable = true;
		}

		public OrderItemReturnsNode(OrderEquipment equipment)
		{
			_orderEquipment = equipment;
			DiscountReasons = _orderEquipment.OrderItem?.DiscountReasons ?? new ObservableList<DiscountReason>();
			IsDiscountReasonsEditable = _orderEquipment.OrderItem != null;
		}

		public OrderItem OrderItem => _orderItem;
		public OrderItem EquipmentOrderItem => _orderEquipment?.OrderItem;

		public IObservableList<DiscountReason> DiscountReasons { get; }

		public string DiscountReasonsNames => string.Join(", ", DiscountReasons.Select(dr => dr.Name));

		public bool IsDiscountReasonsEditable { get; }

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

		public decimal Sum => Price * ActualCount - DiscountMoney;
		public string PromoSetName { get; }
	}
}
