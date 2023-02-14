using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.ViewModels.Complaints
{
	public class ComplaintDetalizationViewModel : EntityTabViewModelBase<ComplaintDetalization>
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

			ComplaintObjects = UoW.Session.QueryOver<ComplaintObject>().List();
			ComplaintKinds = UoW.Session.QueryOver<ComplaintKind>().List();
			VisibleComplaintKinds = ComplaintKinds;
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
			&& RestrictComplainKind is null
			&& !(Entity.ComplaintKind?.IsArchive ?? false);

		public bool CanChangeComplaintObject => CanEdit
			&& RestrictComplaintObject is null
			&& !(SelectedComplainObject?.IsArchive ?? false)
			&& !(Entity.ComplaintKind?.IsArchive ?? false);

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
