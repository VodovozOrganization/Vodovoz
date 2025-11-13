using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ClosedXML.Excel;
using DateTimeHelpers;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Core.Domain.Logistics.Drivers;
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
		public const string EmloyeeTitle = "Сотрудник";
		public const string CarTitle = "Автомобиль";
		public const string FirstEventTitle = "Первое событие";
		public const string DocumentTypeColumn = "Документ";
		public const string DocumentNumberColumn = "Номер\nдокумента";
		public const string EventDistanceTitle = "Расстояние от\nместа фиксации";
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
		private DriverWarehouseEvent _firstEvent;
		private DriverWarehouseEvent _secondEvent;

		private DelegateCommand _generateReportCommand;
		private DelegateCommand _abortGenerateReportCommand;
		private bool _isDateGroup = true;
		private bool _isCarGroup;
		private bool _isDriverGroup;

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

			TabName = "Отчет по событиям нахождения водителей на складе";
			
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

		public IList<DriversWarehousesEventsReportNode> ReportNodes { get; } =
			new List<DriversWarehousesEventsReportNode>();

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
		
		public DriverWarehouseEvent FirstEvent
		{
			get => _firstEvent;
			set => SetField(ref _firstEvent, value);
		}
		
		public DriverWarehouseEvent SecondEvent
		{
			get => _secondEvent;
			set => SetField(ref _secondEvent, value);
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

		public bool IsDateGroup
		{
			get => _isDateGroup;
			set => SetField(ref _isDateGroup, value);
		}

		public bool IsCarGroup
		{
			get => _isCarGroup;
			set => SetField(ref _isCarGroup, value);
		}

		public bool IsDriverGroup
		{
			get => _isDriverGroup;
			set => SetField(ref _isDriverGroup, value);
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
			Employee driverAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			DriversWarehousesEventNode resultAlias = null;

			var eventIds = new int[] { FirstEvent.Id, SecondEvent.Id };


			var query = _unitOfWork.Session.QueryOver(() => completedEventAlias)
				.JoinAlias(ce => ce.DriverWarehouseEvent, () => eventAlias)
				.JoinAlias(ce => ce.Employee, () => driverAlias)
				.Left.JoinAlias(ce => ce.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.WhereRestrictionOn(() => eventAlias.Id).IsIn(eventIds);

			if(Car != null)
			{
				query.And(ce => ce.Car.Id == Car.Id);
			}

			if(Driver != null)
			{
				query.And(ce => ce.Employee.Id == Driver.Id);
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
					.Select(() => eventAlias.Id).WithAlias(() => resultAlias.EventId)
					.Select(() => eventAlias.EventName).WithAlias(() => resultAlias.EventName)
					.Select(() => eventAlias.DocumentType).WithAlias(() => resultAlias.DocumentType)
					.Select(ce => ce.DocumentId).WithAlias(() => resultAlias.DocumentNumber)
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

				InsertReportValues(ws);
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
		
		private void InsertReportValues(IXLWorksheet ws)
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
				EmloyeeTitle,
				CarTitle,
				FirstEventTitle,
				DocumentTypeColumn,
				DocumentNumberColumn,
				EventDistanceTitle,
				EventTimeTitle,
				SecondEventTitle,
				DocumentTypeColumn,
				DocumentNumberColumn,
				EventDistanceTitle,
				EventTimeTitle,
			};
		}

		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<DriversWarehousesEventsReportViewModel>(
				this, this, _unitOfWork, NavigationManager, _scope);
			
			StartEventNameViewModel =
				builder.ForProperty(x => x.FirstEvent)
					.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsJournalViewModel>()
					.UseViewModelDialog<DriverWarehouseEventViewModel>()
					.Finish();
			
			EndEventNameViewModel =
				builder.ForProperty(x => x.SecondEvent)
					.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsJournalViewModel>()
					.UseViewModelDialog<DriverWarehouseEventViewModel>()
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
			
			if(IsDateGroup)
			{
				if(nodes.Count == 0)
				{
					return;
				}
			
				var firstNode = nodes.First();
			
				var date = UpdateLocalParams(firstNode, out var driver, out var eventId);
				var nextNode = AddNextSingleEventNode(firstNode);

				for(var i = 1; i < nodes.Count; i++)
				{
					var node = nodes[i];
					if(date == node.EventDate && driver == node.DriverFio && eventId == FirstEvent.Id && eventId != node.EventId)
					{
						nextNode.SecondEventName = node.EventName;
						nextNode.SecondEventDistance = node.Distance;
						nextNode.SecondEventTime = node.EventTime;
					}
					else
					{
						nextNode = AddNextSingleEventNode(node);
					}
				
					date = UpdateLocalParams(node, out driver, out eventId);
				}
			}
			else if(IsCarGroup)
			{
				if(nodes.Count == 0)
				{
					return;
				}
				
				while(nodes.Count > 0)
				{
					var firstNode = nodes.First();
					var currentCar = firstNode.CarModelWithNumber;
					var nextNode = AddNextSingleEventNode(firstNode);
					var nodeIndex = 1;
					var date = UpdateLocalParams(firstNode, out var driver, out var eventId);

					while(true)
					{

						if(nodeIndex >= nodes.Count)
						{
							nodes.Remove(firstNode);
							break;
						}
						else if(date == nodes[nodeIndex].EventDate && nodes[nodeIndex].CarModelWithNumber == currentCar && 
						        nodes[nodeIndex].EventId == FirstEvent.Id && firstNode.EventId == SecondEvent.Id && nodes[nodeIndex].EventTime <= firstNode.EventTime)
						{
							FillFirstEventData(nodes[nodeIndex], nextNode);

							nodes.Remove(nodes[nodeIndex]);
							nodes.Remove(firstNode);

							break;
						}
						else if(date == nodes[nodeIndex].EventDate && nodes[nodeIndex].CarModelWithNumber == currentCar &&
						        nodes[nodeIndex].EventId == SecondEvent.Id && firstNode.EventId == FirstEvent.Id && nodes[nodeIndex].EventTime >= firstNode.EventTime)
						{
							FillSecondEventData(nodes[nodeIndex], nextNode);

							nodes.Remove(nodes[nodeIndex]);
							nodes.Remove(firstNode);

							break;
						}
						else
						{
							nodeIndex++;
						}
					}
				}
			}
			else if(IsDriverGroup)
			{
				if(nodes.Count == 0)
				{
					return;
				}
				
				while(nodes.Count > 0)
				{
					var firstNode = nodes.First();
					var currentFio = firstNode.DriverFio;
					var nextNode = AddNextSingleEventNode(firstNode);
					var nodeIndex = 1;
					var date = UpdateLocalParams(firstNode, out var driver, out var eventId);

					while(true)
					{
						if(nodeIndex >= nodes.Count)
						{
							nodes.Remove(firstNode);
							break;
						}
						else if(date == nodes[nodeIndex].EventDate && nodes[nodeIndex].DriverFio == currentFio && 
						        nodes[nodeIndex].EventId == FirstEvent.Id && firstNode.EventId == SecondEvent.Id  && nodes[nodeIndex].EventTime <= firstNode.EventTime)
						{
							FillFirstEventData(nodes[nodeIndex], nextNode);

							nodes.Remove(nodes[nodeIndex]);
							nodes.Remove(firstNode);

							break;
						}
						else if(date == nodes[nodeIndex].EventDate && nodes[nodeIndex].DriverFio == currentFio &&
						        nodes[nodeIndex].EventId == SecondEvent.Id && firstNode.EventId == FirstEvent.Id  && nodes[nodeIndex].EventTime >= firstNode.EventTime)
						{
							FillSecondEventData(nodes[nodeIndex], nextNode);

							nodes.Remove(nodes[nodeIndex]);
							nodes.Remove(firstNode);

							break;
						}
						else
						{
							nodeIndex++;
						}
					}
				}
			}
		}

		private static DateTime UpdateLocalParams(DriversWarehousesEventNode firstNode, out string driver, out int eventId)
		{
			var date = firstNode.EventDate;
			driver = firstNode.DriverFio;
			eventId = firstNode.EventId;
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

			if(node.EventId == FirstEvent.Id)
			{
				FillFirstEventData(node, nextNode);
			}
			else
			{
				FillSecondEventData(node, nextNode);
			}

			ReportNodes.Add(nextNode);
			return nextNode;
		}

		private void FillFirstEventData(DriversWarehousesEventNode node, DriversWarehousesEventsReportNode nextNode)
		{
			nextNode.FirstEventId = node.EventId;
			nextNode.FirstEventName = node.EventName;
			nextNode.FirstEventDocumentType = node.DocumentType?.GetEnumTitle();
			nextNode.FirstEventDocumentNumber = node.DocumentNumber;
			nextNode.FirstEventDistance = node.Distance;
			nextNode.FirstEventTime = node.EventTime;
		}

		private void FillSecondEventData(DriversWarehousesEventNode node, DriversWarehousesEventsReportNode nextNode)
		{
			nextNode.SecondEventId = node.EventId;
			nextNode.SecondEventName = node.EventName;
			nextNode.SecondEventDocumentType = node.DocumentType?.GetEnumTitle();
			nextNode.SecondEventDocumentNumber = node.DocumentNumber;
			nextNode.SecondEventDistance = node.Distance;
			nextNode.SecondEventTime = node.EventTime;
		}
	}
}
