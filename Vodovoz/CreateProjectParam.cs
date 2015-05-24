using System;
using System.Collections.Generic;
using QSProjectsLib;

namespace Vodovoz
{
	partial class MainClass
	{
		static void CreateProjectParam ()
		{
			QSMain.ProjectPermission = new Dictionary<string, UserPermission> ();
			QSMain.ProjectPermission.Add ("max_loan_amount", new UserPermission ("max_loan_amount", "Установка максимального кредита",
				"Пользователь имеет права для установки максимальной суммы кредита."));
			QSMain.ProjectPermission.Add ("logistican", new UserPermission ("logistican", "Логист", "Пользователь является логистом."));
		}

	}
}
