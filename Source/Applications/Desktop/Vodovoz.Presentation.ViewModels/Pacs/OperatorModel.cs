using Autofac;
using MoreLinq;
using MoreLinq.Extensions;
using QS.DomainModel.Entity;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class OperatorModel : PropertyChangedBase
	{
		private readonly IEmployeeService _employeeService;

		private bool _onBreak;

		public event EventHandler BreakStarted;
		public event EventHandler BreakEnded;

		public OperatorModel(IEmployeeService employeeService)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
		}

		public Employee Employee { get; private set; }
		public OperatorState CurrentState => States.FirstOrDefault();
		public GenericObservableList<OperatorState> States { get; set; }

		public void AddState(OperatorState state)
		{
			if(States.Count == 0)
			{
				States.Add(state);
				Employee = _employeeService.GetEmployee(state.OperatorId);
				return;
			}

			for(int i = 0; i < States.Count; i++)
			{
				if(state.Started > States[i].Started)
				{
					States.Insert(i, state);
					OnPropertyChanged(nameof(CurrentState));
					CheckBreak();
					return;
				}
			}
		}

		private void CheckBreak()
		{
			if(CurrentState.State == OperatorStateType.Break)
			{
				_onBreak = true;
				BreakStarted?.Invoke(this, EventArgs.Empty);
				return;
			}

			if(CurrentState.State != OperatorStateType.Break && _onBreak)
			{
				_onBreak = false;
				BreakEnded?.Invoke(this, EventArgs.Empty);
				return;
			}
		}

		public bool CanTakeCallBetween(DateTime from, DateTime to)
		{
			var statesAtTime = States
				.Where(x => x.Started >= from)
				.Where(x => x.Started <= to);
			return statesAtTime.All(x => x.State == OperatorStateType.WaitingForCall);
		}
	}
}
