using Mango.Client;
using MangoService;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Vodovoz.Presentation.ViewModels.Mango
{
	public interface IMangoManager : INotifyPropertyChanged
	{
		List<ActiveCall> ActiveCalls { get; set; }
		string CallerName { get; }
		ConnectionState ConnectionState { get; }
		ActiveCall CurrentCall { get; }
		ActiveCall CurrentHold { get; }
		ActiveCall CurrentOutgoingRing { get; }
		ActiveCall CurrentTalk { get; }
		uint ExtensionNumber { get; }
		bool IsOutgoing { get; }
		IEnumerable<ActiveCall> RingingCalls { get; }
		TimeSpan? StageDuration { get; }

		void AddCounterpartyToCall(int clientId);
		void Connect(uint extensionNumber);
		void Disconnect();
		void Dispose();
		void ForwardCall(string to_extension, ForwardingMethod method);
		List<PhoneEntry> GetPhoneBook();
		void HangUp();
		void MakeCall(string to_extension);
		void OpenMangoDialog();

		bool IsActive { get; }
		bool CanConnect { get; }
	}
}
