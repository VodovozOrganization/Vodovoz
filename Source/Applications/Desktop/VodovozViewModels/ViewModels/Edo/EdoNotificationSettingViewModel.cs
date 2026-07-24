using MySqlConnector;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.ViewModels.Edo
{
	public class EdoNotificationSettingViewModel : EntityTabViewModelBase<EdoNotificationSetting>
	{
		public EdoNotificationSettingViewModel(
			IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = "Настройка уведомления по ЭДО";

			SaveCommand = new DelegateCommand(TrySave);
			CloseCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
		}

		private void TrySave()
		{
			try
			{
				SaveAndClose();
			}
			catch(Exception ex)
			{
				var sqlException = ex.InnerException as MySqlException;

				if(sqlException != null && sqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Вы добавляете дубль: настройка с таким уведомление уже существует в справочнике.");

					if(UoW?.Session != null)
					{
						UoW.Session.Clear();
					}
				}
				else
				{
					throw;
				}
			}
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }
	}
}
