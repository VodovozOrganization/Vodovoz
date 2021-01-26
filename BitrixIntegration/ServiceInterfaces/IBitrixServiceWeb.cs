using System.ServiceModel;
using System.ServiceModel.Web;

namespace BitrixIntegration.ServiceInterfaces
{
	[ServiceContract]
	public interface IBitrixServiceWeb
	{
		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool ServiceStatus();
		
		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[OperationContract]
		int Add(int a, int b);
	}
}
