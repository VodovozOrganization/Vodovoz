using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class TariffZoneViewModel : EntityTabViewModelBase<TariffZone>
	{
		public TariffZoneViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Тарифная зона";
		}
	}
}
