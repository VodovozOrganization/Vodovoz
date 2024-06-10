using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Logistic.MileagesWriteOff
{
	public class MileageWriteOffReasonViewModel : EntityTabViewModelBase<MileageWriteOffReason>
	{
		public MileageWriteOffReasonViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(!CanRead)
			{
				AbortOpening("У вас недостаточно прав для просмотра");
			}

			TabName =
				UoWGeneric.IsNew
				? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}"
				: $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} \"{Entity.Title}\"";

			SaveCommand = new DelegateCommand(() => Save(true), () => CanCreateOrUpdate);
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		#region Permissions

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanDelete => PermissionResult.CanDelete;

		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;

		#endregion
	}
}
