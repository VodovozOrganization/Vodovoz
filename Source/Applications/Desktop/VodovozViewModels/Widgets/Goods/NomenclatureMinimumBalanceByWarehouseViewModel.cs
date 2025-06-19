using Autofac;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.Widgets.Goods
{
	public class NomenclatureMinimumBalanceByWarehouseViewModel : WidgetViewModelBase, IDisposable
	{
		private IUnitOfWork _unitOfWork;
		private Nomenclature _nomenclature;
		private DialogViewModelBase _parrentDlg;
		private bool _isNewBalance;
		private IList<NomenclatureMinimumBalanceByWarehouse> _nomenclatureMinimumBalancesByWarehouse;
		private NomenclatureMinimumBalanceByWarehouse _selectedNomenclatureMinimumBalanceByWarehouse;
		private NomenclatureMinimumBalanceByWarehouse _currentNomenclatureMinimumBalanceByWarehouse;
		private bool _unlockMainBox;
		private bool _showEditBox;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _lifetimeScope;

		public NomenclatureMinimumBalanceByWarehouseViewModel(INavigationManager navigationManager, ILifetimeScope lifetimeScope)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			CreateCommands();

			UnlockMainBox = true;
		}

		#region команды
		private void CreateCommands()
		{
			AddCommand = new DelegateCommand(Add);
			SaveCommand = new DelegateCommand(Save);
			CancelCommand = new DelegateCommand(Cancel);
			DeleteCommand = new DelegateCommand(Delete, () => HasSelectedBalance);
			EditCommand = new DelegateCommand(Edit, () => HasSelectedBalance);
		}

		private void Delete()
		{
			_nomenclature.NomenclatureMinimumBalancesByWarehouse.Remove(SelectedNomenclatureMinimumBalanceByWarehouse);
		}

		private void Save()
		{
			if(CurrentWarehouse is null)
			{
				return;
			}

			if(_isNewBalance)
			{
				_nomenclature.NomenclatureMinimumBalancesByWarehouse.Add(CurrentNomenclatureMinimumBalanceByWarehouse);
			}
			else
			{
				SelectedNomenclatureMinimumBalanceByWarehouse.Warehouse = CurrentNomenclatureMinimumBalanceByWarehouse.Warehouse;
				SelectedNomenclatureMinimumBalanceByWarehouse.MinimumBalance = CurrentNomenclatureMinimumBalanceByWarehouse.MinimumBalance;
			}

			ShowEditBox = false;
			UnlockMainBox = true;
		}

		private void Cancel()
		{
			ShowEditBox = false;
			UnlockMainBox = true;
		}

		private void Add()
		{
			_isNewBalance = true;

			UnlockMainBox = false;
			ShowEditBox = true;

			CurrentNomenclatureMinimumBalanceByWarehouse = new NomenclatureMinimumBalanceByWarehouse
			{
				Nomenclature = _nomenclature
			};
		}

		private void Edit()
		{
			_isNewBalance = false;

			CurrentNomenclatureMinimumBalanceByWarehouse =
				new NomenclatureMinimumBalanceByWarehouse
				{
					MinimumBalance = SelectedNomenclatureMinimumBalanceByWarehouse.MinimumBalance,
					Nomenclature = SelectedNomenclatureMinimumBalanceByWarehouse.Nomenclature,
					Warehouse = SelectedNomenclatureMinimumBalanceByWarehouse.Warehouse
				};

			UnlockMainBox = false;
			ShowEditBox = true;
		}

		#endregion

		private EntityEntryViewModel<Warehouse> CreateWarehouseEntryViewModel()
		{
			return new CommonEEVMBuilderFactory<NomenclatureMinimumBalanceByWarehouseViewModel>(_parrentDlg, this, _unitOfWork, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.CurrentWarehouse)
				.UseViewModelDialog<WarehouseViewModel>()
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.Finish();
		}

		public void Initialize(DialogViewModelBase dialog, Nomenclature nomenclature, IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
			_nomenclature = nomenclature;
			_parrentDlg = dialog;

			WarehouseEntryViewModel = CreateWarehouseEntryViewModel();
		}

		public DelegateCommand AddCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }
		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand DeleteCommand { get; private set; }
		public DelegateCommand EditCommand { get; private set; }

		public EntityEntryViewModel<Warehouse> WarehouseEntryViewModel { get; private set; }

		public bool UnlockMainBox
		{
			get => _unlockMainBox;
			private set => SetField(ref _unlockMainBox, value);
		}

		public bool ShowEditBox
		{
			get => _showEditBox;
			private set => SetField(ref _showEditBox, value);
		}

		[PropertyChangedAlso(nameof(CurrentWarehouse), nameof(CurrentMinimumBalance))]
		public NomenclatureMinimumBalanceByWarehouse CurrentNomenclatureMinimumBalanceByWarehouse
		{
			get => _currentNomenclatureMinimumBalanceByWarehouse;
			set => SetField(ref _currentNomenclatureMinimumBalanceByWarehouse, value);
		}

		public NomenclatureMinimumBalanceByWarehouse SelectedNomenclatureMinimumBalanceByWarehouse
		{
			get => _selectedNomenclatureMinimumBalanceByWarehouse;
			set
			{
				if(SetField(ref _selectedNomenclatureMinimumBalanceByWarehouse, value))
				{
					EditCommand.RaiseCanExecuteChanged();
					DeleteCommand.RaiseCanExecuteChanged();
				}
			}
		}

		public int CurrentMinimumBalance
		{
			get
			{
				return CurrentNomenclatureMinimumBalanceByWarehouse?.MinimumBalance ?? 0;
			}
			private set
			{
				if(CurrentNomenclatureMinimumBalanceByWarehouse != null)
				{
					CurrentNomenclatureMinimumBalanceByWarehouse.MinimumBalance = value;
				}
			}
		}

		public Warehouse CurrentWarehouse
		{
			get
			{
				return CurrentNomenclatureMinimumBalanceByWarehouse?.Warehouse;
			}
			private set
			{
				if(CurrentNomenclatureMinimumBalanceByWarehouse != null)
				{
					CurrentNomenclatureMinimumBalanceByWarehouse.Warehouse = value;
				}
			}
		}

		public IList<NomenclatureMinimumBalanceByWarehouse> Balances => _nomenclature.NomenclatureMinimumBalancesByWarehouse;

		public bool HasSelectedBalance => SelectedNomenclatureMinimumBalanceByWarehouse != null;

		public void Dispose()
		{
			_parrentDlg = null;
		}
	}
}
