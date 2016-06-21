using System;
using Vodovoz.Domain.Chat;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class ChatMessageMap : ClassMap<ChatMessage>
	{
		public ChatMessageMap ()
		{
			Table("chat_messages");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map (x => x.Message).Column ("message");
			Map (x => x.DateTime).Column ("datetime");
			References (x => x.Chat).Column("chat_id");
			References (x => x.Sender).Column("sender_id");
		}
	}
}

