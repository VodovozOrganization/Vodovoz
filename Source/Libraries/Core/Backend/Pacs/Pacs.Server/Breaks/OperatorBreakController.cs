using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Breaks
{
	public class OperatorBreakController
	{
		private readonly ILogger<OperatorBreakController> _logger;
		private readonly GlobalBreakController _globalController;
		private readonly IPacsRepository _repository;
		private OperatorBreakAvailability _breakAvailability;
		private IPacsDomainSettings _settings;
		private int _operatorId;

		public event EventHandler<OperatorBreakAvailability> BreakAvailabilityChanged;

		public OperatorBreakController(int operatorId, ILogger<OperatorBreakController> logger, GlobalBreakController globalController, IPacsRepository repository)
		{
			_operatorId = operatorId;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_globalController = globalController ?? throw new ArgumentNullException(nameof(globalController));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));

			_breakAvailability = new OperatorBreakAvailability();
			_settings = _repository.GetPacsDomainSettings();
			_globalController.SettingsChanged += OnSettingsChanged;
		}

		private void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
		{
			_settings = e.Settings;
			UpdateBreakStates(e.AllOperatorsBreakStates);
		}

		internal void UpdateBreakStates(IEnumerable<OperatorState> breakStates)
		{
			var states = breakStates.Where(x => x.OperatorId == _operatorId);
			var newBreakAvailability = GetNewBreakAvailability(states);

			if(!_breakAvailability.Equals(newBreakAvailability))
			{
				_breakAvailability = newBreakAvailability;
				BreakAvailabilityChanged?.Invoke(this, _breakAvailability);
			}
		}

		internal OperatorBreakAvailability GetBreakAvailability()
		{
			var states = _repository.GetOperatorBreakStates(_operatorId, DateTime.Today);
			var newBreakAvailability = GetNewBreakAvailability(states);

			if(!_breakAvailability.Equals(newBreakAvailability))
			{
				_breakAvailability = newBreakAvailability;
			}

			return _breakAvailability;
		}

		private OperatorBreakAvailability GetNewBreakAvailability(IEnumerable<OperatorState> states)
		{
			var breakAvailability = new OperatorBreakAvailability();
			breakAvailability.OperatorId = _operatorId;

			var longLimitValidated = ValidateLongBreakLimitRestriction(states, _settings);
			if(!longLimitValidated)
			{
				breakAvailability.LongBreakAvailable = false;
				breakAvailability.LongBreakDescription = $"Превышено кол-во больших перерывов в день. (Макс. {_settings.LongBreakCountPerDay})";
			}

			var shortBreakAllowedAt = WhenShortBreakAllowed(states, _settings);

			var now = DateTime.Now;
			_logger.LogDebug("Breask allowed at {Date}, now: {DateNow}", shortBreakAllowedAt, now);
			var shortLimitValidated = shortBreakAllowedAt <= now;
			if(!shortLimitValidated)
			{
				breakAvailability.ShortBreakAvailable = false;
				breakAvailability.ShortBreakSupposedlyAvailableAfter = shortBreakAllowedAt;
				breakAvailability.ShortBreakDescription = $"Малый перерыв доступен только 1 раз каждые {_settings.ShortBreakInterval.ToString("h\\ч\\.\\ mm\\м\\.")}";
			}
			else
			{
				breakAvailability.ShortBreakSupposedlyAvailableAfter = null;
			}

			return breakAvailability;
		}

		private bool ValidateLongBreakLimitRestriction(IEnumerable<OperatorState> breakStates, IPacsDomainSettings actualSettings)
		{
			var breaksByType = breakStates
				.Where(x => x.BreakType == OperatorBreakType.Long);

			bool allowByLimit = breaksByType.Count() < actualSettings.LongBreakCountPerDay;
			return allowByLimit;
		}

		private DateTime WhenShortBreakAllowed(IEnumerable<OperatorState> breakStates, IPacsDomainSettings actualSettings)
		{
			var breaksByType = breakStates
				.Where(x => x.BreakType == OperatorBreakType.Short);

			_logger.LogDebug("Breaks in provided collection: {@BreakByTypes}", breaksByType);

			var lastShortBreak = breaksByType.OrderBy(x => x.Started).LastOrDefault();

			_logger.LogDebug("Breaks in provided collection: {@LastBreak}", lastShortBreak);

			if(lastShortBreak == null)
			{
				var now = DateTime.Now;
				_logger.LogDebug("Returned date {Date}", now);
				return now;
			}

			var allowedTime = lastShortBreak.Started + actualSettings.ShortBreakDuration + actualSettings.ShortBreakInterval;
			_logger.LogDebug("Returned date {Date}", allowedTime);
			return allowedTime;
		}
	}
}
