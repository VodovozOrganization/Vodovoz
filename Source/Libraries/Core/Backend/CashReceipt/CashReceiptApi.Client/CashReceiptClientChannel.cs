using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CashReceiptApi.Client
{
	public class CashReceiptClientChannel : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly GrpcChannel _grpcChannel;
		private readonly CashReceiptServiceGrpc.CashReceiptServiceGrpcClient _client;

		public CashReceiptClientChannel(string url, string apiKey)
		{
			if(string.IsNullOrWhiteSpace(url))
			{
				throw new ArgumentException($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));
			}

			if(string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or whitespace.", nameof(apiKey));
			}

			CheckFramework();

			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);

			var options = new GrpcChannelOptions();
			options.HttpClient = _httpClient;
			var channel = GrpcChannel.ForAddress(url, options);
			_grpcChannel = channel;
			_client = new CashReceiptServiceGrpc.CashReceiptServiceGrpcClient(channel);
		}

		private void CheckFramework()
		{
			if(RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework"))
			{
				throw new InvalidOperationException($"Сборка {Assembly.GetExecutingAssembly().GetName().Name} не предназначена " +
					$"для использования в .NET Framework. Воспользуйтесь специальной версией сборки с приставкой Framework.");
			}
		}

		public CashReceiptServiceGrpc.CashReceiptServiceGrpcClient Client => _client;

		public void Dispose()
		{
			_grpcChannel.Dispose();
			_httpClient.Dispose();
		}
	}
}
