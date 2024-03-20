using Core.Infrastructure;
using Mango.Core;
using Mango.Core.Dto;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.MangoCalls.Services
{
	internal class CallEventHandler : IDisposable
	{
		private readonly string _callEntryId;
		private readonly IUnitOfWork _uow;
		private readonly IPacsRepository _pacsRepository;
		private Call _call;

		public CallEventHandler(string callEntryId, IUnitOfWork uow, IPacsRepository pacsRepository)
		{
			_callEntryId = callEntryId;
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
		}

		#region Call event

		internal async Task<Call> HandleCallEvent(MangoCallEvent callEvent)
		{
			await SaveEvent(callEvent);
			await LoadOrCreateCall();
			UpdateCall(callEvent);
			await UpdateSubCall(callEvent);
			await _uow.SaveAsync(_call);
			return _call;
		}

		private async Task LoadOrCreateCall()
		{
			if(_call != null)
			{
				return;
			}

			_call = await _pacsRepository.GetCallByEntryAsync(_uow, _callEntryId);
			if(_call == null)
			{
				_call = new Call
				{
					EntryId = _callEntryId,
					Status = CallStatus.Appeared
				};
				await _uow.SaveAsync(_call);
			}
		}

		private void UpdateCall(MangoCallEvent callEvent)
		{
			var isMainCall = callEvent.From.TakenFromCallId.IsNullOrWhiteSpace();
			if(!isMainCall)
			{
				return;
			}

			_call.StartTime = callEvent.Timestamp.ParseTimestamp();
			_call.CallId = callEvent.CallId;
			_call.FromExtension = callEvent.From.Extension;
			_call.FromNumber = callEvent.From.Number;
			_call.ToExtension = callEvent.To.Extension;
			_call.ToNumber = callEvent.To.Number;
			_call.ToLineNumber = callEvent.To.LineNumber;

			_call.CallDirection = DetectDirection(callEvent);
		}

		private async Task UpdateSubCall(MangoCallEvent callEvent)
		{
			// Определение того что звонок является подзвонком на оператора
			/*var isSubCallToOperator = !callEvent.To.Extension.IsNullOrWhiteSpace();
			if(!isSubCallToOperator)
			{
				return;
			}*/

			var subCall = LoadOrCreateSubCall(callEvent);

			// Возможны ивенты в которых дозвон одному из операторов является первым и основным ивентом звонка
			// Тогда на его основе создается основной звонок и также подзвонок как одному из операторов
			// В таком случае TakenFromCallId подзвонка будет вести на тот же CallId, что и у этого же подзвонка
			// и также CallId у основного звонка 
			if(callEvent.From.TakenFromCallId.IsNullOrWhiteSpace())
			{
				subCall.TakenFromCallId = _call.CallId;
			}
			//Найти callId основного звонка, но как?
			var isFisrtCallLayer = callEvent.From.TakenFromCallId == _call.CallId;

			if(subCall.LastSeq < (int)callEvent.Seq)
			{
				subCall.LastSeq = (int)callEvent.Seq;
				subCall.State = ConvertState(callEvent.CallState);
			}

			if(callEvent.CallState == MangoCallState.Appeared)
			{
				subCall.StartTime = callEvent.Timestamp.ParseTimestamp();
			}

			if(callEvent.CallState == MangoCallState.Disconnected)
			{
				subCall.EndTime = callEvent.Timestamp.ParseTimestamp();
			}

			if(callEvent.CallState == MangoCallState.Connected)
			{
				subCall.WasConnected = true;
			}

			if(isFisrtCallLayer && subCall.LastSeq <= (int)callEvent.Seq)
			{
				if(callEvent.CallState == MangoCallState.Connected)
				{
					_call.Status = CallStatus.Connected;
				}

				if(callEvent.CallState == MangoCallState.OnHold)
				{
					_call.Status = CallStatus.OnHold;
				}

				if(callEvent.From.WasTransfered || callEvent.To.WasTransfered)
				{
					_call.Status = CallStatus.Transfered;
				}
			}

			await _uow.SaveAsync(subCall);
		}

		private SubCall LoadOrCreateSubCall(MangoCallEvent callEvent)
		{
			var subCall = _call.SubCalls.SingleOrDefault(x => x.CallId == callEvent.CallId);
			if(subCall == null)
			{
				subCall = new SubCall
				{
					CallId = callEvent.CallId,
					EntryId = callEvent.EntryId,
					StartTime = callEvent.Timestamp.ParseTimestamp(),
					FromExtension = callEvent.From.Extension,
					FromNumber = callEvent.From.Number,
					TakenFromCallId = callEvent.From.TakenFromCallId,
					ToExtension = callEvent.To.Extension,
					ToNumber = callEvent.To.Number,
					ToLineNumber = callEvent.To.LineNumber,
					ToAcdGroup = callEvent.To.AcdGroup
				};
				_call.SubCalls.Add(subCall);
			}
			return subCall;
		}

		private async Task SaveEvent(MangoCallEvent callEvent)
		{
			var domainCallEvent = new CallEvent
			{
				CreationTime = DateTime.Now,
				EntryId = callEvent.EntryId,
				CallId = callEvent.CallId,
				EventTime = callEvent.Timestamp.ParseTimestamp(),
				CallSequence = (int)callEvent.Seq,
				Location = ConvertLocation(callEvent.Location),
				State = ConvertState(callEvent.CallState),
				FromNumber = callEvent.From.Number,
				FromExtension = callEvent.From.Extension,
				TakenFromCallId = callEvent.From.TakenFromCallId,
				FromWasTransfered = callEvent.From.WasTransfered,
				FromHoldInitiator = callEvent.From.HoldInitiator,
				ToNumber = callEvent.To.Number,
				ToExtension = callEvent.To.Extension,
				ToLineNumber = callEvent.To.LineNumber,
				ToAcdGroup = callEvent.To.AcdGroup,
				ToWasTransfered = callEvent.To.WasTransfered,
				ToHoldInitiator = callEvent.To.HoldInitiator,
				TransferType = ConvertTransfer(callEvent.Transfer),
				DctNumber = callEvent.Dct?.Number,
				DctType = ConvertDctType(callEvent.Dct?.Type),
				DisconnectReason = callEvent.DisconnectReason,
				CommandId = callEvent.CommandId,
				SipCallId = callEvent.SipCallId,
				TaskId = callEvent.TaskId,
				CallbackInitiator = callEvent.CallbackInitiator
			};
			try
			{
				await _uow.SaveAsync(domainCallEvent);
			}
			catch(Exception ex)
			{
				throw;
			}
		}

		private CallDirection? DetectDirection(MangoCallEvent callEvent)
		{
			if(!callEvent.From.TakenFromCallId.IsNullOrWhiteSpace())
			{
				return null;
			}

			var fromAbonent = !callEvent.From.Extension.IsNullOrWhiteSpace();
			var toAbonent = !callEvent.To.Extension.IsNullOrWhiteSpace();

			if(fromAbonent && toAbonent)
			{
				return CallDirection.Internal;
			}

			if(fromAbonent && !toAbonent)
			{
				return CallDirection.Outcomming;
			}

			if(!fromAbonent && toAbonent)
			{
				return CallDirection.Incoming;
			}

			return null;
		}

		private CallLocation ConvertLocation(MangoCallLocation mangoLocation)
		{
			switch(mangoLocation)
			{
				case MangoCallLocation.Ivr:
					return CallLocation.Ivr;
				case MangoCallLocation.Queue:
					return CallLocation.Queue;
				case MangoCallLocation.Abonent:
					return CallLocation.Abonent;
				default:
					throw new NotSupportedException($"Не поддерживааемый тип локации звонка: {mangoLocation}");
			}
		}

		private CallState ConvertState(MangoCallState callState)
		{
			switch(callState)
			{
				case MangoCallState.Appeared:
					return CallState.Appeared;
				case MangoCallState.Connected:
					return CallState.Connected;
				case MangoCallState.OnHold:
					return CallState.OnHold;
				case MangoCallState.Disconnected:
					return CallState.Disconnected;
				default:
					throw new NotSupportedException($"Не поддерживааемый тип состояния звонка: {callState}");
			}
		}

		private CallTransferType? ConvertTransfer(MangoCallTransferType? callTransferType)
		{
			if(callTransferType == null)
			{
				return null;
			}

			switch(callTransferType)
			{
				case MangoCallTransferType.Consultative:
					return CallTransferType.Consultative;
				case MangoCallTransferType.Blind:
					return CallTransferType.Blind;
				case MangoCallTransferType.ReturnBlind:
					return CallTransferType.ReturnBlind;
				default:
					throw new NotSupportedException($"Не поддерживааемый тип перевода: {callTransferType}");
			}
		}

		private CallDctType? ConvertDctType(MangoCallDctType? mangoCallDctType)
		{
			if(mangoCallDctType == null)
			{
				return null;
			}

			switch(mangoCallDctType)
			{
				case MangoCallDctType.None:
					return CallDctType.None;
				case MangoCallDctType.Dynamic:
					return CallDctType.Dynamic;
				case MangoCallDctType.Static:
					return CallDctType.Static;
				default:
					throw new NotSupportedException($"Не поддерживааемый тип Dct: {mangoCallDctType}");
			}
		}

		#endregion Call event

		#region Summary event

		internal async Task<Call> HandleSummaryEvent(MangoSummaryEvent summaryEvent)
		{
			await LoadOrCreateCall();
			UpdateCallSummary(summaryEvent);
			await _uow.SaveAsync(_call);
			return _call;
		}

		private void UpdateCallSummary(MangoSummaryEvent summaryEvent)
		{
			_call.StartTime = summaryEvent.CreateTime.ParseTimestamp();
			_call.EndTime = summaryEvent.EndTime.ParseTimestamp();
			_call.FromExtension = summaryEvent.From.Extension;
			_call.FromNumber = summaryEvent.From.Number;
			_call.ToExtension = summaryEvent.To.Extension;
			_call.ToNumber = summaryEvent.To.Number;
			_call.ToLineNumber = summaryEvent.To.LineNumber;
			_call.CallDirection = ConvertDirection(summaryEvent.CallDirection);
			_call.EntryResult = ConvertEntryResult(summaryEvent.EntryResult);
			_call.DisconnectReason = summaryEvent.DisconnectReason;
			_call.Status = CallStatus.Disconnected;
		}

		private CallDirection ConvertDirection(MangoCallDirection callDirection)
		{
			switch(callDirection)
			{
				case MangoCallDirection.Internal:
					return CallDirection.Internal;
				case MangoCallDirection.Incoming:
					return CallDirection.Incoming;
				case MangoCallDirection.Outcomming:
					return CallDirection.Outcomming;
				default:
					throw new NotSupportedException($"Не поддерживааемый тип направления звонка: {callDirection}");
			}
		}

		private CallEntryResult ConvertEntryResult(MangoCallEntryResult entryResult)
		{
			switch(entryResult)
			{
				case MangoCallEntryResult.Missed:
					return CallEntryResult.Missed;
				case MangoCallEntryResult.Sucess:
					return CallEntryResult.Sucess;
				default:
					throw new NotSupportedException($"Не поддерживааемый тип результата звонка: {entryResult}");
			}
		}

		#endregion Summary event

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
