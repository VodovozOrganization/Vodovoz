using System.Collections;
using System.Collections.Generic;
using Dapper;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
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
		/// Возвращает объект класса (Counterparty / Employ )
		/// </summary>
		/// <returns>The counterparty by phone.</returns>
		/// <param name="digitsPhone">Digits phone.</param>
		/// <param name="UoW">Uo w.</param>
		public IEnumerable GetObjectByPhone(string digitsPhone, IUnitOfWork uow) 
		{
			string number = digitsPhone.ReplaceFirstOccurrence("+7", "");
			var sql = "SELECT * FROM phones WHERE digits_number = @phone";
			var list = uow.Session.Connection.Query(sql, new {phone  = number});
			ArrayList _list = new ArrayList();
			foreach(var phone in list) {
				if(phone.counterparty_id != null) {
					Counterparty client = uow.GetById<Counterparty>((int)phone.counterparty_id);
					_list.Add(client);
				}
				if(phone.counterparty_contact_id != null) {
					CounterpartyContract contract = uow.GetById<CounterpartyContract>((int)phone.counterparty_id);
					_list.Add(contract);
				}
				if(phone.employee_id != null) {
					Employee employee = uow.GetById<Employee>((int)phone.employee_id);
					_list.Add(employee);
				}
			}
			return _list;
		}

		#endregion
	}
}
