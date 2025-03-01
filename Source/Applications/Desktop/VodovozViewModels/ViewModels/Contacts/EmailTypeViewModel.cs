using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;

namespace Vodovoz.ViewModels
{
	public class EmailTypeViewModel : EntityTabViewModelBase<EmailType>
	{
		public EmailTypeViewModel
		(
			IEmailRepository emailRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices
		) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(!CanRead)
				AbortOpening("У вас недостаточно прав для просмотра");

			this.emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			TabName = "Типы e-mail адресов";
		}

		IEmailRepository emailRepository;

		private EmailPurpose emailPurpose;
		public EmailPurpose EmailPurpose {
			get {
				emailPurpose = Entity.EmailPurpose;
				return emailPurpose;
			}
			set {
				if(SetField(ref emailPurpose, value)) {
					Entity.EmailPurpose = value;
				}
			}
		}

		public override bool Save(bool close)
		{
			ValidationContext = Entity.ConfigureValidationContext(UoW, emailRepository);
			return base.Save(close);
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
