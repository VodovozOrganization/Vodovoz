using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using VodovozMangoService.DTO;

namespace VodovozMangoService.Calling
{
    public class CallInfo
    {
        private DateTime created = DateTime.Now;
        public CallInfo(CallEvent callEvent)
        {
            Events[callEvent.Seq] = callEvent;
        }

        public readonly SortedDictionary<uint, CallEvent> Events = new SortedDictionary<uint, CallEvent>();
        public CallInfo OnHoldCall;

        public readonly HashSet<uint> ConnectedExtensions = new HashSet<uint>();
        
        #region Расчетные

        public uint Seq => Events.Keys.Last();
        public CallEvent LastEvent => Events.Last().Value;

        public TimeSpan LiveTime => DateTime.Now - created;

        public bool IsActive => Events.Values.All(e => e.CallStateEnum != CallState.Disconnected);
        
        public string EventsToText()
        {
            return String.Join("\n", Events.Values.Select(x => $"  - {JsonSerializer.Serialize(x)}"));
        }
        
        #endregion

    }
}