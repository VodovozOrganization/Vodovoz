using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.ViewModels.Edo
{
	public class TenderEdoViewModel : EntityTabViewModelBase<TenderEdoTask>
	{
		private readonly ICommonServices _commonServices;
		private readonly IFileDialogService _fileDialogService;

		public TenderEdoViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IFileDialogService fileDialogService)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			TabName = $"Задача по госзаказу {Entity.FormalEdoRequest.Order.Id}";

			ExportCodesCommand = new DelegateCommand(ExportCodes, () => CanCloseTenderEdoTask);
			ExportCodesCommand.CanExecuteChangedWith(this, vm => vm.CanCloseTenderEdoTask);
		}

		private void ExportCodes()
		{
			if(Entity.Stage != TenderEdoTaskStage.Sending)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					$"Задача должна быть в статусе {TenderEdoTaskStage.Sending.GetEnumDisplayName()}, выгрузка кодов невозможна.");

				return;
			}

			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = $"Коды по госзаказу {Entity.FormalEdoRequest.Order.Id}",
				DefaultFileExtention = ".txt"
			};
			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Text", ".txt"));

			var dialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!dialogResult.Successful)
			{
				return;
			}

			var result = string.Join(",\r\n", Codes);
			File.WriteAllText(dialogResult.Path, result);

			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Выгрузка завершена.");

			if(!_commonServices.InteractiveService.Question("Вы успешно вручную загрузили коды в ЕИС и хотите закрыть задачу?"))
			{
				return;
			}

			Entity.Stage = TenderEdoTaskStage.ManualUploaded;

			UoW.Save();
			UoW.Commit();

			OnPropertyChanged(nameof(Info));

			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
				$"Задача переведена в следующий стадию {Entity.Stage.GetEnumDisplayName()}");
		}

		public string Info => $"Заказ: {Entity.FormalEdoRequest.Order.Id}.\r\n" +
		                      $"Статус отправки: {Entity.Stage.GetEnumDisplayName()}";

		public IList<string> Codes => Entity.Items.Select(x =>
			string.Concat("\"01", x.ProductCode.ResultCode.Gtin, "21", x.ProductCode.ResultCode.SerialNumber, "\"")).ToList();

		public DelegateCommand ExportCodesCommand { get; }

		public bool CanCloseTenderEdoTask =>
			CommonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.EdoPermissions.CanCloseTenderEdoTask, CurrentUser.Id);
	}
}
