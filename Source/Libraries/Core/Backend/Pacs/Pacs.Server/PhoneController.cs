using System;
using System.Collections.Generic;

namespace Pacs.Server
{
	public class PhoneController : IPhoneController
	{
		private readonly IPhoneRepository _pacsPhoneRepository;

		private Dictionary<string, int> _phones;
		private Dictionary<int, string> _operatorPhones;
		private object _locker;

		public PhoneController(IPhoneRepository pacsPhoneRepository)
		{
			_phones = new Dictionary<string, int>();
			_operatorPhones = new Dictionary<int, string>();
			_pacsPhoneRepository = pacsPhoneRepository ?? throw new ArgumentNullException(nameof(pacsPhoneRepository));

			LoadAssignments();
		}

		private void LoadAssignments()
		{
			var assignments = _pacsPhoneRepository.GetPhoneAssignments();
			foreach(var assignment in assignments)
			{
				_phones.Add(assignment.Phone, assignment.OperatorId);
				if(assignment.OperatorId != 0)
				{
					_operatorPhones.Add(assignment.OperatorId, assignment.Phone);
				}
			}
		}

		public bool ValidatePhone(string phone)
		{
			return _phones.ContainsKey(phone);
		}

		public bool CanAssign(string phone, int operatorId)
		{
			if(!_phones.ContainsKey(phone))
			{
				return false;
			}

			if(_phones[phone] == 0)
			{
				return true;
			}

			return !_operatorPhones.ContainsKey(operatorId);
		}

		public void AssignPhone(string phone, int operatorId)
		{
			lock(_locker)
			{
				Assign(phone, operatorId);
			}
		}

		public void ReleasePhone(string phone)
		{
			lock(_locker)
			{
				Release(phone);
			}
		}

		private void Assign(string phone, int operatorId)
		{
			if(!_phones.ContainsKey(phone))
			{
				throw new PacsPhoneException($"Неизвестный номер телефона {phone}");
			}

			var currentOperator = _phones[phone];
			if(currentOperator != 0)
			{
				throw new PacsPhoneException($"Телефонный номер {phone} уже использует другой оператор");
			}

			_phones[phone] = operatorId;

			if(_operatorPhones.ContainsKey(operatorId))
			{
				_operatorPhones[operatorId] = phone;
			}
			else
			{
				_operatorPhones.Add(operatorId, phone);
			}
		}

		private void Release(string phone)
		{
			if(!_phones.ContainsKey(phone))
			{
				throw new PacsPhoneException($"Неизвестный номер телефона {phone}");
			}

			var currentOperator = _phones[phone];
			if(currentOperator == 0)
			{
				return;
			}

			_operatorPhones[currentOperator] = null;
			_phones[phone] = 0;
		}
	}
}
