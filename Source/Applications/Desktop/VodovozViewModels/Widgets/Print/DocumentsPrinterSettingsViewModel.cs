using QS.DomainModel.UoW;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Widgets.Print
{
	public class DocumentsPrinterSettingsViewModel : WidgetViewModelBase, IDisposable
	{
		private IUnitOfWork _unitOfWork;
		private UserSettings _userSettings;

		public DocumentsPrinterSettingsViewModel(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWork = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot();
		}

		public UserSettings UserSettings
		{
			get => _userSettings;
			set
			{
				if(!(_userSettings is null))
				{
					throw new InvalidOperationException($"Свойство {nameof(UserSettings)} уже установлено");
				}

				SetField(ref _userSettings, value);
			}
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
