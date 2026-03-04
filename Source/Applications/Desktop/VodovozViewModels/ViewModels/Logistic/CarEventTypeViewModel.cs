using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarEventTypeViewModel : EntityTabViewModelBase<CarEventType>
	{
		public CarEventTypeViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Вид события ТС";
		}
	}
}
