using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class RoboAtsCounterpartyPatronymicViewModel : EntityTabViewModelBase<RoboAtsCounterpartyPatronymic>
	{
		public RoboAtsCounterpartyPatronymicViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Отчества контрагентов RoboATS";
		}
	}
}
