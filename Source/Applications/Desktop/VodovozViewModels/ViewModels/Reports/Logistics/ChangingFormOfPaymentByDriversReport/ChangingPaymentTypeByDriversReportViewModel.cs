using Autofac;
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
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;
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
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private bool _canGenerateReport = true;
		private bool _canCancelGenerateReport;

		public ChangingPaymentTypeByDriversReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDriverApiSettings driverApiSettings,
			IGeographicGroupSettings geographicGroupSettings,
			ISubdivisionSettings subdivisionSettings,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICommonServices commonServices,
			IFileDialogService fileDialogService,
			IGuiDispatcher guiDispatcher,
			IGenericRepository<GeoGroup> geogroupRepository,
			IDialogSettingsFactory dialogSettingsFactory)
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
			_dialogSettingsFactory = dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			CanGenerateReport = true;

			CreateReportCommand = new AsyncCommand(guiDispatcher, CreateReportAsync, () => CanGenerateReport);
			CreateReportCommand.CanExecuteChangedWith(this, x => x.CanGenerateReport);

			AbortCreateCommand = new DelegateCommand(AbortCreate, () => CanCancelGenerateReport);
			AbortCreateCommand.CanExecuteChangedWith(this, x => x.CanCancelGenerateReport);

			SaveReportCommand = new DelegateCommand(SaveReport);
			ShowHelpInfoCommand = new DelegateCommand(ShowHelpInfo);

			var usedGeoGroups = new[] { geographicGroupSettings.NorthGeographicGroupId, geographicGroupSettings.SouthGeographicGroupId };
			AllUsedGeoGroups = geogroupRepository.Get(UoW, x => usedGeoGroups.Contains(x.Id));
		}

		private void ShowHelpInfo()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				"Отчет отображает изменения формы оплаты водителем с наличной формы оплаты на любую другую до закрытия заказа.\n" +
				"В отчёт попадают данные за последние два месяца."
			);
		}
		private void AbortCreate()
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				CanCancelGenerateReport = false;
			});

			CreateReportCommand.Abort();
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

		private async Task CreateReportAsync(CancellationToken cancellationToken)
		{
			if(StartDate is null || EndDate is null)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не указаны даты");
				});

				return;
			}

			_guiDispatcher.RunInGuiTread(() =>
			{
				CanGenerateReport = false;
				CanCancelGenerateReport = true;
			});

			var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(Title + " - генерация отчета");

			try
			{
				Report = new ChangingPaymentTypeByDriversReport
				{
					StartDate = StartDate.Value,
					EndDate = EndDate.Value,
					SelectedGeoGroupName = SelectedGeoGroup?.Name ?? "Все",
					Rows = await GenerateReportRowsAsync(unitOfWork, cancellationToken),
				};

				OnPropertyChanged(() => ReportRows);

			}
			catch(OperationCanceledException)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, "Формирование отчета было прервано");
				});
			}
			catch(Exception e)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, e.Message);
				});
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					CanGenerateReport = true;
					CanCancelGenerateReport = false;
				});

				unitOfWork?.Session?.Clear();
				unitOfWork?.Dispose();
			}

			_guiDispatcher.RunInGuiTread(() =>
			{
				CanGenerateReport = true;
				CanCancelGenerateReport = false;
			});
		}

		private async Task<List<ChangingPaymentTypeByDriversReportRow>> GenerateReportRowsAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
		{
			var driverSubdivisionId = GetSubdivisionIdBySelectedGeoGroup();

			var rows = await (
				from hc in unitOfWork.Session.Query<FieldChange>()
				join hce in unitOfWork.Session.Query<ChangedEntity>()
					on hc.Entity.Id equals hce.Id
				join hcs in unitOfWork.Session.Query<ChangeSet>()
					on hce.ChangeSet.Id equals hcs.Id
				join o in unitOfWork.Session.Query<Order>()
					on hce.EntityId equals o.Id
				join rla in unitOfWork.Session.Query<RouteListItem>()
						on o.Id equals rla.Order.Id
				join rl in unitOfWork.Session.Query<RouteList>()
						on rla.RouteList.Id equals rl.Id
				join e in unitOfWork.Session.Query<Employee>()
					on rl.Driver.Id equals e.Id

				let sum = (decimal?)(
					from oi in unitOfWork.Session.Query<OrderItem>()
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
					&& hce.ChangeTime >= StartDate.Value
					&& hce.ChangeTime <= EndDate.Value.AddDays(1).AddMilliseconds(-1)
					&& (driverSubdivisionId == null || driverSubdivisionId == e.Subdivision.Id)
					&& rla.Status != RouteListItemStatus.Transfered

				select new ChangingPaymentTypeByDriversReportRow
				{
					OrderId = o.Id.ToString(),
					ChangeDateTime = hce.ChangeTime.ToString("G"),
					DriverName = string.Join(" ", e.LastName, e.Name, e.Patronymic),
					OriginalPaymentTypeString = hc.OldValue,
					NewPaymentTypeString = hc.NewValue,
					OrderSum = sum == null ? "0.00" : sum.Value.ToString("0.00")
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
				var totalChangesCount = g.Value.Count();
				var orderChangesCount = g.Value.Select(x => x.OrderId).Distinct().Count();

				rowsWithDriverTitles.Add(
					new ChangingPaymentTypeByDriversReportRow
					{
						DriverName = $"{g.Key} (итого изменений: {totalChangesCount}, заказов: {orderChangesCount}) ",
						IsTitle = true
					});

				rowsWithDriverTitles.AddRange(g.Value);
			}

			return rowsWithDriverTitles;
		}

		public AsyncCommand CreateReportCommand { get; }
		public DelegateCommand AbortCreateCommand { get; }
		public DelegateCommand SaveReportCommand { get; }
		public DelegateCommand ShowHelpInfoCommand { get; }

		public ChangingPaymentTypeByDriversReport Report { get; set; } = new ChangingPaymentTypeByDriversReport();
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public List<ChangingPaymentTypeByDriversReportRow> ReportRows => Report.Rows;
		public bool IsGroupByDriver { get; private set; }
		public IEnumerable<GeoGroup> AllUsedGeoGroups { get; set; }
		public GeoGroup SelectedGeoGroup { get; private set; }

		public bool CanGenerateReport
		{
			get => _canGenerateReport;
			set => SetField(ref _canGenerateReport, value);
		}

		public bool CanCancelGenerateReport
		{
			get => _canCancelGenerateReport;
			set => SetField(ref _canCancelGenerateReport, value);
		}

		public void SaveReport()
		{
			var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(Report);

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				Report.RenderTemplate().Export(saveDialogResult.Path);
			}
		}
	}
}
