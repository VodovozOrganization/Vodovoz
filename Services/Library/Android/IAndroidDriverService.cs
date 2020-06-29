using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Collections.Generic;
using Android.DTO;

namespace Android
{
	[ServiceContract]
	public interface IAndroidDriverService
	{
		[OperationContract]
		[WebInvoke(UriTemplate = "/CheckApplicationVersion", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		CheckVersionResultDTO CheckApplicationVersion(int versionCode);

		[OperationContract]
		[WebInvoke (UriTemplate = "/Auth", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		string Auth (string login, string password);

		[OperationContract]
		[WebInvoke (UriTemplate = "/CheckAuth", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		bool CheckAuth (string authKey);

		[OperationContract]
		[WebInvoke (UriTemplate = "/GetRouteLists", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		List<RouteListDTO> GetRouteLists (string authKey);

		[OperationContract]
		[WebInvoke (UriTemplate = "/GetRouteListOrders", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		List<ShortOrderDTO> GetRouteListOrders (string authKey, int routeListId);

		[OperationContract]
		[WebInvoke (UriTemplate = "/GetOrderDetailed", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		OrderDTO GetOrderDetailed (string authKey, int orderId);

		[OperationContract]
		[WebInvoke(UriTemplate = "/ChangeOrderStatus2", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		bool ChangeOrderStatus2(string authKey, int orderId, string status, string bottlesReturned);

		[OperationContract]
		[WebInvoke (UriTemplate = "/SendCoordinates", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		bool SendCoordinates (string authKey, int trackId, TrackPointList TrackPointList);

		[OperationContract]
		[WebInvoke (UriTemplate = "/StartOrResumeTrack", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		int? StartOrResumeTrack (string authKey, int routeListId);

		[OperationContract]
		[WebInvoke (UriTemplate = "/EnablePushNotifications", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		bool EnablePushNotifications (string authKey, string token);

		[OperationContract]
		[WebInvoke (UriTemplate = "/DisablePushNotifications", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		bool DisablePushNotifications (string authKey);

		[OperationContract]
		[WebInvoke (UriTemplate = "/FinishRouteList", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		bool FinishRouteList (string authKey, int routeListId);

		[OperationContract]
		[WebInvoke(UriTemplate = "/GetPaymentStatus", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		PaymentInfoDTO GetPaymentStatus(string authKey, int orderId);

		[OperationContract]
		[WebInvoke(UriTemplate = "/CreateOrderPayment", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		PaymentInfoDTO CreateOrderPayment(string authKey, int orderId, string phoneNumber);

		[OperationContract]
		[WebInvoke(UriTemplate = "/RefreshPaymentStatus", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		bool RefreshPaymentStatus(int orderId);
	}
}
