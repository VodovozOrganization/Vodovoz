using System.ServiceModel.Web;

namespace VodovozSalesReceiptsService
{
	public class SalesReceiptsServiceHost : WebServiceHost
	{
		public SalesReceiptsServiceHost(SalesReceiptsInstanceProvider salesReceiptsInstanceProvider) : base(typeof(SalesReceiptsService))
		{
			Description.Behaviors.Add(new SalesReceiptsServiceBehavior(salesReceiptsInstanceProvider));
		}
	}
}
