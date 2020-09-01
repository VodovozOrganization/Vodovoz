using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Client;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using QS.Utilities.Text;
using Dapper;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Repositories.Client
{
	public static  class PhoneRepository
	{
		/// <summary>
		/// Возвращает объект класса (Counterparty / Employ )
		/// </summary>
		/// <returns>The counterparty by phone.</returns>
		/// <param name="digitsPhone">Digits phone.</param>
		/// <param name="UoW">Uo w.</param>
		public static IEnumerable<object> GetObjectByPhone(string digitsPhone, IUnitOfWork UoW) 
		{
			string Number = digitsPhone.ReplaceFirstOccurrence("+7", "");
			var sql = "SELECT * FROM phones WHERE digits_number = @phone";
			var list = UoW.Session.Connection.Query(sql, new {phone  = digitsPhone });
			List<object> _list = new List<object>();
			foreach(var phone in list) {
				if(phone.counterparty_id != null)
					_list.AddRange(UoW.GetById<Counterparty>(phone.counterparty_id));
				if(phone.counterparty_contact_id)
					_list.AddRange(UoW.GetById<CounterpartyContract>(phone.counterparty_id));
				if(phone.employee_id)
					_list.AddRange(UoW.GetById<Employee>(phone.employee_id));
			}
			return _list;
		}

	}
}
