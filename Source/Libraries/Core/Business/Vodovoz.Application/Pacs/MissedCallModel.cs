using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Application.Pacs
{
	public class MissedCallModel
	{
		private readonly CallModel _callModel;
		private readonly IEnumerable<OperatorModel> _operators;
		private readonly List<OperatorModel> _possibleOperators;

		public DateTime Started => _callModel.Started;
		public CallModel CallModel => _callModel;
		public int PossibleOperatorsCount => _possibleOperators.Count;
		public IEnumerable<OperatorModel> PossibleOperators => _possibleOperators;

		public MissedCallModel(CallModel callModel, IEnumerable<OperatorModel> operators)
		{
			_callModel = callModel ?? throw new ArgumentNullException(nameof(callModel));
			_operators = operators ?? throw new ArgumentNullException(nameof(operators));

			_possibleOperators = new List<OperatorModel>();

			FindPossibleOperators();
		}

		private void FindPossibleOperators()
		{
			//Операторы которым был дозвон для этого звонка
			var appearedOperators = _callModel.OperatorSubCalls.Select(x => x.ToExtension);

			foreach(var oper in _operators)
			{
				if(!appearedOperators.Contains(oper.CurrentState.PhoneNumber))
				{
					continue;
				}

				if(_callModel.Ended == null)
				{
					continue;
				}

				if(oper.CanTakeCallBetween(_callModel.Started, _callModel.Ended.Value))
				{
					_possibleOperators.Add(oper);
				}
			}
		}
	}
}
