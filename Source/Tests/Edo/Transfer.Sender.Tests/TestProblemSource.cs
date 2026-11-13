using Edo.Common;
using Edo.Problems;
using Edo.Problems.Exception;
using Edo.Transfer;
using Edo.Transfer.Sender;
using MassTransit;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Transfer.Sender.Tests
{
	internal class TestProblemSource : EdoTaskProblemExceptionSource
	{
		public override string Name => "TestProblem";
		public override string Description => "Тестовая проблема";
		public override string Recommendation => "Нет";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
