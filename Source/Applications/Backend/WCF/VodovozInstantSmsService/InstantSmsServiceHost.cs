using System;
using System.ServiceModel;

namespace VodovozInstantSmsService
{
	public class InstantSmsServiceHost : ServiceHost
	{
		public InstantSmsServiceHost(InstantSmsServiceInstanceProvider instantSmsInstanceProvider) : base(typeof(InstantSmsService.InstantSmsService))
		{
			Description.Behaviors.Add(new InstantSmsServiceBehavior(instantSmsInstanceProvider));
		}
	}
}
