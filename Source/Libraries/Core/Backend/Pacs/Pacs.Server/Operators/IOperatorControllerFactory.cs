using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public interface IOperatorControllerFactory
	{
		OperatorController CreateOperatorController(Operator @operator);
	}
}
