using Pacs.Core.Messages.Events;
using Pacs.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Operators.Server
{
	internal class SettingsChangedEventArgs : EventArgs
	{
		public IPacsDomainSettings Settings { get; set; }
		public IEnumerable<OperatorState> AllOperatorsBreakStates { get; set; }
	}

	public class GlobalBreakController
	{
		private readonly IPacsRepository _pacsRepository;
		private readonly IBreakAvailabilityNotifier _breakAvailabilityNotifier;
		private IPacsDomainSettings _actualSettings;

		internal event EventHandler<SettingsChangedEventArgs> SettingsChanged;
		public GlobalBreakAvailability BreakAvailability { get; private set; }

		public GlobalBreakController(IPacsRepository pacsRepository, IBreakAvailabilityNotifier breakAvailabilityNotifier)
		{
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
			_breakAvailabilityNotifier = breakAvailabilityNotifier ?? throw new ArgumentNullException(nameof(breakAvailabilityNotifier));
			BreakAvailability = new GlobalBreakAvailability();
			_actualSettings = _pacsRepository.GetPacsDomainSettings();
			UpdateBreakAvailability();
		}

		internal void UpdateSettings(IPacsDomainSettings domainSettings)
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

		internal void UpdateBreakAvailability()
		{
			var operatorsOnBreak = _pacsRepository.GetOperatorsOnBreak(DateTime.Today);
			UpdateBreakAvailability(operatorsOnBreak, _actualSettings);
		}

		private void UpdateBreakAvailability(IEnumerable<OperatorState> operatorsOnBreak, IPacsDomainSettings currentSettings)
		{
			var longBreakAvailable = ValidateLongBreakMaxOperatorsRestriction(operatorsOnBreak, currentSettings);
			var shortBreakAvailable = ValidateShortBreakMaxOperatorsRestriction(operatorsOnBreak, currentSettings);

			var globalBreakAvailable = new GlobalBreakAvailability();
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
				NotifyOperatorsOnBreak(operatorsOnBreak);
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
			var result  = new List<OperatorState>();
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
				OnBreak	= result
			};

			_breakAvailabilityNotifier.NotifyOperatorsOnBreak(onBreakEvent);
		} 
	}
}
