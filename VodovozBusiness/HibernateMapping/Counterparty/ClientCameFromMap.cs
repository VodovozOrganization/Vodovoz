using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class ClientCameFromMap : ClassMap<ClientCameFrom>
	{
		public ClientCameFromMap()
		{
			Table("counterparty_camefrom");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
