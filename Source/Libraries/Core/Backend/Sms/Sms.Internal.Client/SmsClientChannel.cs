using Grpc.Net.Client;
using System;
using System.Net.Http;

namespace Sms.Internal.Client
{
	public class SmsClientChannel : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly GrpcChannel _grpcChannel;
		private readonly SmsSender .SmsSenderClient _client;


		public SmsClientChannel(string url, string apiKey)
		{
			if(string.IsNullOrWhiteSpace(url))
			{
				throw new ArgumentException($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));
			}

			if(string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or whitespace.", nameof(apiKey));
			}

			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);
			var options = new GrpcChannelOptions();
			options.HttpClient = _httpClient;
			var channel = GrpcChannel.ForAddress(url, options);
			_grpcChannel = channel;
			_client = new SmsSender.SmsSenderClient(channel);
		}

		public SmsSender.SmsSenderClient Client => _client;

		public void Dispose()
		{
			_grpcChannel.Dispose();
			_httpClient.Dispose();
		}
	}
}
