using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.EntityRepositories;
using System;
using Vodovoz.Core.Domain.Contacts;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.ViewModels
{
	public class PhoneTypeViewModel : EntityTabViewModelBase<PhoneType>
	{
		public PhoneTypeViewModel
		(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) 
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(!CanRead)
				AbortOpening("У вас недостаточно прав для просмотра");

			TabName = "Типы телефонов";
		}

		private PhonePurpose phonePurpose;
		public PhonePurpose PhonePurpose {
			get {
				phonePurpose = Entity.PhonePurpose;
				return phonePurpose;
			}
			set {
				if(SetField(ref phonePurpose, value)) {
					Entity.PhonePurpose = value;
				}
			}
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
