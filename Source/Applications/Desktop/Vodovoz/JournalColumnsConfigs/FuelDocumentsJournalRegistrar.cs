using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.Nodes.Cash;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FuelDocumentsJournalRegistrar : ColumnsConfigRegistrarBase<FuelDocumentsJournalViewModel, FuelDocumentJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FuelDocumentJournalNode> config) =>
			config.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Тип").AddTextRenderer(node => node.Title)
				.AddColumn("Дата").AddTextRenderer(node => node.CreationDate.ToShortDateString())
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.AddColumn("Сотрудник").AddTextRenderer(node => node.Employee)
				.AddColumn("Статус").AddTextRenderer(node => node.Status)
				.AddColumn("Литры").AddTextRenderer(node => node.Liters.ToString("0"))
				.AddColumn("Статья расх.").AddTextRenderer(node => node.ExpenseCategory)
				.AddColumn("Отправлено из").AddTextRenderer(node => node.SubdivisionFrom)
				.AddColumn("Время отпр.").AddTextRenderer(node => node.SendTime.HasValue ? node.SendTime.Value.ToShortDateString() : "")
				.AddColumn("Отправлено в").AddTextRenderer(node => node.SubdivisionTo)
				.AddColumn("Время принятия").AddTextRenderer(node => node.ReceiveTime.HasValue ? node.ReceiveTime.Value.ToShortDateString() : "")
				.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
				.Finish();
	}
}
