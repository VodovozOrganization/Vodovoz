using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ClosedXML.Excel;
using DateTimeHelpers;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.NHibernateProjections.Logistics;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics
{
	public class DriversWarehousesEventsReportViewModel : TabViewModelBase
	{
		public const string DateTitle = "Дата";
		public const string DriverTitle = "Водитель";
		public const string CarTitle = "Автомобиль";
		public const string FirstEventTitle = "Первое событие";
		public const string EventDistanceTitle = "Расстояние от места фиксации";
		public const string EventTimeTitle = "Время фиксации";
		public const string SecondEventTitle = "Второе событие";

		private const string _xlsxFileFilter = "XLSX File (*.xlsx)";
		private readonly ILifetimeScope _scope;
		private readonly IFileDialogService _fileDialogService;
		private readonly IUnitOfWork _unitOfWork;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
		private Car _car;
		private bool _isGenerating;
		private DriverWarehouseEventName _firstEventName;
		private DriverWarehouseEventName _secondEventName;

		private DelegateCommand _generateReportCommand;
		private DelegateCommand _abortGenerateReportCommand;

		public DriversWarehousesEventsReportViewModel(
			ILifetimeScope scope,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			IFileDialogService fileDialogService,
			INavigationManager navigation) : base(interactiveService, navigation)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_unitOfWork = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot();

			TabName = "Отчет по событиям нахождения волителей на складе";
			
			ConfigureEntryViewModels();
		}

		public bool IsGenerating
		{
			get => _isGenerating;
			set => SetField(ref _isGenerating, value);
		}
		
		public IEntityEntryViewModel StartEventNameViewModel { get; private set; }
		public IEntityEntryViewModel EndEventNameViewModel { get; private set; }
		public IEntityEntryViewModel DriverViewModel { get; private set; }
		public IEntityEntryViewModel CarViewModel { get; private set; }

		public GenericObservableList<DriversWarehousesEventsReportNode> ReportNodes =
			new GenericObservableList<DriversWarehousesEventsReportNode>();

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}
		
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}
		
		public DriverWarehouseEventName FirstEventName
		{
			get => _firstEventName;
			set => SetField(ref _firstEventName, value);
		}
		
		public DriverWarehouseEventName SecondEventName
		{
			get => _secondEventName;
			set => SetField(ref _secondEventName, value);
		}
		
		public Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}
		
		public Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public DelegateCommand GenerateReportCommand => _generateReportCommand ?? (_generateReportCommand = new DelegateCommand(
			async () =>
			{
				if(IsGenerating)
				{
					return;
				}

				IsGenerating = true;
				CancellationTokenSource = new CancellationTokenSource();

				try
				{
					await GenerateReport();
				}
				catch(AggregateException)
				{
					
				}
				
				IsGenerating = false;
			}
			));
		
		public DelegateCommand AbortGenerateReportCommand => _abortGenerateReportCommand ?? (_abortGenerateReportCommand = new DelegateCommand(
			() =>
			{
				CancellationTokenSource.Cancel();
			}
		));

		public void ShowWarning(string message)
		{
			ShowWarningMessage(message);
		}

		public async Task GenerateReport()
		{
			CancellationTokenSource = new CancellationTokenSource();
			
			CompletedDriverWarehouseEvent completedEventAlias = null;
			DriverWarehouseEvent eventAlias = null;
			DriverWarehouseEventName eventNameAlias = null;
			Employee driverAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			DriversWarehousesEventNode resultAlias = null;

			var eventNameIds = new int[] { FirstEventName.Id, SecondEventName.Id };

			var query = _unitOfWork.Session.QueryOver(() => completedEventAlias)
				.JoinAlias(ce => ce.DriverWarehouseEvent, () => eventAlias)
				.JoinAlias(() => eventAlias.EventName, () => eventNameAlias)
				.JoinAlias(ce => ce.Driver, () => driverAlias)
				.Left.JoinAlias(ce => ce.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.WhereRestrictionOn(() => eventNameAlias.Id).IsIn(eventNameIds);

			if(Car != null)
			{
				query.And(ce => ce.Car.Id == Car.Id);
			}
			
			if(Driver != null)
			{
				query.And(ce => ce.Driver.Id == Driver.Id);
			}
			
			if(StartDate.HasValue)
			{
				query.And(ce => ce.CompletedDate >= StartDate);
			}
			
			if(EndDate.HasValue)
			{
				query.And(ce => ce.CompletedDate <= EndDate.Value.LatestDayTime());
			}

			var nodes = query.SelectList(list => list
				.Select(() => completedEventAlias.CompletedDate).WithAlias(() => resultAlias.EventDateTime)
				.Select(EmployeeProjections.GetDriverFullNameProjection()).WithAlias(() => resultAlias.DriverFio)
				.Select(CarProjections.GetCarModelWithRegistrationNumber()).WithAlias(() => resultAlias.CarModelWithNumber)
				.Select(() => eventNameAlias.Id).WithAlias(() => resultAlias.EventNameId)
				.Select(() => eventNameAlias.Name).WithAlias(() => resultAlias.EventName)
				.Select(ce => ce.DistanceMetersFromScanningLocation).WithAlias(() => resultAlias.Distance)
				)
				.OrderBy(ce => ce.CompletedDate).Asc
				.OrderBy(EmployeeProjections.GetDriverFullNameProjection()).Asc
				.TransformUsing(Transformers.AliasToBean<DriversWarehousesEventNode>())
				.List<DriversWarehousesEventNode>();

			CancellationTokenSource.Token.ThrowIfCancellationRequested();

			await CreateReportNodes(nodes);
		}
		
		public void ExportReport()
		{
			using(var wb = new XLWorkbook())
			{
				var sheetName = $"{DateTime.Now:dd.MM.yyyy}";
				var ws = wb.Worksheets.Add(sheetName);

				InsertBalanceSummaryReportValues(ws);
				ws.Columns().AdjustToContents();

				if(TryGetSavePath(out string path))
				{
					wb.SaveAs(path);
				}
			}
		}
		
		private bool TryGetSavePath(out string path)
		{
			const string extension = ".xlsx";
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = $"{TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}{extension}"
			};

			dialogSettings.FileFilters.Add(new DialogFileFilter(_xlsxFileFilter, $"*{extension}"));
			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			path = result.Path;

			return result.Successful;
		}
		
		private void InsertBalanceSummaryReportValues(IXLWorksheet ws)
		{
			var colNames = GetReportColumnTitles();
			var rows = ReportNodes;
			
			var index = 1;
			foreach(var name in colNames)
			{
				ws.Cell(1, index).Value = name;
				index++;
			}

			ws.Cell(2, 1).InsertData(rows);
		}

		private IEnumerable GetReportColumnTitles()
		{
			return new[]
			{
				DateTitle,
				DriverTitle,
				CarTitle,
				FirstEventTitle,
				EventDistanceTitle,
				EventTimeTitle,
				SecondEventTitle,
				EventDistanceTitle,
				EventTimeTitle,
			};
		}

		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<DriversWarehousesEventsReportViewModel>(
				this, this, _unitOfWork, NavigationManager, _scope);
			
			StartEventNameViewModel =
				builder.ForProperty(x => x.FirstEventName)
					.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsNamesJournalViewModel>()
					.UseViewModelDialog<DriverWarehouseEventNameViewModel>()
					.Finish();
			
			EndEventNameViewModel =
				builder.ForProperty(x => x.SecondEventName)
					.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsNamesJournalViewModel>()
					.UseViewModelDialog<DriverWarehouseEventNameViewModel>()
					.Finish();

			DriverViewModel =
				builder.ForProperty(x => x.Driver)
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
					.UseViewModelDialog<EmployeeViewModel>()
					.Finish();
			
			CarViewModel =
				builder.ForProperty(x => x.Car)
					.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
					.UseViewModelDialog<CarViewModel>()
					.Finish();
		}

		private async Task CreateReportNodes(IList<DriversWarehousesEventNode> nodes)
		{
			ReportNodes.Clear();
			CancellationTokenSource.Token.ThrowIfCancellationRequested();
			
			if(nodes.Count == 0)
			{
				return;
			}
			
			var firstNode = nodes.First();
			
			var date = UpdateLocalParams(firstNode, out var driver, out var eventNameId);
			var nextNode = AddNextSingleEventNode(firstNode);

			for(var i = 1; i < nodes.Count; i++)
			{
				var node = nodes[i];
				if(date == node.EventDate && driver == node.DriverFio && eventNameId == FirstEventName.Id && eventNameId != node.EventNameId)
				{
					nextNode.SecondEventName = node.EventName;
					nextNode.SecondEventDistance = node.Distance;
					nextNode.SecondEventTime = node.EventTime;
				}
				else
				{
					nextNode = AddNextSingleEventNode(node);
				}
				
				date = UpdateLocalParams(node, out driver, out eventNameId);
			}
		}

		private static DateTime UpdateLocalParams(DriversWarehousesEventNode firstNode, out string driver, out int eventNameId)
		{
			var date = firstNode.EventDate;
			driver = firstNode.DriverFio;
			eventNameId = firstNode.EventNameId;
			return date;
		}

		private DriversWarehousesEventsReportNode AddNextSingleEventNode(DriversWarehousesEventNode node)
		{
			var nextNode = new DriversWarehousesEventsReportNode
			{
				EventDate = node.EventDate,
				DriverFio = node.DriverFio,
				CarModelWithNumber = node.CarModelWithNumber
			};

			if(node.EventNameId == FirstEventName.Id)
			{
				nextNode.FirstEventName = node.EventName;
				nextNode.FirstEventDistance = node.Distance;
				nextNode.FirstEventTime = node.EventTime;
			}
			else
			{
				nextNode.SecondEventName = node.EventName;
				nextNode.SecondEventDistance = node.Distance;
				nextNode.SecondEventTime = node.EventTime;
			}

			ReportNodes.Add(nextNode);
			return nextNode;
		}
	}
}
