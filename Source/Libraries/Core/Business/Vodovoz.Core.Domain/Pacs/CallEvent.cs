using System;

namespace Vodovoz.Core.Domain.Pacs
{
	/// <summary>
	/// Хранит сырые данные о событии звонка
	/// Из этих событий можно восстановить всю историю звонка,
	/// в случае проблем в системе СКУД
	/// </summary>
	public class CallEvent
	{
		public virtual int Id { get; set; }
		public virtual DateTime CreationTime { get; set; }
		public virtual string EntryId { get; set; }
		public virtual string CallId { get; set; }
		public virtual DateTime EventTime { get; set; }
		public virtual CallState State { get; set; }
		public virtual int CallSequence { get; set; }
		public virtual CallLocation Location { get; set; }
		public virtual string FromExtension { get; set; }
		public virtual string FromNumber{ get; set; }
		public virtual string TakenFromCallId { get; set; }
		public virtual bool FromWasTransfered { get; set; }
		public virtual bool FromHoldInitiator { get; set; }
		public virtual string ToExtension { get; set; }
		public virtual string ToNumber { get; set; }
		public virtual string ToLineNumber { get; set; }
		public virtual string ToAcdGroup { get; set; }
		public virtual bool ToWasTransfered { get; set; }
		public virtual bool ToHoldInitiator { get; set; }
		public virtual int? DisconnectReason { get; set; }
		public virtual CallTransferType? TransferType { get; set; }
		public virtual string DctNumber { get; set; }
		public virtual CallDctType? DctType { get; set; }
		public virtual string SipCallId { get; set; }
		public virtual string CommandId { get; set; }
		public virtual int? TaskId { get; set; }
		public virtual string CallbackInitiator { get; set; }
	}
}
