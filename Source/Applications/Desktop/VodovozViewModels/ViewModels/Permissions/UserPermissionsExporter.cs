using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using QS.Dialog;
using QS.Project.Services.FileDialog;
using Vodovoz.EntityRepositories.Permissions;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.ViewModels.Permissions
{
	public class UserPermissionsExporter
	{
		private const string _userId = "№";
		private const string _user = "Пользователь";
		private const string _isDeactivatedUser = "Пользователь отключен";
		private const string _userSubdivision = "Подразделение сотрудника";
		private const string _canRead = "Просмотр";
		private const string _canCreate = "Создание";
		private const string _canUpdate = "Редактирование";
		private const string _canDelete = "Удаление";
		private const string _extendedPermission = "Особое право на документ";
		private const string _userPermission = "Пользовательское право";
		private const string _userPermissionBySubdivision = "Право выдано через подразд-е";
		private readonly IFileDialogService _fileDialogService;
		private readonly IInteractiveMessage _interactiveMessage;

		public UserPermissionsExporter(
			IFileDialogService fileDialogService,
			IInteractiveMessage interactiveMessage)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}
		
		public void ExportUsersEntityPermissionToExcel(
			(string PermissionName, string PermissionTitle) permission,
			IList<UserEntityExtendedPermissionNode> usersWithPermission,
			IEnumerable<UserEntityExtendedPermissionWithSubdivisionNode> usersWithPermissionBySubdivisions)
		{
			if(!usersWithPermission.Any() && !usersWithPermissionBySubdivisions.Any())
			{
				_interactiveMessage.ShowMessage(ImportanceLevel.Warning, "Нет данных для эспорта");
				return;
			}
			
			using(var wb = new XLWorkbook())
			{
				if(usersWithPermission.Any())
				{
					var wsByUsers = wb.Worksheets.Add(_userPermission);
					InsertUsersEntityPermission(wsByUsers, usersWithPermission, permission.PermissionTitle);
				}

				if(usersWithPermissionBySubdivisions.Any())
				{
					var wsBySubdivisions = wb.Worksheets.Add(_userPermissionBySubdivision);
					InsertUsersEntityPermissionBySubdivisions(wsBySubdivisions, usersWithPermissionBySubdivisions);
				}

				if(TryGetSavePath("Пользователи_с_правом_на_документ", out string path))
				{
					wb.SaveAs(path);
				}
			}
		}

		public void ExportUsersPresetPermissionToExcel(
			(string PermissionName, string PermissionTitle) permission,
			IList<UserNode> usersWithActivePermission,
			IEnumerable<UserPresetPermissionWithSubdivisionNode> usersWithActivePermissionBySubdivision)
		{
			if(!usersWithActivePermission.Any() && !usersWithActivePermissionBySubdivision.Any())
			{
				_interactiveMessage.ShowMessage(ImportanceLevel.Warning, "Нет данных для эспорта");
				return;
			}

			using(var wb = new XLWorkbook())
			{
				if(usersWithActivePermission.Any())
				{
					var wsByUsers = wb.Worksheets.Add(_userPermission);
					InsertUsersPresetPermission(wsByUsers, usersWithActivePermission, permission.PermissionTitle);
				}

				if(usersWithActivePermissionBySubdivision.Any())
				{
					var wsBySubdivisions = wb.Worksheets.Add(_userPermissionBySubdivision);
					InsertUsersPresetPermissionBySubdivisions(wsBySubdivisions, usersWithActivePermissionBySubdivision);
				}

				if(TryGetSavePath("Пользователи_с_конкретным_правом", out string path))
				{
					wb.SaveAs(path);
				}
			}
		}
		
		private void InsertUsersEntityPermission(IXLWorksheet ws, IList<UserEntityExtendedPermissionNode> users, string permissionTitle)
		{
			var row = 1;
			var colName = new[]
			{
				_userId,
				_user,
				_isDeactivatedUser,
				_userSubdivision,
				_canRead,
				_canCreate,
				_canUpdate,
				_canDelete,
				_extendedPermission
			};
			
			ws.Cell(row, 1).Value = "Право на";
			ws.Cell(row, 2).Value = permissionTitle;

			row += 2;
			
			for(var i = 0; i < colName.Length; i++)
			{
				ws.Cell(row, i + 1).Value = colName[i];
			}
			
			foreach(var item in users)
			{
				var nextRow = ++row;
				var column = 1;
				ws.Cell(nextRow, column).Value = item.UserId;
				ws.Cell(nextRow, ++column).Value = item.UserName;
				ws.Cell(nextRow, ++column).Value = item.IsDeactivatedUser.ConvertToYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.UserSubdivision;
				ws.Cell(nextRow, ++column).Value = item.CanRead.ConvertToYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.CanCreate.ConvertToYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.CanUpdate.ConvertToYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.CanDelete.ConvertToYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.ExtendedPermissionValue.ConvertToNotSetOrYesOrNo();
			}
			
			ws.Column(2).Width = 30;
			ws.Column(4).Width = 30;
			ws.Columns(5, 8).Width = 16;
			ws.Column(3).Width = 25;
			ws.Column(colName.Length).Width = 25;
		}

		private void InsertUsersEntityPermissionBySubdivisions(
			IXLWorksheet ws, IEnumerable<UserEntityExtendedPermissionWithSubdivisionNode> usersBySubdivisions)
		{
			var colName = new[]
			{
				_userId,
				_user,
				_isDeactivatedUser,
				_userSubdivision,
				_canRead,
				_canCreate,
				_canUpdate,
				_canDelete,
				_extendedPermission
			};

			var row = 1;
			for(var i = 0; i < colName.Length; i++)
			{
				ws.Cell(row, i + 1).Value = colName[i];
			}

			foreach(var item in usersBySubdivisions)
			{
				var nextRow = ++row;
				var column = 1;
				ws.Cell(nextRow, column).Value = item.User.Id;
				ws.Cell(nextRow, ++column).Value = item.User.Name;
				ws.Cell(nextRow, ++column).Value = item.User.Deactivated.ConvertToYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.Subdivision.Name;
				ws.Cell(nextRow, ++column).Value = item.CanRead.ConvertToNotSetOrYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.CanCreate.ConvertToNotSetOrYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.CanUpdate.ConvertToNotSetOrYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.CanDelete.ConvertToNotSetOrYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.ExtendedPermissionValue.ConvertToNotSetOrYesOrNo();
			}
			
			ws.Column(2).Width = 30;
			ws.Column(4).Width = 30;
			ws.Columns(5, 8).Width = 16;
			ws.Column(3).Width = 25;
			ws.Column(colName.Length).Width = 25;
		}
		
		private void InsertUsersPresetPermission(IXLWorksheet ws, IList<UserNode> users, string permissionTitle)
		{
			var row = 1;
			var colName = new[]
			{
				_userId,
				_user,
				_isDeactivatedUser,
				_userSubdivision
			};
			
			ws.Cell(row, 1).Value = "Право";
			ws.Cell(row, 2).Value = permissionTitle;

			row += 2;
			
			for(var i = 0; i < colName.Length; i++)
			{
				ws.Cell(row, i + 1).Value = colName[i];
			}
			
			foreach(var item in users)
			{
				var nextRow = ++row;
				var column = 1;
				ws.Cell(nextRow, column).Value = item.UserId;
				ws.Cell(nextRow, ++column).Value = item.UserName;
				ws.Cell(nextRow, ++column).Value = item.IsDeactivatedUser.ConvertToYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.UserSubdivision;
			}
			
			ws.Column(2).Width = 30;
			ws.Column(colName.Length - 1).Width = 25;
			ws.Column(colName.Length).Width = 30;
		}
		
		private void InsertUsersPresetPermissionBySubdivisions(
			IXLWorksheet ws, IEnumerable<UserPresetPermissionWithSubdivisionNode> usersBySubdivisions)
		{
			var colName = new[]
			{
				_userId,
				_user,
				_isDeactivatedUser,
				_userSubdivision
			};

			var row = 1;
			for(var i = 0; i < colName.Length; i++)
			{
				ws.Cell(row, i + 1).Value = colName[i];
			}

			foreach(var item in usersBySubdivisions)
			{
				var nextRow = ++row;
				var column = 1;
				ws.Cell(nextRow, column).Value = item.User.Id;
				ws.Cell(nextRow, ++column).Value = item.User.Name;
				ws.Cell(nextRow, ++column).Value = item.User.Deactivated.ConvertToYesOrNo();
				ws.Cell(nextRow, ++column).Value = item.Subdivision.Name;
			}
			
			ws.Column(2).Width = 30;
			ws.Column(colName.Length - 1).Width = 25;
			ws.Column(colName.Length).Width = 30;
		}
		
		private bool TryGetSavePath(string fileName, out string path)
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = fileName
			};

			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel", ".xlsx"));

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			path = result.Path;

			return result.Successful;
		}
	}
}
