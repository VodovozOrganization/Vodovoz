using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Breaks
{
	public class OperatorBreakAvailabilityService : IOperatorBreakAvailabilityService
	{
		private readonly ILogger<OperatorBreakAvailabilityService> _logger;
		private readonly IGlobalBreakController _globalController;
		private readonly IPacsRepository _repository;
		private IPacsDomainSettings _settings;

		private readonly ConcurrentDictionary<int, OperatorBreakAvailability> _operatorBreakAvailabilityCache;

		public event EventHandler<OperatorBreakAvailability> BreakAvailabilityChanged;

		public OperatorBreakAvailabilityService(
			ILogger<OperatorBreakAvailabilityService> logger,
			IGlobalBreakController globalController,
			IPacsRepository repository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_globalController = globalController ?? throw new ArgumentNullException(nameof(globalController));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));

			_operatorBreakAvailabilityCache = new ConcurrentDictionary<int, OperatorBreakAvailability>();

			_settings = _repository.GetPacsDomainSettings();
			_globalController.SettingsChanged += OnSettingsChanged;
		}

		public void WarmUpCacheForOperatorIds(params int[] operatorIds)
		{
			foreach(int operatorId in operatorIds)
			{
				var breakAviability = GetBreakAvailability(operatorId);

				_operatorBreakAvailabilityCache.TryAdd(operatorId, breakAviability);
			}
		}

		private void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
		{
			_settings = e.Settings;

			foreach(var operatorId in _operatorBreakAvailabilityCache.Keys)
			{
				UpdateBreakStates(operatorId, e.AllOperatorsBreakStates);
			}
		}

		internal void UpdateBreakStates(int operatorId, IEnumerable<OperatorState> breakStates)
		{
			var states = breakStates.Where(x => x.OperatorId == operatorId);

			var newBreakAvailability = GetNewBreakAvailability(operatorId, states);

			if(!_operatorBreakAvailabilityCache.TryGetValue(operatorId, out OperatorBreakAvailability breakAvailability))
			{
				if(_operatorBreakAvailabilityCache.TryAdd(operatorId, newBreakAvailability))
				{
					BreakAvailabilityChanged?.Invoke(this, newBreakAvailability);
					return;
				}
				else
				{
					_logger.LogWarning(
						"Ошибка обновления доступности перерыва, не удалось заменить значение {@OldBreakAviability} новым значением {@NewBreakAviability}",
						breakAvailability,
						newBreakAvailability);
				}
			}

			if(!breakAvailability.Equals(newBreakAvailability))
			{
				if(_operatorBreakAvailabilityCache.TryUpdate(operatorId, newBreakAvailability, breakAvailability))
				{
					BreakAvailabilityChanged?.Invoke(this, newBreakAvailability);
				}
				else
				{
					_logger.LogWarning(
						"Ошибка обновления доступности перерыва, не удалось заменить значение {@OldBreakAviability} новым значением {@NewBreakAviability}",
						breakAvailability,
						newBreakAvailability);
				}
			}
		}

		public OperatorBreakAvailability GetBreakAvailability(int operatorId)
		{
			var states = _repository.GetOperatorBreakStates(operatorId, DateTime.Today);

			if(!_operatorBreakAvailabilityCache.TryGetValue(operatorId, out OperatorBreakAvailability breakAvailability))
			{
				breakAvailability = GetNewBreakAvailability(operatorId, states);

				return breakAvailability;
			}

			var newBreakAvailability = GetNewBreakAvailability(operatorId, states);

			if(!breakAvailability.Equals(newBreakAvailability))
			{
				if(_operatorBreakAvailabilityCache.TryUpdate(operatorId, newBreakAvailability, breakAvailability))
				{
					breakAvailability = newBreakAvailability;
				}
				else
				{
					_logger.LogWarning(
						"Ошибка обновления доступности перерыва, не удалось заменить значение {@OldBreakAviability} новым значением {@NewBreakAviability}",
						breakAvailability,
						newBreakAvailability);
				}
			}

			return breakAvailability;
		}

		private OperatorBreakAvailability GetNewBreakAvailability(int operatorId, IEnumerable<OperatorState> states)
		{
			var breakAvailability = new OperatorBreakAvailability
			{
				OperatorId = operatorId
			};

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
