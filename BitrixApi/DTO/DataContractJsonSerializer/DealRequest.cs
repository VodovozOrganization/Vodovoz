using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using QS.Osm.DTO;
using Vodovoz.Domain.Client;
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
        
        [DataMember(Name="ID")]  public uint Id { get; set; }
        [DataMember(Name="TITLE")]  public string Title { get; set; }
        [DataMember(Name="TYPE_ID")] public string TipeId { get; set; }
        [DataMember(Name="STAGE_ID")] public string StageId { get; set; }
        [DataMember(Name="CURRENCY_ID")]  public string CurrencyId { get; set; }
        [DataMember(Name="OPPORTUNITY")]  public decimal Opportunity { get; set; }
        [DataMember(Name="IS_MANUAL_OPPORTUNITY")]  public string IsManualOpportunity { get; set; }
        [DataMember(Name="TAX_VALUE")]  public decimal TaxValue { get; set; }
        [DataMember(Name="LEAD_ID")] public string LeadId { get; set; }
        [DataMember(Name="COMPANY_ID")] public string CompanyId { get; set; }
        [DataMember(Name="CONTACT_ID")] public string ContancId { get; set; }
        [DataMember(Name="QUOTE_ID")]  public string QuioteId { get; set; }
        [DataMember(Name="BEGINDATE")]  public DateTime BegunDate { get; set; }
        [DataMember(Name="CLOSEDATE")]  public DateTime CloseDate { get; set; }
        [DataMember(Name="ASSIGNED_BY_ID")]  public int AssignedById { get; set; }
        [DataMember(Name="CREATED_BY_ID")] public int CreatedById { get; set; } 
        [DataMember(Name="MODIFY_BY_ID")]  public int ModifyById { get; set; }
        [DataMember(Name="DATE_CREATE")]  public DateTime DateCreate { get; set; }
        [DataMember(Name="DATE_MODIFY")]  public DateTime DateModyfy { get; set; }
        [DataMember(Name="UF_CRM_5DA9BBA018649")]  public DateTime DeliveryDate { get; set; }
        [DataMember(Name="OPENED")]  public string Opened { get; set; }
        [DataMember(Name="CLOSED")]  public string Closed { get; set; }
        [DataMember(Name="UF_CRM_1603522128")]  public string Status { get; set; }
        //2 значения разделены запятой, пример: 59.852624,30.226881
        [DataMember(Name="UF_CRM_1611649517604")]  public string Coordinates { get; set; }
        [DataMember(Name="UF_CRM_5DA85CF9E13B9")]  public string DeliveryAddressWithoutHouse { get; set; }
        [DataMember(Name="UF_CRM_5DA85CFA4B2FD")]  public string RoomNumber { get; set; }
        [DataMember(Name="UF_CRM_5DADB4A25AFE5")]  public string HouseAndBuilding { get; set; }
        [DataMember(Name="UF_CRM_1593010244990")]  public string PaymentStatus { get; set; }
        // Может быть Null or empty
        [DataMember(Name="UF_CRM_1596187803")]  public string Promocode { get; set; }
        //624 - Курьерская, 626 - Самовывоз
        [DataMember(Name="UF_CRM_1573799954786")]  public string DeliveryType { get; set; }
        //158 - Курьеру наличными, 160 - Картой на сайте, 162 - На расчетный счет, 1108 - Курьеру по смс, 1162 - Курьеру по терминалу
        [DataMember(Name="UF_CRM_5DA85CF9B48E1")]  public string PaymentMethod { get; set; }
        [DataMember(Name="UF_CRM_1596187445")]  public string City { get; set; }
        [DataMember(Name="UF_CRM_5DA85CFA35DAD")]  public string RoomType { get; set; } //Парадная/Торговый комплекс/Торговый центр...
        [DataMember(Name="UF_CRM_5DA85CFA297D5")]  public string Entrance { get; set; } //Парадная/Название БЦ
        [DataMember(Name="UF_CRM_1575544790252")]  public string Floor { get; set; } 
        
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
                _ => throw new NotImplementedException("Неизвестный id типа помещения")
            };
        }
        
        public string GetRoom()
        {
            if (HouseAndBuilding == null)
                throw new Exception("HouseAndBuilding = null");
            
            var a = NumbersUtils.GetNumbersFromString(HouseAndBuilding).ToArray();
            if (a.Count() == 2){
                return a[1].ToString();
            }
            else{
                throw new FormatException($"Поле 'Дом и квартира': {HouseAndBuilding} не содержат 2х чисел(для получения номера квартиры)");
            }
        }
        
        public string GetBuilding()
        {
            if (HouseAndBuilding == null)
                throw new Exception("HouseAndBuilding = null");
            
            var a = NumbersUtils.GetNumbersFromString(HouseAndBuilding).ToArray();
            if (a.Count() == 2){
                return a[0].ToString();
            }
            else{
                throw new FormatException($"Поле 'Дом и квартира': {HouseAndBuilding} не содержат 2х чисел(для получения номера квартиры)");
            }
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
                _ => throw new NotImplementedException("Неизвестный id типа оплаты")
            };
        }
    }
}