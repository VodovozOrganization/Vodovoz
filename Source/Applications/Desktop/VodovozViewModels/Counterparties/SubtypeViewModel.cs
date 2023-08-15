using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Counterparties
{
	public class SubtypeViewModel : EntityTabViewModelBase<CounterpartySubtype>
	{
		public SubtypeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = typeof(CounterpartySubtype).GetClassUserFriendlyName().Nominative.CapitalizeSentence();

			SaveCommand = new DelegateCommand(SaveAndClose, CanSave);

			CloseCommand = new DelegateCommand(Close);
		}

		public DelegateCommand SaveCommand { get; }

		public DelegateCommand CloseCommand { get; }

		private bool CanSave() => (UoWGeneric.IsNew && PermissionResult.CanCreate) || PermissionResult.CanUpdate;

		private void Close() => Close(true, CloseSource.Self);
	}
}
