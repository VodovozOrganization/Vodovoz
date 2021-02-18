using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BitrixApi.DTO;
using QS.DomainModel.UoW;
using QS.Osm.DTO;
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
        
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #region ByBitrixId
        public bool MatchOrderByBitrixId(IUnitOfWork uow, Deal deal, out Order outOrder)
        {
            // using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
            // {
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
            // }
        }
        
        public bool MatchCounterpartyByBitrixId(IUnitOfWork uow, uint bitrixId, out Counterparty outCounterparty)
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
        
        public bool MatchNomenclatureByBitrixId(IUnitOfWork uow, uint productId, out Nomenclature outNomenclature)
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

        #endregion ByBitrixId
            
        
        public bool MatchCompanyPhoneAndName(IUnitOfWork uow, Company company, out Counterparty outCounterparty)
        {
            //Формат записанный в Value +7 (981) 944-86-31
            var phone = company.PHONE.First().Value;
            var digitsNum = PhoneUtils.NumberTrim(phone, out var _);

            IList<Counterparty> counterparties = null;
            
            counterparties = CounterpartyRepository.GetCounterpartesByPartOfName(
                uow,
                company.Title,
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
        /// <exception cref="NullReferenceException">Контакт не содержит имени, фамилии и отчества, необходимых для сопоставления</exception>
        public bool MatchCounterpartyByPhoneAndSecondName(IUnitOfWork uow, Contact contact, out Counterparty outCounterparty)
        {
            //Формат записанный в Value +7 (981) 944-86-31
            var phone = contact.Phones.First().Value;
            var digitsNum = PhoneUtils.NumberTrim(phone, out var _);

            // digitsNum = "9215667037";
            // contact.NAME = null;
            // contact.LAST_NAME = null;
            // contact.SECOND_NAME = "Исаевич";
            
            IList<Counterparty> counterparties = null;
            
            counterparties = CounterpartyRepository.GetCounterpartesByPartOfName(
                uow,
                contact.SecondName?? contact.LastName ?? contact.Name ?? 
                    throw new NullReferenceException("Контакт не содержит имени, фамилии и отчества, необходимых для сопоставления"),
                digitsNum
            );
            
            if (counterparties.Count == 1){
                outCounterparty = counterparties.First();
                outCounterparty.BitrixId = contact.Id;
                uow.Save(outCounterparty);
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
        public bool MatchDeliveryPoint(IUnitOfWork uow, Deal deal, Counterparty counterparty, out DeliveryPoint outDeliveryPoint)
        {
            if (deal == null) throw new ArgumentNullException(nameof(deal));
            
            if (!deal.Coordinates.Contains(',')){
                logger.Error($"Ошибка в формате координат {deal.Coordinates}, ожидалось разделение запятой");
                outDeliveryPoint = null;
                return false;
            }

            //parsing coordinates from deal
            var splitted = deal.Coordinates.Split(',');
            var latitudeString = splitted[0].Trim();
            var longitudeString = splitted[1].Trim();

            if (decimal.TryParse(longitudeString,NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude) &&
                decimal.TryParse(latitudeString, NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude)){
                
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
                        var hasBuilding = false;
                        var hasRoom = false;
                        
                        foreach (var i in numsFromHouse)
                            if (dp.Building.Contains(i.ToString())) 
                                hasBuilding = true;
                        
                        foreach (var i in numsFromRoom)
                            if (dp.Room.Contains(i.ToString())) 
                                hasRoom = true;
                        
                        if (hasBuilding && hasRoom){ 
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

        public bool MatchNomenclatureByName(IUnitOfWork uow, string productName, out Nomenclature outNomenclature)
        {
            //Сопоставление номенклатуры
            Nomenclature nomenclature = NomenclatureRepository.GetNomenclatureByName(uow, productName);
            if (nomenclature != null){
                logger.Info($"Номенклатура {nomenclature.ShortName} найдена по bitrix id {nomenclature.BitrixId}");
                outNomenclature = nomenclature;
                
                //
                //Сравнение информации о номенклатуре и обновление если она отличается
                //Проверять по id интернет магазинов, есть справочник интернет магазинов, если указана любая кроме первой значит они пришли извне
                // if (nomenclature.NomenclaturePrice.First(x => x.MinCount == 1).Price != product.Price){
                    //Обновить цену
                // }
                //
                
                return true;
            }
            
            outNomenclature = null;
            return false;
        }

        /// <summary>
        /// Парсит координаты из строки вида 59.830861,30.386583
        /// </summary>
        /// <exception cref="ArgumentException">Координаты разделены не запятой</exception>
        public static bool TryParseCoordinates(string coordinates, out decimal latitude, out decimal longitude)
        {
            if (!coordinates.Contains(",")){
                throw new ArgumentException($"Координаты: {coordinates} переданы в неверном формате, разделитель должен быть запятой");
            }
            var splitted = coordinates.Split(',');
            var latitudeString = splitted[0].Trim();
            var longitudeString = splitted[1].Trim();

            if (decimal.TryParse(longitudeString, NumberStyles.Any, CultureInfo.InvariantCulture, out var _longitude) &&
                decimal.TryParse(latitudeString, NumberStyles.Any, CultureInfo.InvariantCulture, out var _latitude)){
                latitude = _latitude;
                longitude = _longitude;
                return true;
            }
            else{
                latitude = 0;
                longitude = 0;
                return false;
            }
        }
        
        
        //Функция не нужна, если у нас в базе эта категория не найдена,
        //должна будет создаваться категория в группе 502, с каким нибудь системным названием типа "Без названия".
        //Позже служба актуализирует информацию по ней
        
        public bool MatchNomenclatureGroup(IUnitOfWork uow, Product product, out ProductGroup outProductGroup)
        {
            var allProductGroups = product.CategoryObj.Category.Split('/');
            
            //Сопоставление с каждой?
            // foreach (var productGroupName in allProductGroups){
            //     var group = Vodovoz.EntityRepositories.Goods.NomenclatureRepository.GetProductGroupFromBitrixProductGroup(uow, productGroupName);
            //     if (group != null){
            //         
            //     }
            // }
            
            //Сопоставление номенклатуры
            ProductGroup productGroup = NomenclatureRepository.GetProductGroupFromBitrixProductGroup(uow, product.Name);
            if (productGroup != null){
                //Сравнение информации о номенклатуре и обновление если она отличается
                outProductGroup = productGroup;
                return true;
            }
        
            outProductGroup = null;
            return false;
        }
    }
}