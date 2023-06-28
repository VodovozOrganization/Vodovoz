using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class UndeliveryDetalizationViewModel : EntityTabViewModelBase<UndeliveryDetalization>, IAskSaveOnCloseViewModel
	{
		private UndeliveryObject _selectedUndeliveryObject;
		private IEnumerable<UndeliveryKind> _visibleUndeliveryKinds;

		public UndeliveryDetalizationViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null,
			UndeliveryObject undeliveryObject = null,
			UndeliveryKind undeliveryKind = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = "Детализация недовоза";

			var entityUndeliveryObjectId = Entity.UndeliveryKind?.UndeliveryObject?.Id;
			var restrictedUndeliveryObjectId = undeliveryObject?.Id;

			UndeliveryObjects = UoW.Session.QueryOver<UndeliveryObject>()
				.Where(co => !co.IsArchive
					|| co.Id == entityUndeliveryObjectId
					|| co.Id == restrictedUndeliveryObjectId)
				.List();

			var entityUndeliveryKindId = Entity.UndeliveryKind?.Id;
			var restrictedUndeliveryKindId = undeliveryKind?.Id;

			UndeliveryKinds = UoW.Session.QueryOver<UndeliveryKind>()
				.Where(ck => !ck.IsArchive
					|| ck.Id == entityUndeliveryKindId
					|| ck.Id == restrictedUndeliveryKindId)
				.List();

			VisibleUndeliveryKinds = Enumerable.Empty<UndeliveryKind>();

			SelectedUndeliveryObject = undeliveryObject ?? Entity.UndeliveryKind?.UndeliveryObject;

			if(undeliveryKind != null)
			{
				RestrictComplainKind = Entity.UndeliveryKind = undeliveryKind;
			}

			RestrictUndeliveryObject = undeliveryObject;

			SelectedUndeliveryObject = Entity.UndeliveryKind?.UndeliveryObject;

			Entity.PropertyChanged += EntityPropertyChanged;

			SetPropertyChangeRelation(
				undeliveryDetalization => undeliveryDetalization.Id,
				() => CanEdit);
		}

		[PropertyChangedAlso(nameof(CanChangeUndeliveryKind))]
		public UndeliveryKind RestrictComplainKind { get; }

		[PropertyChangedAlso(nameof(CanChangeUndeliveryObject))]
		public UndeliveryObject RestrictUndeliveryObject { get; }

		public IList<UndeliveryObject> UndeliveryObjects { get; }

		public IList<UndeliveryKind> UndeliveryKinds { get; }

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
						VisibleUndeliveryKinds = UndeliveryKinds
							.Where(ck => ck.UndeliveryObject == value);
					}
				}
			}
		}

		public bool CanChangeUndeliveryKind => CanEdit
			&& RestrictComplainKind is null;

		public bool CanChangeUndeliveryObject => CanEdit
			&& RestrictUndeliveryObject is null;

		public bool AskSaveOnClose => CanEdit;

		private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.UndeliveryKind))
			{
				SelectedUndeliveryObject = Entity.UndeliveryKind?.UndeliveryObject;
			}
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= EntityPropertyChanged;
			base.Dispose();
		}
	}
}
