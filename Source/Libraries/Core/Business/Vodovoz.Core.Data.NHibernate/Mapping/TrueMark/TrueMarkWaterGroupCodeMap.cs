﻿using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.TrueMark
{
	public class TrueMarkWaterGroupCodeMap : ClassMap<TrueMarkWaterGroupCode>
	{
		public TrueMarkWaterGroupCodeMap()
		{
			Table("true_mark_water_group_code");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ParentTransportCodeId).Column("parent_transport_code_id");
			Map(x => x.ParentWaterGroupCodeId).Column("parent_water_group_code_id");

			Map(x => x.RawCode).Column("raw_code");
			Map(x => x.IsInvalid).Column("is_invalid");

			HasMany(x => x.InnerGroupCodes)
				.KeyColumn("parent_water_group_code_id")
				.Not.LazyLoad()
				.Inverse()
				.Cascade.All();

			HasMany(x => x.InnerWaterCodes)
				.KeyColumn("parent_water_group_code_id")
				.Not.LazyLoad()
				.Inverse()
				.Cascade.All();
		}
	}
}
