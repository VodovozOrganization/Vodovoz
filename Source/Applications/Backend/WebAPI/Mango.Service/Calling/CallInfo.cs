using Mango.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Mango.Service.Calling
{
	public class CallInfo
	{
		private DateTime created = DateTime.Now;
		public CallInfo(MangoCallEvent callEvent)
		{
			Events[callEvent.Seq] = callEvent;
		}

		public readonly SortedDictionary<uint, MangoCallEvent> Events = new SortedDictionary<uint, MangoCallEvent>();
		public CallInfo OnHoldCall;

		public readonly HashSet<uint> ConnectedExtensions = new HashSet<uint>();

		#region Расчетные

		public uint Seq => Events.Keys.Last();
		public MangoCallEvent LastEvent => Events.Last().Value;

		public TimeSpan LiveTime => DateTime.Now - created;

		public bool IsActive => Events.Values.All(e => (int)e.CallState != (int)CallState.Disconnected);

		public string EventsToText()
		{
			return string.Join("\n", Events.Values.Select(x => $"  - {JsonSerializer.Serialize(x)}"));
		}

		#endregion

	}
}
