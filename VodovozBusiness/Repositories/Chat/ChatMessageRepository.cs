using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Chat;
using ChatClass = Vodovoz.Domain.Chat.Chat;
using System.Collections.Generic;
using NHibernate.Transform;
using System.Linq;
using NHibernate.Criterion;

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

		public static Dictionary<int, int> GetUnreadedChatMessages(IUnitOfWork uow, Employee forEmployee, bool accessLogisticChat) {
			ChatMessage chatMessageAlias = null;
			ChatClass chatAlias = null;
			LastReadedMessage lastReadedAlias = null;

			var chatQuery = uow.Session.QueryOver<ChatClass>(() => chatAlias);
			if (!accessLogisticChat)
				chatQuery.Where(x => x.ChatType != ChatType.DriverAndLogists);

			var resultList = chatQuery.JoinAlias(() => chatAlias.LastReaded, () => lastReadedAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin, Restrictions.Where(() => lastReadedAlias.Employee.Id == forEmployee.Id))
				.JoinAlias(() => chatAlias.Messages, () => chatMessageAlias)
				.Where (() => lastReadedAlias.LastDateTime == null || chatMessageAlias.DateTime > lastReadedAlias.LastDateTime)
				.SelectList(list => list
					.SelectGroup(() => chatAlias.Id)
					.SelectCount(() => chatMessageAlias.Id)
				)
				.List<object[]>();
			return resultList.ToDictionary(x => (int)x[0], y => (int)y[1]);
		}

		public static Dictionary<int, int> GetLastChatMessages(IUnitOfWork uow, Employee forEmployee) {
			ChatMessage chatMessageAlias = null;
			ChatClass chatAlias = null;
			//TODO Когда будут другие типы чатов - нужно будет дописать.
			var resultList = uow.Session.QueryOver<ChatClass> (() => chatAlias)
				.JoinAlias(() => chatAlias.Messages, () => chatMessageAlias)
				.Where (() => chatAlias.ChatType == ChatType.DriverAndLogists)
				.SelectList(list => list
					.SelectGroup(() => chatAlias.Id)
					.SelectMax(() => chatMessageAlias.Id)
				)
				.List<object[]>();
			return resultList.ToDictionary(x => (int) x[0], y => (int)y[1]);
		}
	}
}

