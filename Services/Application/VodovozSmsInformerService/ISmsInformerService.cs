using System.ServiceModel;
using System.ServiceModel.Web;

namespace VodovozSmsInformerService
{
	[ServiceContract]
	public interface ISmsInformerService
	{
		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool ServiceStatus();
	}
}
