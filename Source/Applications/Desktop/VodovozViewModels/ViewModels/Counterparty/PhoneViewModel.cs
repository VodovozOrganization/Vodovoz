using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class PhoneViewModel : EntityTabViewModelBase<Phone>
	{
		public PhoneViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Телефон";
		}
	}
}
