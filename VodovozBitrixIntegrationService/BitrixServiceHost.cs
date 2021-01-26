using System.ServiceModel;
using BitrixIntegration;

// using System.ServiceModel.Web;

namespace VodovozBitrixIntegrationService
{
	public class BitrixServiceHost : ServiceHost
	{
		public BitrixServiceHost(BitrixInstanceProvider bitrixInstanceProvider) : base(typeof(BitrixService))
		{
			Description.Behaviors.Add(new BitrixServiceBehavior(bitrixInstanceProvider));
		}
	}
}
