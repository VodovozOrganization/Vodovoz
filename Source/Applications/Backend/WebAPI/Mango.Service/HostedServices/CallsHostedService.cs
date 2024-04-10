using Mango.Service.Calling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.Utilities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mango.Service.HostedServices
{
	public class CallsHostedService : IHostedService, IDisposable
	{
		private readonly ILogger<CallsHostedService> _logger;
		private readonly ILogger _loggerLostEvents;

		private Timer _timer;

		public CallsHostedService(ILoggerFactory loggerFactory)
		{
			if(loggerFactory is null)
			{
				throw new ArgumentNullException(nameof(loggerFactory));
			}

			_logger = loggerFactory.CreateLogger<CallsHostedService>();
			_loggerLostEvents = loggerFactory.CreateLogger("LostEvents");
		}

		public ConcurrentDictionary<string, CallInfo> Calls = new ConcurrentDictionary<string, CallInfo>();

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис ведения звонков запущен.");
			_timer = new Timer(CleanWorks, null, TimeSpan.Zero,
				TimeSpan.FromMinutes(1));

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис ведения звонков остановлен.");

			_timer?.Change(Timeout.Infinite, 0);

			return Task.CompletedTask;
		}

		private void CleanWorks(object state)
		{
			var toRemove = Calls.Values.Where(x => x.LiveTime.TotalHours > 1).ToList();
			if(!toRemove.Any())
				return;

			var noDisconnected = toRemove.Where(c => c.Events.Values.All(e => (int)e.CallState != (int)CallState.Disconnected)).ToList();
			if(noDisconnected.Count > 0)
			{
				var text = NumberToTextRus.FormatCase(noDisconnected.Count,
					"Следующие {0} звонок не получили события Disconnected в течении 1 часа:\n",
					"Следующие {0} звонка не получили события Disconnected в течении 1 часа:\n",
					"Следующие {0} звонков не получили события Disconnected в течении 1 часа:\n"
					);
				noDisconnected.ForEach(info => text += $"* CallInfo {info.LastEvent.CallId}:\n{info.EventsToText()}\n");
				_loggerLostEvents.LogError(text);
			}

			var lostIncome = toRemove.Where(c => c.Events.Values.All(e => (int)e.CallState == (int)CallState.Disconnected)).ToList();
			if(lostIncome.Count > 0)
			{
				var text = NumberToTextRus.FormatCase(lostIncome.Count,
					"У следующего {0} звонка было только событие Disconnected:\n",
					"У следующих {0} звонков было только событие Disconnected:\n",
					"У следующих {0} звонков было только событие Disconnected:\n"
				);
				lostIncome.ForEach(info => text += $"* CallInfo {info.LastEvent.CallId}:\n{info.EventsToText()}\n");
				_loggerLostEvents.LogError(text);
			}
			//Удаляем
			foreach(var call in toRemove)
			{
				Calls.TryRemove(call.LastEvent.CallId, out var callNull);
			}

			var activeCallsCount = Calls.Count(p => 
				p.Value.Events.Values.All(e => (int)e.CallState != (int)CallState.Disconnected));

			_logger.LogInformation("Забыта информация о {RemoveCount} звонках. Всего сервер знает " +
				"о {CallsCount} звонках, из них {ActiveCallsCount} активных.",
				toRemove.Count, Calls.Count, activeCallsCount);
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
