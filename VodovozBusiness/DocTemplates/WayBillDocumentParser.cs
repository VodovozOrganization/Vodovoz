using QS.DocTemplates;
using System.Globalization;
using Vodovoz.Domain.Documents;

namespace Vodovoz.DocTemplates
{
    public class WayBillDocumentParser : DocParserBase<WayBillDocument>
    {
        public WayBillDocumentParser()
        {

        }

        public override void UpdateFields()
        {
            fieldsList.Clear();

            var rus = CultureInfo.GetCultureInfo("ru-RU");

            // 1 колонка
            // 1 секция 1й колонки
            AddField(x => x.Id, "Идентификатор", PatternFieldType.FString);
            
            AddField(x => x.Date.Day.ToString(), "Дата.День", PatternFieldType.FString);
            AddField(x => rus.DateTimeFormat.MonthGenitiveNames[x.Date.Month - 1], "Дата.Месяц", PatternFieldType.FString);
            AddField(x => x.Date.Year.ToString(), "Дата.Год", PatternFieldType.FString);

            AddField(x => x.Organization.Name, "Организация.Название", PatternFieldType.FString);

            AddField(x => x.CarModelName, "Автомобиль.Модель", PatternFieldType.FString);
            AddField(x => x.CarRegistrationNumber, "Автомобиль.Номер", PatternFieldType.FString);

            AddField(x => x.DriverFIO, "Водитель.ФИО", PatternFieldType.FString);
            AddField(x => x.DriverLicense, "Водитель.Удостоверение", PatternFieldType.FString);
            AddField(x => "B", "Водитель.Класс", PatternFieldType.FString);

            AddField(x => x.CarPassportSerialNumber, "Автомобиль.СерияПТС", PatternFieldType.FString);
            AddField(x => x.CarPassportNumber, "Автомобиль.НомерПТС", PatternFieldType.FString);

            // 2 секция 1й колонки 
            // 1.1 колонка

            AddField(x => x.FirstAddress, "АдресПервойПодачи", PatternFieldType.FString);

            AddField(x => x.GarageLeavingDateTime.ToString("dd.MM.yyyy HH:mm"), "Выезд.Дата", PatternFieldType.FString);
            AddField(x => x.GarageReturningDateTime.ToString("dd.MM.yyyy HH:mm"), "Возвращение.Дата", PatternFieldType.FString);

            AddField(x => x.MechanicLastName, "Механик.Фамилия", PatternFieldType.FString);
            AddField(x => x.MechanicFIO, "Механик.ФИО", PatternFieldType.FString);
            AddField(x => x.DriverLastName, "Водитель.Фамилия", PatternFieldType.FString);

            // 1.2 колонка

            AddField(x => x.GarageLeavingDateTime.ToString("dd.MM.yyyy"), "ПредрейсовыйКонтроль.Дата", PatternFieldType.FString);
            AddField(x => x.GarageLeavingDateTime.AddHours(-1).ToString("HH:mm"), "ПредрейсовыйКонтроль.Время", PatternFieldType.FString);

            AddField(x => x.CarFuelType.Name, "Топливо.Марка", PatternFieldType.FString);

            AddField(x => x.FuelByFuelList.ToString(), "Топливо.Выдано", PatternFieldType.FString);

            AddField(x => 10, "Топливо.ОстатокПриВыезде", PatternFieldType.FString);
            AddField(x => 10, "Топливо.ОстатокПриВозвращении", PatternFieldType.FString);

            AddField(x => x.CarFuelConsumption.ToString(), "Топливо.РасходПоНорме", PatternFieldType.FString);

            AddField(x => x.CarFuelConsumption.ToString(), "Топливо.РасходПоФакту", PatternFieldType.FString);

            // 2 колонка

            customTablesList.Clear();

            AddTable("Путь", x => x.WayBillDocumentItems)
                .AddColumn(r => r.SequenceNumber, "Номер", PatternFieldType.FString)
                .AddColumn(r => r.CounterpartyName, "КодЗаказчика", PatternFieldType.FString)
                .AddColumn(r => r.AddressFrom ?? "", "МестоОтправления", PatternFieldType.FString)
                .AddColumn(r => r.AddressTo ?? "", "МестоНазначения", PatternFieldType.FString)
                .AddColumn(r => r.HoursFrom.Hours, "ВремяВыезда.Часы", PatternFieldType.FString)
                .AddColumn(r => r.HoursFrom.Minutes, "ВремяВыезда.Минуты", PatternFieldType.FString)
                .AddColumn(r => r.HoursTo.Hours, "ВремяПриезда.Часы", PatternFieldType.FString)
                .AddColumn(r => r.HoursTo.Minutes, "ВремяПриезда.Минуты", PatternFieldType.FString)
                .AddColumn(r => r.Mileage, "Пройдено", PatternFieldType.FString)
                .AddColumn(r => r.DriverLastName ?? "", "Подпись", PatternFieldType.FString);

            AddField(x => 8, "ВсегоВНаряде", PatternFieldType.FString);
            AddField(x => x.TotalMileage.ToString(), "Пройдено", PatternFieldType.FString);

            SortFields();
        }
    }
}
