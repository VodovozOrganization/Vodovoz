using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public interface IOperatorControllerFactory
	{
		OperatorController CreateOperatorController(Operator @operator);
	}
}
