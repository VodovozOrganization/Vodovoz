using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public class LateArrivalReasonViewModel : EntityTabViewModelBase<LateArrivalReason>
	{
		public LateArrivalReasonViewModel(IEntityUoWBuilder uowBuilder,
										IUnitOfWorkFactory unitOfWorkFactory,
										ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(uowBuilder.IsNewEntity)
				TabName = "Создание новой причины опоздания водителей";
			else
				TabName = $"{Entity.Title}";
		}
	}
}
