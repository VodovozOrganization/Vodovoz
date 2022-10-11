using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.ViewModels.Employees
{
	public class PremiumTemplateViewModel : EntityTabViewModelBase<PremiumTemplate>
	{
		public PremiumTemplateViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Комментарий для премии";
		}
	}
}
