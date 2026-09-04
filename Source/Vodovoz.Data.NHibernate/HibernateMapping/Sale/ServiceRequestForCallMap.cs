using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Sale.RequestsForCall;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class ServiceRequestForCallMap : SubclassMap<ServiceRequestForCall>
	{
		public ServiceRequestForCallMap()
		{
			DiscriminatorValue(nameof(RequestForCallType.Service));
		}
	}
}
