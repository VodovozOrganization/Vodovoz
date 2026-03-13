using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources.Withdrawal
{
	public class WithdrawalCanBeCreatedOnlyForLegalPersons : EdoTaskProblemCustomSource
	{
		public override string Name => "Custom.WithdrawalCanBeCreatedOnlyForLegalPersons";
		public override string Message => "Документ вывода кодов из оборота может быть создан только для юридических лиц";
		public override string Description => "Проверяет форму клиента перед выводом кодов из оборота";
		public override string Recommendation => "Проверьте форму контрагента в заказе";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
