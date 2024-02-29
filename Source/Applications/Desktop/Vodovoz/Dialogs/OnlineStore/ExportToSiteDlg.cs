using Autofac;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings;
using Vodovoz.Tools.CommerceML;

namespace Vodovoz.Dialogs.OnlineStore
{
	public partial class ExportToSiteDlg : QS.Dialog.Gtk.TdiTabBase
	{
		private readonly ISettingsController _settingsController = ScopeProvider.Scope.Resolve<ISettingsController>();
		
		public ExportToSiteDlg()
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance")) {
				MessageDialogHelper.RunWarningDialog("Доступ запрещён!", "У вас недостаточно прав для доступа к этой вкладке. Обратитесь к своему руководителю.", Gtk.ButtonsType.Ok);
				FailInitialize = true;
				return;
			}

			Build();
			TabName = "Экспорт интернет магазин";
			comboExportMode.ItemsEnum = typeof(ExportMode);
			
			if(_settingsController.ContainsSetting(Export.OnlineStoreUrlParameterName))
			{
				entrySitePath.Text = _settingsController.GetStringValue(Export.OnlineStoreUrlParameterName);
			}

			if(_settingsController.ContainsSetting(Export.OnlineStoreLoginParameterName))
			{
				entryUser.Text = _settingsController.GetStringValue(Export.OnlineStoreLoginParameterName);
			}

			if(_settingsController.ContainsSetting(Export.OnlineStorePasswordParameterName))
			{
				entryPassword.Text = _settingsController.GetStringValue(Export.OnlineStorePasswordParameterName);
			}

			if(_settingsController.ContainsSetting(Export.OnlineStoreExportMode))
			{
				comboExportMode.SelectedItem = Enum.Parse(typeof(ExportMode), _settingsController.GetStringValue(Export.OnlineStoreExportMode));
			}
		}

		protected void OnButtonRunToFileClicked(object sender, EventArgs e)
		{
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				var fileChooser = new Gtk.FileChooserDialog("Выберите папку для сохранения выгрузки",
					(Window)this.Toplevel,
				                                            Gtk.FileChooserAction.SelectFolder,
					"Отмена", ResponseType.Cancel,
					"Выбрать", ResponseType.Accept
				)
				{
					DoOverwriteConfirmation = true
				};

				if(fileChooser.Run() == (int)ResponseType.Cancel)
				{
					fileChooser.Destroy();
					return;
				}

				var directory = fileChooser.Filename;
				fileChooser.Destroy();

				var organizationRepository = ScopeProvider.Scope.Resolve<IOrganizationRepository>();
				var settingsController = ScopeProvider.Scope.Resolve<ISettingsController>();
				var export = new Export(uow, organizationRepository, settingsController);
				export.ProgressUpdated += Export_ProgressUpdated;

				export.RunToDirectory(directory);

				if(UpdateErrors(export.Errors))
					return;

                UpdateResults(export.Results);
                progressbarTotal.Text = "Готово";
				progressbarTotal.Adjustment.Value = progressbarTotal.Adjustment.Upper;
			}
		}

		protected void OnEntrySitePathFocusOutEvent(object o, FocusOutEventArgs args)
		{
			_settingsController.CreateOrUpdateSetting(Export.OnlineStoreUrlParameterName, entrySitePath.Text);
		}

		protected void OnEntryUserFocusOutEvent(object o, FocusOutEventArgs args)
		{
			_settingsController.CreateOrUpdateSetting(Export.OnlineStoreLoginParameterName, entryUser.Text);
		}

		protected void OnEntryPasswordFocusOutEvent(object o, FocusOutEventArgs args)
		{
			_settingsController.CreateOrUpdateSetting(Export.OnlineStorePasswordParameterName, entryPassword.Text);
		}

		protected void OnComboExportModeChangedByUser(object sender, EventArgs e)
		{
			_settingsController.CreateOrUpdateSetting(Export.OnlineStoreExportMode,  comboExportMode.SelectedItem.ToString());
		}

		protected void OnButtonExportToSiteClicked(object sender, EventArgs e)
		{
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot()) {
				var organizationRepository = ScopeProvider.Scope.Resolve<IOrganizationRepository>();
				var settingsController = ScopeProvider.Scope.Resolve<ISettingsController>();
				var export = new Export(uow, organizationRepository, settingsController);
				export.ProgressUpdated += Export_ProgressUpdated;

				export.RunToSite();

				if(UpdateErrors(export.Errors))
					return;

                UpdateResults(export.Results);
				progressbarTotal.Text = "Готово";
				progressbarTotal.Adjustment.Value = progressbarTotal.Adjustment.Upper;
			}
		}

		void Export_ProgressUpdated(object sender, EventArgs e)
		{
			var export = sender as Export;
			progressbarTotal.Text = export.CurrentTaskText + export.CurrentStepText;
			progressbarTotal.Adjustment.Upper = export.TotalTasks;
			progressbarTotal.Adjustment.Value = export.CurrentTask;
            UpdateErrors(export.Errors);
			QSMain.WaitRedraw();
		}

		/// <summary>
		/// Если метод вернул true, это значит что есть ошибки.
		/// </summary>
		private bool UpdateErrors(List<string> errors)
		{
			GtkScrolledWindowErrors.Visible = errors.Count > 0;
			if(errors.Count > 0) {
				TextTagTable textTags = new TextTagTable();
				var tag = new TextTag("Red");
				tag.Foreground = "red";
				textTags.Add(tag);
				TextBuffer tempBuffer = new TextBuffer(textTags);
				TextIter iter = tempBuffer.EndIter;
				tempBuffer.InsertWithTags(ref iter, String.Join("\n", errors), tag);
				textviewErrors.Buffer = tempBuffer;
				return true;
			}
			return false;
		}

		private void UpdateResults(List<string> results)
        {
            GtkScrolledWindowErrors.Visible = true;
            TextTagTable textResultTags = new TextTagTable();
            var tagResult = new TextTag("blue");
            tagResult.Foreground = "blue";
            textResultTags.Add(tagResult);
            TextBuffer tempBuffer = new TextBuffer(textResultTags);
            TextIter iter = tempBuffer.EndIter;
            tempBuffer.InsertWithTags(ref iter, String.Join("\n", results), tagResult);
            textviewErrors.Buffer = tempBuffer;
        }

	}
}
