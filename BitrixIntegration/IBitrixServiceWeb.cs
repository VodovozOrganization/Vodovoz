using System.ServiceModel;
using System.ServiceModel.Web;

namespace BitrixIntegration
{
	[ServiceContract]
	public interface IBitrixServiceWeb
	{
		[OperationContract()]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool ServiceStatus();
	}
}
