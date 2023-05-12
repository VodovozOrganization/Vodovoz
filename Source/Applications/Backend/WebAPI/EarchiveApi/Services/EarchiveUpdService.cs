using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EarchiveApi.Services
{
	public class EarchiveUpdService : EarchiveUpd.EarchiveUpdBase
	{
		private readonly ILogger<EarchiveUpdService> _logger;
		public EarchiveUpdService(ILogger<EarchiveUpdService> logger)
		{
			_logger = logger;
		}

		public override async Task GetCounterparites(None request, IServerStreamWriter<CounterpartyInfo> responseStream, ServerCallContext context)
		{
			for (var i = 0; i< 10; i++)
			{
				await responseStream.WriteAsync(new CounterpartyInfo { Id = i + 1, Name = $"N {i+1}"});
			}
		}

		public override async Task GetAddresses(CounterpartyInfo request, IServerStreamWriter<DeliveryPointInfo> responseStream, ServerCallContext context)
		{
			for(var i = 0; i < 10; i++)
			{
				await responseStream.WriteAsync(new DeliveryPointInfo { Id = i + 1, Address = $"A {i + 1}" });
			}
		}

		public override async Task GetUpdCode(DeliveryPointInfo request, IServerStreamWriter<UpdInfo> responseStream, ServerCallContext context)
		{
			for(var i = 0; i < 10; i++)
			{
				await responseStream.WriteAsync(new UpdInfo { Id = i + 1 });
			}
		}
	}
}
