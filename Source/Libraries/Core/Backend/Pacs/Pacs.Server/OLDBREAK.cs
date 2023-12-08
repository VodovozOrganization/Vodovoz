using Pacs.Core.Messages.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	/*
	public class OperatorBreakController : IOperatorBreakController, ISettingsConsumer
	{
		private readonly IBreakAvailabilityNotifier _breakAvailabilityNotifier;
		private readonly IPacsRepository _pacsRepository;
		private DomainSettings _actualSettings;

		private ConcurrentDictionary<int, OperatorBreakType> _operatorsOnBreak { get; set; }

		public OperatorBreakController(IBreakAvailabilityNotifier breakAvailabilityNotifier, IPacsRepository pacsRepository)
		{
			_breakAvailabilityNotifier = breakAvailabilityNotifier ?? throw new ArgumentNullException(nameof(breakAvailabilityNotifier));
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
			_operatorsOnBreak = new ConcurrentDictionary<int, OperatorBreakType>();

			_actualSettings = _pacsRepository.GetPacsDomainSettings();
			UpdateAvailability();
		}

		public bool CanStartLongBreak { get; private set; }
		public bool CanStartShortBreak { get; private set; }

		public bool CanStartBreak(int operatorId, OperatorBreakType breakType)
		{
			//сколько сейчас на перерыве
			//сколько раз были на перерыве за (день / смену / интервал)

			var breakStates = _pacsRepository.GetOperatorsOnBreak(DateTime.Today);
			var breaksByType = breakStates.Where(x => x.BreakType == breakType);

			// массовое ограничение
			// Количество операторов на перерыве этого типа
			HashSet<int> _onBreak = new HashSet<int>();
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

			int maxOperators;
			if(breakType == OperatorBreakType.Long)
			{
				maxOperators = _actualSettings.OperatorsOnLongBreak;
			}
			else
			{
				maxOperators = _actualSettings.OperatorsOnShortBreak;
			}

			bool allowByMaxOperators = _onBreak.Count < maxOperators;

			// индиидуальное ограничение
			// Огрничение на кол-во перерывов в день/интервал
			bool allowByLimit;
			if(breakType == OperatorBreakType.Long)
			{
				allowByLimit = breaksByType.Count() < _actualSettings.LongBreakCountPerDay;
			}
			else
			{
				var lastShortBreak = breaksByType.OrderBy(x => x.Started).LastOrDefault();
				allowByLimit = lastShortBreak.Started + _actualSettings.ShortBreakInterval < DateTime.Now;
			}

			return allowByMaxOperators && allowByLimit;
		}

		private bool ValidateLongBreakMaxOperatorsRestriction(IEnumerable<OperatorState> statesPerDay)
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

			bool allowByMaxOperators = _onBreak.Count < _actualSettings.OperatorsOnLongBreak;
			return allowByMaxOperators;
		}

		private bool ValidateShortBreakMaxOperatorsRestriction(IEnumerable<OperatorState> statesPerDay)
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

			bool allowByMaxOperators = _onBreak.Count < _actualSettings.OperatorsOnShortBreak;
			return allowByMaxOperators;
		}

		

		public void StartBreak(int operatorId, OperatorBreakType breakType)
		{
			_operatorsOnBreak.TryAdd(operatorId, breakType);
			UpdateAvailability();
		}

		public void EndBreak(int operatorId)
		{
			_operatorsOnBreak.TryRemove(operatorId, out var value);
			UpdateAvailability();
		}

		private void UpdateAvailability()
		{
			var breakStates = _pacsRepository.GetOperatorsOnBreak(DateTime.Today);
			var longBreakMaxOperators = ValidateLongBreakMaxOperatorsRestriction(breakStates);


			var shortBreakMaxOperators = ValidateShortBreakMaxOperatorsRestriction(breakStates);


			/*var operatorsOnLongBreak = _operatorsOnBreak.Where(x => x.Value == OperatorBreakType.Long).Count();
			var maxOperatorsOnLongBreak = _actualSettings.OperatorsOnLongBreak;

			var operatorsOnShortBreak = _operatorsOnBreak.Where(x => x.Value == OperatorBreakType.Short).Count();
			var maxOperatorsOnShortBreak = _actualSettings.OperatorsOnShortBreak;

			CanStartLongBreak = operatorsOnLongBreak < maxOperatorsOnLongBreak;
			CanStartShortBreak = operatorsOnShortBreak < maxOperatorsOnShortBreak;*/
			/*var breakEvent = new BreakAvailability
			{
				LongBreakAvailable 
			};

			_breakAvailabilityNotifier.NotifyBreakAvailability(CanStartLongBreak);
		}

		public void UpdateSettings(DomainSettings newSettings)
		{
			_actualSettings = newSettings;
			UpdateAvailability();
		}
	}*/
}
