using Autofac;
using Core.Infrastructure;
using MoreLinq;
using MoreLinq.Extensions;
using Pacs.Core;
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

	public class OperatorModel : PropertyChangedBase, IOperatorModel
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
			Agent = new OperatorStateAgent();
		}

		public IOperatorStateAgent Agent { get; }

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
				Agent.OperatorState = CurrentState;
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
					Agent.OperatorState = CurrentState;

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
			var possibleStates = States
				.Where(x => x.State == OperatorStateType.WaitingForCall)
				.Where(x => x.Started < to)
				.Where(x => x.Ended == null || x.Ended > from)
				.Where(x =>
				{
					DateTime endPossibleTime;
					if(x.Ended == null)
					{
						endPossibleTime = to;
					}
					else
					{
						endPossibleTime = x.Ended.Value >= to ? to : x.Ended.Value;
					}

					DateTime startPossibleTime = x.Started >= from ? x.Started : from;

					var timeToReaction = endPossibleTime - startPossibleTime;

					return timeToReaction > TimeSpan.FromSeconds(3);
				});

			return possibleStates.Any();
		}
	}
}
