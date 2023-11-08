using MassTransit;
using Microsoft.Extensions.Hosting;
using Pacs.Core.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pacs.Test.OperatorClient
{
	public class StartupService : BackgroundService
	{
		readonly IRequestClient<Connect> _client;

		public StartupService(IRequestClient<Connect> client)
		{
			_client = client;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(true)
			{
				try
				{
					var msg = new Connect();

					msg.OperatorId = 5;

					//await bus.Publish(msg);
					var response = await _client.GetResponse<OperatorResult>(msg, stoppingToken);
					var result = response.Message;

					if(result.Result == Result.Success)
					{
						var state = result.Operator;
						Console.WriteLine($"Оператор Id: {state.Id}");
						Console.WriteLine($"Состояние: {state.State}");
						Console.WriteLine($"Начало: {state.Started}");
					}
				}
				catch(System.Exception ex)
				{

					throw;
				}
				await Task.Delay(500);
			}
		}
	}
}
