using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClosedXML.Report;
using Gamma.Utilities;
using QS.Project.Services.FileDialog;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.ViewModels.Reports.UndeliveryJournalReport
{
	public class UndeliveredOrdersClassificationSummaryReport
	{
		private string _templatePath;
		private readonly IFileDialogService _fileDialogService;
		private readonly Report _report;

		public UndeliveredOrdersClassificationSummaryReport(IList<UndeliveredOrderJournalNode> journalNodes,
			UndeliveredOrdersFilterViewModel undeliveredOrderFilterViewModel, IFileDialogService fileDialogService, bool withTransfers)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			if(undeliveredOrderFilterViewModel == null)
			{
				throw new ArgumentNullException(nameof(undeliveredOrderFilterViewModel));
			}

			_templatePath = withTransfers
				? @".\Reports\Orders\UndeliveredOrdersWithTransfersClassificationSummaryReport.xlsx"
				: @".\Reports\Orders\UndeliveredOrdersClassificationSummaryReport.xlsx";

			var title = GenerateTitle(undeliveredOrderFilterViewModel);

			_report = new Report(journalNodes, title, withTransfers);
		}

		private string GenerateTitle(UndeliveredOrdersFilterViewModel filter)
		{
			var generateDate = $"Время выгрузки: {DateTime.Now}";
			StringBuilder title = new StringBuilder(generateDate);

			if(filter.RestrictOldOrder != null)
			{
				title.Append($", заказ: {filter.RestrictOldOrder.Id}");
			}

			if(filter.RestrictDriver != null)
			{
				title.Append($", водитель: {filter.RestrictDriver.GetPersonNameWithInitials()}");
			}

			if(filter.RestrictClient != null)
			{
				title.Append($", клиент: {filter.RestrictClient.Name}");
			}

			if(filter.RestrictAddress != null)
			{
				title.Append($", адрес: {filter.RestrictAddress.ShortAddress}");
			}

			if(filter.RestrictOldOrderAuthor != null)
			{
				title.Append($", автор заказа: {filter.RestrictOldOrderAuthor.GetPersonNameWithInitials()}");
			}

			if(filter.RestrictOldOrderStartDate != null)
			{
				title.Append($", дата заказа от: {filter.RestrictOldOrderStartDate:d}");
			}

			if(filter.RestrictOldOrderEndDate != null)
			{
				title.Append($", дата заказа до: {filter.RestrictOldOrderEndDate:d}");
			}

			if(filter.RestrictNewOrderStartDate != null)
			{
				title.Append($", дата переноса от: {filter.RestrictNewOrderStartDate:d}");
			}

			if(filter.RestrictNewOrderEndDate != null)
			{
				title.Append($", дата переноса до: {filter.RestrictNewOrderEndDate:d}");
			}

			if(filter.RestrictActionsWithInvoice != null)
			{
				title.Append($", действие с накладной: {filter.RestrictActionsWithInvoice.GetEnumTitle()}");
			}

			if(filter.RestrictAuthorSubdivision != null)
			{
				title.Append($", подразделение автора: {filter.RestrictAuthorSubdivision.ShortName}");
			}

			if(filter.OldOrderStatus != null)
			{
				title.Append($", начальный статус заказа: {filter.OldOrderStatus.GetEnumTitle()}");
			}

			if(filter.RestrictUndeliveryStatus != null)
			{
				title.Append($", статус недовоза: {filter.RestrictUndeliveryStatus.GetEnumTitle()}");
			}

			if(filter.RestrictGuiltySide != null && filter.RestrictGuiltyDepartment == null)
			{
				title.Append($", ответственный: {filter.RestrictGuiltySide.GetEnumTitle()}");
			}

			if(filter.RestrictGuiltyDepartment != null)
			{
				title.Append($", ответственное подразделение: {filter.RestrictGuiltyDepartment.ShortName}");
			}

			if(filter.RestrictUndeliveryAuthor != null)
			{
				title.Append($", автор недовоза: {filter.RestrictUndeliveryAuthor.GetPersonNameWithInitials()}");
			}

			if(filter.RestrictInProcessAtDepartment != null)
			{
				title.Append($", в работе у: {filter.RestrictInProcessAtDepartment.ShortName}");
			}

			if(filter.RestrictIsProblematicCases)
			{
				title.Append(", только проблемные случаи: да");
			}

			return title.ToString();
		}

		public void Export()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{"Сводка по классификации недовозов"} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				SaveReport(result.Path);
			}
		}

		private void SaveReport(string path)
		{
			var template = new XLTemplate(_templatePath);
			template.AddVariable(_report);
			template.Generate();
			template.SaveAs(path);
		}

		private class Report
		{
			public Report(IList<UndeliveredOrderJournalNode> journalNodes, string title, bool withTransfers)
			{
				Rows = GenerateRows(journalNodes, withTransfers);
				Title = title;
			}

			private IEnumerable<ReportNode> GenerateRows(IList<UndeliveredOrderJournalNode> journalNodes, bool withTransfers)
			{
				var reportNodes = new List<ReportNode>();

				foreach(var journalNode in journalNodes)
				{
					var guiltyItems = journalNode.Guilty.Split('\n');

					var transfer = withTransfers ? GetTransfer(journalNode) : null;

					foreach(var item in guiltyItems)
					{
						reportNodes.Add(new ReportNode
						{
							Guilty = item,
							UndeliveryObject = journalNode.UndeliveryObject,
							UndeliveryKind = journalNode.UndeliveryKind,
							UndeliveryDetalization = journalNode.UndeliveryDetalization,
							Transfer = transfer
						});
					}
				}

				var reportRows = reportNodes
					.GroupBy(rn => new { rn.Guilty, rn.Transfer, rn.UndeliveryObject, rn.UndeliveryKind, rn.UndeliveryDetalization })
					.Select(gr =>
						new ReportNode
						{
							Guilty = gr.Key.Guilty,
							Transfer = gr.Key.Transfer,
							UndeliveryObject = gr.Key.UndeliveryObject,
							UndeliveryKind = gr.Key.UndeliveryKind,
							UndeliveryDetalization = gr.Key.UndeliveryDetalization,
							Amount = gr.Count()
						})
					.OrderByDescending(rn => rn.Amount)
					.ThenBy(rn => rn.Guilty);

				return reportRows;
			}

			private string GetTransfer(UndeliveredOrderJournalNode node)
			{
				if(node.NewOrderId == 0)
				{
					return "Нет";
				}
				if(node.OrderTransferType == TransferType.AutoTransferNotApproved)
				{
					return "Не согласован";
				}

				return "Да";
			}

			public class ReportNode
			{
				public string UndeliveryObject { get; set; }
				public string UndeliveryKind { get; set; }
				public string UndeliveryDetalization { get; set; }
				public int Amount { get; set; }
				public string Guilty { get; set; }
				public string Transfer { get; set; }
			}

			public IEnumerable<ReportNode> Rows { get; }
			public string Title { get; }
		}
	}
}
