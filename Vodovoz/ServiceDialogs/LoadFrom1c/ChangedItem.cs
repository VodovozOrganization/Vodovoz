using System;
using System.Collections.Generic;
using QSHistoryLog;
using Vodovoz.Domain.Client;
using Gamma.Utilities;

namespace ServiceDialogs.LoadFrom1c
{
	public class ChangedItem
	{
		public string Title { get; set;}
		public List<FieldChange> Fields;

		public static ChangedItem Compare(Counterparty oldCP, Counterparty newCP)
		{
			if (oldCP == null || newCP == null)
				return null;
			
			var result = new List<FieldChange>();

			if (oldCP.Name != newCP.Name)
				result.Add(new FieldChange("Изменено имя", oldCP.Name, newCP.Name));
			if (oldCP.PersonType != newCP.PersonType)
				result.Add(new FieldChange("Изменен тип", oldCP.PersonType.GetEnumTitle(), newCP.PersonType.GetEnumTitle()));
			if (oldCP.PaymentMethod != newCP.PaymentMethod)
				result.Add(new FieldChange("Изменен метод оплаты", oldCP.PaymentMethod.GetEnumTitle(), newCP.PaymentMethod.GetEnumTitle()));
			if (oldCP.Comment != newCP.Comment)
				result.Add(new FieldChange("Изменен комментарий", oldCP.Comment, newCP.Comment));
			if (oldCP.FullName != newCP.FullName)
				result.Add(new FieldChange("Изменено полное имя", oldCP.FullName, newCP.FullName));
			if (oldCP.INN != newCP.INN)
				result.Add(new FieldChange("Изменен ИНН", oldCP.INN, newCP.INN));
			if (oldCP.KPP != newCP.KPP)
				result.Add(new FieldChange("Изменен КПП", oldCP.KPP, newCP.KPP));
			if (oldCP.TypeOfOwnership != newCP.TypeOfOwnership)
				result.Add(new FieldChange("Изменена форма собственности", oldCP.TypeOfOwnership, newCP.TypeOfOwnership));

			if (result.Count > 0)
				return new ChangedItem
				{
					Title = string.Format("Контрагент с кодом {0} и именем {1}", oldCP.Code1c, oldCP.Name),
					Fields = result
				};
			else
				return null;
		}

	}
}

