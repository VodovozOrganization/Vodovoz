﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Chats;

namespace Vodovoz.HibernateMapping
{
	public class ChatMap : ClassMap<Chat>
	{
		public ChatMap ()
		{
			Table("chats");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.ChatType).Column ("type").CustomType<ChatTypeStringType> ();
			References (x => x.Driver).Column("driver_id");
			HasMany (x => x.Messages).Cascade.AllDeleteOrphan ().Inverse ().LazyLoad ().KeyColumn ("chat_id");
			HasMany (x => x.LastReaded).Cascade.AllDeleteOrphan ().Inverse ().LazyLoad ().KeyColumn ("chat_id");
		}
	}
}

