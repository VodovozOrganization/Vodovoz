using Autofac;
using ClosedXML.Report;
using MassTransit;
using NHibernate.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.ChangingPaymentTypeByDriversReport
{
	public class ChangingPaymentTypeByDriversReportViewModel : DialogTabViewModelBase
	{
		private readonly IDriverApiSettings _driverApiSettings;
		private readonly IGeographicGroupSettings _geographicGroupSettings;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IGuiDispatcher _guiDispatcher;
		private bool _canGenerateReport = true;
		private const string _templatePath = @".\Reports\Logistic\ChangingPaymentTypeByDriversReport.xlsx";

		public ChangingPaymentTypeByDriversReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDriverApiSettings driverApiSettings,
			IGeographicGroupSettings geographicGroupSettings,
			ISubdivisionSettings subdivisionSettings,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICommonServices commonServices,
			IFileDialogService fileDialogService,
			IGuiDispatcher guiDispatcher)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			Title = "Отчет по изменению формы оплаты водителями";

			var now = DateTime.Now;
			StartDate = DateTime.Today;
			EndDate = DateTime.Today;
			_driverApiSettings = driverApiSettings ?? throw new ArgumentNullException(nameof(driverApiSettings));
			_geographicGroupSettings = geographicGroupSettings ?? throw new ArgumentNullException(nameof(geographicGroupSettings));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			CanGenerateReport = true;

			CreateReportCommand = new AsyncCommand(guiDispatcher, CreateReportAsync, () => CanGenerateReport);
			CreateReportCommand.CanExecuteChangedWith(this, x => x.CanGenerateReport);

			SaveReportCommand = new DelegateCommand(SaveReport);
			ShowHelpInfoCommand = new DelegateCommand(ShowHelpInfo);
		}

		private void ShowHelpInfo()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				"Отчет отображает изменения формы оплаты водителем с наличной формы оплаты на любую другую до закрытия заказа.\n" +
				"В отчёт попадают данные за последние два месяца."
			);
		}

		private int? GetSubdivisionIdBySelectedGeoGroup()
		{
			if(SelectedGeoGroup?.Id == _geographicGroupSettings.NorthGeographicGroupId)
			{
				return _subdivisionSettings.LogisticSubdivisionBugriId;
			}

			if(SelectedGeoGroup?.Id == _geographicGroupSettings.SouthGeographicGroupId)
			{
				return _subdivisionSettings.LogisticSubdivisionSofiiskayaId;
			}

			return null;
		}

		private void ExportReport(string path)
		{
			var template = new XLTemplate(_templatePath);
			template.AddVariable(Report);
			template.Generate();
			template.SaveAs(path);
		}

		private async Task CreateReportAsync(CancellationToken cancellationToken)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				CanGenerateReport = false;
			});

			Report = new ChangingPaymentTypeByDriversReport
			{
				StartDate = StartDate,
				EndDate = EndDate,
				SelectedGeoGroupName = SelectedGeoGroup?.Name ?? "Все",
				Rows = await GenerateReportRows(cancellationToken),
			};

			OnPropertyChanged(() => ReportRows);

			_guiDispatcher.RunInGuiTread(() =>
			{
				CanGenerateReport = true;
			});
		}

		private async Task<List<ChangingPaymentTypeByDriversReportRow>> GenerateReportRows(CancellationToken cancellationToken)
		{
			var driverSubdivisionId = GetSubdivisionIdBySelectedGeoGroup();

			var rows = await (
				from hc in UoW.Session.Query<FieldChange>()
				join hce in UoW.Session.Query<ChangedEntity>()
					on hc.Entity.Id equals hce.Id
				join hcs in UoW.Session.Query<ChangeSet>()
					on hce.ChangeSet.Id equals hcs.Id
				join o in UoW.Session.Query<Order>()
					on hce.EntityId equals o.Id
				join rla in UoW.Session.Query<RouteListItem>()
						on o.Id equals rla.Order.Id
				join rl in UoW.Session.Query<RouteList>()
						on rla.RouteList.Id equals rl.Id
				join e in UoW.Session.Query<Employee>()
					on rl.Driver.Id equals e.Id

				let sum = (
					from oi in UoW.Session.Query<OrderItem>()
					where
						oi.Order.Id == o.Id
					select (oi.ActualCount ?? oi.Count) * oi.Price - oi.DiscountMoney
				).Sum()

				where
					hc.Path == nameof(PaymentType)
					&& hc.Type == FieldChangeType.Changed
					&& hc.OldValue == nameof(PaymentType.Cash)
					&& hce.EntityClassName == nameof(Order)
					&& hcs.User.Id == _driverApiSettings.DriverApiUserId
					&& hce.ChangeTime < o.TimeDelivered
					&& hce.ChangeTime >= StartDate
					&& hce.ChangeTime <= EndDate.AddDays(1).AddMilliseconds(-1)
					&& (driverSubdivisionId == null || driverSubdivisionId == e.Subdivision.Id)

				select new ChangingPaymentTypeByDriversReportRow
				{
					OrderId = o.Id.ToString(),
					ChangeDateTime = hce.ChangeTime.ToString("G"),
					DriverName = string.Join(" ", e.LastName, e.Name, e.Patronymic),
					OriginalPaymentTypeString = hc.OldValue,
					NewPaymentTypeString = hc.NewValue,
					OrderSum = sum.ToString("0.00")
				}
			).ToListAsync(cancellationToken);

			if(!IsGroupByDriver)
			{
				return rows;
			}

			var groupedByDriverRows = rows.GroupBy(r => r.DriverName)
				.OrderBy(x => x.Key)
				.ToDictionary(x => x.Key, x => x.Select(v => v));

			var rowsWithDriverTitles = new List<ChangingPaymentTypeByDriversReportRow>();

			foreach(var g in groupedByDriverRows)
			{
				rowsWithDriverTitles.Add(new ChangingPaymentTypeByDriversReportRow { DriverName = g.Key, IsTitle = true });
				rowsWithDriverTitles.AddRange(g.Value);
			}

			return rowsWithDriverTitles;
		}

		public AsyncCommand CreateReportCommand { get; set; }
		public DelegateCommand SaveReportCommand { get; set; }
		public DelegateCommand ShowHelpInfoCommand { get; private set; }

		public ChangingPaymentTypeByDriversReport Report { get; set; } = new ChangingPaymentTypeByDriversReport();
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public List<ChangingPaymentTypeByDriversReportRow> ReportRows => Report.Rows;
		public bool IsGroupByDriver { get; private set; }
		public GeoGroup SelectedGeoGroup { get; private set; }

		public bool CanGenerateReport
		{
			get => _canGenerateReport;
			set => SetField(ref _canGenerateReport, value);
		}

		public void SaveReport()
		{
			var dialogSettings = new DialogSettings()
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"{TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(Report != null && result.Successful)
			{
				ExportReport(result.Path);
			}
		}
	}
}
