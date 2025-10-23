using Autofac;
using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.Settings.Common;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentBase : PropertyChangedBase
	{
		DateTime? createDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate {
			get => createDate;
			set => SetField(ref createDate, value);
		}

		Employee author;
		[Display(Name = "Создатель заказа")]
		public virtual Employee Author {
			get => author;
			set => SetField(ref author, value);
		}

		Counterparty client;
		[Display(Name = "Клиент")]
		public virtual Counterparty Client {
			get => client;
			set 
			{
				if(value == client)
					return;

				SetField(ref client, value);
			}
		}

		bool isBillWithoutShipmentSent;
		[Display(Name = "Счет отправлен")]
		public virtual bool IsBillWithoutShipmentSent {
			get => isBillWithoutShipmentSent;
			set => SetField(ref isBillWithoutShipmentSent, value);
		}
		
		private Organization _organization;
		[Display(Name = "Организация в счете")]
		public virtual Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		public virtual Email GetEmailAddressForBill()
		{
			return Client?.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
		}

		public virtual bool HasPermissionsForAlternativePrice
		{
			get
			{
				var generalSettingsParameters = ScopeProvider.Scope.Resolve<IGeneralSettings>();
				return generalSettingsParameters.SubdivisionsForAlternativePrices.Contains(Author.Subdivision.Id);
			}
		}
	}
}
