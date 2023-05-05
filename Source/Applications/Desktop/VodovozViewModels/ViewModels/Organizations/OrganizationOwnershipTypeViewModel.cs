using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Organizations;
using QS.DomainModel.UoW;
using Vodovoz.Factories;
using System.ComponentModel.DataAnnotations;
using System;
using Vodovoz.EntityRepositories.Organizations;

namespace Vodovoz.ViewModels.ViewModels.Organizations
{
	public class OrganizationOwnershipTypeViewModel : EntityTabViewModelBase<OrganizationOwnershipType>
	{
		private ValidationContext _validationContext;
		private IOrganizationRepository _organizationRepository;

		public OrganizationOwnershipTypeViewModel(
			IEntityUoWBuilder uoWBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			IValidationContextFactory validationContextFactory,
			IOrganizationRepository organizationRepository) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Тип формы собственности";

			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));

			if(validationContextFactory == null)
			{
				throw new ArgumentNullException(nameof(validationContextFactory));
			}

			ConfigureValidationContext(validationContextFactory);
		}

		public override bool Save(bool close)
		{
			ValidationContext = _validationContext;
			return base.Save(close);
		}

		private void ConfigureValidationContext(IValidationContextFactory validationContextFactory)
		{
			_validationContext = validationContextFactory.CreateNewValidationContext(Entity);

			_validationContext.ServiceContainer.AddService(typeof(IUnitOfWork), UoW);
			_validationContext.ServiceContainer.AddService(typeof(IOrganizationRepository), _organizationRepository);
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
