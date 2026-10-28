using System;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxSendingCircuitBreaker : IMindboxSendingCircuitBreaker
	{
		private readonly object _syncRoot = new object();
		private readonly IMindboxSettings _settings;
		private DateTime? _pausedUntil;
		private string _pauseReason;
		private int _transientFailures;
		private DateTime? _unavailableSince;
		private bool _unavailableAlertSent;

		public MindboxSendingCircuitBreaker(IMindboxSettings settings)
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
		public string PauseReason
		{
			get
			{
				lock(_syncRoot)
				{
					return _pauseReason;
				}
			}
		}

		/// <inheritdoc />
		public DateTime? UnavailableSince
		{
			get
			{
				lock(_syncRoot)
				{
					return _unavailableSince;
				}
			}
		}

		/// <inheritdoc />
		public bool TryMarkUnavailableAlertSent(DateTime now, TimeSpan duration)
		{
			lock(_syncRoot)
			{
				if(_unavailableAlertSent
					|| !_unavailableSince.HasValue
					|| now - _unavailableSince.Value < duration)
				{
					return false;
				}

				_unavailableAlertSent = true;

				return true;
			}
		}

		/// <inheritdoc />
		public bool IsPaused(DateTime now)
		{
			lock(_syncRoot)
			{
				if(_pausedUntil.HasValue && _pausedUntil.Value <= now)
				{
					_pausedUntil = null;
					_pauseReason = null;
				}

				return _pausedUntil.HasValue;
			}
		}

		/// <inheritdoc />
		public void Pause(DateTime now, TimeSpan duration, string reason)
		{
			lock(_syncRoot)
			{
				_pausedUntil = now.Add(duration);
				_pauseReason = reason;
				_unavailableSince = _unavailableSince ?? now;
				_transientFailures = 0;
			}
		}

		/// <inheritdoc />
		public void RegisterTransientFailure(DateTime now, string reason)
		{
			lock(_syncRoot)
			{
				_transientFailures++;
				_unavailableSince = _unavailableSince ?? now;

				var threshold = Math.Max(1, _settings.TransientErrorsBeforePause);

				if(_transientFailures >= threshold)
				{
					var pauseMinutes = Math.Max(1, _settings.PauseMinutes);
					_pausedUntil = now.AddMinutes(pauseMinutes);
					_pauseReason = reason;
					_transientFailures = 0;
				}
			}
		}

		/// <inheritdoc />
		public void Reset()
		{
			lock(_syncRoot)
			{
				_transientFailures = 0;
				_pausedUntil = null;
				_pauseReason = null;
				_unavailableSince = null;
				_unavailableAlertSent = false;
			}
		}
	}
}
