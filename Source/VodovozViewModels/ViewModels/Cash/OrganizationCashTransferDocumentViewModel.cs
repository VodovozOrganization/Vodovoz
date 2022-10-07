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
using Vodovoz.Infrastructure.Services;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
    public class OrganizationCashTransferDocumentViewModel : EntityTabViewModelBase<OrganizationCashTransferDocument>
    {
        public IEnumerable<Organization> Organizations { get; }
        public bool CanEdit { get; }
        public bool CanEditRectroactively { get; }
        private readonly Employee author;

        public OrganizationCashTransferDocumentViewModel(
	        IEntityUoWBuilder uowBuilder,
	        IUnitOfWorkFactory unitOfWorkFactory,
	        ICommonServices commonServices,
	        IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
	        IEmployeeService employeeService)
            : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
	        if(employeeService == null)
	        {
		        throw new ArgumentNullException(nameof(employeeService));
	        }

            Organizations = UoW.GetAll<Organization>();
            author = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);

            CanEditRectroactively = entityExtendedPermissionValidator.Validate(typeof(OrganizationCashTransferDocument), CommonServices.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
            CanEdit = (Entity.Id == 0 && PermissionResult.CanCreate) ||
                      (PermissionResult.CanUpdate && Entity.DocumentDate.Date == DateTime.Now.Date) ||
                      CanEditRectroactively;

            if (Entity.Id == 0)
            {
                Entity.DocumentDate = DateTime.Now;
                Entity.Author = author;
            }
        }
        protected override bool BeforeSave()
        {
	        if (!HasChanges)
		        return base.BeforeSave();

			var operationFrom = Entity.OrganisationCashMovementOperationFrom = Entity.OrganisationCashMovementOperationFrom ?? new OrganisationCashMovementOperation { OperationTime = DateTime.Now };
            var operationTo = Entity.OrganisationCashMovementOperationTo = Entity.OrganisationCashMovementOperationTo ?? new OrganisationCashMovementOperation { OperationTime = DateTime.Now };

            operationFrom.Organisation = Entity.OrganizationFrom;
            operationFrom.Amount = -Entity.TransferedSum;
			operationFrom.OperationTime = Entity.DocumentDate;

            operationTo.Organisation = Entity.OrganizationTo;
            operationTo.Amount = Entity.TransferedSum;
			operationTo.OperationTime = Entity.DocumentDate;

			UoW.Save(operationFrom);
            UoW.Save(operationTo);

			return base.BeforeSave();
        }

        public override bool Save(bool close)
        {
            if (!CanEdit)
                return false;

            return base.Save(close);
        }
    }
}
