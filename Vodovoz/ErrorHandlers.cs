using System;
using System.Linq;
using Gtk;
using MySql.Data.MySqlClient;
using NHibernate.Hql.Ast.ANTLR.Tree;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.ErrorReporting;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.VersionControl;
using QS.Services;
using QSProjectsLib;

namespace Vodovoz
{
	public static class ErrorHandlers
	{
		public static bool NHibernateStaleObjectStateExceptionException(Exception exception, IApplicationInfo application, UserBase user, IInteractiveMessage interactiveMessage)
		{
			var staleObjectStateException = ExceptionHelper.FineExceptionTypeInInner<NHibernate.StaleObjectStateException>(exception);
			if(staleObjectStateException != null) {
				var type = OrmConfig.FindMappingByFullClassName(staleObjectStateException.EntityName).MappedClass;
				var objectName = DomainHelper.GetSubjectNames(type);

				string message;

				switch(objectName.Gender) {
					case GrammaticalGender.Feminine:
						message = "Сохраняемая <b>{0}</b> c номером <b>{1}</b> была кем то изменена.";
						break;
					case GrammaticalGender.Neuter:
						message = "Сохраняемое <b>{0}</b> c номером <b>{1}</b> было кем то изменено.";
						break;
					case GrammaticalGender.Masculine:
					default:
						message = "Сохраняемый <b>{0}</b> c номером <b>{1}</b> был кем то изменен.";
						break;
				}
				message = String.Format(message + "\nВаши изменения не будут записаны, чтобы не потерять чужие изменения. \nПереоткройте вкладку.", objectName?.Nominative ?? type.Name, staleObjectStateException.Identifier);

				interactiveMessage.ShowMessage(QS.Dialog.ImportanceLevel.Warning, message);
				return true;
			}
			return false;
		}

		public static bool MySqlExceptionConnectionTimeout(Exception exception, IApplicationInfo application, UserBase user, IInteractiveMessage interactiveMessage)
		{
			var mysqlEx = ExceptionHelper.FineExceptionTypeInInner<MySqlException>(exception);
			var exceptions = new[] { 1159, 1161 };
			if(mysqlEx != null && exceptions.Contains(mysqlEx.Number)) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "Возникла проблема с подключением к серверу, попробуйте снова.");
				return true;
			}
			return false;
		}

		public static bool IsLoginFormOpen = false;
		public static bool MySqlExceptionAuth(Exception exception, IApplicationInfo application, UserBase user, IInteractiveService interactiveMessage)
		{
			var mysqlEx = ExceptionHelper.FineExceptionTypeInInner<MySqlException>(exception);
			if (mysqlEx.Message.Contains("Authentication to host"))
			{
				if (!IsLoginFormOpen)
				{
					interactiveMessage.ShowMessage(ImportanceLevel.Info, "Пароль вашего аккаунта был сброшен, смс придет в течении 15 минут, для продолжения работы перезайдите в программу");
				}
			}
			return false;
		}
	}
}
