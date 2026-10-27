using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark.TrueMarkProductCodes
{
	public class ManualTrueMarkProductCodeMap : SubclassMap<ManualTrueMarkProductCode>
	{
		public ManualTrueMarkProductCodeMap()
		{
			DiscriminatorValue("Manual");
		}
	}
}
