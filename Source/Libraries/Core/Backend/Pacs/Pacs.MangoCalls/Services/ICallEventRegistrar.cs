using Mango.Core.Dto;
using System.Threading.Tasks;

namespace Pacs.MangoCalls.Services
{
	public interface ICallEventRegistrar
	{
		Task RegisterCallEvent(MangoCallEvent callEvent);
	}
}
