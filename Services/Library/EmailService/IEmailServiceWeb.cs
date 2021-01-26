using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace BitrixService
{
	[ServiceContract]
	public interface IEmailServiceWeb
	{
		[OperationContract()]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool ServiceStatus();
	}
}
