using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteColumnViewModel : EntityTabViewModelBase<RouteColumn>
	{
		public RouteColumnViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices
			) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(!CanRead && !CanCreateOrUpdate)
			{
				AbortOpening("У вас недостаточно прав для просмотра");
			}

			TabName = UoWGeneric.IsNew
				? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}"
				: $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} \"{Entity.Title}\"";
		}

		#region Permissions
		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanDelete => PermissionResult.CanDelete;

		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;
		#endregion
	}
}
