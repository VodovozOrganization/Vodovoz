﻿using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Gtk;
using Vodovoz.Domain.Logistic;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FinancialDistrictsSetsJournalRegistrar : ColumnsConfigRegistrarBase<FinancialDistrictsSetsJournalViewModel, FinancialDistrictsSetsJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGray = new Color(0x80, 0x80, 0x80);

		public override IColumnsConfig Configure(FluentColumnsConfig<FinancialDistrictsSetsJournalNode> config) =>
			config.AddColumn("Код")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Статус")
					.AddTextRenderer(node => node.Status.GetEnumTitle())
				.AddColumn("Автор")
					.AddTextRenderer(node => node.Author)
				.AddColumn("Дата создания")
					.AddTextRenderer(node => node.DateCreated.Date.ToString("d"))
					.XAlign(0.5f)
				.AddColumn("Дата активации")
					.AddTextRenderer(node => node.DateActivated != null ? node.DateActivated.Value.Date.ToString("d") : "-")
					.XAlign(0.5f)
				.AddColumn("Дата закрытия")
					.AddTextRenderer(node => node.DateClosed != null ? node.DateClosed.Value.Date.ToString("d") : "-")
					.XAlign(0.5f)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
						c.ForegroundGdk = n.Status == DistrictsSetStatus.Closed ? _colorDarkGray : _colorBlack)
				.Finish();
	}
}
