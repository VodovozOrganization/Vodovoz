using System;
using System.ServiceModel;

namespace VodovozDeliveryRulesService
{
	public class DeliveryRulesServiceHost : ServiceHost
	{
		public DeliveryRulesServiceHost(DeliveryRulesInstanceProvider deliveryRulesInstanceProvider) : base(typeof(DeliveryRulesService))
		{
			Description.Behaviors.Add(new DeliveryRulesServiceBehavior(deliveryRulesInstanceProvider));
		}
	}
}
