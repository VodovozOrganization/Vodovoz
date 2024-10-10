using Autofac;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using AddressTransferTypeEnum = Vodovoz.Domain.Logistic.AddressTransferType;

namespace Vodovoz.ViewModels.Logistic
{
	public partial class RouteListTransferringViewModel
	{
		public class RouteListItemNode
		{
			private AddressTransferTypeEnum? _addressTransferType;
			private RouteListItem _routeListItem;

			public RouteListItem RouteListItem
			{
				get => _routeListItem;
				set
				{
					_routeListItem = value;
					if(value is null)
					{
						AddressTransferType = null;
						return;
					}
					AddressTransferType = RouteListItem.AddressTransferType;
				}
			}

			public Order Order { get; set; }

			public int? AddressId => RouteListItem?.Id;

			public int OrderId => (RouteListItem?.Order ?? Order).Id;

			public string Date => (RouteListItem?.Order ?? Order).DeliveryDate.Value.ToString("d");
			public string Address => (RouteListItem?.Order ?? Order).DeliveryPoint?.ShortAddress ?? "Нет адреса";
			public RouteListItemStatus? AddressStatus => RouteListItem?.Status;

			public RouteListStatus? RouteListStatus => RouteListItem?.RouteList?.Status;

			public bool IsNeedToReload
			{
				get => RouteListItem?.AddressTransferType == AddressTransferTypeEnum.NeedToReload;
				set
				{
					if(value)
					{
						AddressTransferType = AddressTransferTypeEnum.NeedToReload;
					}
					else
					{
						RouteListItem.AddressTransferType = null;
					}
				}
			}

			public bool IsFromHandToHandTransfer
			{
				get => RouteListItem?.AddressTransferType == AddressTransferTypeEnum.FromHandToHand;
				set
				{
					if(value)
					{
						AddressTransferType = AddressTransferTypeEnum.FromHandToHand;
					}
					else
					{
						RouteListItem.AddressTransferType = null;
					}
				}
			}

			public bool IsFromFreeBalance
			{
				get => AddressTransferType == AddressTransferTypeEnum.FromFreeBalance;
				set
				{
					if(value)
					{
						AddressTransferType = AddressTransferTypeEnum.FromFreeBalance;
					}
					else
					{
						AddressTransferType = null;
					}
				}
			}

			public bool IsFastDelivery => (RouteListItem?.Order ?? Order).IsFastDelivery;

			public bool WasTransfered => RouteListItem?.WasTransfered ?? false;
			public string Comment => RouteListItem?.Comment ?? "";

			public decimal BottlesCount =>
				(RouteListItem?.Order.OrderItems ?? Order.OrderItems)
					.Where(orderItem => orderItem.Nomenclature.Category == NomenclatureCategory.water
						&& !orderItem.Nomenclature.IsDisposableTare)
					.Sum(waterBottleItem => waterBottleItem.Count);

			public string FormattedBottlesCount => $"{BottlesCount:N0}";

			public string DalyNumber => (RouteListItem?.Order ?? Order).DailyNumber.ToString();
			public bool NeedTerminal => (RouteListItem?.Order ?? Order).PaymentType == PaymentType.Terminal;

			public AddressTransferTypeEnum? AddressTransferType
			{
				get => _addressTransferType;
				set
				{
					_addressTransferType = value;
					if(RouteListItem is null)
					{
						return;
					}
					RouteListItem.AddressTransferType = value;
				}
			}
		}
	}
}
