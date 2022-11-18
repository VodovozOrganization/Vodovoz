using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class WageDistrictViewModel : EntityTabViewModelBase<WageDistrict>
	{
		public WageDistrictViewModel(IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
		}
	}
}
