using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class Company
    {
        [JsonProperty("ID")] 
        public uint Id { get; set; }
        
        [JsonProperty("COMPANY_TYPE")] 
        public string CompanyType { get; set; }
        
        [JsonProperty("TITLE")] 
        public string Title { get; set; }
        
        [JsonProperty("LEAD_ID")] 
        public string LeadId { get; set; }
        
        [JsonProperty("HAS_PHONE")] 
        public string HasPhone { get; set; }
        
        [JsonProperty("HAS_EMAIL")] 
        public string HasEmail { get; set; }
        
        [JsonProperty("HAS_IMOL")] 
        public string HasIMOL { get; set; }
        
        [JsonProperty("ASSIGNED_BY_ID")] 
        public string AssignedById { get; set; }
        
        [JsonProperty("CREATED_BY_ID")] 
        public string CreatedById { get; set; }
        
        [JsonProperty("MODIFY_BY_ID")] 
        public string ModifyById { get; set; }
        
        /// <summary>
        /// Сфера деятельности. Выбирается из списка(IT например )
        /// </summary>
        [JsonProperty("INDUSTRY")]
        public string Industry { get; set; } 
        
        [JsonProperty("CURRENCY_ID")] 
        public int? CurrencyId { get; set; }
        
        /// <summary>
        /// Количество сотрудников. Выбирается из списка
        /// </summary>
        [JsonProperty("EMPLOYEES")]
        public string Employees { get; set; }
        
        [JsonProperty("COMMENTS")] 
        public string Comments { get; set; }
        
        [JsonProperty("DATE_CREATE")] 
        public DateTime DateCreate { get; set; }
        
        [JsonProperty("DATE_MODIFY")] 
        public DateTime DateModify { get; set; }
        
        /// <summary>
        /// Флаг "Доступна для всех"
        /// </summary>
        [JsonProperty("OPENED")]
        public string Opened { get; set; }
        
        /// <summary>
        /// Улица, дом, корпус, строение (фактический адрес)
        /// </summary>
        [JsonProperty("ADDRESS")]
        public string Address { get; set; }
        
        /// <summary>
        /// Квартира / офис (фактический адрес)
        /// </summary>
        [JsonProperty("ADDRESS_2")]
        public string Address2 { get; set; } 
        
        /// <summary>
        /// Город (фактический адрес)
        /// </summary>
        [JsonProperty("ADDRESS_CITY")]
        public string AddressCity { get; set; }
        
        [JsonProperty("ADDRESS_POSTAL_CODE")] 
        public string AddressPostalCode { get; set; }
        
        [JsonProperty("ADDRESS_REGION")] 
        public string AddressRegion { get; set; }
        
        /// <summary>
        /// Область (фактический адрес)
        /// </summary>
        [JsonProperty("ADDRESS_PROVINCE")]
        public string AddressProvince { get; set; } 
        
        [JsonProperty("ADDRESS_COUNTRY")] 
        public string AddressCountry { get; set; }
        
        [JsonProperty("ADDRESS_COUNTRY_CODE")] 
        public string AddressCountryCode { get; set; }
        
        [JsonProperty("REG_ADDRESS")] 
        public string RegAddress { get; set; }
        
        /// <summary>
        /// Квартира / офис (юридический адрес)
        /// </summary>
        [JsonProperty("REG_ADDRESS_2")]
        public string RegAddress2 { get; set; } 
        
        /// <summary>
        /// Город (юридический адрес)
        /// </summary>
        [JsonProperty("REG_ADDRESS_CITY")]
        public string RegAddressCity { get; set; }
        
        /// <summary>
        /// Почтовый индекс (юридический адрес)
        /// </summary>
        [JsonProperty("REG_ADDRESS_POSTAL_CODE")]
        public string RegAddressPostalCode { get; set; } 
        
        /// <summary>
        /// Район (юридический адрес)
        /// </summary>
        [JsonProperty("REG_ADDRESS_REGION")]
        public string RegAddressRegion { get; set; }
        
        /// <summary>
        /// Область (юридический адрес)
        /// </summary>
        [JsonProperty("REG_ADDRESS_PROVINCE")]
        public string RegAddressProvince { get; set; } 
        
        /// <summary>
        /// Страна (юридический адрес)
        /// </summary>
        [JsonProperty("REG_ADDRESS_COUNTRY")]
        public string RegAddressCountry { get; set; } 
        
        /// <summary>
        /// Код Страны (юридический адрес)
        /// </summary>
        [JsonProperty("REG_ADDRESS_COUNTRY_CODE")]
        public string RegAddressCountryCode { get; set; }
        
        /// <summary>
        /// Банковские реквизиты
        /// </summary>
        [JsonProperty("BANKING_DETAILS")]
        public string BankingDetails { get; set; }
        
        [JsonProperty("ADDRESS_LEGAL")] 
        public string AddressLegal { get; set; }
        
        [JsonProperty("UF_CRM_5DB83D2D840E6")] 
        public string PaymentType { get; set; }
        
        [JsonProperty("UF_CRM_5DB83D2D9FE1F")] 
        public string AddressWithoutHouse { get; set; }
        
        /// <summary>
        /// 550 - Жилой дом, 558 - Прочее, 562 Торговый центр, 556 Общежитие, 548 Детский лагерь,
        /// 552 Мероприятие, 554 Морское судно 546 База отдыха, 560 Строительный объект, 
        /// </summary>
        [JsonProperty("UF_CRM_5DB83D2DB1327")] 
        public string ObjectType { get; set; }
        
        /// <summary>
        /// Всегда пустое, 564 - Парадная, 566 Торговый комплекс, 568 Торговый центр, 570 - Бизнес центр, 572 - Школа, 574 - Общежитие
        /// </summary>
        [JsonProperty("UF_CRM_5DB83D2DD7C22")] 
        public string ObjectSubType { get; set; }
        
        /// <summary>
        /// Параюдная / Название БЦ, всегда пустое
        /// </summary>
        [JsonProperty("UF_CRM_5DB83D2DED598")] 
        public string Entrance { get; set; }

        /// <summary>
        ///Тип помещения всегда пустое
        ///576 - Квартира, 578 Офис, 580 Склад, 582 Помещение, 584 Комната, 586 Секция
        /// </summary>
        [JsonProperty("UF_CRM_5DB83D2E05E81")] 
        public string RoomType { get; set; }
        
        [JsonProperty("PHONE")] 
        public IList<Phone> Phones { get; set; }
    }
}