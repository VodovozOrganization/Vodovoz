using ClosedXML.Report;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Utilities.Text;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public class FastDeliverySalesReportViewModel : DialogTabViewModelBase
	{
		private const string _templatePath = @".\Reports\Orders\FastDeliverySalesReport.xlsx";
		private readonly IFileDialogService _fileDialogService;
		private DelegateCommand _generateCommand;
		private DelegateCommand _exportCommand;
		private FastDeliverySalesReport _report;
		private bool _isRunning;

		public FastDeliverySalesReportViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService,
			INavigationManager navigation, IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			Title = "Отчёт по продажам с доставкой за час";
		}

		private IList<FastDeliverySalesReportRow> GenerateReportRows()
		{
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			Employee driverAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			FastDeliverySalesReportRow resultAlias = null;

			var itemsQuery = UoW.Session.QueryOver(() => orderItemAlias)
					.JoinAlias(() => orderItemAlias.Order, () => orderAlias)
					.JoinEntityAlias(() => routeListItemAlias, () => routeListItemAlias.Order.Id == orderAlias.Id)
					.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
					.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
					.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
					.JoinAlias(() => orderAlias.DeliverySchedule, () => deliveryScheduleAlias)
					.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
					.Where(() => orderAlias.IsFastDelivery);

			if(CreateDateFrom != null && CreateDateTo != null)
			{
				itemsQuery.Where(() => orderAlias.CreateDate >= CreateDateFrom.Value.Date.Add(new TimeSpan(0, 0, 0, 0))
									   && orderAlias.CreateDate <= CreateDateTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));
			}

			var amountPrjection = Projections.Conditional(Restrictions.IsNull(
					Projections.Property(() => orderItemAlias.ActualCount)),
					Projections.Property(() => orderItemAlias.Count),
				Projections.Property(() => orderItemAlias.ActualCount));

			var sumProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "(?1 * ?2 - ?3)"),
				NHibernateUtil.Decimal, Projections.Property(() => orderItemAlias.Price),
				amountPrjection,
				Projections.Property(() => orderItemAlias.DiscountMoney)
				);

			return itemsQuery
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.OrderCreateDateTime)
					.Select(() => routeListItemAlias.RouteList.Id).WithAlias(() => resultAlias.RouteListId)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverLastName)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.District)
					.Select(() => orderAlias.TimeDelivered).WithAlias(() => resultAlias.DeliveredDateTime)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Nomenclature)
					.Select(amountPrjection).WithAlias(() => resultAlias.Amount)
					.Select(sumProjection).WithAlias(() => resultAlias.Sum)
				).OrderBy(() => orderAlias.CreateDate).Desc
				.TransformUsing(Transformers.AliasToBean<FastDeliverySalesReportRow>())
				.List<FastDeliverySalesReportRow>();
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

					Report = new FastDeliverySalesReport
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
		public FastDeliverySalesReport Report
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

	public class FastDeliverySalesReport
	{
		public IList<FastDeliverySalesReportRow> Rows { get; set; }
	}

	public class FastDeliverySalesReportRow
	{
		public int OrderId { get; set; }
		public DateTime OrderCreateDateTime { get; set; }
		public int RouteListId { get; set; }
		public string DriverLastName { get; set; }
		public string DriverName { get; set; }
		public string DriverPatronymic { get; set; }
		public string District { get; set; }
		public string Nomenclature { get; set; }
		public decimal Amount { get; set; }
		public decimal Sum { get; set; }
		public DateTime DeliveredDateTime { get; set; }

		public string DriverNameWithInitials => PersonHelper.PersonNameWithInitials(DriverLastName, DriverName, DriverPatronymic);
		public string OrderCreateDate => OrderCreateDateTime.ToShortDateString();
		public string OrderCreateTime => OrderCreateDateTime.ToShortTimeString();
		public string DeliveredDate => DeliveredDateTime > DateTime.MinValue ? DeliveredDateTime.ToShortDateString() : "";
		public string DeliveredTime => DeliveredDateTime > DateTime.MinValue ? DeliveredDateTime.ToShortTimeString() : "";
	}
}
