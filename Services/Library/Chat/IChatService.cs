using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;

namespace Chats
{
	[ServiceContract]
	public interface IChatService
	{
		[OperationContract]
		[WebInvoke (UriTemplate = "/SendMessageToLogistician", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		bool SendMessageToLogistician (string authKey, string message);

		[OperationContract]
		[WebInvoke (UriTemplate = "/SendMessageToDriver", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		bool SendMessageToDriver (int senderId, int recipientId, string message);

		[OperationContract]
		[WebInvoke (UriTemplate = "/AndroidGetChatMessages", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		List<MessageDTO> AndroidGetChatMessages (string authKey, int days);

		[OperationContract]
		[WebInvoke (UriTemplate = "/SendOrderStatusNotificationToDriver", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		bool SendOrderStatusNotificationToDriver (int senderId, int routeListItemId);

		[OperationContract]
		[WebInvoke (UriTemplate = "/SendDeliveryScheduleNotificationToDriver", BodyStyle = WebMessageBodyStyle.WrappedRequest)] 
		bool SendDeliveryScheduleNotificationToDriver (int senderId, int routeListItemId);
	}
}

