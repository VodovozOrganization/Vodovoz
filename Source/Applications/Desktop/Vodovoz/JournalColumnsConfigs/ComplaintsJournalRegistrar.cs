using Gamma.ColumnConfig;
using Gtk;
using System;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ComplaintsJournalRegistrar : ColumnsConfigRegistrarBase<ComplaintsJournalViewModel, ComplaintJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ComplaintJournalNode> config) =>
			config.AddColumn("№ п/п").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.SequenceNumber.ToString())
					.XAlign(0.5f)
				.AddColumn("№ рекламации").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Тип").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.TypeString)
					.XAlign(0.5f)
				.AddColumn("Дата").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DateString)
					.XAlign(0.5f)
				.AddColumn("Время").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.TimeString)
					.XAlign(0.5f)
				.AddColumn("Статус").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.StatusString)
					.XAlign(0.5f)
				.AddColumn("В работе у").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.WorkInSubdivision)
					.XAlign(0f)
				.AddColumn("Дата план.\nзавершения").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.PlannedCompletionDate)
					.XAlign(0.5f)
				.AddColumn("Клиент и адрес").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ClientNameWithAddress)
					.WrapWidth(300).WrapMode(WrapMode.WordChar)
					.XAlign(0f)
				.AddColumn("Ответственный").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Guilties)
					.XAlign(0f)
				.AddColumn("Водитель").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Driver)
					.XAlign(0f)
				.AddColumn("Проблема").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ComplaintText)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
					.XAlign(0f)
				.AddColumn("Объект рекламации").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ComplaintObjectString)
					.XAlign(0.5f)
				.AddColumn("Вид рекламации").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ComplaintKindString)
					.XAlign(0.5f)
				.AddColumn("Детализация").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ComplaintDetalizationString)
					.XAlign(0.5f)
				.AddColumn("Автор").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Author)
					.XAlign(0f)
				.AddColumn("Штрафы").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Fines)
					.XAlign(0.5f)
				.AddColumn("Результат").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ResultText)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
					.XAlign(0f)
				.AddColumn("Дата факт.\nзавершения").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ActualCompletionDateString)
					.XAlign(0.5f)
				.AddColumn("Дни").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DaysInWork)
					.XAlign(0.5f)
				.AddColumn("Мероприятия").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ArrangementText)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
					.XAlign(0f)
				.AddColumn("Результат по клиенту").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ResultOfCounterparty)
					.XAlign(0f)
				.AddColumn("Результат по сотруднику").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ResultOfEmployees)
					.XAlign(0f)
				.RowCells()
				.AddSetter<CellRenderer>(
					(cell, node) =>
					{
						var color = GdkColors.PrimaryBase;

						if(node.Status == ComplaintStatuses.NotTakenInProcess)
						{
							color = GdkColors.Pink;
						}

						if(node.IsNeedWork)
						{
							color = GdkColors.YellowMustard;
						}

						cell.CellBackgroundGdk = color;
					})
				.Finish();
	}
}
