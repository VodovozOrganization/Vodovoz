using System;
using System.Linq;
using Newtonsoft.Json;
using QS.Osm.DTO;
using Vodovoz.Domain.Client;
using VodovozInfrastructure.Utils;

namespace BitrixApi.DTO
{
    public class DealRequest
    {
        [JsonProperty("result")] public Deal Result { get; set; }
        [JsonProperty("time")] public ResponseTime ResponseTime { get; set; }
    }
    
    public class Deal
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        [JsonProperty("ID")]  public uint Id { get; set; }
        [JsonProperty("TITLE")]  public string Title { get; set; }
        [JsonProperty("TYPE_ID")] public string TipeId { get; set; }
        [JsonProperty("STAGE_ID")] public string StageId { get; set; }
        [JsonProperty("CURRENCY_ID")]  public string CurrencyId { get; set; }
        [JsonProperty("OPPORTUNITY")]  public decimal Opportunity { get; set; }
        [JsonProperty("IS_MANUAL_OPPORTUNITY")]  public string IsManualOpportunity { get; set; }
        [JsonProperty("TAX_VALUE")]  public decimal TaxValue { get; set; }
        [JsonProperty("LEAD_ID")] public string LeadId { get; set; }
        [JsonProperty("COMPANY_ID")] public uint? CompanyId { get; set; }
        [JsonProperty("CONTACT_ID")] public uint ContancId { get; set; }
        [JsonProperty("QUOTE_ID")]  public string QuioteId { get; set; }
        [JsonProperty("BEGINDATE")]  public DateTime BegunDate { get; set; }
        [JsonProperty("CLOSEDATE")]  public DateTime CloseDate { get; set; }
        [JsonProperty("ASSIGNED_BY_ID")]  public int AssignedById { get; set; }
        [JsonProperty("CREATED_BY_ID")] public int CreatedById { get; set; } 
        [JsonProperty("MODIFY_BY_ID")]  public int ModifyById { get; set; }
        [JsonProperty("DATE_CREATE")]  public DateTime CreateDate { get; set; }
        [JsonProperty("DATE_MODIFY")]  public DateTime ModifyDate { get; set; }
        [JsonProperty("UF_CRM_5DA9BBA018649")]  public DateTime DeliveryDate { get; set; }
        [JsonProperty("OPENED")]  public string Opened { get; set; }
        [JsonProperty("CLOSED")]  public string Closed { get; set; }
        [JsonProperty("CATEGORY_ID")]  public int CategoryId { get; set; }
        [JsonProperty("UF_CRM_1603522128")]  public string Status { get; set; }
        [JsonProperty("UF_CRM_1611649517604")]  public string Coordinates { get; set; }
        [JsonProperty("UF_CRM_5DA85CF9E13B9")]  public string DeliveryAddressWithoutHouse { get; set; }
        //Пример значения: "143"
        [JsonProperty("UF_CRM_5DA85CFA4B2FD")]  public string RoomNumber { get; set; }
        //Пример значения: "д 104"
        [JsonProperty("UF_CRM_5DADB4A25AFE5")]  public string HouseAndBuilding { get; set; }
        [JsonProperty("UF_CRM_1593010244990")]  public string PaymentStatus { get; set; }
        // Может быть Null or empty
        [JsonProperty("UF_CRM_1596187803")]  public string Promocode { get; set; }
        //624 - Курьерская, 626 - Самовывоз
        [JsonProperty("UF_CRM_1573799954786")]  public string DeliveryType { get; set; }
        //158 - Курьеру наличными, 160 - Картой на сайте, 162 - На расчетный счет, 1108 - Курьеру по смс, 1162 - Курьеру по терминалу
        [JsonProperty("UF_CRM_5DA85CF9B48E1")]  public string PaymentMethod { get; set; }
        [JsonProperty("UF_CRM_1596187445")]  public string City { get; set; } // Санкт-Петербург
        [JsonProperty("UF_CRM_5DA85CFA35DAD")]  public string RoomType { get; set; } //Парадная/Торговый комплекс/Торговый центр...
        [JsonProperty("UF_CRM_5DA85CFA297D5")]  public string Entrance { get; set; } //Парадная/Название БЦ
        [JsonProperty("UF_CRM_1575544790252")]  public string Floor { get; set; }
        [JsonProperty("UF_CRM_5DA85CFA0C838")] public string EntranceType { get; set; }


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