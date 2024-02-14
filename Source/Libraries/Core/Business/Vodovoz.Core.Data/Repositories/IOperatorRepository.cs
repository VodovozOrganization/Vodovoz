using System.Collections.Generic;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IOperatorRepository
	{
		OperatorState GetOperatorState(int operatorId);
		IEnumerable<OperatorState> GetOperatorHistory(int operatorId);
		Operator GetOperator(int operatorId);
	}
}
