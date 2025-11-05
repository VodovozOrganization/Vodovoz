using System;
using MySqlConnector;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OnlineOrderNotificationSettingViewModel : EntityTabViewModelBase<OnlineOrderNotificationSetting>
	{
		public OnlineOrderNotificationSettingViewModel(
			IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = "Настройка уведомления для онлайн заказов";

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

				if (sqlException != null && sqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Вы добавляете дубль: настройка с таким статусом заказа уже существует в справочнике.");
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
