using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.CarModelSelection
{
	public class CarModelSelectionFilterViewModel : WidgetViewModelBase
	{
		private readonly List<CarModel> _carModels;
		private readonly IUnitOfWork _unitOfWork;

		private string _searchString;
		private bool _isShowArchiveCarModels;
		private IEnumerable<CarTypeOfUse> _selectedCarTypesOfUse;
		public GenericObservableList<CarModelSelectableNode> _carModelRows;

		private DelegateCommand _selectAllRowsCommand;
		private DelegateCommand _removeAllRowsSelectionCommand;
		private DelegateCommand _inverseSelectionCommand;

		public event Action<object, EventArgs> FilterSelectionChanged;

		public CarModelSelectionFilterViewModel(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

			_carModels = GetCarModels();

			_carModelRows = new GenericObservableList<CarModelSelectableNode>();
			_carModelRows.ElementChanged += OnCarModelRowsElementChanged;
			_carModelRows.ListContentChanged += _carModelRows_ListContentChanged;
			_carModelRows.PropertyChanged += _carModelRows_PropertyChanged;
			_carModelRows.PropertyOfElementChanged += _carModelRows_PropertyOfElementChanged;

			UpdateCarModelsRows();
		}

		private void _carModelRows_PropertyOfElementChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void _carModelRows_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			throw new NotImplementedException();
		}
		 
		private void _carModelRows_ListContentChanged(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		#region Properties

		public string SearchString
		{
			get => _searchString;
			set => SetField(ref _searchString, value);
		}

		public IEnumerable<CarTypeOfUse> SelectedCarTypesOfUse
		{
			get => _selectedCarTypesOfUse ?? new List<CarTypeOfUse>();
			set
			{
				SetField(ref _selectedCarTypesOfUse, value);
				UpdateCarModelsRows();
			}
		}

		public GenericObservableList<CarModelSelectableNode> CarModelRows
		{
			get
			{
				if(_carModelRows == null)
				{
					_carModelRows = new GenericObservableList<CarModelSelectableNode>();
				}

				return _carModelRows;
			}
		}

		public bool IsShowArchiveCarModels
		{
			get => _isShowArchiveCarModels;
			set => SetField(ref _isShowArchiveCarModels, value);
		}

		public string SelectedRowsCountInfo =>
			$"Выбрано: {IncludedCarModelRowsCount}\nИсключено: {ExcludedCarModelRowsCount}";

		#endregion Properties

		private List<CarModel> GetCarModels()
		{
			var carModels = _unitOfWork.GetAll<CarModel>().ToList();

			return carModels;
		}

		public void UpdateCarModelsRows()
		{
			List<CarModelSelectableNode> carModelRows = new List<CarModelSelectableNode>();

			foreach(var carModel in _carModels)
			{
				if(SelectedCarTypesOfUse.Contains(carModel.CarTypeOfUse))
				{
					var carModelRow = _carModelRows
						.Where(r => r.ModelId == carModel.Id)
						.FirstOrDefault();

					if(carModelRow == null)
					{
						carModelRow = CarModelSelectableNode.CreateCarModelRow(carModel);
					}

					carModelRows.Add(carModelRow);
				}
			}

			_carModelRows.Clear();

			_carModelRows = new GenericObservableList<CarModelSelectableNode>(carModelRows);

			UpdateCarModelsRowsVisibility();
		}

		private void UpdateCarModelsRowsVisibility()
		{
			foreach(var carModelRow in _carModelRows)
			{
				carModelRow.IsVisible = 
					(string.IsNullOrWhiteSpace(SearchString)
					|| carModelRow.ModelInfo.Contains(SearchString))
					&& (IsShowArchiveCarModels || !carModelRow.IsArchive);
			}
		}

		public int IncludedCarModelRowsCount =>
			_carModelRows
				.Where(r => r.IsIncluded)
				.Count();

		public int ExcludedCarModelRowsCount =>
			_carModelRows
				.Where(r => r.IsIncluded)
				.Count();

		private void OnCarModelRowsElementChanged(object aList, int[] aIdx)
		{
			OnPropertyChanged(nameof(SelectedRowsCountInfo));

			FilterSelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		#region Commands

		#region SelectAllRowsCommand
		public DelegateCommand SelectAllRowsCommand
		{
			get
			{
				if(_selectAllRowsCommand == null)
				{
					_selectAllRowsCommand = new DelegateCommand(SelectAllRows, () => CanSelectAllRows);
					_selectAllRowsCommand.CanExecuteChangedWith(this, x => x.CanSelectAllRows);
				}
				return _selectAllRowsCommand;
			}
		}

		public bool CanSelectAllRows => true;

		private void SelectAllRows()
		{
			foreach(var carModelRow in _carModelRows)
			{
				if(carModelRow.IsVisible)
				{
					carModelRow.IsIncluded = true;
				}
			}
		}
		#endregion SelectAllRowsCommand

		#region RemoveAllRowsSelectionCommand
		public DelegateCommand RemoveAllRowsSelectionCommand
		{
			get
			{
				if(_removeAllRowsSelectionCommand == null)
				{
					_removeAllRowsSelectionCommand = new DelegateCommand(RemoveAllRowsSelection, () => CanRemoveAllRowsSelection);
					_removeAllRowsSelectionCommand.CanExecuteChangedWith(this, x => x.CanRemoveAllRowsSelection);
				}
				return _removeAllRowsSelectionCommand;
			}
		}

		public bool CanRemoveAllRowsSelection => true;

		private void RemoveAllRowsSelection()
		{
			foreach(var carModelRow in _carModelRows)
			{
				if(carModelRow.IsVisible)
				{
					carModelRow.IsIncluded = false;
				}
			}
		}
		#endregion RemoveAllRowsSelectionCommand

		#region InverseSelectionCommand
		public DelegateCommand InverseSelectionCommand
		{
			get
			{
				if(_inverseSelectionCommand == null)
				{
					_inverseSelectionCommand = new DelegateCommand(InverseSelection, () => CanInverseSelection);
					_inverseSelectionCommand.CanExecuteChangedWith(this, x => x.CanInverseSelection);
				}
				return _inverseSelectionCommand;
			}
		}

		public bool CanInverseSelection => true;

		private void InverseSelection()
		{
			foreach(var carModelRow in _carModelRows)
			{
				if(carModelRow.IsVisible)
				{
					carModelRow.IsIncluded = !carModelRow.IsIncluded;
				}
			}
		}
		#endregion InverseSelectionCommand

		#endregion Commands

		public class CarModelSelectableNode : PropertyChangedBase
		{
			private int _modelId;
			private string _modelInfo;
			private bool _isArchive;
			private bool _isIncluded;
			private bool _isExcluded;
			private bool _isVisible;

			public int ModelId
			{
				get => _modelId;
				set => SetField(ref _modelId, value);
			}

			public string ModelInfo
			{
				get => _modelInfo;
				set => SetField(ref _modelInfo, value);
			}

			public bool IsArchive
			{
				get => _isArchive;
				set => SetField(ref _isArchive, value);
			}

			public bool IsIncluded
			{
				get => _isIncluded;
				set
				{
					if(_isIncluded != value)
					{
						SetField(ref _isIncluded, value);

						if(_isIncluded)
						{
							IsExcluded = false;
						}
					}
				}
			}

			public bool IsExcluded
			{
				get => _isExcluded;
				set
				{
					if(_isExcluded != value)
					{
						SetField(ref _isExcluded, value);

						if(_isExcluded)
						{
							IsIncluded = false;
						}
					}
				}
			}

			public bool IsVisible
			{
				get => _isVisible;
				set => SetField(ref _isVisible, value);
			}

			public static CarModelSelectableNode CreateCarModelRow(CarModel carModel)
			{
				var carModelRow = new CarModelSelectableNode
				{
					ModelId = carModel.Id,
					ModelInfo = $"{carModel.CarManufacturer.Name} {carModel.Name}",
					IsArchive = carModel.IsArchive,
					IsIncluded = false,
					IsExcluded = false,
					IsVisible = true
				};

				return carModelRow;
			}
		}
	}
}
