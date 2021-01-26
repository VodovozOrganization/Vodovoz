using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Vodovoz.Services;
namespace VodovozEmailService
{
	public class EmailInstanceProvider : IInstanceProvider
	{
		private readonly IBitrixServiceSettings bitrixServiceSettings;

		public EmailInstanceProvider(IBitrixServiceSettings bitrixServiceSettings)
		{
			this.bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new EmailService.EmailService(bitrixServiceSettings);
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
