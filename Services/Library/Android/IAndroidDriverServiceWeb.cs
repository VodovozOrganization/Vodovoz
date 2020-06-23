using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Android
{
	[ServiceContract]
	public interface IAndroidDriverServiceWeb
	{
		[OperationContract()]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool ServiceStatus();
	}
}
