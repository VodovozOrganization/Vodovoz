using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace VodovozSalesReceiptsService
{
	[ServiceContract]
	public interface ISalesReceiptsService
	{
		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool ServiceStatus();
	}
}
