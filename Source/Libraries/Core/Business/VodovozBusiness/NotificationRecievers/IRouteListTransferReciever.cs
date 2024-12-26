using DriverApi.Contracts.V5.Requests;
using System.Threading.Tasks;
using Vodovoz.Errors;


namespace Vodovoz.NotificationRecievers
{
	public interface IRouteListTransferReciever
	{
		Task<Result> NotifyOfOrderWithGoodsTransferingIsTransfered(NotificationRouteListChangesRequest notificationRouteListChangesRequest);
	}
}
