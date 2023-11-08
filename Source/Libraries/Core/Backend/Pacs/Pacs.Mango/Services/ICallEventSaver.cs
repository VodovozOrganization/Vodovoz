using Mango.Core.Dto;
using System.Threading.Tasks;

namespace Pacs.Mango.Services
{
	public interface ICallEventSaver
	{
		Task SaveCallEvent(CallEvent callEvent);
	}
}