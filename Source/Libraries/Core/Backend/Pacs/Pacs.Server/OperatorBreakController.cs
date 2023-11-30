using System;
using System.Collections.Concurrent;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public class OperatorBreakController : IOperatorBreakController, ISettingsConsumer
	{
		private readonly IBreakAvailabilityNotifier _breakAvailabilityNotifier;
		private readonly IPacsRepository _pacsRepository;
		private DomainSettings _actualSettings;

		private ConcurrentDictionary<int, bool> _operatorsOnBreak { get; set; }

		public OperatorBreakController(IBreakAvailabilityNotifier breakAvailabilityNotifier, IPacsRepository pacsRepository)
		{
			_breakAvailabilityNotifier = breakAvailabilityNotifier ?? throw new ArgumentNullException(nameof(breakAvailabilityNotifier));
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
			_operatorsOnBreak = new ConcurrentDictionary<int, bool>();

			_actualSettings = _pacsRepository.GetPacsDomainSettings();
			UpdateAvailability();
		}

		public bool CanStartBreak { get; private set; }

		public void StartBreak(int operatorId)
		{
			_operatorsOnBreak.TryAdd(operatorId, false);
			UpdateAvailability();
		}

		public void EndBreak(int operatorId)
		{
			_operatorsOnBreak.TryRemove(operatorId, out var value);
			UpdateAvailability();
		}

		private void UpdateAvailability()
		{
			var currentOperators = _operatorsOnBreak.Count;
			var maxOperators = _actualSettings.MaxOperatorsOnBreak;

			CanStartBreak = currentOperators < maxOperators;
			_breakAvailabilityNotifier.NotifyBreakAvailability(CanStartBreak);
		}

		public void UpdateSettings(DomainSettings newSettings)
		{
			_actualSettings = newSettings;
			UpdateAvailability();
		}
	}
}
