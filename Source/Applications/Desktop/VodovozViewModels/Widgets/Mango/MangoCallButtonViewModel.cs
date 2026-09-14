using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.ViewModels;
using System;
using System.ComponentModel;
using Vodovoz.Application.Mango;

namespace Vodovoz.ViewModels.Widgets.Mango
{
	/// <summary>
	/// Вью-модель кнопки исходящего звонка через Манго
	/// </summary>
	public class MangoCallButtonViewModel : WidgetViewModelBase, IDisposable
	{
		private const string _mangoIsNotConnectedMessage = "Нет подключения к Манго";

		private string _phoneNumber;
		private string _unavailabilityReason;
		private bool _disposed;

		private readonly IMangoManager _mangoManager;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="mangoManager">Менеджер Манго, через который совершается звонок</param>
		/// <param name="guiDispatcher">Диспетчер, через который обновления от Манго передаются в основной поток приложения</param>
		/// <param name="interactiveService">Сервис взаимодействия с пользователем</param>
		public MangoCallButtonViewModel(
			IMangoManager mangoManager,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService)
		{
			_mangoManager = mangoManager ?? throw new ArgumentNullException(nameof(mangoManager));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			MakeCallCommand = new DelegateCommand(MakeCall, () => CanMakeCall);
			MakeCallCommand.CanExecuteChangedWith(this, x => x.CanMakeCall);

			_mangoManager.PropertyChanged += OnMangoManagerPropertyChanged;
		}

		/// <summary>
		/// Команда совершения звонка
		/// </summary>
		public DelegateCommand MakeCallCommand { get; }

		/// <summary>
		/// Номер, на который совершается звонок
		/// </summary>
		[PropertyChangedAlso(nameof(CanMakeCall), nameof(TooltipText))]
		public string PhoneNumber
		{
			get => _phoneNumber;
			private set => SetField(ref _phoneNumber, value);
		}

		/// <summary>
		/// Причина, по которой звонок недоступен, заданная владельцем виджета.
		/// Задаётся через <see cref="SetUnavailabilityReason(string)"/>
		/// </summary>
		[PropertyChangedAlso(nameof(CanMakeCall), nameof(TooltipText))]
		public string UnavailabilityReason
		{
			get => _unavailabilityReason;
			private set => SetField(ref _unavailabilityReason, value);
		}

		/// <summary>
		/// Можно ли совершить звонок
		/// </summary>
		public bool CanMakeCall =>
			string.IsNullOrWhiteSpace(UnavailabilityReason)
			&& _mangoManager.IsActive
			&& PhoneNumber != null;

		/// <summary>
		/// Подсказка к кнопке звонка
		/// </summary>
		public string TooltipText
		{
			get
			{
				if(!string.IsNullOrWhiteSpace(UnavailabilityReason))
				{
					return UnavailabilityReason;
				}

				if(!_mangoManager.IsActive)
				{
					return _mangoIsNotConnectedMessage;
				}

				if(string.IsNullOrWhiteSpace(PhoneNumber))
				{
					return "Номер для звонка не указан";
				}

				return $"Позвонить: {PhoneNumber}";
			}
		}

		/// <summary>
		/// Задать внутренний добавочный номер, на который будет совершён звонок
		/// </summary>
		/// <param name="extensionNumber">Добавочный номер</param>
		public void SetExtension(int extensionNumber)
		{
			UnavailabilityReason = null;
			PhoneNumber = extensionNumber.ToString();
		}

		/// <summary>
		/// Задать причину, по которой звонок недоступен
		/// </summary>
		/// <param name="unavailabilityReason">Текст причины недоступности звонка</param>
		public void SetUnavailabilityReason(string unavailabilityReason)
		{
			PhoneNumber = null;
			UnavailabilityReason = unavailabilityReason;
		}

		private void OnMangoManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName != nameof(IMangoManager.ConnectionState))
			{
				return;
			}

			_guiDispatcher.RunInGuiTread(() =>
			{
				OnPropertyChanged(nameof(CanMakeCall));
				OnPropertyChanged(nameof(TooltipText));
			});
		}

		private void MakeCall()
		{
			if(!string.IsNullOrWhiteSpace(UnavailabilityReason))
			{
				return;
			}

			if(!_mangoManager.IsActive)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, _mangoIsNotConnectedMessage);
				return;
			}

			if(PhoneNumber == null)
			{
				return;
			}

			_mangoManager.MakeCall(PhoneNumber);
		}

		public void Dispose()
		{
			if(_disposed)
			{
				return;
			}

			_disposed = true;
			_mangoManager.PropertyChanged -= OnMangoManagerPropertyChanged;
		}
	}
}
