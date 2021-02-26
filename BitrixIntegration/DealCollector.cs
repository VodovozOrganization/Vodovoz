using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BitrixApi.DTO;
using BitrixApi.REST;
using Newtonsoft.Json;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using VodovozInfrastructure.Utils;

namespace BitrixIntegration {
    public class DealCollector {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IBitrixRestApi bitrixApi;

        public DealCollector(IBitrixRestApi _bitrixRestApi)
        {
            bitrixApi = _bitrixRestApi ?? throw new ArgumentNullException(nameof(_bitrixRestApi));
        }

        public async Task<IList<Deal>> CollectDeals(IUnitOfWork uow, DateTime day)
        {
            var listOfIds = await bitrixApi.GetDealsIdsBetweenDates(uow, day.StartOfDay(), day.EndOfDay());

            listOfIds.Add(16088200);
            // listOfIds.Add(160882);
            
            int j = 0;
            Dictionary<uint, string> failedIdToExeprion = new Dictionary<uint, string>();
            IList<Deal> listOfdeals = new List<Deal>();
            foreach (var dealId in listOfIds){
                Deal deal = null;
                try{
                    deal = await bitrixApi.GetDealAsync(dealId);
                }
                catch (JsonSerializationException e){
                    
                    #region Нет периода доставки

                    if (e.Message.Contains("UF_CRM_5DA9BBA03A12A")){
                        string exceptionText = 
                            $"Сделка с id: {dealId} не содержит периода доставки, " +
                            $"скорее всего это сделка появилась в битриксе не из CRM, " +
                            $"а была добавлена из ДВ в виде подтверждения оплдаты по СМС, " +
                            $"эта сделка не должна была сюда попасть (выборка по сделкам со статусом завести в ДВ)";
                        logger.Warn(exceptionText);
                        SendFailedDealFromBitrixToDB(uow, dealId, exceptionText);
                    }

                    #endregion
                    
                    else{
                        failedIdToExeprion[dealId] = e.ToString();
                    }
                    j++;
                    continue;
                }
                catch (HttpRequestException e){
                    if (e.Message.Contains("400 (Bad Request)")){
                        string exeption = $"Сделка с id: {dealId} не найдена в системе битрикс";
                        logger.Warn(exeption);
                        // SendFailedDealFromBitrixToDB(uow, dealId, exeption);
                        var ordr = uow.GetById<Order>(100); // TODO gavr это для теста
                        SendSuccessDealFromBitrixToDB(uow, dealId, ordr);
                    }
                    else{
                        failedIdToExeprion[dealId] = e.ToString();
                    }
                    j++;
                    continue;
                }
                catch (Exception e){
                    failedIdToExeprion[dealId] = e.ToString();
                    j++;
                    continue;
                }
                
                listOfdeals.Add(deal);
                
                if (j  == 50){
                    Thread.Sleep(1000);
                    j = 0;
                }
                j++;
            }
            logger.Info($"Десериализовано: {listOfdeals.Count} сделок," +
                        $" не отправленных в базу ошибок: {failedIdToExeprion.Count}");

            int errCounter = 1;
            foreach (var keyValuePair in failedIdToExeprion){
                logger.Info($"Отправка ошибки номер: {errCounter++}");
                SendFailedDealFromBitrixToDB(uow, keyValuePair.Key, keyValuePair.Value);
            }
            if (errCounter > 1)
                logger.Info("Ошибки отправлены");
            
            return listOfdeals;
        }
        
        
        private void SendFailedDealFromBitrixToDB(IUnitOfWork uow, uint dealId, string exeption)
        {
            var deal = uow.GetById<DealFromBitrix>((int)dealId);  //TODO gavr нужен новый GetById с UInt иначе NHibernate кидает ошибку
            if (deal != null && deal.Success == false){
                #region Обновление существующей ошибочной сделки
                logger.Info($"Сделка {dealId} уже была добавлена как обработанная с ошибкой, обновление...");
                deal.Success = false;
                deal.ProcessedDate = DateTime.Now;
                deal.ExtensionText = exeption;
                try{
                    uow.Save(deal);
                    uow.Commit();
                }
                catch (Exception exception){
                    logger.Error($"!Ошибка при отправке обновленной ошибочной сделки {dealId}\n{exception.Message}\n{exception?.InnerException}");
                }
                #endregion
            }
            else{
                #region Загрузка новой ошибочной сделки

                var dealFromBitrix = new DealFromBitrix()
                {
                    Success = false,
                    BitrixId = dealId,
                    CreateDate = DateTime.Now,
                    ExtensionText = exeption
                };
                try{
                    uow.Save(dealFromBitrix);
                    uow.Commit();
                }
                catch (Exception exception){
                    logger.Error($"!Ошибка при отправке ошибочной сделки {dealId}\n{exception.Message}\n{exception?.InnerException}");
                }

                #endregion
                
            }
        }   
        
        private void SendSuccessDealFromBitrixToDB(IUnitOfWork uow, uint dealId, Order order)
        {
            var deal = uow.GetById<DealFromBitrix>(dealId);
            if (deal != null && deal.Success == true){
                
                #region Обновление существующей успешной сделки
                
                logger.Info($"Сделка {dealId} уже была добавлена как обработанная с ошибкой, обновление...");
                deal.Order = order;
                deal.Success = true;
                deal.ProcessedDate = DateTime.Now;
                deal.ExtensionText = "";
                try{
                    uow.Save(deal);
                    uow.Commit();
                }
                catch (Exception exception){
                    logger.Error($"!Ошибка при отправке обновленной успешной сделки сделки {dealId}\n{exception.Message}\n{exception?.InnerException}");
                }
                
                #endregion
            }
            else{
                #region Загрузка новой ошибочной сделки
                
                var dealFromBitrix = new DealFromBitrix()
                {
                    Success = true,
                    BitrixId = dealId,
                    Order = order,
                    CreateDate = DateTime.Now,
                    ProcessedDate = DateTime.Now
                };
                try{
                    uow.Save(dealFromBitrix);
                    uow.Commit();
                }
                catch (Exception exception){
                    logger.Error($"!Ошибка при отправке ошибочной сделки {dealId}\n{exception.Message}\n{exception?.InnerException}");
                }
                
                #endregion
            }
            
        }   
    }
}