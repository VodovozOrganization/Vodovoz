using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System.Collections.Generic;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.ViewModels.Complaints
{
	public class ComplaintDetalizationViewModel : EntityTabViewModelBase<ComplaintDetalization>
	{
		public ComplaintDetalizationViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = "Детализации рекламаций";

			ComplaintObjects = UoW.Session.QueryOver<ComplaintObject>().List();
			ComplaintKinds = UoW.Session.QueryOver<ComplaintObject>().List();
		}
		
		public IList<ComplaintObject> ComplaintObjects { get; }
		
		public IList<ComplaintObject> ComplaintKinds { get; }
	}
}
