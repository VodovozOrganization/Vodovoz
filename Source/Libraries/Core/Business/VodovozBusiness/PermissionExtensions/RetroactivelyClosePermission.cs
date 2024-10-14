using System;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Documents;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.PermissionExtensions
{
	public class RetroactivelyClosePermission : IPermissionExtension
	{
		public string PermissionId { get => nameof(RetroactivelyClosePermission); }
		public string Name { get => "Изменение документа задним числом"; }
		public string Description { get => "Возможность изменять документы задним числом"; }

		public RetroactivelyClosePermission() { }

		public bool IsValidType(Type typeOfEntity)
		{
			if(typeOfEntity == null)
			{
				return false;
			}

			return typeOfEntity.IsSubclassOf(typeof(Document))
				|| typeOfEntity == typeof(PaymentWriteOff)
				|| typeOfEntity == typeof(Income)
				|| typeOfEntity == typeof(Expense)
				|| typeOfEntity == typeof(AdvanceReport)
				|| typeOfEntity == typeof(OrganizationCashTransferDocument);
		}
	}
}
