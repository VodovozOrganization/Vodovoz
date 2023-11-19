using Pacs.Core;
using Pacs.Operator.Client;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsOperatorViewModel : WidgetViewModelBase
	{
		//Выбор телефона
		//Начало смены
		//Завершение смены
		//Перерыв
		//Смена телефона

		private readonly Employee _employee;
		private readonly IOperatorStateAgent _operatorStateAgent;
		private readonly IEmployeeService _employeeService;
		private readonly IOperatorClient _operatorClient;
		private readonly IOperatorRepository _operatorRepository;

		private string _phoneNumber;

		public DelegateCommand StartWorkShiftCommand { get; private set; }
		public DelegateCommand EndWorkShiftCommand { get; private set; }
		public DelegateCommand ChangePhoneCommand { get; private set; }
		public DelegateCommand StartBreakCommand { get; private set; }
		public DelegateCommand EndBreakCommand { get; private set; }

		public PacsOperatorViewModel(
			IOperatorStateAgent operatorStateAgent,
			IEmployeeService employeeService, 
			IOperatorClientFactory operatorClientFactory,
			IOperatorRepository operatorRepository)
		{
			if(operatorClientFactory is null)
			{
				throw new ArgumentNullException(nameof(operatorClientFactory));
			}

			_operatorStateAgent = operatorStateAgent ?? throw new System.ArgumentNullException(nameof(operatorStateAgent));
			_employeeService = employeeService ?? throw new System.ArgumentNullException(nameof(employeeService));
			_operatorRepository = operatorRepository ?? throw new System.ArgumentNullException(nameof(operatorRepository));

			_employee = employeeService.GetEmployeeForCurrentUser();
			if(_employee == null)
			{
				throw new AbortCreatingPageException(
					"Должен быть привязан сотрудник к пользователю. Обратитесь в отдел кадров.",
					"Не настроен пользователь");
			}

			_operatorClient = operatorClientFactory.CreateOperatorClient(_employee.Id);

			ConfigureCommands();
		}

		public OperatorState CurrentState
		{
			get => _operatorStateAgent.OperatorState;
			set => _operatorStateAgent.OperatorState = value;
		}

		private void ConfigureCommands()
		{
			StartWorkShiftCommand = new DelegateCommand(() => StartWorkShift().Wait(), () => CanStartWorkShift);
			StartWorkShiftCommand.CanExecuteChangedWith(this, x => x.CanStartWorkShift);

			EndWorkShiftCommand = new DelegateCommand(() => EndWorkShift().Wait(), () => CanEndWorkShift);
			EndWorkShiftCommand.CanExecuteChangedWith(this, x => x.CanEndWorkShift);

			ChangePhoneCommand = new DelegateCommand(() => ChangePhone().Wait(), () => CanChangePhone);
			ChangePhoneCommand.CanExecuteChangedWith(this, x => x.CanChangePhone);

			StartBreakCommand = new DelegateCommand(() => StartBreak().Wait(), () => CanStartBreak);
			StartBreakCommand.CanExecuteChangedWith(this, x => x.CanStartBreak);

			EndBreakCommand = new DelegateCommand(() => EndBreak().Wait(), () => CanEndBreak);
			EndBreakCommand.CanExecuteChangedWith(this, x => x.CanEndBreak);
		}

		private void LoadDetails()
		{
			var history = _operatorRepository.GetOperatorHistory(_employee.Id);
		}

		public IEnumerable<string> AvailablePhones { get; set; }

		[PropertyChangedAlso(nameof(CanStartWorkShift))]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}


		public bool CanStartWorkShift => string.IsNullOrWhiteSpace(PhoneNumber)
			&& AvailablePhones.Contains(PhoneNumber)
			&& _operatorStateAgent.CanStartWorkShift;

		private async Task StartWorkShift()
		{
			CurrentState = await _operatorClient.StartWorkShift(PhoneNumber);
		}


		public bool CanEndWorkShift => _operatorStateAgent.CanEndWorkShift;

		private async Task EndWorkShift()
		{
			CurrentState = await _operatorClient.EndWorkShift();
		}


		public bool CanChangePhone => string.IsNullOrWhiteSpace(PhoneNumber)
			&& AvailablePhones.Contains(PhoneNumber)
			&& CurrentState.PhoneNumber != PhoneNumber
			&& _operatorStateAgent.CanStartWorkShift;

		private async Task ChangePhone()
		{
			CurrentState = await _operatorClient.EndWorkShift();
		}


		public bool CanStartBreak => _operatorStateAgent.CanStartBreak;

		private async Task StartBreak()
		{
			CurrentState = await _operatorClient.StartBreak();
		}


		public bool CanEndBreak => _operatorStateAgent.CanEndBreak;

		private async Task EndBreak()
		{
			CurrentState = await _operatorClient.EndBreak();
		}
	}
}
