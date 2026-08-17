using QS.Commands;
using QS.DomainModel.Entity;
using QS.ViewModels;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Vodovoz.Application.Mango;
using Vodovoz.Core.Domain.Extensions;

namespace Vodovoz.ViewModels.Widgets.Mango
{
	/// <summary>
	/// Вью-модель кнопки исходящего звонка через Манго
	/// </summary>
	public class MangoCallButtonViewModel : WidgetViewModelBase, IDisposable
	{
		private const string _notDigitPattern = "[^0-9]";
		private const string _russianCountryCode = "7";
		private const int _minExtensionNumberLength = 3;
		private const int _maxExtensionNumberLength = 6;
		private const int _localPhoneNumberLength = 10;

		private readonly IMangoManager _mangoManager;

		private string _phoneNumber;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="mangoManager">Менеджер Манго, через который совершается звонок</param>
		public MangoCallButtonViewModel(IMangoManager mangoManager)
		{
			_mangoManager = mangoManager ?? throw new ArgumentNullException(nameof(mangoManager));
			_mangoManager.PropertyChanged += OnMangoManagerPropertyChanged;

			MakeCallCommand = new DelegateCommand(MakeCall, () => CanMakeCall);
			MakeCallCommand.CanExecuteChangedWith(this, x => x.CanMakeCall);
		}

		/// <summary>
		/// Команда совершения звонка
		/// </summary>
		public DelegateCommand MakeCallCommand { get; }

		/// <summary>
		/// Номер, на который совершается звонок.
		/// Может быть как внутренним добавочным номером, так и телефонным номером в любом из принятых форматов
		/// </summary>
		[PropertyChangedAlso(nameof(CanMakeCall), nameof(TooltipText))]
		public string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		/// <summary>
		/// Можно ли совершить звонок
		/// </summary>
		public bool CanMakeCall => _mangoManager.IsActive && NumberToCall != null;

		/// <summary>
		/// Подсказка к кнопке звонка
		/// </summary>
		public string TooltipText
		{
			get
			{
				if(!_mangoManager.IsActive)
				{
					return "Нет подключения к Манго";
				}

				if(string.IsNullOrWhiteSpace(PhoneNumber))
				{
					return "Номер для звонка не указан";
				}

				if(NumberToCall == null)
				{
					return $"Некорректный номер для звонка: {PhoneNumber}";
				}

				return $"Позвонить: {PhoneNumber}";
			}
		}

		/// <summary>
		/// Номер в том виде, в котором его принимает Манго,
		/// либо <c>null</c>, если из указанного номера не удалось его получить
		/// </summary>
		private string NumberToCall
		{
			get
			{
				if(string.IsNullOrWhiteSpace(PhoneNumber))
				{
					return null;
				}

				var digits = Regex.Replace(PhoneNumber, _notDigitPattern, string.Empty);

				if(digits.Length >= _minExtensionNumberLength && digits.Length <= _maxExtensionNumberLength)
				{
					return digits;
				}

				var localNumber = PhoneNumber.NormalizePhone();

				return localNumber.Length == _localPhoneNumberLength
					? _russianCountryCode + localNumber
					: null;
			}
		}

		private void OnMangoManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName != nameof(IMangoManager.ConnectionState))
			{
				return;
			}

			OnPropertyChanged(nameof(CanMakeCall));
			OnPropertyChanged(nameof(TooltipText));
		}

		private void MakeCall()
		{
			var numberToCall = NumberToCall;

			if(!_mangoManager.IsActive || numberToCall == null)
			{
				return;
			}

			_mangoManager.MakeCall(numberToCall);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_mangoManager.PropertyChanged -= OnMangoManagerPropertyChanged;
		}
	}
}
