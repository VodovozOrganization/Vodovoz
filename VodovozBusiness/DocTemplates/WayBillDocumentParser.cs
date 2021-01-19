using QS.DocTemplates;
using Vodovoz.Domain.Documents;

namespace Vodovoz.DocTemplates
{
    public class WayBillDocumentParser : DocParserBase<WayBillDocument>
    {
        public override void UpdateFields()
        {
            fieldsList.Clear();

            AddField(x => x.CarModel, "МаркаАвтомобиля", PatternFieldType.FString);
            AddField(x => x.CarRegistrationNumber, "АвтомобильНомер", PatternFieldType.FString);
            AddField(x => x.DriverFIO, "ВодительФИО", PatternFieldType.FString);
            AddField(x => x.Organization.Name, "ОрганизацияНазвание", PatternFieldType.FString);
			// AddField(x => x.Date.ToString("dd.MM.yyyy"), "ДатаДоверенности", PatternFieldType.FString);

			
			// AddField(x => x.ExpirationDate.ToString("dd.MM.yyyy"), "ДатаОкончания", PatternFieldType.FString);
			// AddField(x => !x.TicketDate.HasValue ? "\t" : x.TicketDate.Value.ToString("dd.MM.yyyy"), "ДатаНаряда", PatternFieldType.FString);
			// AddField(x => x.TicketNumber ?? "\t", "НомерНаряда", PatternFieldType.FString);
			//
			// AddField(x => x.Organization.Name, PatternFieldType.FString);
			// AddField(x => x.Organization.FullName, PatternFieldType.FString);
			// AddField(x => x.Organization.Address, PatternFieldType.FString);
			// AddField(x => x.Organization.JurAddress, PatternFieldType.FString);
			// AddField(x => x.Organization.INN, PatternFieldType.FString);
			// AddField(x => x.Organization.KPP, PatternFieldType.FString);
			// AddField(x => x.Organization.OKPO, PatternFieldType.FString);
			// AddField(x => x.Organization.OKVED, PatternFieldType.FString);
			// AddField(x => x.Organization.OGRN, PatternFieldType.FString);
			// AddField(x => x.Organization.DefaultAccount.Number, PatternFieldType.FString);
			// AddField(x => x.Organization.DefaultAccount.InBank.Bik, PatternFieldType.FString);
			// AddField(x => x.Organization.DefaultAccount.InBank.Name, PatternFieldType.FString);
			// AddField(x => x.Organization.DefaultAccount.BankCorAccount.CorAccountNumber, PatternFieldType.FString);
			// AddField(x => x.Organization.Leader.FullName, PatternFieldType.FString);
			// AddField(x => x.Organization.Leader.ShortName, PatternFieldType.FString);
			// AddField(x => x.Organization.Buhgalter.FullName, PatternFieldType.FString);
			// AddField(x => x.Organization.Buhgalter.ShortName, PatternFieldType.FString);
			//
			// AddField(x => x.Supplier.Name, PatternFieldType.FString);
			// AddField(x => x.Employee.FullName, PatternFieldType.FString);
			// AddField(x => x.Employee.ShortName, PatternFieldType.FString);
			// AddField(x => x.EmployeeDocument.PassportSeria, PatternFieldType.FString);
			// AddField(x => x.EmployeeDocument.PassportNumber, PatternFieldType.FString);
			// AddField(x => x.EmployeeDocument.PassportIssuedOrg, PatternFieldType.FString);
			// AddField(x => x.EmployeeDocument.PassportIssuedDate.HasValue ? x.EmployeeDocument.PassportIssuedDate.Value.ToString("dd.MM.yyyy") : "", "Водитель.ДатаВыдачиПаспорта", PatternFieldType.FString);
			// AddField(x => x.Employee.AddressRegistration, PatternFieldType.FString);

			SortFields();
        }
    }
}