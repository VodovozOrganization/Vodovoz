using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Zabbix.Sender
{
	public interface IZabbixSender
	{
		Task<bool> SendIsHealthyAsync(CancellationToken cancellationToken);
		Task<bool> SendIsUnhealthyAsync(ZabixSenderMessageType zabixSenderMessageType, string message, CancellationToken cancellationToken);
	}
}
