using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class WarehouseDocumentsItemsJournalRegistrar : ColumnsConfigRegistrarBase<WarehouseDocumentsItemsJournalViewModel, WarehouseDocumentsItemsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<WarehouseDocumentsItemsJournalNode> config) =>
			config
				.AddColumn("Номер строки").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
				.AddColumn("Номер документа").AddTextRenderer(node => node.DocumentId.ToString()).SearchHighlight()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.NomenclatureName).SearchHighlight()
				.AddColumn("Кол-во").AddTextRenderer(node => node.Amount.ToString("0.00")).SearchHighlight()
				.AddColumn("Тип документа").AddTextRenderer(node => node.DocumentTypeString)
				.AddColumn("Дата").AddTextRenderer(node => node.DateString)
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.AddColumn("Изменил").AddTextRenderer(node => node.LastEditor)
				.AddColumn("Послед. изменения").AddTextRenderer(node =>
					node.LastEditedTime != default ? node.LastEditedTime.ToString() : string.Empty)
				.AddColumn("Детали").AddTextRenderer(node => node.Description).SearchHighlight()
				.AddColumn("Источник").AddTextRenderer(node => node.Source)
				.AddColumn("Получатель").AddTextRenderer(node => node.Target)
				.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) =>
				{
					var color = GdkColors.WhiteColor;
					if(node.DocumentTypeEnum == DocumentType.MovementDocument)
					{
						switch(node.MovementDocumentStatus)
						{
							case MovementDocumentStatus.Sended:
								color = new Color(255, 255, 125);
								break;
							case MovementDocumentStatus.Discrepancy:
								color = new Color(255, 125, 125);
								break;
							case MovementDocumentStatus.Accepted:
								color = node.MovementDocumentDiscrepancy ? new Color(125, 125, 255) : color;
								break;
						}
					}
					cell.CellBackgroundGdk = color;
				})
				.Finish();
	}
}
