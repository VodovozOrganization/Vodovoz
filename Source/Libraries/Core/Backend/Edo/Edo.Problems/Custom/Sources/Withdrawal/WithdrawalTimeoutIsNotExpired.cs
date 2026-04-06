using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources.Withdrawal
{
	public class WithdrawalTimeoutIsNotExpired : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.WithdrawalTimeoutIsNotExpired";
		public override string Message => "Таймаут вывода из оборота не истек";
		public override string Description => "Проверяет таймаут вывода из оборота";
		public override string Recommendation => "Проверьте дату документа ЭДО и настройку таймаута";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
