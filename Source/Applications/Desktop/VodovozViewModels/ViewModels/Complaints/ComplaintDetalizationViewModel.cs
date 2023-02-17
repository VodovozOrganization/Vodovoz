using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.ViewModels.Complaints
{
	public class ComplaintDetalizationViewModel : EntityTabViewModelBase<ComplaintDetalization>, IAskSaveOnCloseViewModel
	{
		private ComplaintObject _selectedComplainObject;
		private IEnumerable<ComplaintKind> _visibleComplaintKinds;

		public ComplaintDetalizationViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null,
			ComplaintObject complaintObject = null,
			ComplaintKind complaintKind = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = "Детализация рекламации";

			var entityComplaintObjectId = Entity.ComplaintKind?.ComplaintObject?.Id;
			var restrictedComplaintObjectId = complaintObject?.Id;

			ComplaintObjects = UoW.Session.QueryOver<ComplaintObject>()
				.Where(co => !co.IsArchive
					|| co.Id == entityComplaintObjectId
					|| co.Id == restrictedComplaintObjectId)
				.List();

			var entityComplaintKindId = Entity.ComplaintKind?.Id;
			var restrictedComplaintKindId = complaintKind?.Id;

			ComplaintKinds = UoW.Session.QueryOver<ComplaintKind>()
				.Where(ck => !ck.IsArchive
					|| ck.Id == entityComplaintKindId
					|| ck.Id == restrictedComplaintKindId)
				.List();

			VisibleComplaintKinds = Enumerable.Empty<ComplaintKind>();

			SelectedComplainObject = complaintObject ?? Entity.ComplaintKind?.ComplaintObject;

			if(complaintKind != null)
			{
				RestrictComplainKind = Entity.ComplaintKind = complaintKind;
			}

			RestrictComplaintObject = complaintObject;

			SelectedComplainObject = Entity.ComplaintKind?.ComplaintObject;

			Entity.PropertyChanged += EntityPropertyChanged;

			SetPropertyChangeRelation(
				complaintDetalization => complaintDetalization.Id,
				() => CanEdit);
		}

		[PropertyChangedAlso(nameof(CanChangeComplaintKind))]
		public ComplaintKind RestrictComplainKind { get; }

		[PropertyChangedAlso(nameof(CanChangeComplaintObject))]
		public ComplaintObject RestrictComplaintObject { get; }

		public IList<ComplaintObject> ComplaintObjects { get; }

		public IList<ComplaintKind> ComplaintKinds { get; }

		public bool CanCreate => CommonServices.CurrentPermissionService
			.ValidateEntityPermission(typeof(ComplaintDetalization)).CanCreate;

		public bool CanEdit => CommonServices.CurrentPermissionService
			.ValidateEntityPermission(typeof(ComplaintDetalization)).CanUpdate
			|| (UoW.IsNew && CanCreate);

		public IEnumerable<ComplaintKind> VisibleComplaintKinds
		{
			get => _visibleComplaintKinds;
			private set => SetField(ref _visibleComplaintKinds, value);
		}

		public ComplaintObject SelectedComplainObject
		{
			get => _selectedComplainObject;
			set
			{
				if(SetField(ref _selectedComplainObject, value))
				{
					if(value is null)
					{
						VisibleComplaintKinds = Enumerable.Empty<ComplaintKind>();
					}
					else
					{
						VisibleComplaintKinds = ComplaintKinds
							.Where(ck => ck.ComplaintObject == value);
					}
				}
			}
		}

		public bool CanChangeComplaintKind => CanEdit
			&& RestrictComplainKind is null;

		public bool CanChangeComplaintObject => CanEdit
			&& RestrictComplaintObject is null;

		public bool AskSaveOnClose => CanEdit;

		private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.ComplaintKind))
			{
				SelectedComplainObject = Entity.ComplaintKind?.ComplaintObject;
			}
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= EntityPropertyChanged;
			base.Dispose();
		}
	}
}
