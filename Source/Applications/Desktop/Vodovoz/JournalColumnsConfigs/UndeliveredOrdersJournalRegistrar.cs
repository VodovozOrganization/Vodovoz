using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class UndeliveredOrdersJournalRegistrar : ColumnsConfigRegistrarBase<UndeliveredOrdersJournalViewModel, UndeliveredOrderJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<UndeliveredOrderJournalNode> config) =>
			config.AddColumn("№").HeaderAlignment(0.5f).AddNumericRenderer(node => node.NumberInList)
				.AddColumn("Код").HeaderAlignment(0.5f).AddTextRenderer(node => node.Id != 0 ? node.Id.ToString() : "")
				.AddColumn("Статус").HeaderAlignment(0.5f).AddTextRenderer(node => node.Status)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Дата\nзаказа").HeaderAlignment(0.5f).AddTextRenderer(node => node.OldOrderDeliveryDate)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Автор\nзаказа").HeaderAlignment(0.5f).AddTextRenderer(node => node.OldOrderAuthor)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Клиент и адрес").HeaderAlignment(0.5f).AddTextRenderer(node => node.ClientAndAddress)
					.WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("Интервал\nдоставки").HeaderAlignment(0.5f).AddTextRenderer(node => node.OldDeliverySchedule)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Количество\nбутылей").HeaderAlignment(0.5f).AddTextRenderer(node => node.UndeliveredOrderItems)
					.WrapWidth(75).WrapMode(WrapMode.WordChar)
				.AddColumn("Статус\nначальный ➔\n ➔ текущий").HeaderAlignment(0.5f).AddTextRenderer(node => node.OldOrderStatus)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Ответственный").HeaderAlignment(0.5f).AddTextRenderer(node => node.Guilty)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Причина").HeaderAlignment(0.5f).AddTextRenderer(node => node.Reason)
					.WrapWidth(200).WrapMode(WrapMode.WordChar)
				.AddColumn("Объект\nнедовоза").HeaderAlignment(0.5f).AddTextRenderer(node => node.UndeliveryObject)
					.WrapWidth(200).WrapMode(WrapMode.WordChar)
				.AddColumn("Вид\nнедовоза").HeaderAlignment(0.5f).AddTextRenderer(node => node.UndeliveryKind)
					.WrapWidth(200).WrapMode(WrapMode.WordChar)
				.AddColumn("Детализация\nнедовоза").HeaderAlignment(0.5f).AddTextRenderer(node => node.UndeliveryDetalization)
					.WrapWidth(200).WrapMode(WrapMode.WordChar)
				.AddColumn("Звонок\nв офис").HeaderAlignment(0.5f).AddTextRenderer(node => node.DriversCall)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Звонок\nклиенту").HeaderAlignment(0.5f).AddTextRenderer(node => node.DispatcherCall)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Водитель").HeaderAlignment(0.5f).AddTextRenderer(node => node.DriverName)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Перенос").HeaderAlignment(0.5f).AddTextRenderer(node => node.TransferDateTime)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Кто недовоз\nзафиксировал").HeaderAlignment(0.5f).AddTextRenderer(node => node.Registrator)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Автор\nнедовоза").HeaderAlignment(0.5f).AddTextRenderer(node => node.UndeliveryAuthor)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Оштрафованные").HeaderAlignment(0.5f).AddTextRenderer(node => node.FinedPeople)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("В работе\nу отдела").HeaderAlignment(0.5f).AddTextRenderer(node => node.InProcessAt)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Отработатно\nОКС").HeaderAlignment(0.5f).AddTextRenderer(node => node.LastResultCommentAuthor)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.Finish();
	}
}
