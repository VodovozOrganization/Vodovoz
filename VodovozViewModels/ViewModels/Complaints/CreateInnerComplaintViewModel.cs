using System;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class CreateInnerComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		public CreateInnerComplaintViewModel(
			IEntityConstructorParam ctorParam,
			ICommonServices commonServices
			) : base(ctorParam, commonServices)
		{
		}

		public bool CanEdit => PermissionResult.CanUpdate;
	}
}
