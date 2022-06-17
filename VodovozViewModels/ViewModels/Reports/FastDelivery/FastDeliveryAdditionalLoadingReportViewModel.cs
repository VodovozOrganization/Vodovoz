using ClosedXML.Report;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public class FastDeliveryAdditionalLoadingReportViewModel : DialogTabViewModelBase
	{
		private const string _templatePath = @".\Reports\Orders\FastDeliveryAdditionalLoadingReport.xlsx";
		private readonly IFileDialogService _fileDialogService;
		private DelegateCommand _generateCommand;
		private DelegateCommand _exportCommand;
		private FastDeliveryAdditionalLoadingReport _report;
		private bool _isRunning;
		public FastDeliveryAdditionalLoadingReportViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService,
			INavigationManager navigation, IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			Title = "Отчёт по дозагрузке МЛ";
		}

		private IList<FastDeliveryAdditionalLoadingReportRow> GenerateReportRows()
		{
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Order orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			FastDeliveryAdditionalLoadingReportRow resultAlias = null;
			AdditionalLoadingDocumentItem additionalLoadingDocumentItemAlias = null;
			AdditionalLoadingDocument additionalLoadingDocumentAlias = null;

			var itemsQuery = UoW.Session.QueryOver(() => additionalLoadingDocumentItemAlias)
				.JoinAlias(() => additionalLoadingDocumentItemAlias.AdditionalLoadingDocument, () => additionalLoadingDocumentAlias)
				.JoinEntityAlias(() => routeListAlias, () => routeListAlias.AdditionalLoadingDocument.Id == additionalLoadingDocumentAlias.Id)
				.JoinAlias(() => additionalLoadingDocumentItemAlias.Nomenclature, () => nomenclatureAlias);

			if(CreateDateFrom != null && CreateDateTo != null)
			{
				itemsQuery.Where(() => routeListAlias.Date >= CreateDateFrom.Value.Date.Add(new TimeSpan(0, 0, 0, 0))
									   && routeListAlias.Date <= CreateDateTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));
			}

			var ownOrdersAmountSubquery = QueryOver.Of(() => routeListItemAlias)
				.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				//.Where(() => (routeListItemAlias.Status != RouteListItemStatus.Canceled
				//              && routeListItemAlias.Status != RouteListItemStatus.Overdue
				//              && (!routeListItemAlias.WasTransfered || routeListItemAlias.NeedToReload))
				//             || (routeListItemAlias.Status == RouteListItemStatus.Transfered && !routeListItemAlias.NeedToReload))
				.Where(() => routeListItemAlias.Status != RouteListItemStatus.Canceled 
				             && routeListItemAlias.Status != RouteListItemStatus.Overdue 
				             && routeListItemAlias.Status != RouteListItemStatus.Transfered)
				.And(() => routeListItemAlias.RouteList.Id == routeListAlias.Id)
				.And(() => !orderAlias.IsFastDelivery)
				.Select(Projections.Count(Projections.Id()));

			return itemsQuery
				.SelectList(list => list
					.Select(() => routeListAlias.Date).WithAlias(() => resultAlias.RouteListDate)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
					.SelectSubQuery(ownOrdersAmountSubquery).WithAlias(() => resultAlias.OwnOrdersCount)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.AdditionaLoadingNomenclature)
					.Select(() => additionalLoadingDocumentItemAlias.Amount).WithAlias(() => resultAlias.AdditionaLoadingAmount)
				).OrderBy(() => routeListAlias.Date).Desc
				.ThenBy(() => routeListAlias.Id).Desc
				.TransformUsing(Transformers.AliasToBean<FastDeliveryAdditionalLoadingReportRow>())
				.List<FastDeliveryAdditionalLoadingReportRow>();
		}

		#region Commands

		public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(
				() =>
				{
					var dialogSettings = new DialogSettings();
					dialogSettings.Title = "Сохранить";
					dialogSettings.DefaultFileExtention = ".xlsx";
					dialogSettings.FileName = $"{this.GetType().Name} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

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
					Report = null;
					IsRunning = true;

					Report = new FastDeliveryAdditionalLoadingReport
					{
						Rows = GenerateReportRows()
					};

					IsRunning = false;
					UoW.Session.Clear();
				},
				() => true)
			);

		#endregion

		public DateTime? CreateDateFrom { get; set; }
		public DateTime? CreateDateTo { get; set; }

		[PropertyChangedAlso(nameof(IsHasRows))]
		public FastDeliveryAdditionalLoadingReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public string ProgressText
		{
			get
			{
				if(IsRunning)
				{
					return "Отчёт формируется...";
				}

				if(Report != null && !Report.Rows.Any())
				{
					return "По данному запросу отсутствуют записи";
				}

				if(IsHasRows)
				{
					return "Отчёт сформирован";
				}

				return "";
			}
		}

		[PropertyChangedAlso(nameof(ProgressText))]
		public bool IsRunning
		{
			get => _isRunning;
			set => SetField(ref _isRunning, value);
		}

		[PropertyChangedAlso(nameof(ProgressText))]
		public bool IsHasRows => Report != null && Report.Rows.Any();
	}

	public class FastDeliveryAdditionalLoadingReport
	{
		public IList<FastDeliveryAdditionalLoadingReportRow> Rows { get; set; }
	}

	public class FastDeliveryAdditionalLoadingReportRow
	{
		public DateTime RouteListDate { get; set; }
		public int RouteListId { get; set; }
		public int OwnOrdersCount { get; set; }
		public string AdditionaLoadingNomenclature { get; set; }
		public decimal AdditionaLoadingAmount { get; set; }
		public string RouteListDateString => RouteListDate.ToShortDateString();
	}
}

