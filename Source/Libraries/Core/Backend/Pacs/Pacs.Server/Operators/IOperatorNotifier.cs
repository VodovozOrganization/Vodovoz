using Pacs.Core.Messages.Events;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public interface IOperatorNotifier
	{
		Task OperatorChanged(OperatorState operatorState, OperatorBreakAvailability breakAvailability);
	}
}
