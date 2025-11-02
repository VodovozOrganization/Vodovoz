using Mango.Core.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pacs.MangoCalls.Services
{
	public interface ICallEventRegistrar
	{
		Task RegisterCallEvents(IEnumerable<MangoCallEvent> callEvent);
		Task RegisterSummaryEvent(MangoSummaryEvent summaryEvent);
	}
}
