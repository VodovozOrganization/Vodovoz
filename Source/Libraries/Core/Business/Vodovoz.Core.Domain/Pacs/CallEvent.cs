using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public class CallEvent : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationTime;
		private DateTime _eventTime;
		private string _callId;
		private uint _callSequence;
		private CallState _callState;
		private int _disconnectReason;
		private string _fromNumber;
		private string _fromExtension;
		private string _takenFromCallId;
		private string _toNumber;
		private string _toExtension;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		[Display(Name = "Время события")]
		public virtual DateTime EventTime
		{
			get => _eventTime;
			set => SetField(ref _eventTime, value);
		}

		[Display(Name = "Идентификатор звонка")]
		public virtual string CallId
		{
			get => _callId;
			set => SetField(ref _callId, value);
		}

		[Display(Name = "Номер события")]
		public virtual uint CallSequence
		{
			get => _callSequence;
			set => SetField(ref _callSequence, value);
		}

		[Display(Name = "Состояние звонка")]
		public virtual CallState CallState
		{
			get => _callState;
			set => SetField(ref _callState, value);
		}

		[Display(Name = "Код причины отключения")]
		public virtual int DisconnectReason
		{
			get => _disconnectReason;
			set => SetField(ref _disconnectReason, value);
		}

		[Display(Name = "Номер звонящего")]
		public virtual string FromNumber
		{
			get => _fromNumber;
			set => SetField(ref _fromNumber, value);
		}

		[Display(Name = "Добавочный номер звонящего")]
		public virtual string FromExtension
		{
			get => _fromExtension;
			set => SetField(ref _fromExtension, value);
		}

		[Display(Name = "Переведен с")]
		public virtual string TakenFromCallId
		{
			get => _takenFromCallId;
			set => SetField(ref _takenFromCallId, value);
		}

		[Display(Name = "Номер вызываемого")]
		public virtual string ToNumber
		{
			get => _toNumber;
			set => SetField(ref _toNumber, value);
		}

		[Display(Name = "Добавочный номер вызываемого")]
		public virtual string ToExtension
		{
			get => _toExtension;
			set => SetField(ref _toExtension, value);
		}
	}
}
