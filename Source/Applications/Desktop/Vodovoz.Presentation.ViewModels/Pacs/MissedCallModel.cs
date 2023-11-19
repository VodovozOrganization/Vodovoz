using System;
using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class MissedCallModel
	{
		private readonly CallModel _callModel;
		private readonly IEnumerable<OperatorModel> _operators;
		private readonly List<OperatorModel> _possibleOperators;

		public DateTime Started => _callModel.Started;
		public CallModel Call => _callModel;
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
			foreach(var oper in _operators)
			{
				if(oper.CanTakeCallBetween(_callModel.Started, _callModel.Ended))
				{
					_possibleOperators.Add(oper);
				}
			}
		}
	}
}
