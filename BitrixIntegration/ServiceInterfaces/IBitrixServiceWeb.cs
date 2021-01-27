using System.ServiceModel;
using System.ServiceModel.Web;

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
	}
}
