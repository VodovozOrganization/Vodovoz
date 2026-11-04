using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class OnlyActiveFilter : FilterDefinition
	{
		public const string FilterName = "only_active";

		public OnlyActiveFilter()
		{
			WithName(FilterName);
		}
	}
}
