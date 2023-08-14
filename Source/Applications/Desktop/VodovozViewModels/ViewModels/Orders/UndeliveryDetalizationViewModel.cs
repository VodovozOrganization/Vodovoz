using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class UndeliveryDetalizationViewModel : EntityTabViewModelBase<UndeliveryDetalization>, IAskSaveOnCloseViewModel
	{
		private UndeliveryObject _selectedUndeliveryObject;
		private IEnumerable<UndeliveryKind> _visibleUndeliveryKinds;
		private readonly IList<UndeliveryKind> _allUndeliveryKinds;

		public UndeliveryDetalizationViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = "Детализация недовоза";

			_allUndeliveryKinds = UoW.Session.QueryOver<UndeliveryKind>()
				.Where(uk => !uk.IsArchive)
				.List();

			UndeliveryObjects = UoW.Session.QueryOver<UndeliveryObject>()
				.Where(uo => !uo.IsArchive)
				.List();

			VisibleUndeliveryKinds = Enumerable.Empty<UndeliveryKind>();

			SelectedUndeliveryObject = Entity.UndeliveryKind?.UndeliveryObject;
		}


		public IList<UndeliveryObject> UndeliveryObjects { get; }

		public bool CanCreate => CommonServices.CurrentPermissionService
			.ValidateEntityPermission(typeof(UndeliveryDetalization)).CanCreate;

		public bool CanEdit => CommonServices.CurrentPermissionService
			.ValidateEntityPermission(typeof(UndeliveryDetalization)).CanUpdate
			|| (UoW.IsNew && CanCreate);

		public IEnumerable<UndeliveryKind> VisibleUndeliveryKinds
		{
			get => _visibleUndeliveryKinds;
			private set => SetField(ref _visibleUndeliveryKinds, value);
		}

		public UndeliveryObject SelectedUndeliveryObject
		{
			get => _selectedUndeliveryObject;
			set
			{
				if(SetField(ref _selectedUndeliveryObject, value))
				{
					if(value is null)
					{
						VisibleUndeliveryKinds = Enumerable.Empty<UndeliveryKind>();
					}
					else
					{
						VisibleUndeliveryKinds = _allUndeliveryKinds
							.Where(uk => uk.UndeliveryObject == value);
					}
				}
			}
		}

		public bool CanChangeUndeliveryKind => CanEdit;

		public bool CanChangeUndeliveryObject => CanEdit;

		public bool AskSaveOnClose => CanEdit;
	}
}
