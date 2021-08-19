using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Chats;

namespace Chats
{
	public class ChatCallbackObservable
	{
		private static ChatCallbackObservable instance;
		private const uint REFRESH_INTERVAL = 30000;
		private uint refreshInterval = REFRESH_INTERVAL;
		private uint timerId;
		private IUnitOfWorkGeneric<Employee> employeeUoW;
		private IList<IChatCallbackObserver> observers;
		private readonly IChatMessageRepository _chatMessageRepository;

		private Dictionary<int, int> unreadedMessages;

		public static bool IsInitiated { get { return instance != null; } }

		public static void CreateInstance(int employeeId, IChatMessageRepository chatMessageRepository) 
		{
			instance = new ChatCallbackObservable (employeeId, chatMessageRepository);
		}

		public static ChatCallbackObservable GetInstance() 
		{
			if (instance == null)
				throw new NullReferenceException("Попытка вызова метода ChatCallbackObservable.GetInstance()" +
					" без предварительной инициализации. Сначала требуется вызвать CreateInstance(int employeeId).");
			return instance;
		}

		private ChatCallbackObservable(int employeeId, IChatMessageRepository chatMessageRepository)
		{
			_chatMessageRepository = chatMessageRepository ?? throw new ArgumentNullException(nameof(chatMessageRepository));

			observers = new List<IChatCallbackObserver>();
			employeeUoW = UnitOfWorkFactory.CreateForRoot<Employee>(employeeId, $"[CS]Слежение за чатами");
			unreadedMessages = _chatMessageRepository.GetLastChatMessages(employeeUoW);

			//Initiates new message check every 30 seconds.
			timerId = GLib.Timeout.Add(refreshInterval, new GLib.TimeoutHandler (refresh));
		}

		public void AddObserver(IChatCallbackObserver observer) 
		{
			if (!observers.Contains(observer))
			{
				observers.Add(observer);
				if (observer.RequestedRefreshInterval != null && observer.RequestedRefreshInterval < refreshInterval)
				{
					refreshInterval = (uint)observer.RequestedRefreshInterval;
					GLib.Source.Remove(timerId);
					timerId = GLib.Timeout.Add(refreshInterval, new GLib.TimeoutHandler (refresh));
				}
			}
		}

		public void RemoveObserver(IChatCallbackObserver observer) 
		{
			if (observers.Contains(observer))
				observers.Remove(observer);

			uint interval = REFRESH_INTERVAL;
			foreach (var obs in observers)
			{
				if (obs.RequestedRefreshInterval != null && obs.RequestedRefreshInterval < interval)
					interval = (uint)obs.RequestedRefreshInterval;
			}
			if (refreshInterval != interval)
			{
				refreshInterval = interval;
				GLib.Source.Remove(timerId);
				timerId = GLib.Timeout.Add(refreshInterval, new GLib.TimeoutHandler (refresh));
			}
		}

		private bool refresh()
		{
			var tempUnreadedMessages = _chatMessageRepository.GetLastChatMessages(employeeUoW);
			foreach (var item in tempUnreadedMessages)
			{
				if (!unreadedMessages.ContainsKey(item.Key) || unreadedMessages[item.Key] != item.Value)
					NotifyChatUpdate(item.Key);
			}
			unreadedMessages = tempUnreadedMessages;
			return true;
		}
			
		public void NotifyChatUpdate(int chatId)
		{
			for (int i = 0; i < observers.Count; i++)
			{
				if (observers[i] == null)
				{
					observers.RemoveAt(i);
					i--;
					continue;
				}
				if (observers[i].ChatId == null || observers[i].ChatId == chatId)
					observers[i].HandleChatUpdate();
			}
		}

		public void NotifyChatUpdate(int chatId, IChatCallbackObserver observer)
		{
			for (int i = 0; i < observers.Count; i++)
			{
				if (observers[i] == null)
				{
					observers.RemoveAt(i);
					i--;
					continue;
				}
				if (observers[i] != observer && (observers[i].ChatId == null || observers[i].ChatId == chatId))
					observers[i].HandleChatUpdate();
			}
		}
	}
}

