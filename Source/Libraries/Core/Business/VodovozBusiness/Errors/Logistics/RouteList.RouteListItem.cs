using NPOI.SS.Formula.Functions;
using Vodovoz.Domain.Logistic;
using Vodovoz.Extensions;

namespace Vodovoz.Errors.Logistics
{
	public static partial class RouteList
	{
		public static class RouteListItem
		{
			public static Error TransferTypeNotSet => new Error(
				typeof(RouteListItem),
				nameof(TransferTypeNotSet),
				"Для адреса не указана необходимость загрузки");

			public static Error CreateTransferTypeNotSet(int routeListItemId,
string shortAddress) => new Error(
				typeof(RouteListItem),
				nameof(TransferTypeNotSet),
				$"Для адреса #{routeListItemId} \"{shortAddress}\" не указана необходимость загрузки");

			public static Error TransferRequiresLoadingWhenRouteListEnRoute => new Error(
				typeof(RouteListItem),
				nameof(TransferRequiresLoadingWhenRouteListEnRoute),
				"Для адреса была указана необходимость загрузки" +
				$" при переносе в маршрутный лист со статусом \"{RouteListStatus.EnRoute.GetEnumDisplayName()}\" и выше");

			public static Error CreateTransferRequiresLoadingWhenRouteListEnRoute(int routeListItemId, 
string shortAddress, int routeListId) => new Error(
				typeof(RouteListItem),
				nameof(TransferRequiresLoadingWhenRouteListEnRoute),
				$"Для адреса #{routeListItemId} \"{shortAddress}\" была указана необходимость загрузки" +
				$" при переносе в маршрутный лист #{routeListId} со статусом \"{RouteListStatus.EnRoute.GetEnumDisplayName()}\" и выше");

			public static Error TransferNotEnoughtFreeBalance => new Error(
				typeof(RouteListItem),
				nameof(TransferNotEnoughtFreeBalance),
				"Для переноса адреса недостаточно свободных остатков");

			public static Error CreateAddressTransferNotEnoughtFreeBalance(int routeListItemId, int routeListId) => new Error(
				typeof(RouteListItem),
				nameof(TransferNotEnoughtFreeBalance),
				$"Для переноса адреса #{routeListItemId} недостаточно свободных остатков в маршрутном листе #{routeListId}");

			public static Error CreateOrderTransferNotEnoughtFreeBalance(int orderId, int routeListId) => new Error(
				typeof(RouteListItem),
				nameof(TransferNotEnoughtFreeBalance),
				$"Для переноса заказа #{orderId} недостаточно свободных остатков в маршрутном листе #{routeListId}");

			public static Error AlreadyTransfered => new Error(
				typeof(RouteListItem),
				nameof(AlreadyTransfered),
				"Адрес маршрутного листа уже перенесен в другой маршрутный лист");

			public static Error CreateAlreadyTransfered(int routeListItemId, string routeListItemShortAddress, int transferedToRouteListId) => new Error(
				typeof(RouteListItem),
				nameof(AlreadyTransfered),
				$"Адрес #{routeListItemId} \"{routeListItemShortAddress}\" сам перенесен в МЛ №{transferedToRouteListId}." +
				$" Отмена этого переноса не возможна. Сначала необходимо отменить перенос в МЛ #{transferedToRouteListId}.");

			public static Error InvalidTransferType => new Error(
				typeof(RouteListItem),
				nameof(InvalidTransferType),
				"Не поддерживаемый тип переноса");

			public static Error CreateInvalidOrderTransferType(int orderId) => new Error(
				typeof(RouteListItem),
				nameof(InvalidTransferType),
				$"Не поддерживаемый тип переноса заказа #{orderId}");
		}
	}
}
