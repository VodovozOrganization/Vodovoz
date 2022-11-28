using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using ClosedXML.Report;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport
{
	public class EdoUpdReportViewModel : DialogTabViewModelBase
	{
		private const string _templatePath = @".\Reports\Orders\EdoUpdReport.xlsx";
		private readonly IFileDialogService _fileDialogService;
		private DelegateCommand _generateCommand;
		private DelegateCommand _exportCommand;

		public EdoUpdReportViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService,
			INavigationManager navigation, IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			Title = "Отчёт об УПД, не отражённых в ЧЗ";

			DateFrom = DateTime.Now.Date;
			DateTo = DateTime.Now.Date.Add(new TimeSpan(0, 23, 59, 59));
		}

		private IList<EdoUpdReportRow> GenerateReportRows()
		{
			if(!HasDates)
			{
				return new List<EdoUpdReportRow>();
			}

			Domain.Client.Counterparty counterpartyAlias = null;
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			TrueMarkApiDocument trueMarkApiDocumentAlias = null;
			EdoContainer edoContainerAlias = null;
			EdoUpdReportRow resultAlias = null;

			var orderStatuses = new[] { OrderStatus.OnTheWay, OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };
			var edoDocFlowStatuses = new[] { EdoDocFlowStatus.Succeed, EdoDocFlowStatus.CompletedWithDivergences };

			var query = UoW.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinEntityAlias(() => orderItemAlias, () => orderAlias.Id == orderItemAlias.Order.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinEntityAlias(() => trueMarkApiDocumentAlias, () => orderAlias.Id == trueMarkApiDocumentAlias.Order.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => edoContainerAlias, () => orderAlias.Id == edoContainerAlias.Order.Id, JoinType.LeftOuterJoin);

			query
				.Where(() => orderAlias.DeliveryDate >= DateFrom && orderAlias.DeliveryDate <= DateTo)
				.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(orderStatuses)
				.And(() => counterpartyAlias.PersonType == PersonType.legal)
				.And(() => nomenclatureAlias.IsAccountableInChestniyZnak)
				.And(() => nomenclatureAlias.Gtin != null);


			query.Where(Restrictions.Disjunction()
				.Add(() => trueMarkApiDocumentAlias.IsSuccess)
				.Add(Restrictions.On(() => edoContainerAlias.EdoDocFlowStatus).IsIn(edoDocFlowStatuses))
			);

			var result = query
				.SelectList(list => list
					.Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.UpdDate)
					.Select(() => nomenclatureAlias.Gtin).WithAlias(() => resultAlias.Gtin)
					.Select(() => orderItemAlias.Price).WithAlias(() => resultAlias.Price)
					.Select(() => orderItemAlias.Count).WithAlias(() => resultAlias.Count)
					.Select(() => edoContainerAlias.EdoDocFlowStatus).WithAlias(() => resultAlias.EdoDocFlowStatus)
					.Select(() => trueMarkApiDocumentAlias.IsSuccess).WithAlias(() => resultAlias.IsTrueMarkApiSuccess)
					.Select(() => trueMarkApiDocumentAlias.ErrorMessage).WithAlias(() => resultAlias.TrueMarkApiError)
				)
				.TransformUsing(Transformers.AliasToBean<EdoUpdReportRow>())
				.List<EdoUpdReportRow>();

			return result;
		}

		//private string GenerateSelectedFiltersString()
		//{
		//	var selectedFilters = new StringBuilder().AppendLine("Выбранные фильтры:");

		//	if(DateFrom != null && DateTo != null)
		//	{
		//		selectedFilters.AppendLine(
		//			$"Время события: с {DateFrom.Value.ToShortDateString()} по {DateTo.Value.ToShortDateString()}; ");
		//	}

		//	if(Counterparty != null)
		//	{
		//		selectedFilters.AppendLine($"Контрагент: {Counterparty.Name}; ");
		//	}

		//	if(BulkEmailEventReason != null)
		//	{
		//		selectedFilters.AppendLine($"Причина: {BulkEmailEventReason.Name}; ");
		//	}

		//	return selectedFilters.ToString();
		//}

		#region Commands

		public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(
				() =>
				{
					var dialogSettings = new DialogSettings();
					dialogSettings.Title = "Сохранить";
					dialogSettings.DefaultFileExtention = ".xlsx";
					dialogSettings.FileName = $"Отчёт о событиях рассылки {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

					var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
					if(result.Successful)
					{
						var template = new XLTemplate(_templatePath);
						template.AddVariable(Report);
						template.Generate();
						template.SaveAs(result.Path);
					}
				},
				() => true)
			);

		public DelegateCommand GenerateCommand => _generateCommand ?? (_generateCommand = new DelegateCommand(
				() =>
				{
					Report = new EdoUpdReport
					{
						Rows = GenerateReportRows(),
						//SelectedFilters = GenerateSelectedFiltersString()
					};
				},
				() => true)
			);

		#endregion

		public EdoUpdReport Report { get; set; }
		public DateTime? DateFrom { get; set; }
		public DateTime? DateTo { get; set; }
		public bool HasDates => DateFrom != null && DateTo != null;
		public bool IsSuccess { get; set; }
		public bool IsNotSuccess { get; set; }
	}
}

