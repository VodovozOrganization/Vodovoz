using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class FuelApiRequestMap : ClassMap<FuelApiRequest>
	{
		public FuelApiRequestMap()
		{
			Table("fuel_api_requests");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.RequestDateTime).Column("request_date_time");
			Map(x => x.RequestFunction).Column("request_function");
			Map(x => x.ResponseResult).Column("response_result");
			Map(x => x.ErrorResponseMessage).Column("error_response_message");

			References(x => x.Author).Column("author_id");
		}
	}
}
