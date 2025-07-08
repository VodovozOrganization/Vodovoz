using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class EdoOperatorViewModel : EntityTabViewModelBase<EdoOperator>
	{
		public EdoOperatorViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Оператор ЭДО";
		}
	}
}
