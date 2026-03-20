using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources.Withdrawal
{
	public class WithdrawalCanNotBeCreatedForRegisteredInTrueMarkClient : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.WithdrawalCanNotBeCreatedForRegisteredInTrueMarkClient";
		public override string Message => "Документ вывода кодов из оборота не может быть создан для клиента, зарегистрированного в ЧЗ";
		public override string Description => "Проверяет статус клиента в ЧЗ перед выводом из оборота";
		public override string Recommendation => "Проверьте статус клиента в ЧЗ, а также таймаут завершения ДО по заказу";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
