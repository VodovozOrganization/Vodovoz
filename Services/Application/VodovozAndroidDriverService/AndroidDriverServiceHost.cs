using System;
using System.ServiceModel;
using Android;

namespace VodovozAndroidDriverService
{
	public class AndroidDriverServiceHost : ServiceHost
	{
		public AndroidDriverServiceHost(AndroidDriverServiceInstanceProvider androidDriverServiceInstanceProvider) : base(typeof(AndroidDriverService))
		{
			Description.Behaviors.Add(new AndroidDriverServiceBehavior(androidDriverServiceInstanceProvider));
		}
	}
}
