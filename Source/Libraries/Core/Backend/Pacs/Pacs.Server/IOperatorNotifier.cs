using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	//public interface ISessionRepository
	//{
	//	Task SaveOperatorSession(OperatorSession operatorSession);
	//	Task<OperatorSession> LoadOperatorSession(Guid sessionId);
	//}

	public interface IOperatorNotifier
	{
		Task OperatorChanged(OperatorState operatorState);
	}
}
