using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintSourceViewModel : EntityTabViewModelBase<ComplaintSource>
	{
		public ComplaintSourceViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Источники рекламаций";
		}
	}
}
