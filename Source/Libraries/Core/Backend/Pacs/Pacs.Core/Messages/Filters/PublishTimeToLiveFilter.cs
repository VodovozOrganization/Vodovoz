using MassTransit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Settings.Pacs;

namespace Pacs.Core.Messages.Filters
{
	public class PublishTimeToLiveFilter<T> : IFilter<PublishContext<T>>
		where T : class
	{
		private readonly IMessageTransportSettings _transportSettings;

		public PublishTimeToLiveFilter(IMessageTransportSettings transportSettings)
		{
			_transportSettings = transportSettings ?? throw new ArgumentNullException(nameof(transportSettings));
		}

		public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
		{
			var ttlSetting = _transportSettings.MessagesTimeToLive
				.FirstOrDefault(x => x.ClassFullName == context.Message.GetType().FullName);

			if(ttlSetting != null)
			{
				context.TimeToLive = TimeSpan.FromSeconds(ttlSetting.TTL);
			}

			await next.Send(context);
		}

		public void Probe(ProbeContext context) { }
	}
}
