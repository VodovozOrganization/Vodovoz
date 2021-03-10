using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using QS.Osm.DTO;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using VodovozInfrastructure.Utils;

namespace BitrixApi.DTO.DataContractJsonSerializer
{
    [DataContract]
    public class DealRequest
    {
        [DataMember(Name="result")] public Deal Result { get; set; }
        [DataMember(Name="time")] public DTO.ResponseTime ResponseTime { get; set; }
    }
    
    [DataContract]
    public class Deal
    {
      private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        [DataMember(Name="ID")]  public uint Id { get; set; }
        [DataMember(Name="TITLE")]  public string Title { get; set; }
        [DataMember(Name="TYPE_ID")] public string TipeId { get; set; }
        [DataMember(Name="STAGE_ID")] public string StageId { get; set; }
        [DataMember(Name="CURRENCY_ID")]  public string CurrencyId { get; set; }
        [DataMember(Name="OPPORTUNITY")]  public decimal Opportunity { get; set; }
        [DataMember(Name="IS_MANUAL_OPPORTUNITY")]  public string IsManualOpportunity { get; set; }
        [DataMember(Name="TAX_VALUE")]  public decimal TaxValue { get; set; }
        [DataMember(Name="LEAD_ID")] public string LeadId { get; set; }
        [DataMember(Name="COMPANY_ID")] public uint? CompanyId { get; set; }
        [DataMember(Name="CONTACT_ID")] public uint ContancId { get; set; }
        [DataMember(Name="COMMENTS")] public string Comment { get; set; }
        [DataMember(Name="QUOTE_ID")]  public string QuioteId { get; set; }
        [DataMember(Name="BEGINDATE")]  public DateTime BegunDate { get; set; }
        [DataMember(Name="CLOSEDATE")]  public DateTime CloseDate { get; set; }
        [DataMember(Name="ASSIGNED_BY_ID")]  public int AssignedById { get; set; }
        [DataMember(Name="CREATED_BY_ID")] public int CreatedById { get; set; } 
        [DataMember(Name="MODIFY_BY_ID")]  public int ModifyById { get; set; }
        [DataMember(Name="DATE_CREATE")]  public DateTime CreateDate { get; set; }
        [DataMember(Name="DATE_MODIFY")]  public DateTime ModifyDate { get; set; }
        [DataMember(Name="UF_CRM_1597998841845")]  public string PartOfTown { get; set; }
        
        [DataMember(Name="UF_CRM_5DA9BBA018649")]  public DateTime DeliveryDate { get; set; }
        [DataMember(Name="OPENED")]  public string Opened { get; set; }
        [DataMember(Name="CLOSED")]  public string Closed { get; set; }
        [DataMember(Name="CATEGORY_ID")]  public int CategoryId { get; set; }
        [DataMember(Name="UF_CRM_1603522128")]  public string Status { get; set; }
        [DataMember(Name="UF_CRM_1611649517604")]  public string Coordinates { get; set; }
        [DataMember(Name="UF_CRM_5DA85CF9E13B9")]  public string DeliveryAddressWithoutHouse { get; set; }
        //Пример значения: "143"
        [DataMember(Name="UF_CRM_5DA85CFA4B2FD")]  public string RoomNumber { get; set; }
        //Пример значения: "д 104"
        [DataMember(Name="UF_CRM_5DADB4A25AFE5")]  public string HouseAndBuilding { get; set; }
        [DataMember(Name="UF_CRM_1593010244990")]  public string PaymentStatus { get; set; }
        // Может быть Null or empty
        [DataMember(Name="UF_CRM_1596187803")]  public string Promocode { get; set; }
        //624 - Курьерская, 626 - Самовывоз
        [DataMember(Name="UF_CRM_1573799954786")]  public string DeliveryType { get; set; }
        //158 - Курьеру наличными, 160 - Картой на сайте, 162 - На расчетный счет, 1108 - Курьеру по смс, 1162 - Курьеру по терминалу
        [DataMember(Name="UF_CRM_5DA85CF9B48E1")]  public string PaymentMethod { get; set; }
        [DataMember(Name="UF_CRM_1596187445")]  public string City { get; set; } // Санкт-Петербург
        [DataMember(Name="UF_CRM_5DA85CFA35DAD")]  public string RoomType { get; set; } //Парадная/Торговый комплекс/Торговый центр...
        [DataMember(Name="UF_CRM_5DA85CFA297D5")]  public string Entrance { get; set; } //Парадная/Название БЦ
        [DataMember(Name="UF_CRM_1575544790252")]  public string Floor { get; set; }
        [DataMember(Name="UF_CRM_5DA85CFA0C838")] public string EntranceType { get; set; }
        [DataMember(Name="UF_CRM_5DA9BBA03A12A")] public uint DeliverySchedule { get; set; }
        [DataMember(Name="UF_CRM_1603521814")] public uint CreateInDV { get; set; }
        
        
        
        
        
        public bool IsSelfDelivery()
        {
            
            if (string.IsNullOrWhiteSpace(DeliveryType)){
                throw new ArgumentNullException(nameof(DeliveryType));
            }

            return DeliveryType switch
            {
                "624" => false,
                "626" => true,
                _ => throw new ArgumentException($"Неизвестный id способа доставки {DeliveryType}")
            };
        }
        
        public RoomType GetRoomType()
        {
            return RoomType switch
            {
                "194" => QS.Osm.DTO.RoomType.Apartment,
                "196" => QS.Osm.DTO.RoomType.Office,
                "198" => QS.Osm.DTO.RoomType.Store,
                "200" => QS.Osm.DTO.RoomType.Room,
                "202" => QS.Osm.DTO.RoomType.Chamber,
                "204" => QS.Osm.DTO.RoomType.Section,
                _ => throw new ArgumentException($"Неизвестный id типа помещения {RoomType}")
            };
        }
        
        public string GetDeliveryScheduleString()
        {
            return DeliverySchedule switch
            {
                402 =>  "с 10 до 18",
                404 =>  "с 18 до 23",
                406 =>  "с 09 до 13",
                606 =>  "с 12 до 18",
                608 =>  "с 12 до 15",
                610 =>  "с 15 до 18",
                1174 => "с 18 до 21",
                1176 => "с 21 до 23",
                _ => throw new ArgumentException($"Неизвестный id типа расписания доставки {DeliverySchedule}")
            };
        }

        public string GetPartOfTown()
        {
            return PartOfTown switch
            {
                "1120" => "Север",
                "1122" => "Юг",
                _ => throw new ArgumentException($"Неизвестный id части города {PartOfTown}")
            };
        }
        
        public OrderPaymentStatus GetOrderPaymentStatus()
        {
            return PaymentStatus switch
            {
                "Не оплачен" => OrderPaymentStatus.UnPaid,
                "Оплачен" => OrderPaymentStatus.Paid,
                "Частично оплачен" => OrderPaymentStatus.PartiallyPaid,
                "Нет" => OrderPaymentStatus.None,
                _ => throw new ArgumentException($"Неизвестный статус оплаты {PaymentStatus}")
            };
        }

         public EntranceType GetEntranceType()
         {
             if (EntranceType == null)
                throw new ArgumentNullException(nameof(EntranceType));
             return EntranceType switch
             {
                 "182" => Vodovoz.Domain.Client.EntranceType.Entrance,
                 "184" => Vodovoz.Domain.Client.EntranceType.TradeComplex,
                 "186" => Vodovoz.Domain.Client.EntranceType.BusinessCenter,
                 "190" => Vodovoz.Domain.Client.EntranceType.School,
                 "192" => Vodovoz.Domain.Client.EntranceType.Hostel,
                 _ => throw new ArgumentException($"Неизвестный id Подтипа объекта {EntranceType}")
             };
         }
         
         public PaymentType GetPaymentMethod()
         {
             return PaymentMethod switch
             {
                 //Курьеру наличными
                 "158" => PaymentType.cash,
                 //Картой на сайте
                 "160" => PaymentType.ByCard,
                 //На расчетный счет
                 "162" => PaymentType.cashless,
                 //Курьеру по смс //TODO gavr добавлять в заказ галку 
                 "1108" => PaymentType.cash,
                 //Курьеру по терминалу
                 "1162" => PaymentType.Terminal,
                 _ => throw new ArgumentException($"Неизвестный id типа оплаты {PaymentMethod}")
             };
         }
        
        public string GetRoom()
        {
            if (HouseAndBuilding == null)
                throw new ArgumentNullException(nameof(HouseAndBuilding));
            
            var a = NumbersUtils.GetNumbersFromString(HouseAndBuilding).ToArray();
            if (a.Count() == 2){
                return a[1].ToString();
            }
            else{
                throw new FormatException($"Поле 'Дом и корпус': {HouseAndBuilding} не содержат 2х чисел(для получения номера квартиры)");
            }
        }
        
        public string GetBuilding()
        {
            if (HouseAndBuilding == null)
                throw new ArgumentNullException(nameof(HouseAndBuilding));
            
            var nums = NumbersUtils.GetNumbersFromString(HouseAndBuilding).ToArray();
            if (nums.Count() == 2){
                return nums[0].ToString();
            }
            else{
                logger.Warn($"Поле 'Дом и корпус': {HouseAndBuilding} не содержат 2х чисел");
                //Может быть ситуация что в поле попал только номер дома или только квартира
                if (DeliveryAddressWithoutHouse.Contains(HouseAndBuilding)){
                    return (HouseAndBuilding);
                }
                else{
                    var firstNum = nums.First().ToString();
                    logger.Warn($"Поле адрес без дома:{DeliveryAddressWithoutHouse} не содержит текст поля {HouseAndBuilding}," +
                                $" попытка сопоставить по числам" );
                    
                    if (DeliveryAddressWithoutHouse.Contains(firstNum)){
                        return (HouseAndBuilding);
                    }
                    
                    throw new FormatException($"Поле 'Дом и корпус': {HouseAndBuilding} не содержат двух чисел и поле" +
                                              $" 'Адрес без дома': {DeliveryAddressWithoutHouse} не содержит числе содержащихся в поле 'Дом и корпус'");
                    
                }
            }
        }
    }
}