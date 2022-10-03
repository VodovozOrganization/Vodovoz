using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.ViewModels.Logistic;
using QS.Project.Filter;
using System;

namespace Vodovoz.ViewModels.Dialogs.Logistic
{
	public class AtWorkFilterViewModel : FilterViewModelBase<AtWorkFilterViewModel>
	{
		private IEnumerable<CarTypeOfUse> _selectedCarTypesOfUse;
		private IEnumerable<CarOwnType> _selectedCarOwnTypes;
		private IEnumerable<AtWorkDriver.DriverStatus> _selectedDriverStatuses;
		private SortAtWorkDriversType _sortType;
		private DateTime _atDate;
		private GenericObservableList<GeographicGroupNode> _geographicGroupNodes;
		private readonly Func<bool> _checkFunc;

		public AtWorkFilterViewModel(IUnitOfWork uow, IGeographicGroupRepository geographicGroupRepository, Func<bool> checkFunc = null)
		{
			_checkFunc = checkFunc;

			_selectedCarTypesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>();
			_selectedCarOwnTypes = EnumHelper.GetValuesList<CarOwnType>();
			_selectedDriverStatuses = EnumHelper.GetValuesList<AtWorkDriver.DriverStatus>();

			var geographicGroups = geographicGroupRepository.GeographicGroupsWithCoordinates(uow, isActiveOnly: true);
			GeographicGroupNodes = new GenericObservableList<GeographicGroupNode>(
				geographicGroups.Select(ggn => new GeographicGroupNode(ggn)).ToList());

			foreach(var geographicGroupNode in GeographicGroupNodes)
			{
				geographicGroupNode.Selected = true;
			}

			GeographicGroupNodes.ElementChanged += (s, a) =>
			{
				if(!CanUpdateFilterField)
				{
					_geographicGroupNodes[a[0]].Selected = !_geographicGroupNodes[a[0]].Selected;
				}
				else
				{
					Update();
				}
			};

			AtDate = DateTime.Today;
		}

		public IEnumerable<CarTypeOfUse> SelectedCarTypesOfUse
		{
			get => _selectedCarTypesOfUse;
			set
			{
				var newValue = CanUpdateFilterField ? value : _selectedCarTypesOfUse;
				if(!UpdateFilterField(ref _selectedCarTypesOfUse, newValue))
				{
					OnPropertyChanged(nameof(SelectedCarTypesOfUse));
				}
			}

		}

		public IEnumerable<CarOwnType> SelectedCarOwnTypes
		{
			get => _selectedCarOwnTypes;
			set
			{
				var newValue = CanUpdateFilterField ? value : _selectedCarOwnTypes;
				if(!UpdateFilterField(ref _selectedCarOwnTypes, newValue))
				{
					OnPropertyChanged(nameof(SelectedCarOwnTypes));
				}
			}
		}

		public IEnumerable<AtWorkDriver.DriverStatus> SelectedDriverStatuses
		{
			get => _selectedDriverStatuses;
			set
			{
				var newValue = CanUpdateFilterField ? value : _selectedDriverStatuses;
				if(!UpdateFilterField(ref _selectedDriverStatuses, newValue))
				{
					OnPropertyChanged(nameof(SelectedDriverStatuses));
				}
			}
		}

		public SortAtWorkDriversType SortType
		{
			get => _sortType;
			set
			{
				var newValue = CanUpdateFilterField ? value : _sortType;
				if(!UpdateFilterField(ref _sortType, newValue))
				{
					OnPropertyChanged(nameof(SortType));
				}
			}
		}

		public DateTime AtDate
		{
			get => _atDate;
			set
			{
				var newValue = CanUpdateFilterField ? value : _atDate;
				if(!UpdateFilterField(ref _atDate, newValue))
				{
					OnPropertyChanged(nameof(AtDate));
				}
			}
		}

		public GenericObservableList<GeographicGroupNode> GeographicGroupNodes 
		{ 
			get => _geographicGroupNodes;
			set
			{
				_geographicGroupNodes = value;
				//var newValue = CanUpdateFilterField ? value : _geographicGroupNodes;
				//if(!UpdateFilterField(ref _geographicGroupNodes, newValue))
				//{
				//	//OnPropertyChanged(nameof(GeographicGroupNodes));
				//}
			}
		}

		private bool CanUpdateFilterField => _checkFunc != null && _checkFunc.Invoke();
	}

	public enum SortAtWorkDriversType
	{
		[Display(Name = "ФИО")]
		ByName,
		[Display(Name = "Принадлежность авто")]
		ByCarOwn
	}
}
