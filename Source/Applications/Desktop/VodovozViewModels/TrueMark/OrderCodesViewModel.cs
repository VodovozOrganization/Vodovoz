using Core.Infrastructure;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Vodovoz.Core.Data.Logistics;
using Vodovoz.Core.Domain.Interfaces.TrueMark;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Models.TrueMark;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.TrueMark
{
	public class OrderCodesViewModel : DialogViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IClipboard _clipboard;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private int _codesRequired;
		private int _codesProvided;
		private int _codesProvidedFromScan;
		private int _totalScannedByDriver;
		private IList<OrderCodeItemViewModel> _scannedByDriverCodesOrigin;
		private IList<OrderCodeItemViewModel> _scannedByDriverCodes;
		private IEnumerable<OrderCodeItemViewModel> _scannedByDriverCodesSelected;
		private int _totalScannedByWarehouse;
		private IList<OrderCodeItemViewModel> _scannedByWarehouseCodesOrigin;
		private IList<OrderCodeItemViewModel> _scannedByWarehouseCodes;
		private IEnumerable<OrderCodeItemViewModel> _scannedByWarehouseCodesSelected;
		private int _totalScannedBySelfdelivery;
		private IList<OrderCodeItemViewModel> _scannedBySelfdeliveryCodesOrigin;
		private IList<OrderCodeItemViewModel> _scannedBySelfdeliveryCodes;
		private IEnumerable<OrderCodeItemViewModel> _scannedBySelfdeliveryCodesSelected;
		private int _totalAddedFromPool;
		private IList<OrderCodeItemViewModel> _addedFromPoolCodesOrigin;
		private IList<OrderCodeItemViewModel> _addedFromPoolCodes;
		private IEnumerable<OrderCodeItemViewModel> _addedFromPoolCodesSelected;
		private int _totalScannedStagingCodes;
		private IList<OrderCodeItemViewModel> _scannedStagingCodesOrigin;
		private IList<OrderCodeItemViewModel> _scannedStagingCodes;
		private IEnumerable<OrderCodeItemViewModel> _scannedStagingCodesSelected;
		private string _searchText;
		private bool? _isValidSearchCodeText;
		private string _parsedSearchCodeSerialNumber;

		public OrderCodesViewModel(
			int orderId,
			IUnitOfWorkFactory uowFactory,
			ITrueMarkRepository trueMarkRepository,
			IGtkTabsOpener gtkTabsOpener,
			IClipboard clipboard,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			INavigationManager navigation,
			IRouteListItemRepository routeListItemRepository,
			IGenericRepository<Employee> employeeRepository
			) : base(navigation)
		{
			OrderId = orderId;
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_scannedByDriverCodes = new List<OrderCodeItemViewModel>();
			_scannedByDriverCodesSelected = Enumerable.Empty<OrderCodeItemViewModel>();
			_scannedByWarehouseCodes = new List<OrderCodeItemViewModel>();
			_scannedByWarehouseCodesSelected = Enumerable.Empty<OrderCodeItemViewModel>();
			_scannedBySelfdeliveryCodes = new List<OrderCodeItemViewModel>();
			_scannedBySelfdeliveryCodesSelected = Enumerable.Empty<OrderCodeItemViewModel>();
			_addedFromPoolCodes = new List<OrderCodeItemViewModel>();
			_addedFromPoolCodesSelected = Enumerable.Empty<OrderCodeItemViewModel>();
			_scannedStagingCodes = new List<OrderCodeItemViewModel>();
			_scannedStagingCodesSelected = Enumerable.Empty<OrderCodeItemViewModel>();

			Title = $"Коды ЧЗ для заказа {orderId}";

			CreateCommands();
			Reload();
		}

		public ICommand RefreshCommand { get; private set; }
		public ICommand CopyDriverSourceCodesCommand { get; private set; }
		public ICommand CopyDriverResultCodesCommand { get; private set; }
		public ICommand CopyWarehouseSourceCodesCommand { get; private set; }
		public ICommand CopyWarehouseResultCodesCommand { get; private set; }
		public ICommand CopySelfdeliverySourceCodesCommand { get; private set; }
		public ICommand CopySelfdeliveryResultCodesCommand { get; private set; }
		public ICommand CopyPoolCodesCommand { get; private set; }
		public ICommand CopyStagingCodesCommand { get; private set; }
		public ICommand OpenRouteListCommand { get; private set; }
		public ICommand OpenCarLoadDocumentCommand { get; private set; }
		public ICommand OpenSelfdeliveryDocumentCommand { get; private set; }
		public ICommand OpenFromDriverAuthorCommand { get; private set; }
		public ICommand OpenFromWarehouseAuthorCommand { get; private set; }
		public ICommand OpenFromSelfdeliveryAuthorCommand { get; private set; }

		public int OrderId { get; private set; }

		public virtual int CodesRequired
		{
			get => _codesRequired;
			set => SetField(ref _codesRequired, value);
		}

		public virtual int CodesProvided
		{
			get => _codesProvided;
			set => SetField(ref _codesProvided, value);
		}

		public virtual int CodesProvidedFromScan
		{
			get => _codesProvidedFromScan;
			set => SetField(ref _codesProvidedFromScan, value);
		}

		public virtual int TotalScannedByDriver
		{
			get => _totalScannedByDriver;
			set => SetField(ref _totalScannedByDriver, value);
		}

		public virtual IList<OrderCodeItemViewModel> ScannedByDriverCodes
		{
			get => _scannedByDriverCodes;
			set => SetField(ref _scannedByDriverCodes, value);
		}

		public virtual IEnumerable<OrderCodeItemViewModel> ScannedByDriverCodesSelected
		{
			get => _scannedByDriverCodesSelected;
			set => SetField(ref _scannedByDriverCodesSelected, value);
		}

		public virtual int TotalScannedByWarehouse
		{
			get => _totalScannedByWarehouse;
			set => SetField(ref _totalScannedByWarehouse, value);
		}

		public virtual IList<OrderCodeItemViewModel> ScannedByWarehouseCodes
		{
			get => _scannedByWarehouseCodes;
			set => SetField(ref _scannedByWarehouseCodes, value);
		}

		public virtual IEnumerable<OrderCodeItemViewModel> ScannedByWarehouseCodesSelected
		{
			get => _scannedByWarehouseCodesSelected;
			set => SetField(ref _scannedByWarehouseCodesSelected, value);
		}

		public virtual int TotalScannedBySelfdelivery
		{
			get => _totalScannedBySelfdelivery;
			set => SetField(ref _totalScannedBySelfdelivery, value);
		}

		public virtual IList<OrderCodeItemViewModel> ScannedBySelfdeliveryCodes
		{
			get => _scannedBySelfdeliveryCodes;
			set => SetField(ref _scannedBySelfdeliveryCodes, value);
		}

		public virtual IEnumerable<OrderCodeItemViewModel> ScannedBySelfdeliveryCodesSelected
		{
			get => _scannedBySelfdeliveryCodesSelected;
			set => SetField(ref _scannedBySelfdeliveryCodesSelected, value);
		}

		public virtual int TotalAddedFromPool
		{
			get => _totalAddedFromPool;
			set => SetField(ref _totalAddedFromPool, value);
		}

		public virtual IList<OrderCodeItemViewModel> AddedFromPoolCodes
		{
			get => _addedFromPoolCodes;
			set => SetField(ref _addedFromPoolCodes, value);
		}

		public virtual IEnumerable<OrderCodeItemViewModel> AddedFromPoolCodesSelected
		{
			get => _addedFromPoolCodesSelected;
			set => SetField(ref _addedFromPoolCodesSelected, value);
		}

		public virtual int TotalScannedStagingCodes
		{
			get => _totalScannedStagingCodes;
			set => SetField(ref _totalScannedStagingCodes, value);
		}

		public virtual IList<OrderCodeItemViewModel> ScannedStagingCodes
		{
			get => _scannedStagingCodes;
			set => SetField(ref _scannedStagingCodes, value);
		}

		public virtual IEnumerable<OrderCodeItemViewModel> ScannedStagingCodesSelected
		{
			get => _scannedStagingCodesSelected;
			set => SetField(ref _scannedStagingCodesSelected, value);
		}

		public virtual string SearchText
		{
			get => _searchText;
			set
			{
				SetField(ref _searchText, value);
				FilterCodes();
			}
		}

		public virtual bool? IsValidSearchCodeText
		{
			get => _isValidSearchCodeText;
			set => SetField(ref _isValidSearchCodeText, value);
		}

		public virtual string ParsedSearchCodeSerialNumber
		{
			get => _parsedSearchCodeSerialNumber;
			set => SetField(ref _parsedSearchCodeSerialNumber, value);
		}

		private void CreateCommands()
		{
			RefreshCommand = new DelegateCommand(Reload);

			// Copy driver codes
			var copyDriverSourceCodesCommand = new DelegateCommand(
				() => CopySourceCodesToClipboard(ScannedByDriverCodesSelected),
				() => ScannedByDriverCodesSelected.Any()
			);
			copyDriverSourceCodesCommand.CanExecuteChangedWith(this, x => x.ScannedByDriverCodesSelected);
			CopyDriverSourceCodesCommand = copyDriverSourceCodesCommand;

			var copyDriverResultCodesCommand = new DelegateCommand(
				() => CopyResultCodesToClipboard(ScannedByDriverCodesSelected),
				() => ScannedByDriverCodesSelected.Any()
			);
			copyDriverSourceCodesCommand.CanExecuteChangedWith(this, x => x.ScannedByDriverCodesSelected);
			CopyDriverResultCodesCommand = copyDriverResultCodesCommand;

			//Copy warehouse codes
			var copyWarehouseSourceCodesCommand = new DelegateCommand(
				() => CopySourceCodesToClipboard(ScannedByWarehouseCodesSelected),
				() => ScannedByWarehouseCodesSelected.Any()
			);
			copyWarehouseSourceCodesCommand.CanExecuteChangedWith(this, x => x.ScannedByWarehouseCodesSelected);
			CopyWarehouseSourceCodesCommand = copyWarehouseSourceCodesCommand;

			var copyWarehouseResultCodesCommand = new DelegateCommand(
				() => CopyResultCodesToClipboard(ScannedByWarehouseCodesSelected),
				() => ScannedByWarehouseCodesSelected.Any()
			);
			copyWarehouseResultCodesCommand.CanExecuteChangedWith(this, x => x.ScannedByWarehouseCodesSelected);
			CopyWarehouseResultCodesCommand = copyWarehouseResultCodesCommand;

			//Copy selfdelivery codes
			var copySelfdeliverySourceCodesCommand = new DelegateCommand(
				() => CopySourceCodesToClipboard(ScannedBySelfdeliveryCodesSelected),
				() => ScannedBySelfdeliveryCodesSelected.Any()
			);
			copySelfdeliverySourceCodesCommand.CanExecuteChangedWith(this, x => x.ScannedBySelfdeliveryCodesSelected);
			CopySelfdeliverySourceCodesCommand = copySelfdeliverySourceCodesCommand;

			var copySelfdeliveryResultCodesCommand = new DelegateCommand(
				() => CopyResultCodesToClipboard(ScannedBySelfdeliveryCodesSelected),
				() => ScannedBySelfdeliveryCodesSelected.Any()
			);
			copySelfdeliveryResultCodesCommand.CanExecuteChangedWith(this, x => x.ScannedBySelfdeliveryCodesSelected);
			CopySelfdeliveryResultCodesCommand = copySelfdeliveryResultCodesCommand;

			//Copy pool codes
			var copyPoolCodesCommand = new DelegateCommand(
				() => CopyResultCodesToClipboard(AddedFromPoolCodesSelected),
				() => AddedFromPoolCodesSelected.Any()
			);
			copyPoolCodesCommand.CanExecuteChangedWith(this, x => x.AddedFromPoolCodesSelected);
			CopyPoolCodesCommand = copyPoolCodesCommand;

			//Copy staging codes
			var copyStagingCodesCommand = new DelegateCommand(
				() => CopyResultCodesToClipboard(ScannedStagingCodesSelected),
				() => ScannedStagingCodesSelected.Any()
			);
			copyStagingCodesCommand.CanExecuteChangedWith(this, x => x.ScannedStagingCodesSelected);
			CopyStagingCodesCommand = copyStagingCodesCommand;

			//Open documents
			var openRouteListCommand = new DelegateCommand(
				() => OpenRouteList(),
				() => OnlyOneSelected(ScannedByDriverCodesSelected)
			);
			openRouteListCommand.CanExecuteChangedWith(this, x => x.ScannedByDriverCodesSelected);
			OpenRouteListCommand = openRouteListCommand;

			var openCarLoadDocumentCommand = new DelegateCommand(
				() => OpenCarLoadDocument(),
				() => OnlyOneSelected(ScannedByWarehouseCodesSelected)
			);
			openCarLoadDocumentCommand.CanExecuteChangedWith(this, x => x.ScannedByWarehouseCodesSelected);
			OpenCarLoadDocumentCommand = openCarLoadDocumentCommand;

			var openSelfdeliveryDocumentCommand = new DelegateCommand(
				() => OpenSelfdeliveryDocument(),
				() => OnlyOneSelected(ScannedBySelfdeliveryCodesSelected)
			);
			openSelfdeliveryDocumentCommand.CanExecuteChangedWith(this, x => x.ScannedBySelfdeliveryCodesSelected);
			OpenSelfdeliveryDocumentCommand = openSelfdeliveryDocumentCommand;

			//Open authors
			var openFromDriverAuthorCommand = new DelegateCommand(
				() => OpenAuthor(ScannedByDriverCodesSelected),
				() => OnlyOneSelected(ScannedByDriverCodesSelected)
			);
			openFromDriverAuthorCommand.CanExecuteChangedWith(this, x => x.ScannedByDriverCodesSelected);
			OpenFromDriverAuthorCommand = openFromDriverAuthorCommand;

			var openFromWarehouseAuthorCommand = new DelegateCommand(
				() => OpenAuthor(ScannedByWarehouseCodesSelected),
				() => OnlyOneSelected(ScannedByWarehouseCodesSelected)
			);
			openFromWarehouseAuthorCommand.CanExecuteChangedWith(this, x => x.ScannedByWarehouseCodesSelected);
			OpenFromWarehouseAuthorCommand = openFromWarehouseAuthorCommand;

			var openFromSelfdeliveryAuthorCommand = new DelegateCommand(
				() => OpenAuthor(ScannedBySelfdeliveryCodesSelected),
				() => OnlyOneSelected(ScannedBySelfdeliveryCodesSelected)
			);
			openFromSelfdeliveryAuthorCommand.CanExecuteChangedWith(this, x => x.ScannedBySelfdeliveryCodesSelected);
			OpenFromSelfdeliveryAuthorCommand = openFromSelfdeliveryAuthorCommand;
		}

		private void Reload()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{

				ReloadCodesFromDriver(uow);
				ReloadCodesFromWarehouse(uow);
				ReloadCodesFromSelfdelivery(uow);
				ReloadCodesFromPool(uow);
				ReloadScanndedStagingCodes(uow);

				_codesRequired = _trueMarkRepository.GetCodesRequiredByOrder(uow, OrderId);
				_codesProvidedFromScan = TotalScannedByDriver
					+ TotalScannedByWarehouse
					+ TotalScannedBySelfdelivery;

				_searchText = null;

				FilterCodes();

				OnPropertyChanged(nameof(CodesRequired));
				OnPropertyChanged(nameof(SearchText));
				CodesProvidedFromScan = TotalScannedByDriver
					+ TotalScannedByWarehouse
					+ TotalScannedBySelfdelivery;
				CodesProvided = CodesProvidedFromScan + TotalAddedFromPool;
			}
		}

		private void ReloadCodesFromDriver(IUnitOfWork uow)
		{
			var instanceCodes = _trueMarkRepository.GetCodesFromDriverByOrder(uow, OrderId);

			var groupCodesIds = instanceCodes
				.Where(x => x.SourceCode != null)
				.Where(x => x.SourceCode.ParentWaterGroupCodeId != null)
				.Select(x => x.SourceCode.ParentWaterGroupCodeId.Value)
				.Distinct();
			var groupCodes = _trueMarkRepository.GetGroupWaterCodes(uow, groupCodesIds);

			var transportCodesIdsFromIndividual = instanceCodes
				.Where(x => x.SourceCode != null)
				.Where(x => x.SourceCode.ParentTransportCodeId != null)
				.Select(x => x.SourceCode.ParentTransportCodeId.Value);
			var transportCodesIdsFromGroups = groupCodes
				.Where(x => x.ParentTransportCodeId != null)
				.Select(x => x.ParentTransportCodeId.Value);
			var transportCodesIds = transportCodesIdsFromIndividual
				.Concat(transportCodesIdsFromGroups)
				.Distinct();
			var transportCodes = _trueMarkRepository.GetTransportCodes(uow, transportCodesIds);

			var transportItemViewModels = transportCodes
				.Select(x => new OrderCodeItemViewModel { TransportCode = x })
				.ToDictionary(x => x.TransportCode.Id);
			var groupItemViewModels = groupCodes
				.Select(x =>
				{
					var vm = new OrderCodeItemViewModel { GroupCode = x };
					if(x.ParentTransportCodeId.HasValue)
					{
						vm.Parent = transportItemViewModels[x.ParentTransportCodeId.Value];
						vm.Parent.Children.Add(vm);
					}
					return vm;
				})
				.ToDictionary(x => x.GroupCode.Id);

			_totalScannedByDriver = instanceCodes.Count();
			_scannedByDriverCodesOrigin = instanceCodes.Select(x =>
			{
				var vm = new OrderCodeItemViewModel
				{
					SourceCode = x.SourceCode,
					ResultCode = x.ResultCode,
					Status = x.SourceCodeStatus,
					ReplacedFromPool = x.SourceCodeStatus == SourceProductCodeStatus.Changed,
					Problem = x.Problem,
					SourceDocumentId = x.RouteListItem.RouteList.Id,
					CodeAuthorId = x.RouteListItem.RouteList.Driver.Id,
					CodeAuthor = x.RouteListItem.RouteList.Driver.FullName
				};

				if(x.SourceCode != null && x.SourceCode.ParentTransportCodeId.HasValue)
				{
					vm.Parent = transportItemViewModels[x.SourceCode.ParentTransportCodeId.Value];
					vm.Parent.Children.Add(vm);
				}

				if(x.SourceCode != null && x.SourceCode.ParentWaterGroupCodeId.HasValue)
				{
					vm.Parent = groupItemViewModels[x.SourceCode.ParentWaterGroupCodeId.Value];
					vm.Parent.Children.Add(vm);
				}
				return vm;
			})
			// для рекурсивной модели необходимо найти все корневые узлы
			.Where(x => x.Parent == null)
			.Concat(groupItemViewModels.Values.Where(x => x.Parent == null))
			.Concat(transportItemViewModels.Values.Where(x => x.Parent == null))
			.ToList();
		}

		private void ReloadCodesFromWarehouse(IUnitOfWork uow)
		{
			var instanceCodes = _trueMarkRepository.GetCodesFromWarehouseByOrder(uow, OrderId);

			var groupCodesIds = instanceCodes
				.Where(x => x.SourceCode != null)
				.Where(x => x.SourceCode.ParentWaterGroupCodeId != null)
				.Select(x => x.SourceCode.ParentWaterGroupCodeId.Value)
				.Distinct();
			var groupCodes = _trueMarkRepository.GetGroupWaterCodes(uow, groupCodesIds);

			var transportCodesIdsFromIndividual = instanceCodes
				.Where(x => x.SourceCode != null)
				.Where(x => x.SourceCode.ParentTransportCodeId != null)
				.Select(x => x.SourceCode.ParentTransportCodeId.Value);
			var transportCodesIdsFromGroups = groupCodes
				.Where(x => x.ParentTransportCodeId != null)
				.Select(x => x.ParentTransportCodeId.Value);
			var transportCodesIds = transportCodesIdsFromIndividual
				.Concat(transportCodesIdsFromGroups)
				.Distinct();
			var transportCodes = _trueMarkRepository.GetTransportCodes(uow, transportCodesIds);

			var transportItemViewModels = transportCodes
				.Select(x => new OrderCodeItemViewModel { TransportCode = x })
				.ToDictionary(x => x.TransportCode.Id);
			var groupItemViewModels = groupCodes
				.Select(x =>
				{
					var vm = new OrderCodeItemViewModel { GroupCode = x };
					if(x.ParentTransportCodeId.HasValue)
					{
						vm.Parent = transportItemViewModels[x.ParentTransportCodeId.Value];
						vm.Parent.Children.Add(vm);
					}
					return vm;
				})
				.ToDictionary(x => x.GroupCode.Id);

			var carLoadDocumentIds = instanceCodes.Select(x => x.CarLoadDocumentItem.Document.Id).Distinct();
			var carLoadEvents = uow.Session.QueryOver<CompletedDriverWarehouseEventProxy>()
				.WhereRestrictionOn(x => x.DocumentId).IsIn(carLoadDocumentIds.ToArray())
				.List();

			_totalScannedByWarehouse = instanceCodes.Count();
			_scannedByWarehouseCodesOrigin = instanceCodes.Select(x =>
			{
				var vm = new OrderCodeItemViewModel
				{
					SourceCode = x.SourceCode,
					ResultCode = x.ResultCode,
					Status = x.SourceCodeStatus,
					ReplacedFromPool = x.SourceCodeStatus == SourceProductCodeStatus.Changed,
					Problem = x.Problem,
					SourceDocumentId = x.CarLoadDocumentItem.Document.Id
				};

				var author = carLoadEvents
					.Where(y => y.DocumentId == x.CarLoadDocumentItem.Document.Id)
					.FirstOrDefault()?.Employee;

				vm.CodeAuthorId = author?.Id;
				vm.CodeAuthor = author == null ? "" : author.FullName;

				if(x.SourceCode != null && x.SourceCode.ParentTransportCodeId.HasValue)
				{
					vm.Parent = transportItemViewModels[x.SourceCode.ParentTransportCodeId.Value];
					vm.Parent.Children.Add(vm);
				}

				if(x.SourceCode != null && x.SourceCode.ParentWaterGroupCodeId.HasValue)
				{
					vm.Parent = groupItemViewModels[x.SourceCode.ParentWaterGroupCodeId.Value];
					vm.Parent.Children.Add(vm);
				}
				return vm;
			})
			// для рекурсивной модели необходимо найти все корневые узлы
			.Where(x => x.Parent == null)
			.Concat(groupItemViewModels.Values.Where(x => x.Parent == null))
			.Concat(transportItemViewModels.Values.Where(x => x.Parent == null))
			.ToList();
		}

		private void ReloadCodesFromSelfdelivery(IUnitOfWork uow)
		{
			var instanceCodes = _trueMarkRepository.GetCodesFromSelfdeliveryByOrder(uow, OrderId);

			var groupCodesIds = instanceCodes
				.Where(x => x.SourceCode != null)
				.Where(x => x.SourceCode.ParentWaterGroupCodeId != null)
				.Select(x => x.SourceCode.ParentWaterGroupCodeId.Value)
				.Distinct();
			var groupCodes = _trueMarkRepository.GetGroupWaterCodes(uow, groupCodesIds);

			var transportCodesIdsFromIndividual = instanceCodes
				.Where(x => x.SourceCode != null)
				.Where(x => x.SourceCode.ParentTransportCodeId != null)
				.Select(x => x.SourceCode.ParentTransportCodeId.Value);
			var transportCodesIdsFromGroups = groupCodes
				.Where(x => x.ParentTransportCodeId != null)
				.Select(x => x.ParentTransportCodeId.Value);
			var transportCodesIds = transportCodesIdsFromIndividual
				.Concat(transportCodesIdsFromGroups)
				.Distinct();
			var transportCodes = _trueMarkRepository.GetTransportCodes(uow, transportCodesIds);

			var transportItemViewModels = transportCodes
				.Select(x => new OrderCodeItemViewModel { TransportCode = x })
				.ToDictionary(x => x.TransportCode.Id);
			var groupItemViewModels = groupCodes
				.Select(x =>
				{
					var vm = new OrderCodeItemViewModel { GroupCode = x };
					if(x.ParentTransportCodeId.HasValue)
					{
						vm.Parent = transportItemViewModels[x.ParentTransportCodeId.Value];
						vm.Parent.Children.Add(vm);
					}
					return vm;
				})
				.ToDictionary(x => x.GroupCode.Id);

			var authorId = instanceCodes.FirstOrDefault()?.SelfDeliveryDocumentItem?.Document?.AuthorId;
			var author = authorId.HasValue
				? _employeeRepository.GetFirstOrDefault(uow, e => e.Id == authorId.Value)
				: null;

			_totalScannedBySelfdelivery = instanceCodes.Count();
			_scannedBySelfdeliveryCodesOrigin = instanceCodes.Select(x =>
			{
				var vm = new OrderCodeItemViewModel
				{
					SourceCode = x.SourceCode,
					ResultCode = x.ResultCode,
					Status = x.SourceCodeStatus,
					ReplacedFromPool = x.SourceCodeStatus == SourceProductCodeStatus.Changed,
					Problem = x.Problem,
					SourceDocumentId = x.SelfDeliveryDocumentItem.Document.Id,
					CodeAuthorId = authorId,
					CodeAuthor = author?.FullName
				};

				if(x.SourceCode != null && x.SourceCode.ParentTransportCodeId.HasValue)
				{
					vm.Parent = transportItemViewModels[x.SourceCode.ParentTransportCodeId.Value];
					vm.Parent.Children.Add(vm);
				}

				if(x.SourceCode != null && x.SourceCode.ParentWaterGroupCodeId.HasValue)
				{
					vm.Parent = groupItemViewModels[x.SourceCode.ParentWaterGroupCodeId.Value];
					vm.Parent.Children.Add(vm);
				}
				return vm;
			})
			// для рекурсивной модели необходимо найти все корневые узлы
			.Where(x => x.Parent == null)
			.Concat(groupItemViewModels.Values.Where(x => x.Parent == null))
			.Concat(transportItemViewModels.Values.Where(x => x.Parent == null))
			.ToList();
		}

		private void ReloadCodesFromPool(IUnitOfWork uow)
		{
			var poolCodes = _trueMarkRepository.GetCodesFromPoolByOrder(uow, OrderId);
			var unscannedReason = _routeListItemRepository.GetUnscannedCodesReason(uow, OrderId);
			_totalAddedFromPool = poolCodes.Count();

			if(poolCodes.Any())
			{
				_addedFromPoolCodesOrigin = poolCodes.Select(x => new OrderCodeItemViewModel
				{
					SourceCode = x.SourceCode,
					ResultCode = x.ResultCode,					
					Status = x.SourceCodeStatus,
					ReplacedFromPool = true,
					Problem = x.Problem,
					UnscannedCodesReason = unscannedReason
				}).ToList();
			}
			else
			{
				if(!string.IsNullOrWhiteSpace(unscannedReason))
				{
					_addedFromPoolCodesOrigin = new List<OrderCodeItemViewModel>
					{
						new OrderCodeItemViewModel
						{
							UnscannedCodesReason = unscannedReason
						}
					};
				}
				else
				{
					_addedFromPoolCodesOrigin = new List<OrderCodeItemViewModel>();
				}
			}
		}

		private void ReloadScanndedStagingCodes(IUnitOfWork uow)
		{
			var stagingTrueMarkCodes = _trueMarkRepository.GetAllStagingCodesByOrderId(uow, OrderId);
			TotalScannedStagingCodes = stagingTrueMarkCodes.Where(x => x.IsIdentification).Count();
			var allCodes =
				stagingTrueMarkCodes
				.Select(x => new OrderCodeItemViewModel
				{
					StagingTrueMarkCode = x
				}).ToList();

			foreach(var code in allCodes)
			{
				if(code.StagingTrueMarkCode.ParentCodeId.HasValue)
				{
					code.Parent = allCodes.FirstOrDefault(x => x.StagingTrueMarkCode.Id == code.StagingTrueMarkCode.ParentCodeId.Value);
				}

				var children = allCodes
					.Where(x => x.StagingTrueMarkCode.ParentCodeId != null && x.StagingTrueMarkCode.ParentCodeId == code.StagingTrueMarkCode.Id);

				code.Children = children.ToList();
			}

			_scannedStagingCodesOrigin = allCodes.Where(x => x.Parent == null).ToList();
		}

		private void FilterCodes()
		{
			if(SearchText.IsNullOrWhiteSpace())
			{
				ParsedSearchCodeSerialNumber = null;
				IsValidSearchCodeText = null;

				_scannedBySelfdeliveryCodes = _scannedBySelfdeliveryCodesOrigin;
				_scannedByWarehouseCodes = _scannedByWarehouseCodesOrigin;
				_scannedByDriverCodes = _scannedByDriverCodesOrigin;
				_addedFromPoolCodes = _addedFromPoolCodesOrigin;
				_scannedStagingCodes = _scannedStagingCodesOrigin;
			}
			else if(_trueMarkWaterCodeParser.FuzzyParse(SearchText, out var parsedCode))
			{
				ParsedSearchCodeSerialNumber = parsedCode.SerialNumber;
				IsValidSearchCodeText = true;
				FilterDriverCodes(parsedCode);
				FilterWarehouseCodes(parsedCode);
				FilterSelfdeliveryCodes(parsedCode);
				FilterPoolCodes(parsedCode);
				FilterStagingCodes(parsedCode);
			}
			else if(_trueMarkWaterCodeParser.IsTransportCode(SearchText))
			{
				ParsedSearchCodeSerialNumber = SearchText;
				IsValidSearchCodeText = true;
				FilterDriverCodes(SearchText);
				FilterWarehouseCodes(SearchText);
				FilterSelfdeliveryCodes(SearchText);
				FilterPoolCodes(SearchText);
			}
			else
			{
				CleanCodesFilter();
				ParsedSearchCodeSerialNumber = null;
				IsValidSearchCodeText = false;

				_scannedBySelfdeliveryCodes = _scannedBySelfdeliveryCodesOrigin;
				_scannedByWarehouseCodes = _scannedByWarehouseCodesOrigin;
				_scannedByDriverCodes = _scannedByDriverCodesOrigin;
				_addedFromPoolCodes = _addedFromPoolCodesOrigin;
				_scannedStagingCodes = _scannedStagingCodesOrigin;
			}

			OnPropertyChanged(nameof(TotalScannedByDriver));
			OnPropertyChanged(nameof(ScannedByDriverCodes));
			OnPropertyChanged(nameof(TotalScannedByWarehouse));
			OnPropertyChanged(nameof(ScannedByWarehouseCodes));
			OnPropertyChanged(nameof(TotalScannedBySelfdelivery));
			OnPropertyChanged(nameof(ScannedBySelfdeliveryCodes));
			OnPropertyChanged(nameof(TotalAddedFromPool));
			OnPropertyChanged(nameof(AddedFromPoolCodes));
			OnPropertyChanged(nameof(ScannedStagingCodes));
		}

		private void FilterDriverCodes(ITrueMarkWaterCode code)
		{
			var filteredCodes = _scannedByDriverCodesOrigin.Where(x => CodeMatched(x, code));
			_scannedByDriverCodes = filteredCodes.ToList();
		}

		private void FilterDriverCodes(string transportCode)
		{
			var filteredCodes = _scannedByDriverCodesOrigin.Where(x => CodeMatched(x, transportCode));
			_scannedByDriverCodes = filteredCodes.ToList();
		}

		private void FilterWarehouseCodes(ITrueMarkWaterCode code)
		{
			var filteredCodes = _scannedByWarehouseCodesOrigin.Where(x => CodeMatched(x, code));
			_scannedByWarehouseCodes = filteredCodes.ToList();
		}

		private void FilterWarehouseCodes(string transportCode)
		{
			var filteredCodes = _scannedByWarehouseCodesOrigin.Where(x => CodeMatched(x, transportCode));
			_scannedByWarehouseCodes = filteredCodes.ToList();
		}

		private void FilterSelfdeliveryCodes(ITrueMarkWaterCode code)
		{
			var filteredCodes = _scannedBySelfdeliveryCodesOrigin.Where(x => CodeMatched(x, code));
			_scannedBySelfdeliveryCodes = filteredCodes.ToList();
		}

		private void FilterSelfdeliveryCodes(string transportCode)
		{
			var filteredCodes = _scannedBySelfdeliveryCodesOrigin.Where(x => CodeMatched(x, transportCode));
			_scannedBySelfdeliveryCodes = filteredCodes.ToList();
		}

		private void FilterPoolCodes(ITrueMarkWaterCode code)
		{
			var filteredCodes = _addedFromPoolCodesOrigin.Where(x => CodeMatched(x, code));
			_addedFromPoolCodes = filteredCodes.ToList();
		}

		private void FilterStagingCodes(ITrueMarkWaterCode code)
		{
			var filteredCodes = _scannedStagingCodesOrigin.Where(x => CodeMatched(x, code));
			_scannedStagingCodes = filteredCodes.ToList();
		}

		private void FilterPoolCodes(string transportCode)
		{
			var filteredCodes = _addedFromPoolCodesOrigin.Where(x => CodeMatched(x, transportCode));
			_addedFromPoolCodes = filteredCodes.ToList();
		}

		private bool CodeMatched(OrderCodeItemViewModel codeItem, ITrueMarkWaterCode desiredCode)
		{
			if(codeItem.SourceCode != null && codeItem.SourceCode.SerialNumber == desiredCode.SerialNumber)
			{
				return true;
			}

			if(codeItem.ResultCode != null && codeItem.ResultCode.SerialNumber == desiredCode.SerialNumber)
			{
				return true;
			}

			if(codeItem.GroupCode != null && codeItem.GroupCode.SerialNumber == desiredCode.SerialNumber)
			{
				return true;
			}

			if(codeItem.StagingTrueMarkCode != null && codeItem.StagingTrueMarkCode.SerialNumber == desiredCode.SerialNumber)
			{
				return true;
			}

			if(codeItem.Children.Any(x => CodeMatched(x, desiredCode)))
			{
				return true;
			}

			return false;
		}

		private bool CodeMatched(OrderCodeItemViewModel codeItem, string transportCode)
		{
			if(codeItem.TransportCode != null)
			{
				if(codeItem.TransportCode.RawCode == transportCode)
				{
					return true;
				}

				if(codeItem.TransportCode.RawCode.Length > transportCode.Length)
				{
					return codeItem.TransportCode.RawCode.Remove(0, 2) == transportCode;
				}
				else
				{
					return codeItem.TransportCode.RawCode == transportCode.Remove(0, 2);
				}
			}

			if(codeItem.Children.Any(x => CodeMatched(x, transportCode)))
			{
				return true;
			}

			return false;
		}

		private void CleanCodesFilter()
		{
			_scannedByDriverCodes = _scannedByDriverCodesOrigin;
			_scannedByWarehouseCodes = _scannedByWarehouseCodesOrigin;
			_scannedBySelfdeliveryCodes = _scannedBySelfdeliveryCodesOrigin;
			_addedFromPoolCodes = _addedFromPoolCodesOrigin;
			_scannedStagingCodes = _scannedStagingCodesOrigin;
		}

		private void CopySourceCodesToClipboard(IEnumerable<OrderCodeItemViewModel> codes)
		{
			CopyCodesToClipboard(codes.Select(x => x.SourceIdentificationCode));
		}

		private void CopyResultCodesToClipboard(IEnumerable<OrderCodeItemViewModel> codes)
		{
			CopyCodesToClipboard(codes.Select(x => x.ResultIdentificationCode));
		}

		private void CopyCodesToClipboard(IEnumerable<string> codes)
		{
			if(codes == null || !codes.Any())
			{
				return;
			}
			var text = string.Join(Environment.NewLine, codes.Where(x => !x.IsNullOrWhiteSpace()));
			_clipboard.SetText(text);
		}

		private void OpenRouteList()
		{
			var selectedItem = ScannedByDriverCodesSelected.FirstOrDefault();
			if(selectedItem == null || selectedItem.SourceDocumentId == null)
			{
				return;
			}
			var entityId = EntityUoWBuilder.ForOpen(selectedItem.SourceDocumentId.Value);
			NavigationManager.OpenViewModel<RouteListKeepingViewModel, IEntityUoWBuilder>(this, entityId);
		}

		private void OpenCarLoadDocument()
		{
			var selectedItem = ScannedByWarehouseCodesSelected.FirstOrDefault();
			if(selectedItem == null || selectedItem.SourceDocumentId == null)
			{
				return;
			}
			_gtkTabsOpener.OpenCarLoadDocumentDlg(selectedItem.SourceDocumentId.Value);
		}

		private void OpenSelfdeliveryDocument()
		{
			var selectedItem = ScannedBySelfdeliveryCodesSelected.FirstOrDefault();
			if(selectedItem == null || selectedItem.SourceDocumentId == null)
			{
				return;
			}
			_gtkTabsOpener.OpenSelfDeliveryDocumentDlg(selectedItem.SourceDocumentId.Value);
		}

		private void OpenAuthor(IEnumerable<OrderCodeItemViewModel> selected)
		{
			var selectedItem = selected.FirstOrDefault();
			if(selectedItem == null || selectedItem.SourceDocumentId == null)
			{
				return;
			}
			var entityId = EntityUoWBuilder.ForOpen(selectedItem.CodeAuthorId.Value);
			NavigationManager.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(this, entityId);
		}

		private bool OnlyOneSelected(IEnumerable<OrderCodeItemViewModel> selected)
		{
			if(selected == null)
			{
				return false;
			}

			return selected.Count() == 1;
		}
	}
}
