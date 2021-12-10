using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class RoboAtsCounterpartyNameViewModel : EntityTabViewModelBase<RoboAtsCounterpartyName>
	{
		public RoboAtsCounterpartyNameViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Имена контрагентов RoboATS";
		}
	}
}
