using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mango.Service.Calling;
using Mango.Service.Extensions;
using Microsoft.Extensions.Hosting;
using QS.Utilities;

namespace Mango.Service.HostedServices
{
	public class CallsHostedService : IHostedService, IDisposable
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private static NLog.Logger loggerLostEvents = NLog.LogManager.GetLogger("LostEvents");

		private Timer timer;

		public ConcurrentDictionary<string, CallInfo> Calls = new ConcurrentDictionary<string, CallInfo>();

		public Task StartAsync(CancellationToken cancellationToken)
		{
			logger.Info("Сервис ведения звонков запущен.");
			timer = new Timer(CleanWorks, null, TimeSpan.Zero,
				TimeSpan.FromMinutes(1));

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			logger.Info("Сервис ведения звонков остановлен.");

			timer?.Change(Timeout.Infinite, 0);

			return Task.CompletedTask;
		}

		private void CleanWorks(object state)
		{
			var toRemove = Calls.Values.Where(x => x.LiveTime.TotalHours > 1).ToList();
			if(!toRemove.Any())
				return;

			var noDisconnected = toRemove.Where(c => c.Events.Values.All(e => e.CallState.ParseCallState() != CallState.Disconnected)).ToList();
			if(noDisconnected.Count > 0)
			{
				var text = NumberToTextRus.FormatCase(noDisconnected.Count,
					"Следующие {0} звонок не получили события Disconnected в течении 1 часа:\n",
					"Следующие {0} звонка не получили события Disconnected в течении 1 часа:\n",
					"Следующие {0} звонков не получили события Disconnected в течении 1 часа:\n"
					);
				noDisconnected.ForEach(info => text += $"* CallInfo {info.LastEvent.CallId}:\n{info.EventsToText()}\n");
				loggerLostEvents.Error(text);
			}

			var lostIncome = toRemove.Where(c => c.Events.Values.All(e => e.CallState.ParseCallState() == CallState.Disconnected)).ToList();
			if(lostIncome.Count > 0)
			{
				var text = NumberToTextRus.FormatCase(lostIncome.Count,
					"У следующего {0} звонка было только событие Disconnected:\n",
					"У следующих {0} звонков было только событие Disconnected:\n",
					"У следующих {0} звонков было только событие Disconnected:\n"
				);
				lostIncome.ForEach(info => text += $"* CallInfo {info.LastEvent.CallId}:\n{info.EventsToText()}\n");
				loggerLostEvents.Error(text);
			}
			//Удаляем
			foreach(var call in toRemove)
			{
				Calls.TryRemove(call.LastEvent.CallId, out var callNull);
			}

			var activeCallsCount =
				Calls.Count(p => p.Value.Events.Values.All(e => e.CallState.ParseCallState() != CallState.Disconnected));
			logger.Info($"Забыта информация о {toRemove.Count} звонках. Всего сервер знает о {Calls.Count} звонках, из них {activeCallsCount} активных.");
		}

		public void Dispose()
		{
			timer?.Dispose();
		}
	}
}
