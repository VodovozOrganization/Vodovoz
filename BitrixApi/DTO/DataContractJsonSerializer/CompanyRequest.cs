#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BitrixApi.DTO.DataContractJsonSerializer
{
    [DataContract]
    public class CompanyRequest
    {
        [DataMember(Name="result")] public Company Result { get; set; }
        [DataMember(Name="time")]  public ResponseTime ResponseTime { get; set; }
    }

    [DataContract]
    public class Company
    {
        [DataMember(Name="ID")] public int Id { get; set; }
        [DataMember(Name="COMPANY_TYPE")] public string? COMPANY_TYPE { get; set; }
        [DataMember(Name="TITLE")] public string? TITLE { get; set; }
        [DataMember(Name="LEAD_ID")] public string? LEAD_ID { get; set; }
        [DataMember(Name="HAS_PHONE")] public string? HAS_PHONE { get; set; }
        [DataMember(Name="HAS_EMAIL")] public string? HAS_EMAIL { get; set; }
        [DataMember(Name="HAS_IMOL")] public string? HAS_IMOL { get; set; }
        [DataMember(Name="ASSIGNED_BY_ID")] public string? ASSIGNED_BY_ID { get; set; }
        [DataMember(Name="CREATED_BY_ID")] public string? CREATED_BY_ID { get; set; }
        [DataMember(Name="MODIFY_BY_ID")] public string? MODIFY_BY_ID { get; set; }
        [DataMember(Name="INDUSTRY")] public string? INDUSTRY { get; set; } // Сфера деятельности. Выбирается из списка(IT например )
        [DataMember(Name="CURRENCY_ID")] public int? CURRENCY_ID { get; set; }
        [DataMember(Name="EMPLOYEES")] public string? EMPLOYEES { get; set; } //Количество сотрудников. Выбирается из списка
        [DataMember(Name="COMMENTS")] public string? COMMENTS { get; set; }
        [DataMember(Name="DATE_CREATE")] public DateTime DATE_CREATE { get; set; }
        [DataMember(Name="DATE_MODIFY")] public DateTime DATE_MODIFY { get; set; }
        [DataMember(Name="OPENED")] public string OPENED { get; set; } //Флаг "Доступна для всех" //TODO gavr to bool
        [DataMember(Name="ADDRESS")] public string? ADDRESS { get; set; } //Улица, дом, корпус, строение (фактический адрес)
        [DataMember(Name="ADDRESS_2")] public string? ADDRESS_2 { get; set; } //Квартира / офис (фактический адрес)
        [DataMember(Name="ADDRESS_CITY")] public string? ADDRESS_CITY { get; set; }//Город (фактический адрес)
        [DataMember(Name="ADDRESS_POSTAL_CODE")] public string? ADDRESS_POSTAL_CODE { get; set; }
        [DataMember(Name="ADDRESS_REGION")] public string? ADDRESS_REGION { get; set; }
        [DataMember(Name="ADDRESS_PROVINCE")] public string? ADDRESS_PROVINCE { get; set; } //Область (фактический адрес)
        [DataMember(Name="ADDRESS_COUNTRY")] public string? ADDRESS_COUNTRY { get; set; }
        [DataMember(Name="ADDRESS_COUNTRY_CODE")] public string? ADDRESS_COUNTRY_CODE { get; set; } //TODO gavr есть еще какой то REG_ADDRESS https://vodovoz.bitrix24.ru/rest/2364/op80mphx95tg819j/crm.company.get.json?id=22
        [DataMember(Name="REG_ADDRESS")] public string? REG_ADDRESS { get; set; }
        [DataMember(Name="REG_ADDRESS_2")] public string? REG_ADDRESS_2 { get; set; } //Квартира / офис (юридический адрес)
        [DataMember(Name="REG_ADDRESS_CITY")] public string? REG_ADDRESS_CITY { get; set; } //Город (юридический адрес)
        [DataMember(Name="REG_ADDRESS_POSTAL_CODE")] public string? REG_ADDRESS_POSTAL_CODE { get; set; } //Почтовый индекс (юридический адрес)
        [DataMember(Name="REG_ADDRESS_REGION")] public string? REG_ADDRESS_REGION { get; set; } //Район (юридический адрес)
        [DataMember(Name="REG_ADDRESS_PROVINCE")] public string? REG_ADDRESS_PROVINCE { get; set; } //Область (юридический адрес)
        [DataMember(Name="REG_ADDRESS_COUNTRY")] public string? REG_ADDRESS_COUNTRY { get; set; } //Страна (юридический адрес)
        [DataMember(Name="REG_ADDRESS_COUNTRY_CODE")] public string? REG_ADDRESS_COUNTRY_CODE { get; set; } //Код Страны (юридический адрес)
        [DataMember(Name="BANKING_DETAILS")] public string? BANKING_DETAILS { get; set; } // Банковские реквизиты
        [DataMember(Name="ADDRESS_LEGAL")] public string? ADDRESS_LEGAL { get; set; }
        
        [DataMember(Name="PHONE")] public IList<Phone> PHONE { get; set; }
    }
    
    
}