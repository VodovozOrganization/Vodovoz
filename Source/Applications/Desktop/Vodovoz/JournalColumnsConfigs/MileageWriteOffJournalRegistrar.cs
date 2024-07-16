using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Logistic.MileagesWriteOff;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class MileageWriteOffJournalRegistrar : ColumnsConfigRegistrarBase<MileageWriteOffJournalViewModel, MileageWriteOffJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<MileageWriteOffJournalNode> config) =>
			config.AddColumn("Id").AddNumericRenderer(node => node.Id)
				.AddColumn("Дата списания").AddTextRenderer(node => node.WriteOffDate.ToString("dd.MM.yyyy"))
				.AddColumn("Списано, км").AddTextRenderer(node => node.DistanceKm.ToString("F2")).WrapWidth(150).WrapMode(WrapMode.WordChar)
				.AddColumn("Автомобиль").AddTextRenderer(node => node.CarRegNumber)
				.AddColumn("Водитель").AddTextRenderer(node => node.DriverFullName).WrapWidth(250).WrapMode(WrapMode.WordChar)
				.AddColumn("Автор").AddTextRenderer(node => node.AuthorFullName).WrapWidth(250).WrapMode(WrapMode.WordChar)
				.AddColumn("Дата создания").AddTextRenderer(node => node.CreateDate.ToString("dd.MM.yyyy HH:mm:ss")).WrapWidth(150).WrapMode(WrapMode.WordChar)
				.Finish();
	}
}
