using Mango.Core.Dto;
using Pacs.Server;
using System;
using System.Collections.Concurrent;
using System.Timers;
using Vodovoz.Settings.Pacs;

namespace Pacs.MangoCalls.Services
{
	public class CallEventSequenceValidator : IDisposable, ICallEventSequenceValidator
	{
		private readonly IPacsSettings _pacsSettings;
		private readonly Timer _timer;

		private ConcurrentDictionary<string, CallEventSeqCache> _cache = new ConcurrentDictionary<string, CallEventSeqCache>();

		public CallEventSequenceValidator(IPacsSettings pacsSettings)
		{
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));

			_timer = new Timer(pacsSettings.CallEventsSeqCacheCleanInterval.TotalMilliseconds);
			_timer.Elapsed += (s, e) => ClearStaleCache();
			_timer.Start();
		}

		public bool ValidateCallSequence(MangoCallEvent callEvent)
		{

			var isFrstEvent = !_cache.ContainsKey(callEvent.CallId);
			var cachedSeq = _cache.GetOrAdd(callEvent.CallId, (key) => new CallEventSeqCache(callEvent.CallId, callEvent.Seq));
			if (isFrstEvent)
			{
				return true;
			}

			var hasSeq = cachedSeq.HasSeq(callEvent.Seq);
			if(hasSeq)
			{
				return false;
			}
			else
			{
				cachedSeq.AddSeq(callEvent.Seq);
				return true;
			}
		}

		private void ClearStaleCache()
		{
			var bound = DateTime.Now - _pacsSettings.CallEventsSeqCacheTimeout;
			foreach(var item in _cache)
			{
				if(item.Value.LastAction < bound)
				{
					_cache.TryRemove(item.Key, out var deletedItem);
				}
			}
		}

		public void Dispose()
		{
			_timer.Dispose();
		}
	}
}
