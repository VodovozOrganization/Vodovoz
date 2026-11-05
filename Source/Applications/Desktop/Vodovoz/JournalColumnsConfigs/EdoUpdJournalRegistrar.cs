using Core.Infrastructure;
using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.Journals.JournalViewModels.Edo;

namespace Vodovoz.JournalColumnsConfigs
{
	/// <summary>
	/// Колонки журнала УПД.
	/// </summary>
	public class EdoUpdJournalRegistrar : ColumnsConfigRegistrarBase<EdoUpdJournalViewModel, EdoUpdJournalNode>
	{
		/// <summary>Настраивает колонки сохранённых УПД.</summary>
		/// <param name="config">Конфигурация колонок.</param>
		/// <returns>Конфигурация таблицы журнала.</returns>
		public override IColumnsConfig Configure(FluentColumnsConfig<EdoUpdJournalNode> config) => config
			.AddColumn("Номер УПД").AddTextRenderer(x => x.Number)
			.AddColumn("Дата УПД").AddTextRenderer(x => x.Date.ToString("dd.MM.yyyy"))
			.AddColumn("Тип").AddTextRenderer(x => x.DocumentType.GetEnumDisplayName(false))
			.AddColumn("Продавец").AddTextRenderer(x => x.Seller)
			.AddColumn("Покупатель").AddTextRenderer(x => x.Customer)
			.AddColumn("ИНН покупателя").AddTextRenderer(x => x.CustomerInn)
			.AddColumn("Сумма").AddNumericRenderer(x => x.Sum)
			.AddColumn("Задача ЭДО").AddNumericRenderer(x => x.TaskId)
			.AddColumn("").Finish();
	}
}
