using Gamma.ColumnConfig;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CarEventTypeJournalRegistrar : ColumnsConfigRegistrarBase<CarEventTypeJournalViewModel, CarEventTypeJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CarEventTypeJournalNode> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Сокращённое\nназвание").AddTextRenderer(node => node.ShortName).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Комментарий\nобязателен")
					.AddToggleRenderer(node => node.NeedComment)
					.Editing(false)
				.AddColumn("В архиве")
				.AddToggleRenderer(node => node.IsArchive)
					.Editing(false)
					.XAlign(0f)
				.AddColumn("Не отображать в эксплуатации ТС")
				.AddToggleRenderer(node => node.IsDoNotShowInOperation)
					.Editing(false)
					.XAlign(0f)
				.AddColumn("Прикреплять акт списания")
				.AddToggleRenderer(node => node.IsAttachWriteOffDocument)
					.Editing(false)
					.XAlign(0f)
				.AddColumn("Зона\nответственности")
					.AddTextRenderer(node => node.AreaOfResponsibilityValue)
				.Finish();
	}
}
