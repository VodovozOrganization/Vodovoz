using System;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Application.Pacs
{
	public interface IOperatorModel
	{
		OperatorState CurrentState { get; }
		Employee Employee { get; }
		IPacsDomainSettings Settings { get; }
		GenericObservableList<OperatorState> States { get; set; }

		event EventHandler BreakEnded;
		event EventHandler BreakStarted;

		void AddState(OperatorState state);
		bool CanTakeCallBetween(DateTime from, DateTime to);
	}
}
