using System.ServiceModel;
using System.ServiceModel.Web;

namespace InstantSmsService
{
	[ServiceContract]
	public interface IInstantSmsService
	{
		[OperationContract]
		ResultMessage SendSms(InstantSmsMessage smsNotification);

		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool ServiceStatus();
	}
}
