using Gamma.ColumnConfig;
using System;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DriverTareMessagesJournalRegistrar : ColumnsConfigRegistrarBase<DriverTareMessagesJournalViewModel, DriverMessageJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DriverMessageJournalNode> config) =>
			config.AddColumn("Дата").AddTextRenderer(node => node.CommentDate.ToString("dd.MM.yy"))
				.AddColumn("Время").AddTextRenderer(node => node.CommentDate.ToString("HH:mm:ss"))
				.AddColumn("ФИО водителя").AddTextRenderer(node => node.DriverName)
				.AddColumn("Телефон водителя").AddTextRenderer(node => node.DriverPhone)
				.AddColumn("№ МЛ").AddNumericRenderer(node => node.RouteListId)
				.AddColumn("№ заказа").AddNumericRenderer(node => node.OrderId)
				.AddColumn("План бут.").AddNumericRenderer(node => node.BottlesReturn)
				.AddColumn("Факт бут.").AddNumericRenderer(node => node.ActualBottlesReturn)
				.AddColumn("Долг бут. по адресу").AddNumericRenderer(node => node.AddressBottlesDebt)
				.AddColumn("Комментарий водителя").AddTextRenderer(node => node.DriverComment).WrapMode(WrapMode.WordChar).WrapWidth(500)
				.AddColumn("Комментарий ОП/ОСК").AddTextRenderer(node => node.OPComment != null ? node.OPComment : string.Empty).WrapMode(WrapMode.WordChar).WrapWidth(500)
				.AddColumn("Автор комментария ОП/ОСК").AddTextRenderer(node =>
					node.CommentOPManagerChangedBy != null ? node.CommentOPManagerChangedBy : string.Empty)
				.AddColumn("Время комментария").AddTextRenderer(node =>
					node.CommentOPManagerUpdatedAt != null && node.CommentOPManagerUpdatedAt != DateTime.MinValue
					? node.CommentOPManagerUpdatedAt.ToString("HH:mm dd.MM.yy") : string.Empty)
				.AddColumn("Время реакции").AddTextRenderer(node => node.ResponseTime)
				.Finish();
	}
}
