using System.ServiceModel;
using System.ServiceModel.Web;
using EmailService.Mailjet;

namespace EmailService
{
	[ServiceContract]
	public interface IMailjetEventService
	{
		[WebInvoke(Method = "POST", UriTemplate = "/PostEvent", RequestFormat = WebMessageFormat.Json)]
		[OperationContract]
		void PostEvent(MailjetEvent content);
	}
}
