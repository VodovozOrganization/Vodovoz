using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace VodovozInfrastructure.ConnectionStringPipeListener
{
	public class PipeStreamConnectionStringListener
	{
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private const int _timeoutSeconds = 5;
		private string[] _commandLineArgs;
		private int _initialDelayMilliseconds;

		public EventHandler<ErrorEventArgs> Failed;
		public EventHandler<ConnectionStringEventArgs> GotConnectionString;

		public void SetupPipeStream(string[] args, int delayMilliseconds)
		{
			_initialDelayMilliseconds = delayMilliseconds;
			_commandLineArgs = args;
			Task.Run(Setup);
		}

		private async Task Setup()
		{
			await Task.Delay(_initialDelayMilliseconds);

			if(_commandLineArgs.Length < 3)
			{
				var mes = "Неверно переданы параметры запуска";
				_logger.Error(mes);
				Failed?.Invoke(this, new ErrorEventArgs(null, mes));
			}

			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));

			string[] parameters;
			try
			{
				parameters = await GetConnectionStringAsync(_commandLineArgs[2], cts.Token);
			}
			catch(OperationCanceledException ex)
			{
				var mes = $"Не была получена строка подключения за {_timeoutSeconds} секунд";
				_logger.Error(ex, mes);
				Failed?.Invoke(this, new ErrorEventArgs(ex, mes));
				return;
			}

			var connString = parameters.FirstOrDefault(x => x.StartsWith("-cs "))?.Replace("-cs ", "");
			var rootProcessIdStr = parameters.FirstOrDefault(x => x.StartsWith("-root "))?.Replace("-root ", "");

			if(String.IsNullOrWhiteSpace(connString) || !Int32.TryParse(rootProcessIdStr, out var rootProcessId))
			{
				var mes = "Неверно переданы параметры запуска";
				_logger.Error(mes);
				Failed?.Invoke(this, new ErrorEventArgs(null, mes));
				return;
			}

			try
			{
				SubscribeToRootProcess(rootProcessId);
			}
			catch(Exception ex)
			{
				var mes = "Не удалось подписаться на root процесс";
				_logger.Error(ex, mes);
				Failed?.Invoke(this, new ErrorEventArgs(ex, mes));
			}

			GotConnectionString?.Invoke(this, new ConnectionStringEventArgs(connString));
		}

		private async Task<string[]> GetConnectionStringAsync(string pipeHandleAsString, CancellationToken token)
		{
			_logger.Info("Запускаю Pipe Client...");

			var result = await Task.Run<string[]>(async () =>
			{
				using(PipeStream pipeStream = new AnonymousPipeClientStream(PipeDirection.In, pipeHandleAsString))
				{
					using(var sr = new StreamReader(pipeStream))
					{
						_logger.Info("Читаю данные...");

						IList<string> data = new List<string>();
						var i = 1;
						while(true)
						{
							data.Clear();
							_logger.Info($"Попытка чтения №{i++}");
							string line;
							while(!String.IsNullOrWhiteSpace(line = await sr.ReadLineAsync()))
							{
								data.Add(line);
							}
							if(data.Any())
							{
								return data.ToArray();
							}
							await Task.Delay(500, token);
							token.ThrowIfCancellationRequested();
						}
					}
				}
			}, token);

			return result;
		}

		private void SubscribeToRootProcess(int rootProcessId)
		{
			var rootProcess = Process.GetProcessById(rootProcessId);
			rootProcess.EnableRaisingEvents = true;
			var mes = "Корневой процесс был остановлен";

			if(rootProcess.HasExited)
			{
				_logger.Error(mes);
				Failed?.Invoke(this, new ErrorEventArgs(null, mes));
			}
			rootProcess.Exited += (sender, args) =>
			{
				_logger.Error(mes);
				Failed?.Invoke(this, new ErrorEventArgs(null, mes));
			};
		}
	}
}
