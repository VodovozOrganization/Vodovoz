using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Sale.RequestsForCall;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class RequestForCallMap : SubclassMap<RequestForCall>
	{
		public RequestForCallMap()
		{
			DiscriminatorValue(nameof(RequestForCallType.General));
		}
	}
}
