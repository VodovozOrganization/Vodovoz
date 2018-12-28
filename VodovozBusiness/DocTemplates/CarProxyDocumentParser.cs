using System;
using QSDocTemplates;
using Vodovoz.Domain.Employees;

namespace Vodovoz.DocTemplates
{
	public class CarProxyDocumentParser : DocParserBase<CarProxyDocument>
	{
		public CarProxyDocumentParser()
		{
		}

		public override void UpdateFields()
		{
			fieldsList.Clear();

			AddField(x => x.Id, "НомерДоверенности",PatternFieldType.FString);
			AddField(x => x.Date.ToString("dd.MM.yyyy"), "ДатаДоверенности", PatternFieldType.FString);
			AddField(x => x.ExpirationDate.ToString("dd.MM.yyyy"), "ДатаОкончания", PatternFieldType.FString);

			AddField(x => x.Organization.Name, PatternFieldType.FString);
			AddField(x => x.Organization.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.Address, PatternFieldType.FString);
			AddField(x => x.Organization.JurAddress, PatternFieldType.FString);
			AddField(x => x.Organization.INN, PatternFieldType.FString);
			AddField(x => x.Organization.KPP, PatternFieldType.FString);
			AddField(x => x.Organization.OKPO, PatternFieldType.FString);
			AddField(x => x.Organization.OKVED, PatternFieldType.FString);
			AddField(x => x.Organization.OGRN, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.Number, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.InBank.Bik, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.InBank.Name, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.BankCorAccount.CorAccountNumber, PatternFieldType.FString);
			AddField(x => x.Organization.Leader.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.Leader.ShortName, PatternFieldType.FString);
			AddField(x => x.Organization.Buhgalter.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.Buhgalter.ShortName, PatternFieldType.FString);

			AddField(x => x.Car.Model, PatternFieldType.FString);
			AddField(x => x.Car.RegistrationNumber, PatternFieldType.FString);
			AddField(x => x.Car.VIN, PatternFieldType.FString);
			AddField(x => x.Car.ManufactureYear, PatternFieldType.FString);
			AddField(x => x.Car.MotorNumber, PatternFieldType.FString);
			AddField(x => x.Car.ChassisNumber, PatternFieldType.FString);
			AddField(x => x.Car.Carcase, PatternFieldType.FString);
			AddField(x => x.Car.Color, PatternFieldType.FString);
			AddField(x => x.Car.DocSeries, PatternFieldType.FString);
			AddField(x => x.Car.DocNumber, PatternFieldType.FString);
			AddField(x => x.Car.DocIssuedOrg, PatternFieldType.FString);
			AddField(x => x.Car.DocIssuedDate, PatternFieldType.FString);

			AddField(x => x.Driver.FullName, PatternFieldType.FString);
			AddField(x => x.Driver.ShortName, PatternFieldType.FString);
			//TODO: Проверка на наличие паспорта и правильный индекс
			AddField(x => x.Driver.Documents[0].PassportSeria, PatternFieldType.FString);
			AddField(x => x.Driver.Documents[0].PassportNumber, PatternFieldType.FString);
			AddField(x => x.Driver.Documents[0].PassportIssuedOrg, PatternFieldType.FString);
			AddField(x => x.Driver.Documents[0].PassportIssuedDate.HasValue ? x.Driver.Documents[0].PassportIssuedDate.Value.ToString("dd.MM.yyyy") : "", "Водитель.ДатаВыдачиПаспорта", PatternFieldType.FString);
			AddField(x => x.Driver.AddressRegistration, PatternFieldType.FString);

			SortFields();
		}
	}
}
