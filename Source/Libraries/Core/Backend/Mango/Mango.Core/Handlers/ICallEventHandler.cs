using Mango.Core.Dto;
using System.Threading.Tasks;

namespace Mango.Core.Handlers
{
	public interface ICallEventHandler
	{
		Task HandleAsync(CallEvent callEvent);
	}
}
