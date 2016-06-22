using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Chat;
using ChatClass = Vodovoz.Domain.Chat.Chat;
using System.Collections.Generic;

namespace Vodovoz.Repository.Chat
{
	public static class ChatMessageRepository
	{
		public static IList<ChatMessage> GetChatMessagesForPeriod(IUnitOfWork uow, ChatClass chat, int days) {
			ChatMessage chatMessageAlias = null;

			return uow.Session.QueryOver<ChatMessage> (() => chatMessageAlias)
				.Where (() => chatMessageAlias.Chat.Id == chat.Id)
				.Where (() => chatMessageAlias.DateTime.Date >= DateTime.Now.Date.AddDays(-days))
				.OrderBy(() => chatMessageAlias.DateTime).Asc
				.List();
		}
	}
}

