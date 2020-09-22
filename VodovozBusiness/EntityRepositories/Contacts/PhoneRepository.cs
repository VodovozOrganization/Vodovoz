using System;
using System.Collections;
using System.Collections.Generic;
using Dapper;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories
{
	public class PhoneRepository : IPhoneRepository
	{

		#region PhoneType

		public IList<PhoneType> GetPhoneTypes(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<PhoneType>().List<PhoneType>();
		}

		public PhoneType PhoneTypeWithPurposeExists(IUnitOfWork uow, PhonePurpose phonePurpose)
		{
			return uow.Session.QueryOver<PhoneType>()
				.Where(x => x.PhonePurpose == phonePurpose)
				.SingleOrDefault<PhoneType>();
		}

		#endregion

		#region Телефоны

		/// <summary>
		/// Возвращает список объектов которым принадлежит телефон
		/// </summary>
		/// <returns>Counterparty / DeliveryPoint / Employee.</returns>
		/// <param name="digitsPhone">Digits phone.</param>
		public object[] GetObjectByPhone(IUnitOfWork uow, string digitsPhone) 
		{
			string number = digitsPhone.Substring(digitsPhone.Length - Math.Min(10, digitsPhone.Length));
			var sql = "SELECT * FROM phones WHERE digits_number = @phone";
			var list = uow.Session.Connection.Query(sql, new {phone  = number});
			ArrayList _list = new ArrayList();
			foreach(var phone in list) {
				if(phone.counterparty_id != null) {
					Counterparty client = uow.GetById<Counterparty>((int)phone.counterparty_id);
					_list.Add(client);
				}
				if(phone.delivery_point_id != null) {
					DeliveryPoint deliveryPoint = uow.GetById<DeliveryPoint>((int)phone.delivery_point_id);
					_list.Add(deliveryPoint);
				}
				if(phone.employee_id != null) {
					Employee employee = uow.GetById<Employee>((int)phone.employee_id);
					_list.Add(employee);
				}
			}
			return _list.ToArray();
		}

		#endregion
	}
}
