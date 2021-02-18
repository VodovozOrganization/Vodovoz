using System.ServiceModel;
using System.ServiceModel.Web;
using BitrixApi.DTO.DataContractJsonSerializer;
using BitrixIntegration.DTO.Mailjet;

namespace BitrixIntegration.ServiceInterfaces
{
	[ServiceContract (Name = "Bitrix", Namespace="urn:bitrixintegration:serviceinterfaces")]
	public interface IBitrixEventService {
		[WebInvoke(Method = "POST", UriTemplate = "/PostEvent", RequestFormat = WebMessageFormat.Json)]
		[OperationContract]
		void PostEvent(BitrixPostResponse response);
	}
}
