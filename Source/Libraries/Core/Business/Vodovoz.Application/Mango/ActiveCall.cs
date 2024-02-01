using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Utilities.Numeric;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Application.Mango
{
	public class ActiveCall : PropertyChangedBase
	{
		public NotificationMessage Message { get; private set; }

		public ActiveCall(NotificationMessage message)
		{
			this.Message = message ?? throw new ArgumentNullException(nameof(message));
		}

		public string CallId => Message.CallId;
		public CallState CallState => Message.State;

		public DateTime? StageBegin => Message.Timestamp.ToDateTime();
		public TimeSpan? StageDuration => DateTime.UtcNow - StageBegin;

		public string CallerName => Message.CallFrom?.Names != null ? String.Join("\n", Message.CallFrom.Names.Select(x => x.Name)) : null;
		public string CallerNumber => Message.CallFrom?.Number;

		public bool IsTransfer => Message.IsTransfer;
		public string PrimaryCallerNames => Message.PrimaryCaller?.Names != null ? String.Join("\n", Message.PrimaryCaller.Names.Select(x => x.Name)) : null;

		public string OnHoldText { get {
				if(Message.IsTransfer && Message.PrimaryCaller != null) {
					string number;
					if(Message.PrimaryCaller.Number.Length == 11) {
						var formatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
						number = "+7 " + formatter.FormatString(Message.PrimaryCaller.Number);
					} else number = Message.PrimaryCaller.Number;
					return $"{number}\n{PrimaryCallerNames}".TrimEnd();
				}
				return null;
			}
		}

		public void NewMessage(NotificationMessage message)
		{
			Message = message;
			OnPropertyChanged(nameof(CallState));
		}

		#region Clients
		private List<int> AddedClients = new List<int>();
		public void AddClientId(int id)
		{
			AddedClients.Add(id);
		}
		#endregion


		#region Ids
		public IEnumerable<int> CounterpartyIds => Message.CallFrom.Names?.Where(n => n.CounterpartyId > 0).Select(n => Convert.ToInt32(n.CounterpartyId)).Concat(AddedClients).Distinct();
		public int EmployeeId => Message.CallFrom.Names.Where(n => n.EmployeeId > 0).Select(n => Convert.ToInt32(n.EmployeeId)).FirstOrDefault();
		#endregion
		#region Calculated
		public bool IsOutgoing => Message?.Direction == CallDirection.Outgoing;

		public Phone Phone => CallerNumber != null ? new Phone(CallerNumber) : null;

		public string CallerNumberText { get {
				if(CallerNumber.Length == 11) {
					var formatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
					return "+7 " + formatter.FormatString(CallerNumber);
				} 
				else 
					return CallerNumber;
			} }

		#endregion
	}
}
