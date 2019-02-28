using System;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.Project.DB;
using QSOrmProject;
using QSProjectsLib;

namespace Vodovoz.Additions
{
	public static class EntityChangedExceptionHelper
	{
		public static void ShowExceptionMessage(Exception ex)
		{
			var staleObjectStateException = ex as StaleObjectStateException;
			if(staleObjectStateException == null) {
				return;
			}

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

			MessageDialogWorks.RunErrorDialog(message);

		}
	}
}
