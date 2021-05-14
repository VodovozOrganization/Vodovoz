using System;
using System.Collections.Generic;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Organizations;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
    public class OrganisationCashTransferDocumentViewModel : EntityTabViewModelBase<OrganisationCashTransferDocument>
    {
        public IEnumerable<Organization> Organizations { get; }
        public bool CanEdit { get; }
        public bool CanEditRectroactively { get; }
        private readonly Employee Author;

        public OrganisationCashTransferDocumentViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, IEntityExtendedPermissionValidator entityExtendedPermissionValidator)
            : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            Organizations = UoW.GetAll<Organization>();
            Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);

            CanEditRectroactively = entityExtendedPermissionValidator.Validate(typeof(OrganisationCashTransferDocument), CommonServices.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
            CanEdit = (UoW.IsNew && PermissionResult.CanCreate) ||
                      (PermissionResult.CanUpdate && Entity.DocumentDate.Date == DateTime.Now.Date) ||
                      CanEditRectroactively;

            if (UoW.IsNew)
            {
                Entity.DocumentDate = DateTime.Now;
                Entity.Author = Author;
            }
        }

        public override bool Save(bool close)
        {
            if (!CanEdit || !Validate())
                return false;

            var operationFrom = Entity.OrganisationCashMovementOperationFrom = Entity.OrganisationCashMovementOperationFrom ?? new OrganisationCashMovementOperation { OperationTime = DateTime.Now };
            var operationTo = Entity.OrganisationCashMovementOperationTo = Entity.OrganisationCashMovementOperationTo ?? new OrganisationCashMovementOperation { OperationTime = DateTime.Now };

            operationFrom.Organisation = Entity.OrganizationFrom;
            operationFrom.Amount = -Entity.TransferedSum;

            operationTo.Organisation = Entity.OrganizationTo;
            operationTo.Amount = Entity.TransferedSum;

            UoW.Save(operationFrom);
            UoW.Save(operationTo);

            return base.Save(close);
        }
    }
}
