using Core.Infrastructure;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Application.Pacs
{
	public class CallModel : PropertyChangedBase
	{
		private readonly IEnumerable<OperatorModel> _operators;
		private Call _call;

		public CallModel(IEnumerable<OperatorModel> operators)
		{
			_operators = operators ?? throw new ArgumentNullException(nameof(operators));
		}

		public Call Call
		{
			get => _call;
			set => SetField(ref _call, value);
		}

		public IList<SubCall> OperatorSubCalls { get; private set; }

		public OperatorModel Operator { get; set; }
		public DateTime Started => Call.StartTime ?? Call.CreationTime;
		public DateTime? Ended => Call.EndTime;
		public TimeSpan? Duration
		{
			get
			{
				if(Ended.HasValue)
				{
					return Ended - Started;
				}
				return null;
			}
		}

		public bool IsIncomingCall => GetAppearedExtensions().Any();

		private IList<SubCall> GetAppearedExtensions()
		{
			return Call.SubCalls
				.Where(x => x.TakenFromCallId == Call.CallId)
				.Where(x => !x.ToExtension.IsNullOrWhiteSpace())
				.ToList();
		}

		public void UpdateCall(Call call)
		{
			Call = call;
			OperatorSubCalls = GetAppearedExtensions();
			Operator = _operators
				.Where(x => x.CurrentState.State.IsNotIn(
					OperatorStateType.New, 
					OperatorStateType.Connected, 
					OperatorStateType.Disconnected))
				.Where(x => !x.CurrentState.PhoneNumber.IsNullOrWhiteSpace())
				.FirstOrDefault(x => x.CurrentState.PhoneNumber == Call.ToExtension);
		}
	}
}
