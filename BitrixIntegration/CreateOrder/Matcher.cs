using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BitrixApi.DTO;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Repositories;
using Vodovoz.Repository.Client;
using VodovozInfrastructure.Utils;
using Contact = BitrixApi.DTO.Contact;
using NomenclatureRepository = Vodovoz.EntityRepositories.Goods.NomenclatureRepository;

namespace BitrixIntegration {
    public class Matcher {
        
        #region ByBitrixId
        public static bool MatchOrderByBitrixId(/*IUnitOfWork uow,*/ Deal deal, out Order outOrder)
        {
            using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                Order order = null;
                order = OrderSingletonRepository.GetInstance().GetOrderByBitrixId(uow, deal.Id);
                
                if (order == null){
                    outOrder = null;
                    return false;
                }
                else{
                    outOrder = order;
                    return true;
                }
            }
        }
        
        public static bool MatchCounterpartyByBitrixId(/*IUnitOfWork uow,*/ uint bitrixId, out Counterparty outCounterparty)
        {
            using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                Counterparty counterparty = null;
                counterparty = CounterpartyRepository.GetCounterpartyByBitrixId(uow, bitrixId);
                
                if (counterparty == null){
                    outCounterparty = null;
                    return false;
                }
                else{
                    outCounterparty = counterparty;
                    return true;
                }
            }
        }
        
        public static bool MatchNomenclatureByBitrixId(/*IUnitOfWork uow,*/ uint productId, out Nomenclature outNomenclature)
        {
            using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                Nomenclature nomenclature = null;
                
                nomenclature = NomenclatureRepository.GetNommenclatureByBitrixId(uow, productId);
                
                if (nomenclature == null){
                    outNomenclature = null;
                    return false;
                }
                else{
                    outNomenclature = nomenclature;
                    return true;
                }
            }
        }

        #endregion ByBitrixId
            
        
        public static bool MatchCompanyPhoneAndName(Company company, out Counterparty outCounterparty)
        {
            var uow = UnitOfWorkFactory.CreateWithoutRoot();
            //Формат записанный в Value +7 (981) 944-86-31
            var phone = company.PHONE.First().VALUE;
            var digitsNum = PhoneUtils.NumberTrim(phone, out var _);

            IList<Counterparty> counterparties = null;
            
            counterparties = CounterpartyRepository.GetCounterpartesByPartOfName(
                uow,
                company.Title ?? 
                throw new NullReferenceException("Контакт не содержит имени, фамилии и отчества, необходимых для сопоставления"),
                digitsNum
            );
            
            if (counterparties.Count == 1){
                outCounterparty = counterparties.First();
                return true;
            }
            else{
                outCounterparty = null;
                return false;
            }
        }

        /// <summary>
        /// Находит контрагента по номеру и ФИО контакта
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="contact"></param>
        /// <param name="outCounterparty"></param>
        /// <exception cref="NullReferenceException">Контакт не содержит имени, фамилии и отчества, необходимых для сопоставления</exception>
        public static bool MatchCounterpartyByPhoneAndSecondName(/*IUnitOfWork uow,*/ Contact contact, out Counterparty outCounterparty)
        {
            var uow = UnitOfWorkFactory.CreateWithoutRoot();
            //Формат записанный в Value +7 (981) 944-86-31
            var phone = contact.PHONE.First().VALUE;
            var digitsNum = PhoneUtils.NumberTrim(phone, out var _);

            // digitsNum = "9215667037";
            // contact.NAME = null;
            // contact.LAST_NAME = null;
            // contact.SECOND_NAME = "Исаевич";
            
            IList<Counterparty> counterparties = null;
            
            counterparties = CounterpartyRepository.GetCounterpartesByPartOfName(
                uow,
                contact.SECOND_NAME?? contact.LAST_NAME ?? contact.NAME ?? 
                    throw new NullReferenceException("Контакт не содержит имени, фамилии и отчества, необходимых для сопоставления"),
                digitsNum
            );
            
            if (counterparties.Count == 1){
                outCounterparty = counterparties.First();
                return true;
            }
            else{
                outCounterparty = null;
                return false;
            }
        }

        /// <summary>
        /// Находит точку доставки по номеру и ФИО 
        /// </summary>
        /// <exception cref="NullReferenceException">deal = null</exception>
        public static bool MatchDeliveryPoint(Deal deal, Counterparty counterparty, out DeliveryPoint outDeliveryPoint)
        {
            if (deal == null) throw new ArgumentNullException(nameof(deal));
            
            if (!deal.Coordinates.Contains(',')){
                // logger("Ошибка в формате координат");
                outDeliveryPoint = null;
                return false;
            }
            
            //TODO сначала проверить по битрикс Id
            
            //parsing coordinates from deal
            var splitted = deal.Coordinates.Split(',');
            var latitudeString = splitted[0].Trim();
            var longitudeString = splitted[1].Trim();

            if (decimal.TryParse(longitudeString,NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude) &&
                decimal.TryParse(latitudeString, NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude)){
                
                var uow = UnitOfWorkFactory.CreateWithoutRoot();
                IList<DeliveryPoint> deliveryPoints = DeliveryPointRepository.GetDeliveryPointForCounterpartyByCoordinates(uow, latitude, longitude, counterparty.Id);
            
                if (deliveryPoints.Count == 1){
                    outDeliveryPoint = deliveryPoints.First();
                    return true;
                }
                // В одном доме несколько наших клиентов
                else if(deliveryPoints.Count > 1){
                    //СОПОСТАВЛЯЕМ ПО ДОМУ УЛИЦЕ ИТД
                    foreach (var dp in deliveryPoints){
                       if(dp.Room != null && dp.Room != "." && dp.Room != "-") {
                            // Тк кк значения бывают такие "13-Н ком.21"
                            if (dp.Room.Contains(deal.RoomNumber)){
                                outDeliveryPoint = dp;
                                return true;
                            }
                            else{
                                // Более медленный способ, на случай если RoomNumber содержит не только одно число, проверяем по вхождению каждого числа по отдельности
                                var numbersFromRoom = NumbersUtils.GetNumbersFromString(deal.RoomNumber);
                                foreach (var num in numbersFromRoom){
                                    if (dp.Room.Contains(num.ToString())){
                                        outDeliveryPoint = dp;
                                        return true;
                                    }
                                }
                            }
                       }
                    }
                
                    outDeliveryPoint = null;
                    return false;
                }
                else{
                    // У контрагента не нашлось точки доставки с координатами из битрикса
                    // Берем все точки доставки контрагента и пытаемся сопоставить по наличию в них дома + квартиры
                    var deliveryPointsForCounterparty = DeliveryPointRepository.DeliveryPointsForCounterpartyQuery(uow, counterparty);
                    foreach (var dp in deliveryPointsForCounterparty){
                        var numsFromHouse = NumbersUtils.GetNumbersFromString(deal.HouseAndBuilding);
                        var numsFromRoom = (NumbersUtils.GetNumbersFromString(deal.RoomNumber));
                        var flag1 = false;
                        var flag2 = false;
                        
                        foreach (var i in numsFromHouse)
                            if (dp.Building.Contains(i.ToString())) 
                                flag1 = true;
                        
                        foreach (var i in numsFromRoom)
                            if (dp.Room.Contains(i.ToString())) 
                                flag2 = true;
                        
                        if (flag1 && flag2){ 
                            outDeliveryPoint = dp; 
                            return true;
                        };
                    }
                    outDeliveryPoint = null;
                    return false;
                }
            } 
            else{
                throw new Exception($"Ошибка в парсинге координат{longitudeString} и {latitudeString}");
            }
        }

        public static bool MatchNomenclature(Product product, out Nomenclature outNomenclature)
        {
            var uow = UnitOfWorkFactory.CreateWithoutRoot();
            
            //Сопоставление номенклатуры
            Nomenclature nomenclature = Vodovoz.EntityRepositories.Goods.NomenclatureRepository.GetNomenclatureFromBitrixProduct(uow, product);
            if (nomenclature != null){
                
                //Сравнение информации о номенклатуре и обновление если она отличается
                //Проверять по id интернет магазинов, есть справочник интернет магазинов, если указана любая кроме первой значит они пришли извне
                if (nomenclature.NomenclaturePrice.First(x => x.MinCount == 1).Price != product.PRICE){
                    //Обновить цену
                    
                }
                
                outNomenclature = nomenclature;
                return true;
            }

            outNomenclature = null;
            return false;
        }
        
        
        //Функция не нужна, если у нас в базе эта категория не найдена,
        //должна будет создаваться категория в группе 502, с каким нибудь системным названием типа "Без названия".
        //Позже служба актуализирует информацию по ней
        
        // public static bool MatchNomenclatureGroup(Product product, out ProductGroup outProductGroup)
        // {
        //     var uow = UnitOfWorkFactory.CreateWithoutRoot();
        //
        //     var allProductGroups = product.CategoryObj.Category.Split('/');
        //     
        //     foreach (var productGroupName in allProductGroups){
        //         var group = Vodovoz.EntityRepositories.Goods.NomenclatureRepository.GetProductGroupFromBitrixProductGroup(uow, productGroupName);
        //         if (group != null){
        //             
        //         }
        //     }
        //     
        //     //Сопоставление номенклатуры
        //     Nomenclature nomenclature = Vodovoz.EntityRepositories.Goods.NomenclatureRepository.GetProductGroupFromBitrixProductGroup(uow, product);
        //     if (nomenclature != null){
        //         
        //         //Сравнение информации о номенклатуре и обновление если она отличается
        //         
        //         
        //         outProductGroup = nomenclature;
        //         return true;
        //     }
        //
        //     outProductGroup = null;
        //     return false;
        // }
    }
}