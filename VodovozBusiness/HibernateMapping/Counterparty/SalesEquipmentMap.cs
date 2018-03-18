using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class SalesEquipmentMap : ClassMap<SalesEquipment>
	{
		public SalesEquipmentMap()
		{
			Table("sales_equipment");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Price).Column("price");
			Map(x => x.Count).Column("count");
			References(x => x.AdditionalAgreement).Column("additional_agreement_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}