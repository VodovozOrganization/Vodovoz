using Microsoft.Extensions.Logging;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pacs.Server
{
	public class PhoneController : IPhoneController
	{
		private readonly ILogger<PhoneController> _logger;
		private readonly IPhoneRepository _pacsPhoneRepository;

		private Dictionary<string, int> _phones;
		private Dictionary<int, string> _operatorPhones;

		public PhoneController(ILogger<PhoneController> logger, IPhoneRepository pacsPhoneRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_pacsPhoneRepository = pacsPhoneRepository ?? throw new ArgumentNullException(nameof(pacsPhoneRepository));
			_phones = new Dictionary<string, int>();
			_operatorPhones = new Dictionary<int, string>();

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

			_logger.LogInformation("Загружены привязки номеров. Используемых номеров: {AssignmentPhonesCount}", _operatorPhones.Count);
			if(_operatorPhones.Any())
			{
				_logger.LogInformation(string.Join("\n", _operatorPhones.Select(x => $"Оператор: {x.Key}, Тел.: {x.Value}")));
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
			lock(_operatorPhones)
			{
				Assign(phone, operatorId);
			}
		}

		public void ReleasePhone(string phone)
		{
			lock(_operatorPhones)
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

			_logger.LogInformation("Телефон {Phone} привязан к оператору {OperatorId}", phone, operatorId);
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

			_logger.LogInformation("Телефон {Phone} отвязан от оператора {OperatorId}", phone, currentOperator);
		}
	}
}
