using Mango.Client;
using MangoService;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Vodovoz.Application.Mango
{
	public interface IMangoManager : INotifyPropertyChanged
	{
		List<ActiveCall> ActiveCalls { get; set; }
		string CallerName { get; }
		ActiveCall CurrentCall { get; }
		ActiveCall CurrentHold { get; }
		ActiveCall CurrentOutgoingRing { get; }
		ActiveCall CurrentTalk { get; }
		uint ExtensionNumber { get; }
		bool IsOutgoing { get; }
		IEnumerable<ActiveCall> RingingCalls { get; }
		TimeSpan? StageDuration { get; }

		void AddCounterpartyToCall(int clientId);
		void Dispose();
		void ForwardCall(string to_extension, ForwardingMethod method);
		List<PhoneEntry> GetPhoneBook();
		void HangUp();
		void MakeCall(string to_extension);

		bool IsActive { get; }

		ConnectionState ConnectionState { get; }
		bool CanConnect { get; }
		void OpenMangoDialog();
		void Connect(uint extensionNumber);
		void Disconnect();
	}
}
