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
        public bool MatchOrderByBitrixId(IUnitOfWork uow, uint dealId, out Order outOrder)
        {
            Order order = null;
            order = OrderSingletonRepository.GetInstance().GetOrderByBitrixId(uow, dealId);
            
            if (order == null){
                logger.Info($"Не удалось сопоставить Order по BitrixId сделки: {dealId}");
                outOrder = null;
                return false;
            }
            else{
                outOrder = order;
                logger.Info($"Сопоставление Order: {outOrder.Id} по BitrixId сделки: {dealId} прошло успешно");
                return true;
            }
        }
        
        public bool MatchCounterpartyByBitrixId(IUnitOfWork uow, uint bitrixId, out Counterparty outCounterparty)
        {
            
            Counterparty counterparty = null;
            counterparty = CounterpartyRepository.GetCounterpartyByBitrixId(uow, bitrixId);
            
            if (counterparty == null){
                outCounterparty = null;
                logger.Info($"Не удалось сопоставить Counterparty по BitrixId: {bitrixId}");

                return false;
            }
            else{
                outCounterparty = counterparty;
                logger.Info($"Сопоставление Counterparty: {outCounterparty.Id} по BitrixId: {bitrixId} прошло успешно");

                return true;
            }
            
        }
        
        public bool MatchNomenclatureByBitrixId(IUnitOfWork uow, uint productId, out Nomenclature outNomenclature)
        {
            Nomenclature nomenclature = null;
            nomenclature = NomenclatureRepository.GetNommenclatureByBitrixId(uow, productId);
            
            if (nomenclature == null){
                outNomenclature = null;
                logger.Info($"Не удалось сопоставить Nommenclature по BitrixId: {productId}");

                return false;
            }
            else{
                outNomenclature = nomenclature;
                logger.Info($"Сопоставление Counterparty: {outNomenclature.Id} по BitrixId: {productId} прошло успешно");
                return true;
            }
        }

        #endregion ByBitrixId
            
        
        /// <summary>
        /// Находит компанию по номеру телефона и названию
        /// </summary>
        /// <exception cref="NullReferenceException">Контакт не содержит имени, фамилии и отчества, необходимых для сопоставления</exception>
        public bool MatchCompanyPhoneAndName(IUnitOfWork uow, Company company, out Counterparty outCounterparty)
        {
            //Формат записанный в Value +7 (981) 944-86-31
            var phone = company.Phones.First().Value;
            var digitsNum = PhoneUtils.NumberTrim(phone, out var _);

            IList<Counterparty> counterparties = null;
            
            counterparties = CounterpartyRepository.GetCounterpartesByPartOfName(
                uow,
                company.Title?? 
                throw new NullReferenceException($"Компания с BitrixId {company.Id} не содержит названия, необходимого для сопоставления"),
                digitsNum
            );
            
            if (counterparties.Count == 1){
                outCounterparty = counterparties.First();
                outCounterparty.BitrixId = company.Id;
                logger.Info($"Для компании с BitrixId {company.Id} у нас найден 1 контрагент {outCounterparty.Id} по телефону и названию");
                uow.Save(outCounterparty);
                return true;
            }
            else{
                logger.Warn($"Для контакта с BitrixId {company.Id} у нас не найдено контрагентов по телефону и названию");
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
                    throw new NullReferenceException("Контакт не содержит имени, фамилии и отчества, " +
                                                     "необходимых для сопоставления"),
                digitsNum
            );
            
            if (counterparties.Count == 1){
                outCounterparty = counterparties.First();
                outCounterparty.BitrixId = contact.Id;
                logger.Info($"Для контакта с BitrixId {contact.Id} у нас найден 1 контрагент {outCounterparty.Id}" +
                            " по телефону и части имени");
                uow.Save(outCounterparty);
                return true;
            }
            else{
                logger.Warn($"Для контакта с BitrixId {contact.Id} у нас не найдено контрагентов " +
                            "по телефону и части имени");
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
                logger.Info($"Распаршены координаты: longitude: {longitude} и latitude {latitude}");
                IList<DeliveryPoint> deliveryPointsOfCounerparty = DeliveryPointRepository.GetDeliveryPointForCounterpartyByCoordinates(uow, latitude, longitude, counterparty.Id);
            
                if (deliveryPointsOfCounerparty.Count == 1){
                    logger.Info($"Сопоставлена 1 точка доставки Id: {deliveryPointsOfCounerparty.First().Id}, {deliveryPointsOfCounerparty.First().ShortAddress}");
                    outDeliveryPoint = deliveryPointsOfCounerparty.First();
                    return true;
                }
                // В одном доме несколько наших клиентов
                else if(deliveryPointsOfCounerparty.Count > 1){
                    logger.Info($"В одном доме несколько наших клиентов deliveryPoints.Count: {deliveryPointsOfCounerparty.Count}");

                    //СОПОСТАВЛЯЕМ ПО ДОМУ УЛИЦЕ ИТД
                    foreach (var dp in deliveryPointsOfCounerparty){
                       if(dp.Room != null && dp.Room != "." && dp.Room != "-") {
                            // Тк кк значения бывают такие "13-Н ком.21"
                            if (dp.Room.Contains(deal.RoomNumber)){
                                logger.Info($"Сопоставлено по комнате: {dp.Room} и {deal.RoomNumber}");
                                outDeliveryPoint = dp;
                                return true;
                            }
                            else{
                                logger.Info($"Сопоставлено по комнате не удалось, попытка сопоставить по вхождению чисел");

                                // Более медленный способ, на случай если RoomNumber содержит не только одно число, проверяем по вхождению каждого числа по отдельности
                                var numbersFromRoom = NumbersUtils.GetNumbersFromString(deal.RoomNumber);
                                uint counter = 0;
                                foreach (var num in numbersFromRoom){
                                    if (dp.Room.Contains(num.ToString())){
                                        counter++;
                                    }
                                }

                                if (counter >= 1){
                                    logger.Info($"Между номерами комнат сошлось: {counter} чисел");
                                    logger.Info($"Сопоставлено по комнате: {dp.Room} и {deal.RoomNumber}");
                                    outDeliveryPoint = dp;
                                    return true;
                                }
                            }
                       }
                    }
                    logger.Warn($"Сопоставлено точки доставки для сделки: {deal.Id} для контрагента: {counterparty.Id} не удалось");
                    
                    outDeliveryPoint = null;
                    return false;
                }
                else{
                    logger.Warn($"У контрагента {counterparty.Id} не нашлось точки доставки с координатами из битрикса {deal.Id}");
                    // У контрагента не нашлось точки доставки с координатами из битрикса
                    // Берем все точки доставки контрагента и пытаемся сопоставить по наличию в них дома + квартиры
                    var deliveryPointsForCounterparty = DeliveryPointRepository.DeliveryPointsForCounterpartyQuery(uow, counterparty);
                    foreach (var dp in deliveryPointsForCounterparty){
                        var numsFromHouse = NumbersUtils.GetNumbersFromString(deal.HouseAndBuilding);
                        var numsFromRoom = (NumbersUtils.GetNumbersFromString(deal.RoomNumber));
                        var hasBuilding = false;
                        var hasRoom = false;
                        
                        foreach (var i in numsFromHouse)
                            if (dp.Building.Contains(i.ToString())){
                                logger.Info($"Номер дома {dp.Building} сопоставился с {i.ToString()}");
                                hasBuilding = true;
                            }
                        
                        foreach (var i in numsFromRoom)
                            if (dp.Room.Contains(i.ToString())){
                                logger.Info($"Офис/Квартира {dp.Building} сопоставилась с {i.ToString()}");

                                hasRoom = true;
                            }
                        
                        if (hasBuilding && hasRoom){ 
                            logger.Info($"Для сделки {deal.Id} с контрагентом {counterparty.Id} сопоставлен адрес");
                            outDeliveryPoint = dp; 
                            return true;
                        };
                    }
                    logger.Warn($"Для сделки {deal.Id} с контрагентом {counterparty.Id} не получилось сопоставить адрес");
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
                logger.Info($"Номенклатура {nomenclature.ShortName} сопоставлена по названию {productName}");
                outNomenclature = nomenclature;
                return true;
            }
            logger.Warn($"Номенклатура не найдена по названию {productName}");

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
                logger.Info($"Парсинг координат удался longitude {_longitude}, latitude {_latitude}");

                latitude = _latitude;
                longitude = _longitude;
                return true;
            }
            else{
                logger.Warn($"Парсинг координат не удался longitude {_longitude},  latitude");

                latitude = 0;
                longitude = 0;
                return false;
            }
        }
        
        
        //Функция не нужна, если у нас в базе эта категория не найдена,
        //должна будет создаваться категория в группе 502, с каким нибудь системным названием типа "Без названия".
        //Позже служба актуализирует информацию по ней
        
        public bool MatchNomenclatureGroupByName(IUnitOfWork uow, string lastGroup, out ProductGroup outProductGroup)
        {
            var group = NomenclatureRepository.GetProductGroupByName(uow, lastGroup);
            if (group != null){
                outProductGroup = group;
                return true;
            }
            else{
                outProductGroup = null;
                return false;
            }
        }
    }
}