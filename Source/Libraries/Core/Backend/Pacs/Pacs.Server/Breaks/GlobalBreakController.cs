using Pacs.Core.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Breaks
{
	public class GlobalBreakController : IGlobalBreakController
	{
		private readonly IPacsRepository _pacsRepository;
		private readonly IBreakAvailabilityNotifier _breakAvailabilityNotifier;
		private IPacsDomainSettings _actualSettings;
		private List<OperatorState> _onBreak = new List<OperatorState>();

		public GlobalBreakAvailabilityEvent BreakAvailability { get; private set; }

		public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

		public GlobalBreakController(IPacsRepository pacsRepository, IBreakAvailabilityNotifier breakAvailabilityNotifier)
		{
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
			_breakAvailabilityNotifier = breakAvailabilityNotifier ?? throw new ArgumentNullException(nameof(breakAvailabilityNotifier));
			BreakAvailability = new GlobalBreakAvailabilityEvent { EventId = Guid.NewGuid() };
			_actualSettings = _pacsRepository.GetPacsDomainSettings();
			UpdateBreakAvailability();
		}

		public void UpdateSettings(IPacsDomainSettings domainSettings)
		{
			_actualSettings = domainSettings;
			var operatorsOnBreak = _pacsRepository.GetOperatorsOnBreak(DateTime.Today);

			var args = new SettingsChangedEventArgs
			{
				Settings = domainSettings,
				AllOperatorsBreakStates = operatorsOnBreak,
			};
			SettingsChanged?.Invoke(this, args);

			UpdateBreakAvailability(operatorsOnBreak, domainSettings);
		}

		public void UpdateBreakAvailability()
		{
			var operatorsOnBreak = _pacsRepository.GetOperatorsOnBreak(DateTime.Today);
			UpdateBreakAvailability(operatorsOnBreak, _actualSettings);
			NotifyOperatorsOnBreak(operatorsOnBreak);
		}

		private void UpdateBreakAvailability(IEnumerable<OperatorState> operatorsOnBreak, IPacsDomainSettings currentSettings)
		{
			var longBreakAvailable = ValidateLongBreakMaxOperatorsRestriction(operatorsOnBreak, currentSettings);
			var shortBreakAvailable = ValidateShortBreakMaxOperatorsRestriction(operatorsOnBreak, currentSettings);

			var globalBreakAvailable = new GlobalBreakAvailabilityEvent { EventId = Guid.NewGuid() };
			if(!longBreakAvailable)
			{
				globalBreakAvailable.LongBreakAvailable = false;
				globalBreakAvailable.LongBreakDescription = $"Превышено кол-во операторов на большом перерыве. Макс. {currentSettings.OperatorsOnLongBreak}";
			}
			if(!shortBreakAvailable)
			{
				globalBreakAvailable.ShortBreakAvailable = false;
				globalBreakAvailable.ShortBreakDescription = $"Превышено кол-во операторов на малом перерыве. Макс. {currentSettings.OperatorsOnShortBreak}";
			}

			if(!BreakAvailability.Equals(globalBreakAvailable))
			{
				BreakAvailability = globalBreakAvailable;
				_breakAvailabilityNotifier.NotifyGlobalBreakAvailability(globalBreakAvailable);
			}
		}

		private bool ValidateLongBreakMaxOperatorsRestriction(IEnumerable<OperatorState> statesPerDay, IPacsDomainSettings currentSettings)
		{
			var breaksByType = statesPerDay.Where(x => x.BreakType == OperatorBreakType.Long);
			var _onBreak = new HashSet<int>();

			foreach(var bs in breaksByType)
			{
				if(bs.Ended != null)
				{
					continue;
				}

				if(!_onBreak.Contains(bs.OperatorId))
				{
					_onBreak.Add(bs.OperatorId);
				}
			}

			bool allowByMaxOperators = _onBreak.Count < currentSettings.OperatorsOnLongBreak;
			return allowByMaxOperators;
		}

		private bool ValidateShortBreakMaxOperatorsRestriction(IEnumerable<OperatorState> statesPerDay, IPacsDomainSettings currentSettings)
		{
			var breaksByType = statesPerDay.Where(x => x.BreakType == OperatorBreakType.Short);
			var _onBreak = new HashSet<int>();

			foreach(var bs in breaksByType)
			{
				if(bs.Ended != null)
				{
					continue;
				}

				if(!_onBreak.Contains(bs.OperatorId))
				{
					_onBreak.Add(bs.OperatorId);
				}
			}

			bool allowByMaxOperators = _onBreak.Count < currentSettings.OperatorsOnShortBreak;
			return allowByMaxOperators;
		}

		private void NotifyOperatorsOnBreak(IEnumerable<OperatorState> operatorsOnBreak)
		{
			bool hasChanges = false;
			var newOnBreak = operatorsOnBreak.ToList();
			if(_onBreak.Count != newOnBreak.Count)
			{
				hasChanges = true;
			}
			else
			{
				for(int i = 0; i < newOnBreak.Count; i++)
				{
					if(newOnBreak[i].BreakType != _onBreak[i].BreakType)
					{
						hasChanges = true;
						break;
					}
					if(newOnBreak[i].OperatorId != _onBreak[i].OperatorId)
					{
						hasChanges = true;
						break;
					}
					if(newOnBreak[i].Ended != _onBreak[i].Ended)
					{
						hasChanges = true;
						break;
					}
				}
			}

			if(!hasChanges)
			{
				return;
			}

			_onBreak = newOnBreak;
			var operatorsOnBreakEvent = GetOperatorsOnBreak(_onBreak);
			_breakAvailabilityNotifier.NotifyOperatorsOnBreak(operatorsOnBreakEvent);
		}

		private OperatorsOnBreakEvent GetOperatorsOnBreak(IEnumerable<OperatorState> operatorsOnBreak)
		{
			var result = new List<OperatorState>();
			foreach(var state in operatorsOnBreak.GroupBy(x => x.OperatorId))
			{
				var onBreakOperator = state.Where(x => x.Ended == null).LastOrDefault();
				if(onBreakOperator == null)
				{
					continue;
				}
				result.Add(onBreakOperator);
			}

			var onBreakEvent = new OperatorsOnBreakEvent
			{
				EventId = Guid.NewGuid(),
				OnBreak = result
			};

			return onBreakEvent;
		}

		public OperatorsOnBreakEvent GetOperatorsOnBreak()
		{
			var operatorsOnBreak = _pacsRepository.GetOperatorsOnBreak(DateTime.Today);
			var result = GetOperatorsOnBreak(operatorsOnBreak);
			return result;
		}
	}
}
