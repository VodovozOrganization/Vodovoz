using Autofac;
using Core.Infrastructure;
using MoreLinq;
using MoreLinq.Extensions;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
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
		private readonly HashSet<int> _stateIds;

		private IPacsDomainSettings _settings;
		private bool _onBreak;

		public event EventHandler BreakStarted;
		public event EventHandler BreakEnded;

		public OperatorModel(IEmployeeService employeeService)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_stateIds = new HashSet<int>();
			States = new GenericObservableList<OperatorState>();
		}

		public IPacsDomainSettings Settings
		{
			get => _settings;
			internal set => SetField(ref _settings, value);
		}

		public Employee Employee { get; private set; }
		public OperatorState CurrentState => States.FirstOrDefault();
		public GenericObservableList<OperatorState> States { get; set; }

		public void AddState(OperatorState state)
		{
			if(States.Count == 0)
			{
				States.Add(state);
				_stateIds.Add(state.Id);
				Employee = _employeeService.GetEmployee(state.OperatorId);
				return;
			}

			if(_stateIds.Contains(state.Id))
			{
				return;
			}

			for(int i = 0; i < States.Count; i++)
			{
				if(state.Started > States[i].Started)
				{
					_stateIds.Add(state.Id);
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
			var statesInCallPeriod = States.Where(x =>
				{
					if(x.Ended == null && x.Started < to)
					{
						return true;
					}
					if(x.Ended != null && x.Ended > from && x.Ended < to)
					{
						return true;
					}
					return false;
				}).OrderBy(x => x.Started);

			OperatorState lastWaitingState = null;
			foreach(var state in statesInCallPeriod)
			{
				if(state.State == OperatorStateType.WaitingForCall)
				{
					lastWaitingState = state;
				}

				if(state.Ended == null)
				{
					if(state.State == OperatorStateType.WaitingForCall)
					{
						return true;
					}

					if(lastWaitingState != null && state.State.IsIn(OperatorStateType.Connected, OperatorStateType.Break))
					{
						return true;
					}
				}
			}


			/*var lastOpenedState = States
				.Where(x => x.Started < from)
				.Where(x => x.Ended == null)
				.Where(x => x.State == OperatorStateType.WaitingForCall)
				.FirstOrDefault();

			if(lastOpenedState != null)
			{
				return true;
			}

			var lastDeniedState = States
				.Where(x => x.Ended == null)
				.Where(x => x.Started < to)
				.Where(x => x.Started > from)
				.Where(x => x.State.IsIn(OperatorStateType.Connected, OperatorStateType.Break))
				.FirstOrDefault();

			if(lastDeniedState != null)
			{
				States.
			}*/



			/*var statesAtTime = States
				.Where(x => x.Started >= from)
				.Where(x => x.Started <= to);
			if(!statesAtTime.Any())
			{
				return false;
			}
			var result = statesAtTime.All(x => x.State == OperatorStateType.WaitingForCall);
			return result;*/

			return false;
		}
	}
}
