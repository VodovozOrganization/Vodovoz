using System;
using System.Collections.Concurrent;

namespace Pacs.MangoCalls.Services
{
	internal class CallEventSeqCache
	{
		private ConcurrentDictionary<uint, bool> _eventSequence = new ConcurrentDictionary<uint, bool>();
		public string CallId { get; set; }
		public DateTime LastAction { get; private set; } = DateTime.Now;

		public CallEventSeqCache(string callId, uint seq)
		{
			CallId = callId;
			AddSeq(seq);
		}

		public bool HasSeq(uint seq)
		{
			LastAction = DateTime.Now;
			return _eventSequence.ContainsKey(seq);
		}

		public void AddSeq(uint seq)
		{
			LastAction = DateTime.Now;
			_eventSequence.TryAdd(seq, false);
		}
	}
}
