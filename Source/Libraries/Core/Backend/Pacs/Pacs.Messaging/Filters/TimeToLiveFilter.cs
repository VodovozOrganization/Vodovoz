using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pacs.Messaging.Filters
{
	//public class TimeToLiveFilter<T> : IFilter<PublishContext<T>>
	//	where T : class
	//{
	//	public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
	//	{
	//		//if(typeof(T) == typeof(ClientAvailable))
	//		//{
	//		//	context.TimeToLive = TimeSpan.FromSeconds(2);
	//		//}
	//		//else
	//		//{
	//		//	context.TimeToLive = TimeSpan.FromSeconds(1);
	//		//}

	//		await next.Send(context);
	//	}

	//	public void Probe(ProbeContext context) { }
	//}
}
