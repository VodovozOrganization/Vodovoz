﻿using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CarJournalRegistrar : ColumnsConfigRegistrarBase<CarJournalViewModel, CarJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CarJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Производитель").AddTextRenderer(x => x.ManufacturerName).WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("Модель").AddTextRenderer(x => x.ModelName).WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("Гос. номер").AddTextRenderer(x => x.RegistrationNumber)
				.AddColumn("Водитель").AddTextRenderer(x => x.DriverName)
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
			        .AddSetter<CellRenderer>((c, n) => c.CellBackgroundGdk = n.IsUpcomingTechInspect || n.IsCarInsuranceExpires ? GdkColors.Pink : GdkColors.PrimaryBase)
				.Finish();
	}
}
