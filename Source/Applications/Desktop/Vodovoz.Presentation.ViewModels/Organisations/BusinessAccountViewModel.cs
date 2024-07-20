using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Presentation.ViewModels.Organisations
{
	public class BusinessAccountViewModel : EntityDialogViewModelBase<BusinessAccount>
	{
		public BusinessAccountViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}
	}
}
