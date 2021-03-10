#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitrixApi.DTO
{
     public class CompanyRequest
    {
        [JsonProperty("result")] public Company Result { get; set; }
        [JsonProperty("time")]  public ResponseTime ResponseTime { get; set; }
    }

    public class Company
    {
        [JsonProperty("ID")] public uint Id { get; set; }
        [JsonProperty("COMPANY_TYPE")] public string? COMPANY_TYPE { get; set; }
        [JsonProperty("TITLE")] public string? Title { get; set; }
        [JsonProperty("LEAD_ID")] public string? LEAD_ID { get; set; }
        [JsonProperty("HAS_PHONE")] public string? HAS_PHONE { get; set; }
        [JsonProperty("HAS_EMAIL")] public string? HAS_EMAIL { get; set; }
        [JsonProperty("HAS_IMOL")] public string? HAS_IMOL { get; set; }
        [JsonProperty("ASSIGNED_BY_ID")] public string? ASSIGNED_BY_ID { get; set; }
        [JsonProperty("CREATED_BY_ID")] public string? CREATED_BY_ID { get; set; }
        [JsonProperty("MODIFY_BY_ID")] public string? MODIFY_BY_ID { get; set; }
        [JsonProperty("INDUSTRY")] public string? INDUSTRY { get; set; } // Сфера деятельности. Выбирается из списка(IT например )
        [JsonProperty("CURRENCY_ID")] public int? CURRENCY_ID { get; set; }
        [JsonProperty("EMPLOYEES")] public string? EMPLOYEES { get; set; } //Количество сотрудников. Выбирается из списка
        [JsonProperty("COMMENTS")] public string? COMMENTS { get; set; }
        [JsonProperty("DATE_CREATE")] public DateTime DateCreate { get; set; }
        [JsonProperty("DATE_MODIFY")] public DateTime DATE_MODIFY { get; set; }
        [JsonProperty("OPENED")] public string? OPENED { get; set; } //Флаг "Доступна для всех" 
        [JsonProperty("ADDRESS")] public string? ADDRESS { get; set; } //Улица, дом, корпус, строение (фактический адрес)
        [JsonProperty("ADDRESS_2")] public string? ADDRESS_2 { get; set; } //Квартира / офис (фактический адрес)
        [JsonProperty("ADDRESS_CITY")] public string? ADDRESS_CITY { get; set; }//Город (фактический адрес)
        [JsonProperty("ADDRESS_POSTAL_CODE")] public string? ADDRESS_POSTAL_CODE { get; set; }
        [JsonProperty("ADDRESS_REGION")] public string? ADDRESS_REGION { get; set; }
        [JsonProperty("ADDRESS_PROVINCE")] public string? ADDRESS_PROVINCE { get; set; } //Область (фактический адрес)
        [JsonProperty("ADDRESS_COUNTRY")] public string? ADDRESS_COUNTRY { get; set; }
        [JsonProperty("ADDRESS_COUNTRY_CODE")] public string? ADDRESS_COUNTRY_CODE { get; set; }
        [JsonProperty("REG_ADDRESS")] public string? REG_ADDRESS { get; set; }
        [JsonProperty("REG_ADDRESS_2")] public string? REG_ADDRESS_2 { get; set; } //Квартира / офис (юридический адрес)
        [JsonProperty("REG_ADDRESS_CITY")] public string? REG_ADDRESS_CITY { get; set; } //Город (юридический адрес)
        [JsonProperty("REG_ADDRESS_POSTAL_CODE")] public string? REG_ADDRESS_POSTAL_CODE { get; set; } //Почтовый индекс (юридический адрес)
        [JsonProperty("REG_ADDRESS_REGION")] public string? REG_ADDRESS_REGION { get; set; } //Район (юридический адрес)
        [JsonProperty("REG_ADDRESS_PROVINCE")] public string? REG_ADDRESS_PROVINCE { get; set; } //Область (юридический адрес)
        [JsonProperty("REG_ADDRESS_COUNTRY")] public string? REG_ADDRESS_COUNTRY { get; set; } //Страна (юридический адрес)
        [JsonProperty("REG_ADDRESS_COUNTRY_CODE")] public string? REG_ADDRESS_COUNTRY_CODE { get; set; } //Код Страны (юридический адрес)
        [JsonProperty("BANKING_DETAILS")] public string? BANKING_DETAILS { get; set; } // Банковские реквизиты
        [JsonProperty("ADDRESS_LEGAL")] public string? ADDRESS_LEGAL { get; set; }
        
        [JsonProperty("UF_CRM_5DB83D2D840E6")] public string? PaymentType { get; set; }
        [JsonProperty("UF_CRM_5DB83D2D9FE1F")] public string? AddressWithoutHouse { get; set; }
        //550 - Жилой дом, 558 - Прочее, 562 Торговый центр, 556 Общежитие, 548 Детский лагерь, 552 Мероприятие, 554 Морское судно 546 База отдыха, 560 Строительный объект, 
        [JsonProperty("UF_CRM_5DB83D2DB1327")] public string? ObjectType { get; set; }
        //Всегда пустое, 564 - Парадная, 566 Торговый комплекс, 568 Торговый центр, 570 - Бизнес центр, 572 - Школа, 574 - Общежитие
        [JsonProperty("UF_CRM_5DB83D2DD7C22")] public string? ObjectSubType { get; set; }
        //Параюдная / Название БЦ, всегда пустое
        [JsonProperty("UF_CRM_5DB83D2DED598")] public string? Entrance { get; set; }
        //Тип помещения всегда пустое
        //576 - Квартира, 578 Офис, 580 Склад, 582 Помещение, 584 Комната, 586 Секция
        [JsonProperty("UF_CRM_5DB83D2E05E81")] public string? RoomType { get; set; }

        
        [JsonProperty("PHONE")] public IList<Phone> Phones { get; set; }
    }
    
    
}