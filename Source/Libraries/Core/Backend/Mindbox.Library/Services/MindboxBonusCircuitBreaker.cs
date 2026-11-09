using System;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxBonusCircuitBreaker : IMindboxBonusCircuitBreaker
	{
		private readonly object _syncRoot = new object();
		private readonly IMindboxSettings _settings;

		private DateTime? _pausedUntil;
		private bool _trialRequired;
		private bool _trialInProgress;
		private int _consecutiveFailures;

		public MindboxBonusCircuitBreaker(IMindboxSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		/// <inheritdoc />
		public DateTime? PausedUntil
		{
			get
			{
				lock(_syncRoot)
				{
					return _pausedUntil;
				}
			}
		}

		/// <inheritdoc />
		public bool IsPaused(DateTime now)
		{
			lock(_syncRoot)
			{
				RefreshPauseState(now);

				return _pausedUntil.HasValue;
			}
		}

		/// <inheritdoc />
		public bool TryBeginCall(DateTime now)
		{
			lock(_syncRoot)
			{
				RefreshPauseState(now);

				if(_pausedUntil.HasValue)
				{
					return false;
				}

				if(_trialRequired)
				{
					if(_trialInProgress)
					{
						return false;
					}

					_trialInProgress = true;
				}

				return true;
			}
		}

		/// <inheritdoc />
		public void RegisterSuccess()
		{
			lock(_syncRoot)
			{
				_pausedUntil = null;
				_trialRequired = false;
				_trialInProgress = false;
				_consecutiveFailures = 0;
			}
		}

		/// <inheritdoc />
		public void RegisterFailure(DateTime now, bool pauseEligible, string reason)
		{
			lock(_syncRoot)
			{
				var wasTrial = _trialInProgress;
				_trialInProgress = false;

				if(!pauseEligible)
				{
					// Mindbox ответил, поэтому пробный вызов завершён,
					// но пауза за такую ошибку не назначается.
					_trialRequired = false;
					return;
				}

				_consecutiveFailures++;

				var threshold = Math.Max(1, _settings.BonusErrorsBeforePause);

				if(!wasTrial && _consecutiveFailures < threshold)
				{
					return;
				}

				var pauseMinutes = Math.Max(1, _settings.BonusPauseMinutes);

				_pausedUntil = now.AddMinutes(pauseMinutes);
				_trialRequired = true;
				_consecutiveFailures = 0;
			}
		}

		private void RefreshPauseState(DateTime now)
		{
			if(_pausedUntil.HasValue && _pausedUntil.Value <= now)
			{
				_pausedUntil = null;
				_trialRequired = true;
				_trialInProgress = false;
			}
		}
	}
}
