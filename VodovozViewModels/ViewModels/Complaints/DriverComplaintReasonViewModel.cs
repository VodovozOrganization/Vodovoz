using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.ViewModels.Complaints
{
	public class DriverComplaintReasonViewModel : EntityTabViewModelBase<DriverComplaintReason>
	{
		public DriverComplaintReasonViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null)
					: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
		}
	}
}
