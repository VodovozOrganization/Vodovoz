using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Parameters;

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

		public virtual Email GetEmailAddressForBill()
		{
			return Client?.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
		}

		public virtual bool HasPermissionsForAlternativePrice
		{
			get
			{
				var generalSettingsParameters = new GeneralSettingsParametersProvider(new ParametersProvider());
				return generalSettingsParameters.SubdivisionsForAlternativePrices.Contains(Author.Subdivision.Id);
			}
		}
	}
}
