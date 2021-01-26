using System.ServiceModel;
using System.ServiceModel.Web;
using BitrixIntegration.DTO.Mailjet;

namespace BitrixIntegration
{
	[ServiceContract]
	public interface IMailjetEventService
	{
		[WebInvoke(Method = "POST", UriTemplate = "/PostEvent", RequestFormat = WebMessageFormat.Json)]
		[OperationContract]
		void PostEvent(MailjetEvent content);
	}
}
