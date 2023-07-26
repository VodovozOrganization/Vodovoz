using Gamma.ColumnConfig;
using Gamma.Utilities;
using QSProjectsLib;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PayoutRequestsJournalRegistrar : ColumnsConfigRegistrarBase<PayoutRequestsJournalViewModel, PayoutRequestJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<PayoutRequestJournalNode> config) =>
			config.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Дата создания")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Date.ToShortDateString())
					.XAlign(0.5f)
				.AddColumn("Тип документа")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.PayoutRequestDocumentType.GetEnumTitle())
					.WrapWidth(155)
					.WrapMode(WrapMode.WordChar)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(n => n.PayoutRequestState)
					.XAlign(0.5f)
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Author)
					.XAlign(0.5f)
				.AddColumn("Подотчетное лицо /\r\n\tПоставщик")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => !string.IsNullOrWhiteSpace(n.AccountablePerson) ? n.AccountablePerson : n.CounterpartyName)
					.XAlign(0.5f)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => CurrencyWorks.GetShortCurrencyString(n.Sum))
					.XAlign(0.5f)
				.AddColumn("Основание")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Basis)
					.WrapWidth(1000)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Статья")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.ExpenseCategory)
					.WrapWidth(300)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Наличие чека")
				.AddToggleRenderer(n => n.HaveReceipt)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();
	}
}
