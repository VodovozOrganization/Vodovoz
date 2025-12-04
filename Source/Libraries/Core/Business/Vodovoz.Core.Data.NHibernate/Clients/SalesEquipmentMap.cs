using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.NHibernate.Clients
{
	public class SalesEquipmentMap : ClassMap<SalesEquipmentEntity>
	{
		public SalesEquipmentMap()
		{
			Table("sales_equipment");

			Id(x => x.Id).Column("id")
				.GeneratedBy.Native();

			Map(x => x.Price)
				.Column("price");

			Map(x => x.Count)
				.Column("count");

			References(x => x.AdditionalAgreement)
				.Column("additional_agreement_id");

			References(x => x.Nomenclature)
				.Column("nomenclature_id");
		}
	}
}
