using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using NHibernate.Util;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Pacs.Server.Phones
{
	public class OperatorPhoneService : IOperatorPhoneService
	{
		private readonly ILogger<OperatorPhoneService> _logger;
		private readonly IPhoneRepository _pacsPhoneRepository;

		private readonly ConcurrentDictionary<string, int> _phones = new ConcurrentDictionary<string, int>();
		private readonly ConcurrentDictionary<int, string> _operatorPhones = new ConcurrentDictionary<int, string>();

		public OperatorPhoneService(ILogger<OperatorPhoneService> logger, IPhoneRepository pacsPhoneRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_pacsPhoneRepository = pacsPhoneRepository ?? throw new ArgumentNullException(nameof(pacsPhoneRepository));

			LoadAssignments();
		}

		private void LoadAssignments()
		{
			var assignments = _pacsPhoneRepository.GetPhoneAssignments();

			foreach(var assignment in assignments)
			{
				if(!_phones.TryGetValue(assignment.Phone, out var operatorId))
				{
					_phones.TryAdd(assignment.Phone, assignment.OperatorId);
				}
				else if(operatorId != assignment.OperatorId)
				{
					_phones.TryUpdate(assignment.Phone, assignment.OperatorId, operatorId);
				}

				if(assignment.OperatorId != 0)
				{
					if(!_operatorPhones.TryGetValue(assignment.OperatorId, out var phoneNumber))
					{
						_operatorPhones.TryAdd(assignment.OperatorId, assignment.Phone);
					}
					else if(phoneNumber != assignment.Phone)
					{
						_operatorPhones.TryUpdate(assignment.OperatorId, assignment.Phone, phoneNumber);
					}
				}
			}

			_logger.LogInformation("Загружены привязки номеров. Используемых номеров: {AssignmentPhonesCount}", _operatorPhones.Count);

			if(!_operatorPhones.Any())
			{
				return;
			}

			foreach(var operatorAssignment in _operatorPhones)
			{
				_logger.LogInformation("Загружена привязка оператора: {OperatorId}, к телефону: {PhoneNumber}", operatorAssignment.Key, operatorAssignment.Value);
			}
		}

		private void UpdatePhones()
		{
			_logger.LogInformation("Обновление телефонов");
			var assignments = _pacsPhoneRepository.GetPhoneAssignments();

			foreach(var assignment in assignments)
			{
				if(_phones.ContainsKey(assignment.Phone))
				{
					continue;
				}

				_logger.LogInformation("Добавление телефона {phone}", assignment.Phone);

				if(!_phones.TryGetValue(assignment.Phone, out var operatorId))
				{
					_phones.TryAdd(assignment.Phone, assignment.OperatorId);
				}
				else if(operatorId != assignment.OperatorId)
				{
					_phones.TryUpdate(assignment.Phone, assignment.OperatorId, operatorId);
				}
			}
		}

		public bool ValidatePhone(string phone)
		{
			if(!_phones.ContainsKey(phone))
			{
				UpdatePhones();
			}

			return _phones.ContainsKey(phone);
		}

		public bool CanAssign(string phone, int operatorId)
		{
			if(!_phones.ContainsKey(phone))
			{
				return false;
			}

			if(_phones.TryGetValue(phone, out var currentAssignedOperatorId))
			{
				if(currentAssignedOperatorId == 0)
				{
					return true;
				}
			}

			return !_operatorPhones.ContainsKey(operatorId);
		}

		public void AssignPhone(string phone, int operatorId)
		{
			if(!_phones.TryGetValue(phone, out var currentOperatorId))
			{
				throw new PacsPhoneException($"Неизвестный номер телефона {phone}");
			}

			if(currentOperatorId != 0)
			{
				throw new PacsPhoneException($"Телефонный номер {phone} уже использует другой оператор");
			}

			if(!_phones.TryUpdate(phone, operatorId, currentOperatorId))
			{
				throw new PacsPhoneException($"Телефонный номер {phone} уже использует другой оператор");
			}

			if(_operatorPhones.TryGetValue(operatorId, out var currentOperatorPhone))
			{
				_operatorPhones.TryUpdate(operatorId, phone, currentOperatorPhone);
			}
			else
			{
				if(!_operatorPhones.TryAdd(operatorId, phone))
				{
					throw new PacsPhoneException($"Не удалось зарезервировать телефон {phone} попробуйте еще раз");
				}
			}

			_logger.LogInformation("Телефон {Phone} привязан к оператору {OperatorId}", phone, operatorId);
		}

		public void ReleasePhone(string phone)
		{
			if(phone.IsNullOrWhiteSpace())
			{
				return;
			}

			if(!_phones.TryGetValue(phone, out var currentOperatorId))
			{
				throw new PacsPhoneException($"Неизвестный номер телефона {phone}");
			}

			if(currentOperatorId == 0)
			{
				return;
			}

			if(!_operatorPhones.TryRemove(currentOperatorId, out var _))
			{
				_logger.LogWarning("Ошибка при удалении резервации оператора к телефону {OperatorId}", currentOperatorId);
			}

			if(!_phones.TryUpdate(phone, 0, currentOperatorId))
			{
				_logger.LogWarning("Ошибка при удалении резервации телефона к оператору {OperatorId}", currentOperatorId);
			}

			_logger.LogInformation("Телефон {Phone} отвязан от оператора {OperatorId}", phone, currentOperatorId);
		}
	}
}
