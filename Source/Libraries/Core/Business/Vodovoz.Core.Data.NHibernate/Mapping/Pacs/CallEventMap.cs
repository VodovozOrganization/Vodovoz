using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class CallEventMap : ClassMap<CallEvent>
	{
		public CallEventMap()
		{
			Table("pacs_call_events");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreationTime).Column("creation_time").ReadOnly();

			Map(x => x.EntryId).Column("entry_id");
			Map(x => x.CallId).Column("call_id");
			Map(x => x.EventTime).Column("event_time");
			Map(x => x.State).Column("call_state");
			Map(x => x.CallSequence).Column("call_sequence");
			Map(x => x.Location).Column("location");
			Map(x => x.FromExtension).Column("from_extension");
			Map(x => x.FromNumber).Column("from_number");
			Map(x => x.TakenFromCallId).Column("taken_from_call_id");
			Map(x => x.FromWasTransfered).Column("from_was_transfered");
			Map(x => x.FromHoldInitiator).Column("from_hold_initiator");
			Map(x => x.ToExtension).Column("to_extension");
			Map(x => x.ToNumber).Column("to_number");
			Map(x => x.ToLineNumber).Column("to_line_number");
			Map(x => x.ToAcdGroup).Column("to_acd_group");
			Map(x => x.ToWasTransfered).Column("to_was_transfered");
			Map(x => x.ToHoldInitiator).Column("to_hold_initiator");
			Map(x => x.DisconnectReason).Column("disconnect_reason");
			Map(x => x.TransferType).Column("transfer_type");
			Map(x => x.DctNumber).Column("dct_number");
			Map(x => x.DctType).Column("dct_type");
			Map(x => x.SipCallId).Column("sip_call_id");
			Map(x => x.CommandId).Column("command_id");
			Map(x => x.TaskId).Column("task_id");
			Map(x => x.CallbackInitiator).Column("callback_initiator");
		}
	}
}
