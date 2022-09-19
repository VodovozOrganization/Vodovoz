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

namespace Vodovoz.ViewModels.Dialogs.Logistic
{
	public class AtWorkFilterViewModel : PropertyChangedBase
	{
		private IEnumerable<CarTypeOfUse> _selectedCarTypesOfUse;
		private IEnumerable<CarOwnType> _selectedCarOwnTypes;
		private IEnumerable<AtWorkDriver.DriverStatus> _selectedDriverStatuses;
		private SortAtWorkDriversType _sortType;

		public AtWorkFilterViewModel(IUnitOfWork uow, IGeographicGroupRepository geographicGroupRepository)
		{
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
				OnPropertyChanged(nameof(GeographicGroupNodes));
			};
		}

		public IEnumerable<CarTypeOfUse> SelectedCarTypesOfUse
		{
			get => _selectedCarTypesOfUse;
			set => SetField(ref _selectedCarTypesOfUse, value);
		}

		public IEnumerable<CarOwnType> SelectedCarOwnTypes
		{
			get => _selectedCarOwnTypes;
			set => SetField(ref _selectedCarOwnTypes, value);
		}

		public IEnumerable<AtWorkDriver.DriverStatus> SelectedDriverStatuses
		{
			get => _selectedDriverStatuses;
			set => SetField(ref _selectedDriverStatuses, value);
		}

		public SortAtWorkDriversType SortType
		{
			get => _sortType;
			set => SetField(ref _sortType, value);
		}

		public GenericObservableList<GeographicGroupNode> GeographicGroupNodes { get; set; }
	}

	public enum SortAtWorkDriversType
	{
		[Display(Name = "ФИО")]
		ByName,
		[Display(Name = "Принадлежность авто")]
		ByCarOwn
	}
}
