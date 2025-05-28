using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using System;
using Vodovoz.Core.Domain.Warehouses.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class WarehouseDocumentsJournalRegistrar : ColumnsConfigRegistrarBase<WarehouseDocumentsJournalViewModel, WarehouseDocumentsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<WarehouseDocumentsJournalNode> config) => config
			.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
			.AddColumn("Тип документа").AddTextRenderer(node => node.DocTypeString)
			.AddColumn("Дата").AddTextRenderer(node => node.DateString)
			.AddColumn("Автор").AddTextRenderer(node => node.Author)
			.AddColumn("Изменил").AddTextRenderer(node => node.LastEditor)
			.AddColumn("Послед. изменения").AddTextRenderer(node =>
				node.LastEditedTime != default ? node.LastEditedTime.ToString() : string.Empty)
			.AddColumn("Детали").AddTextRenderer(node => node.Description).SearchHighlight()
			.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
			.RowCells()
			.AddSetter<CellRenderer>((cell, node) =>
			{
				Color color = GdkColors.PrimaryBase;
				if(node.DocTypeEnum == DocumentType.MovementDocument)
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
