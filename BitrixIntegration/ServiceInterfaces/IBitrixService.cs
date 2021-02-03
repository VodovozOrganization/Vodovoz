using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using BitrixIntegration.DTO;
using Vodovoz.Domain.Orders;

namespace BitrixIntegration.ServiceInterfaces
{
	[ServiceContract(Name = "Bitrix", Namespace="urn:bitrixintegration:serviceinterfaces")]
	public interface IBitrixService {
		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[OperationContract]
		Tuple<bool, string> SendNewStatus(OrderStatus status, Order order);
	}
}
