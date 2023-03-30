using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System;
using System.Net.Http;

namespace CashReceiptApi.Client.Framework
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

			var handler = new GrpcWebHandler(new HttpClientHandler());
			_httpClient = new HttpClient(handler);
			_httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);

			var options = new GrpcChannelOptions();
			options.HttpClient = _httpClient;

			var channel = GrpcChannel.ForAddress(url, options);
			_grpcChannel = channel;
			_client = new CashReceiptServiceGrpc.CashReceiptServiceGrpcClient(channel);
		}

		public CashReceiptServiceGrpc.CashReceiptServiceGrpcClient Client => _client;

		public void Dispose()
		{
			_grpcChannel.Dispose();
			_httpClient.Dispose();
		}
	}
}
