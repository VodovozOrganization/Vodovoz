using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.Core.Domain.Warehouses.Documents;
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
				.AddColumn("Кол-во").AddTextRenderer(node => node.Amount.ToString("0.#####")).SearchHighlight()
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
				.AddColumn("Штраф").AddTextRenderer(node => node.FinesDescription).WrapWidth(300)
				.AddColumn("Тип брака").AddTextRenderer(node => node.TypeOfDefect)
				.AddColumn("Источник брака").AddTextRenderer(node => node.DefectSourceString)
				.AddColumn("Причина пересортицы").AddTextRenderer(node => node.RegradingOfGoodsReason)
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) =>
				{
					var color = GdkColors.PrimaryBase;
					if(node.DocumentTypeEnum == DocumentType.MovementDocument)
					{
						switch(node.MovementDocumentStatus)
						{
							case MovementDocumentStatus.Sended:
								color = GdkColors.WarningBase;
								break;
							case MovementDocumentStatus.Discrepancy:
								color = GdkColors.Pink;
								break;
							case MovementDocumentStatus.Accepted:
								color = node.MovementDocumentDiscrepancy ? GdkColors.BabyBlue : color;
								break;
						}
					}
					cell.CellBackgroundGdk = color;
				})
				.Finish();
	}
}
