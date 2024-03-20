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

		public CallModel(IEnumerable<OperatorModel> operators)
		{
			_operators = operators ?? throw new ArgumentNullException(nameof(operators));
		}

		public Call Call { get; private set; }

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

		public IEnumerable<string> GetAppearedExtensions()
		{
			return Call.SubCalls
				.Where(x => x.TakenFromCallId == Call.CallId)
				.Where(x => !x.ToExtension.IsNullOrWhiteSpace())
				.Select(x => x.ToExtension);
		}

		public void UpdateCall(Call call)
		{
			Call = call;
		}
	}
}
