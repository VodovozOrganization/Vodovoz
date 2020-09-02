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
		public static IEnumerable GetObjectByPhone(string digitsPhone, IUnitOfWork UoW) 
		{
			string Number = digitsPhone.ReplaceFirstOccurrence("+7", "");
			var sql = "SELECT * FROM phones WHERE digits_number = @phone";
			var list = UoW.Session.Connection.Query(sql, new {phone  = Number});
			ArrayList _list = new ArrayList();
			foreach(var phone in list) {
				if(phone.counterparty_id != null) {
					Counterparty client = UoW.GetById<Counterparty>((int)phone.counterparty_id);
					_list.Add(client);
				}
				if(phone.counterparty_contact_id != null) {
					CounterpartyContract contract = UoW.GetById<CounterpartyContract>((int)phone.counterparty_id);
					_list.Add(contract);
				}
				if(phone.employee_id != null) {
					Employee employee = UoW.GetById<Employee>((int)phone.employee_id);
					_list.Add(employee);
				}
			}
			return _list;
		}

	}
}
