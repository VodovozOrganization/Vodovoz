using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;

namespace Pacs.Server
{
	public class OperatorPhonesController
	{
		/*private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(30);
		private readonly IMangoPhoneRepository _mangoPhoneRepository;
		
		//Ключ номер телефона, значение id оператора
		private readonly ConcurrentDictionary<string, string> _assignedPhones = new ConcurrentDictionary<string, string>();
		private readonly ConcurrentDictionary<string, bool> _freePhones = new ConcurrentDictionary<string, bool>();

		private DateTime _lastRefresh;

		public OperatorPhonesController(IMangoPhoneRepository mangoPhoneRepository)
		{
			_mangoPhoneRepository = mangoPhoneRepository ?? throw new ArgumentNullException(nameof(mangoPhoneRepository));
		}

		public async Task<IEnumerable<string>> GetFreePhones()
		{
			await RefreshPhones();
			return _freePhones.Keys;
		}

		public bool AssignPhone(string phoneNumber, string operatorId)
		{
			if(!_freePhones.TryRemove(phoneNumber, out bool dummy))
			{
				return false;
			}

			if(_assignedPhones.TryAdd(phoneNumber, operatorId))
			{
				return true;
			}

			return false;
		}

		public void FreePhone(string phoneNumber)
		{
			_assignedPhones.TryRemove(phoneNumber, out string operatorId);
		}

		private async Task RefreshPhones()
		{
			if((DateTime.Now - _lastRefresh) < _refreshInterval)
			{
				return;
			}

			var allPhones = await _mangoPhoneRepository.GetMangoPhones();
			foreach (var phone in allPhones)
			{
				if(_assignedPhones.ContainsKey(phone.PhoneNumber))
				{
					continue;
				}

				if(_freePhones.ContainsKey(phone.PhoneNumber))
				{
					continue;
				}

				_freePhones.AddOrUpdate(phone.PhoneNumber, true, (key, old) => old);
			}
		}

		private void LoadAssignedPhones()
		{
			var assignedPhones = await _mangoPhoneRepository.LoadAssignedPhones();
		}*/
	}
}
