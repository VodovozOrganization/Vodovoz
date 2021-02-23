using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using BitrixApi.DTO.DataContractJsonSerializer;
using Newtonsoft.Json.Linq;

namespace BitrixIntegration.ServiceInterfaces
{
	[ServiceContract (Name = "Bitrix", Namespace="urn:bitrixintegration:serviceinterfaces")]
	public interface IBitrixServiceWeb
	{
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		[OperationContract]
		bool ServiceStatus();
		
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		[OperationContract]
		int Add(int a, int b);
		
		// [OperationContract]
		// [WebInvoke(Method = "POST",
		// 	BodyStyle = WebMessageBodyStyle.Wrapped,
		// 	ResponseFormat = WebMessageFormat.Json,
		// 	RequestFormat = WebMessageFormat.Json)]
		// void OnCrmDealUpdate(BitrixApi.DTO.DataContractJsonSerializer.DealRequest dealRequest); 

		[OperationContract()]
		// [WebInvoke(
		// 		// Method = "POST",
		// 	// BodyStyle = WebMessageBodyStyle.Wrapped,
		// 	ResponseFormat = WebMessageFormat.Json)
		// 	// RequestFormat = WebMessageFormat.Json)
		// ]
		[WebGet(ResponseFormat = WebMessageFormat.Xml)]
		void PostEvent(BitrixPostResponse response);
		// void PostEvent(BitrixPostResponse response);
	}
}
