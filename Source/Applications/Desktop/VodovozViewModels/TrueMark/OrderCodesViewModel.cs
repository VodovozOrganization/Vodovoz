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
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.EntityRepositories.TrueMark;
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

		private int _codesRequired;
		private int _codesProvided;
		private int _codesProvidedFromScan;
		private int _totalScannedByDriver;
		private IList<OrderCodeItemViewModel> _scannedByDriverCodes;
		private IEnumerable<OrderCodeItemViewModel> _scannedByDriverCodesSelected;
		private int _totalScannedByWarehouse;
		private IList<OrderCodeItemViewModel> _scannedByWarehouseCodes;
		private IEnumerable<OrderCodeItemViewModel> _scannedByWarehouseCodesSelected;
		private int _totalScannedBySelfdelivery;
		private IList<OrderCodeItemViewModel> _scannedBySelfdeliveryCodes;
		private IEnumerable<OrderCodeItemViewModel> _scannedBySelfdeliveryCodesSelected;
		private int _totalAddedFromPool;
		private IList<OrderCodeItemViewModel> _addedFromPoolCodes;
		private IEnumerable<OrderCodeItemViewModel> _addedFromPoolCodesSelected;

		public OrderCodesViewModel(
			int orderId,
			IUnitOfWorkFactory uowFactory,
			ITrueMarkRepository trueMarkRepository,
			IGtkTabsOpener gtkTabsOpener,
			IClipboard clipboard,
			INavigationManager navigation
			) : base(navigation)
		{
			OrderId = orderId;
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));

			_scannedByDriverCodes = new List<OrderCodeItemViewModel>();
			_scannedByDriverCodesSelected = Enumerable.Empty<OrderCodeItemViewModel>();
			_scannedByWarehouseCodes = new List<OrderCodeItemViewModel>();
			_scannedByWarehouseCodesSelected = Enumerable.Empty<OrderCodeItemViewModel>();
			_scannedBySelfdeliveryCodes = new List<OrderCodeItemViewModel>();
			_scannedBySelfdeliveryCodesSelected = Enumerable.Empty<OrderCodeItemViewModel>();
			_addedFromPoolCodes = new List<OrderCodeItemViewModel>();
			_addedFromPoolCodesSelected = Enumerable.Empty<OrderCodeItemViewModel>();

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

				_codesRequired = _trueMarkRepository.GetCodesRequiredByOrder(uow, OrderId);
				_codesProvidedFromScan = TotalScannedByDriver
					+ TotalScannedByWarehouse
					+ TotalScannedBySelfdelivery;

				OnPropertyChanged(nameof(TotalScannedByDriver));
				OnPropertyChanged(nameof(ScannedByDriverCodes));
				OnPropertyChanged(nameof(TotalScannedByWarehouse));
				OnPropertyChanged(nameof(ScannedByWarehouseCodes));
				OnPropertyChanged(nameof(TotalScannedBySelfdelivery));
				OnPropertyChanged(nameof(ScannedBySelfdeliveryCodes));
				OnPropertyChanged(nameof(TotalAddedFromPool));
				OnPropertyChanged(nameof(AddedFromPoolCodes));

				OnPropertyChanged(nameof(CodesRequired));
				CodesProvidedFromScan = TotalScannedByDriver
					+ TotalScannedByWarehouse
					+ TotalScannedBySelfdelivery;
				CodesProvided = CodesProvidedFromScan + TotalAddedFromPool;
			}
		}

		private void ReloadCodesFromDriver(IUnitOfWork uow)
		{
			var driverCodes = _trueMarkRepository.GetCodesFromDriverByOrder(uow, OrderId);
			_totalScannedByDriver = driverCodes.Count();
			_scannedByDriverCodes = driverCodes.Select(x => new OrderCodeItemViewModel
			{
				SourceIdentificationCode = x.SourceCode?.IdentificationCode,
				ResultIdentificationCode = x.ResultCode?.IdentificationCode,
				ReplacedFromPool = x.SourceCodeStatus == SourceProductCodeStatus.Changed,
				Problem = x.Problem,
				SourceDocumentId = x.RouteListItem.RouteList.Id,
				CodeAuthorId = x.RouteListItem.RouteList.Driver.Id,
				CodeAuthor = x.RouteListItem.RouteList.Driver.FullName
			}).ToList();
		}

		private void ReloadCodesFromWarehouse(IUnitOfWork uow)
		{
			var warehouseCodes = _trueMarkRepository.GetCodesFromWarehouseByOrder(uow, OrderId);
			_totalScannedByWarehouse = warehouseCodes.Count();

			var carLoadDocumentIds = warehouseCodes.Select(x => x.CarLoadDocumentItem.Document.Id).Distinct();
			var carLoadEvents = uow.Session.QueryOver<CompletedDriverWarehouseEventProxy>()
				.WhereRestrictionOn(x => x.DocumentId).IsIn(carLoadDocumentIds.ToArray())
				.List();
			_scannedByWarehouseCodes = warehouseCodes.Select(x => {
				var vm = new OrderCodeItemViewModel
				{
					SourceIdentificationCode = x.SourceCode?.IdentificationCode,
					ResultIdentificationCode = x.ResultCode?.IdentificationCode,
					ReplacedFromPool = x.SourceCodeStatus == SourceProductCodeStatus.Changed,
					Problem = x.Problem,
					SourceDocumentId = x.CarLoadDocumentItem.Document.Id
				};
				var author = carLoadEvents
					.Where(y => y.DocumentId == x.CarLoadDocumentItem.Document.Id)
					.FirstOrDefault()?.Employee;

				vm.CodeAuthorId = author?.Id;
				vm.CodeAuthor = author == null ? "" : author.FullName;
				return vm;
			}).ToList();
		}

		private void ReloadCodesFromSelfdelivery(IUnitOfWork uow)
		{
			var selfdeliveryCodes = _trueMarkRepository.GetCodesFromSelfdeliveryByOrder(uow, OrderId);
			_totalScannedBySelfdelivery = selfdeliveryCodes.Count();
			_scannedBySelfdeliveryCodes = selfdeliveryCodes.Select(x => new OrderCodeItemViewModel
			{
				SourceIdentificationCode = x.SourceCode?.IdentificationCode,
				ResultIdentificationCode = x.ResultCode?.IdentificationCode,
				ReplacedFromPool = x.SourceCodeStatus == SourceProductCodeStatus.Changed,
				Problem = x.Problem,
				SourceDocumentId = x.SelfDeliveryDocumentItem.SelfDeliveryDocument.Id,
				CodeAuthorId = x.SelfDeliveryDocumentItem.SelfDeliveryDocument.Author.Id,
				CodeAuthor = x.SelfDeliveryDocumentItem.SelfDeliveryDocument.Author.FullName
			}).ToList();
		}

		private void ReloadCodesFromPool(IUnitOfWork uow)
		{
			var poolCodes = _trueMarkRepository.GetCodesFromPoolByOrder(uow, OrderId);
			_totalAddedFromPool = poolCodes.Count();
			_addedFromPoolCodes = poolCodes.Select(x => new OrderCodeItemViewModel
			{
				SourceIdentificationCode = x.SourceCode?.IdentificationCode,
				ResultIdentificationCode = x.ResultCode?.IdentificationCode,
				ReplacedFromPool = true,
				Problem = x.Problem
			}).ToList();
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
