using System;

namespace Chats
{
	public interface IChatCallbackObserver
	{
		/// <summary>
		/// Represents the chat identifier for subscription.
		/// </summary>
		/// <value>The chat identifier for which HandleChatUpdate will be fired. 
		/// If <c>null</c> - handle will be fired for any chat update.</value>
		int? ChatId { get; }

		/// <summary>
		/// Sets requested refresh interval.
		/// </summary>
		/// <value>The requested refresh interval in milliseconds.
		/// If <c>null</c> - current refresh interval will not be affected.</value>
		uint? RequestedRefreshInterval { get; }

		/// <summary>
		/// Method to handle the chat update.
		/// </summary>
		void HandleChatUpdate();
	}
}

