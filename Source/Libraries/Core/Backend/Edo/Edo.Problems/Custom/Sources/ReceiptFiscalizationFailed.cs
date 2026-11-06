using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom.Sources
{
	/// <summary>
	/// Ошибка фискализации чека в МодульКассе (MarkingCheckViolated, таймаут 24 часа и др.)
	/// </summary>
	public class ReceiptFiscalizationFailed : EdoTaskProblemCustomSource
	{
		/// <inheritdoc/>
		public override string Name => "Custom.ReceiptFiscalizationFailed";

		/// <inheritdoc/>
		public override string Message => "Ошибка фискализации чека в МодульКассе";

		/// <inheritdoc/>
		public override string Description => "Возникает, когда касса вернула статус FAILED (нарушение разрешительного режима КМ, превышение ожидания фискализации 24ч или ошибка ФН)";

		/// <inheritdoc/>
		public override string Recommendation => "Проверить коды маркировки чека, статус смены и ФН в МодульКассе";

		/// <inheritdoc/>
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
