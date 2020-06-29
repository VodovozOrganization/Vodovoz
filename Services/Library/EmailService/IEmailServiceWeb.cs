using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace EmailService
{
	[ServiceContract]
	public interface IEmailServiceWeb
	{
		[OperationContract()]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool ServiceStatus();
	}
}
