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
				Entity.ComplaintKind = complaintKind;
			}

			SelectedComplainObject = Entity.ComplaintKind?.ComplaintObject;

			CanChangeComplaintObject = CanEdit
				&& complaintObject is null
				&& !(SelectedComplainObject?.IsArchive ?? false)
				&& !(Entity.ComplaintKind?.IsArchive ?? false);
			CanChangeComplaintKind = CanEdit
				&& complaintKind is null
				&& !(Entity.ComplaintKind?.IsArchive ?? false);

			Entity.PropertyChanged += EntityPropertyChanged;
		}

		public IList<ComplaintObject> ComplaintObjects { get; }

		public IList<ComplaintKind> ComplaintKinds { get; }

		public bool CanEdit => CommonServices.CurrentPermissionService
			.ValidateEntityPermission(typeof(ComplaintDetalization)).CanUpdate;

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

		public bool CanChangeComplaintKind { get; }

		public bool CanChangeComplaintObject { get; }

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
