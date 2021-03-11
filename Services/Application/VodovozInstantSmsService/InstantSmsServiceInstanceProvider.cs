using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using SmsSendInterface;

namespace VodovozInstantSmsService
{
	public class InstantSmsServiceInstanceProvider : IInstanceProvider
	{
		private readonly ISmsSender smsSender;

		public InstantSmsServiceInstanceProvider(ISmsSender smsSender)
		{
			this.smsSender = smsSender;
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new InstantSmsService.InstantSmsService(smsSender);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance) { }

		#endregion
	}
}