using System;
using QSDocTemplates;
using Vodovoz.Domain.Employees;

namespace Vodovoz.DocTemplates
{
	public class EmployeeContractParser : DocParserBase<EmployeeContract>
	{
		public override void UpdateFields()
		{
			fieldsList.Clear();

			AddField(x => x.Document.PassportSeria, PatternFieldType.FString);
			AddField(x => x.Document.PassportNumber, PatternFieldType.FString);
			AddField(x => x.Document.PassportIssuedOrg, PatternFieldType.FString);
			AddField(x => x.Document.PassportIssuedDate, PatternFieldType.FString);

			AddField(x => x.Employee.FullName, PatternFieldType.FString);
			AddField(x => x.Employee.ShortName, PatternFieldType.FString);
			AddField(x => x.Employee.AddressRegistration, PatternFieldType.FString);
			AddField(x => x.Employee.AddressCurrent, PatternFieldType.FString);

			AddField(x => x.FirstDay.ToString("dd/MM/yyyy"), "ДатаНачалаДоговора", PatternFieldType.FString);
			AddField(x => x.LastDay.ToString("dd/MM/yyyy") , "ДатаОкончанияДоговора", PatternFieldType.FString);
			AddField(x => x.ContractDate.ToString("dd/MM/yyyy"),"ДатаДоговора", PatternFieldType.FString);

			AddField(x => x.Organization.Name, PatternFieldType.FString);
			AddField(x => x.Organization.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.JurAddress, PatternFieldType.FString);
			AddField(x => x.Organization.INN, PatternFieldType.FString);
			AddField(x => x.Organization.KPP, PatternFieldType.FString);


			AddField(x => x.Organization.DefaultAccount.InBank.Bik, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.BankCorAccount.CorAccountNumber, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.InBank.Name, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.Number, PatternFieldType.FString);

			SortFields();
		}
	}
}
