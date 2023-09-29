using QS.Commands;
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
		private string _selectedRowsCountInfo;
		private bool _isShowArchiveCarModels;
		private IEnumerable<CarTypeOfUse> _selectedCarTypesOfUse;
		private GenericObservableList<CarModelRow> _carModelRows;

		private DelegateCommand _selectAllRowsCommand;
		private DelegateCommand _removeAllRowsSelectionCommand;
		private DelegateCommand _inverseSelectionCommand;

		public event Action<object, EventArgs> FilterSelectionChanged;

		public CarModelSelectionFilterViewModel(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

			_carModels = GetCarModels();
			_carModelRows.ListContentChanged += OnCarModelRowsListContentChanged;

			UpdateCarModelsRows();
		}

		#region Properties

		public string SearchString
		{
			get => _searchString;
			set => SetField(ref _searchString, value);
		}

		public IEnumerable<CarTypeOfUse> SelectedCarTypesOfUse
		{
			get => _selectedCarTypesOfUse;
			set => SetField(ref _selectedCarTypesOfUse, value);
		}

		public GenericObservableList<CarModelRow> CarModelRows
		{
			get
			{
				if(_carModelRows == null)
				{
					_carModelRows = new GenericObservableList<CarModelRow>();
				}

				return _carModelRows;
			}
		}

		public bool IsShowArchiveCarModels
		{
			get => _isShowArchiveCarModels;
			set => SetField(ref _isShowArchiveCarModels, value);
		}

		public string SelectedRowsCountInfo
		{
			get => _selectedRowsCountInfo;
			set => SetField(ref _selectedRowsCountInfo, value);
		}

		#endregion Properties

		private List<CarModel> GetCarModels()
		{
			var carModels = _unitOfWork.GetAll<CarModel>().ToList();

			return carModels;
		}

		public void UpdateCarModelsRows()
		{
			List<CarModelRow> carModelRows = new List<CarModelRow>();

			foreach(var carModel in _carModels)
			{
				if(SelectedCarTypesOfUse.Contains(carModel.CarTypeOfUse))
				{
					var carModelRow = _carModelRows
						.Where(r => r.ModelId == carModel.Id)
						.FirstOrDefault();

					if(carModelRow != null)
					{
						carModelRows.Add(CarModelRow.CreateCarModelRow(carModel));
					}

					carModelRows.Add(carModelRow);
				}
			}

			_carModelRows.Clear();

			_carModelRows = new GenericObservableList<CarModelRow>(carModelRows);

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

		public int SelectedCarModelRowsCount =>
			_carModelRows
				.Where(r => r.IsIncluded)
				.Count();

		private void OnCarModelRowsListContentChanged(object sender, EventArgs e)
		{
			SelectedRowsCountInfo = $"Выбрано моделей: {SelectedCarModelRowsCount}";

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

		public class CarModelRow
		{
			public int ModelId { get; set; }
			public string ModelInfo { get; set; }
			public bool IsArchive { get; set; }
			public bool IsIncluded { get; set; }
			public bool IsExcluded { get; set; }
			public bool IsVisible { get; set; }

			public static CarModelRow CreateCarModelRow(CarModel carModel)
			{
				var carModelRow = new CarModelRow
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
