using System.ServiceModel;

namespace BitrixIntegration.ServiceInterfaces
{
	[ServiceContract (Name = "Bitrix", Namespace="urn:bitrixintegration:serviceinterfaces")]
	public interface IBitrixEventService {
		// [WebInvoke(Method = "POST", UriTemplate = "/PostEvent", RequestFormat = WebMessageFormat.Json)]
		// [OperationContract]
		// void PostEvent(BitrixPostResponse response);
	}
}
