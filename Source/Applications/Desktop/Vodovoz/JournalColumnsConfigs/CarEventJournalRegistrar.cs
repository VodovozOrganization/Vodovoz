using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CarEventJournalRegistrar : ColumnsConfigRegistrarBase<CarEventJournalViewModel, CarEventJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CarEventJournalNode> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Дата и время создания").AddTextRenderer(node => node.CreateDate.ToString("g"))
				.AddColumn("Событие").AddTextRenderer(node => node.CarEventTypeName).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Порядковый\nномер ТС").AddNumericRenderer(node => node.CarOrderNumber)
				.AddColumn("Гос.номер ТС").AddTextRenderer(node => node.CarRegistrationNumber)
				.AddColumn("Тип авто").AddTextRenderer(node => node.CarTypeOfUseAndOwnTypeString)
				.AddColumn("Часть города").AddTextRenderer(node => node.GeographicGroups)
				.AddColumn("Водитель").AddTextRenderer(node => node.DriverFullName)
				.AddColumn("Дата начала").AddTextRenderer(node => node.StartDate.ToString("d"))
				.AddColumn("Дата окончания").AddTextRenderer(node => node.EndDate.ToString("d"))
				.AddColumn("Стоимость").AddTextRenderer(node => node.RepairAndPartsSummaryCost.ToString("0.##"))
				.AddColumn("Комментарий").AddTextRenderer(node => node.Comment).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Автор").AddTextRenderer(node => node.AuthorFullName)
				.Finish();
	}
}
