using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Chat;
using Vodovoz.Domain.Employees;
using ChatClass = Vodovoz.Domain.Chat.Chat;

namespace Vodovoz.Repository.Chat
{
	public static class ChatMessageRepository
	{
		public static IList<ChatMessage> GetChatMessagesForPeriod(IUnitOfWork uow, ChatClass chat, int days) {
			ChatMessage chatMessageAlias = null;

			return uow.Session.QueryOver<ChatMessage> (() => chatMessageAlias)
				.Where (() => chatMessageAlias.Chat.Id == chat.Id)
				.Where (() => chatMessageAlias.DateTime >= DateTime.Today.AddDays(-days))
				.OrderBy(() => chatMessageAlias.DateTime).Asc
				.List();
		}

		public static IList<UnreadedChatDTO> GetUnreadedChatMessages(IUnitOfWork uow, Employee forEmployee, bool accessLogisticChat) {
			ChatMessage chatMessageAlias = null;
			ChatClass chatAlias = null;
			LastReadedMessage lastReadedAlias = null;
			Employee driverAlias = null;
			UnreadedChatDTO resultAlias = null;

			var chatQuery = uow.Session.QueryOver<ChatClass>(() => chatAlias);
			if (!accessLogisticChat)
				chatQuery.Where(x => x.ChatType != ChatType.DriverAndLogists);

			var resultList = chatQuery.JoinAlias(() => chatAlias.LastReaded, () => lastReadedAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin, Restrictions.Where(() => lastReadedAlias.Employee.Id == forEmployee.Id))
									  .JoinAlias(() => chatAlias.Messages, () => chatMessageAlias)
			                          .JoinAlias(() => chatAlias.Driver, () => driverAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where (() => (lastReadedAlias.LastDateTime == null && chatMessageAlias.DateTime > forEmployee.DateOfCreate) || 
							  (lastReadedAlias.LastDateTime != null && chatMessageAlias.DateTime > lastReadedAlias.LastDateTime))
				.SelectList(list => list
			                .SelectGroup(() => chatAlias.Id).WithAlias(() => resultAlias.ChatId)
			                .Select (() => chatAlias.ChatType).WithAlias (() => resultAlias.ChatType)
			                .Select (() => driverAlias.LastName).WithAlias (() => resultAlias.EmployeeLastName)
			                .Select (() => driverAlias.Name).WithAlias (() => resultAlias.EmployeeName)
			                .Select (() => driverAlias.Patronymic).WithAlias (() => resultAlias.EmployeePatronymic)
			                .SelectCount(() => chatMessageAlias.Id).WithAlias (() => resultAlias.UnreadedMessages)
				).TransformUsing (Transformers.AliasToBean<UnreadedChatDTO> ())
				.List<UnreadedChatDTO>();
			return resultList;
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


		public class UnreadedChatDTO{

			public int ChatId { get; set;}
			public int UnreadedMessages { get; set;}
			public ChatType ChatType { get; set; }

			public string EmployeeLastName { get; set; }
			public string EmployeeName { get; set; }
			public string EmployeePatronymic { get; set; }
		}
	}
}

