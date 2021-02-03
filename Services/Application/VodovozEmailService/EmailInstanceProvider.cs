using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Vodovoz.Services;
namespace VodovozEmailService
{
	public class EmailInstanceProvider : IInstanceProvider
	{
		private readonly IEmailServiceSettings emailServiceSettings;

		public EmailInstanceProvider(IEmailServiceSettings emailServiceSettings)
		{
			this.emailServiceSettings = emailServiceSettings ?? throw new ArgumentNullException(nameof(emailServiceSettings));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new EmailService.EmailService(emailServiceSettings);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
		}

		#endregion IInstanceProvider implementation
	}
}
