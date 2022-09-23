using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Dialogs.Complaints
{
	public class ResponsibleViewModel : EntityTabViewModelBase<Responsible>
	{
		private bool _canUpdate;
		private bool _canCreate;

		public ResponsibleViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{

			TabName = "Ответственный";

			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Responsible));
			var isSpecialResponsible = Entity.IsEmployeeResponsible || Entity.IsSubdivisionResponsible;
			_canUpdate = permissionResult.CanUpdate && !isSpecialResponsible;
			_canCreate = permissionResult.CanCreate;
		}

		public bool CanEdit => _canUpdate || (_canCreate && UoW.IsNew);

	}
}
