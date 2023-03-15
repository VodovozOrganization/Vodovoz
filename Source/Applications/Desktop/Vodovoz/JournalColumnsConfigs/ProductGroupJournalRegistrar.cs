﻿using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ProductGroupJournalRegistrar : ColumnsConfigRegistrarBase<ProductGroupJournalViewModel, ProductGroupJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ProductGroupJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.IsArchive ? "grey" : "black") // Проверить!!!
				.Finish();
	}
}
