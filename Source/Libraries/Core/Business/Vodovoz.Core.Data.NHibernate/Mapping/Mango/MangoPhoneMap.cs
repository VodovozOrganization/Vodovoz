using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Mango;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Mango
{
	public class MangoPhoneMap : ClassMap<MangoPhone>
	{
		public MangoPhoneMap()
		{
			Table("mango_phones");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.Description).Column("description");
		}
	}
}
