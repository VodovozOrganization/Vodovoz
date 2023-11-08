using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public interface IOperatorRepository
	{
		OperatorState GetOperatorState(int operatorId);
	}
}
