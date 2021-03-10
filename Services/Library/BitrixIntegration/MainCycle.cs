using System;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace BitrixIntegration {
    public class MainCycle {
        private static int eventCount;
        
        private readonly IUnitOfWork uow;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly DealCollector dealCollector;
        private readonly CoR cor;
        private const int MINDEALWAITSEC = 60;

        public MainCycle(IUnitOfWork _uow, DealCollector _dealCollector, CoR _cor)
        {
            uow = _uow ?? throw new ArgumentNullException(nameof(_uow));
            dealCollector = _dealCollector ?? throw new ArgumentNullException(nameof(_dealCollector));
            cor = _cor ?? throw new ArgumentNullException(nameof(_cor));
        }
        public async Task RunProcessCycle()
        {
            await EventAAsync();
        }
        
        
        private int SavedDay;
        private int SavedHour;
        
        private async Task EventAAsync()
        {
            SavedDay = DateTime.Now.Day;
            SavedHour = DateTime.Now.Hour;
            while (true)
            {
                await Task.Run(async () =>
                {
                    //Сменился день
                    if (DateTime.Now.Day != SavedDay)
                    {
                        Console.Out.WriteLine("Day Changed");
                        var date = DateTime.Now.AddDays(-1);
                        
                        var dealsList = await dealCollector.CollectDeals(uow, date, true);
                        foreach (var deal in dealsList){
                            try{
                                var order = await cor.Process(deal);
                                await dealCollector.SendSuccessDealFromBitrixToDB(uow, deal.Id, order);

                            }
                            catch (Exception e){
                                dealCollector.SendFailedDealFromBitrixToDB(uow, deal.Id,
                                    e.Message + "\n" + e.InnerException?.Message);

                                logger.Error(e);
                            }
                        }
                        
                        SavedDay = DateTime.Now.Day;
                    }
                    //Сменился час
                    else if (DateTime.Now.Hour != SavedHour)
                    {
                        var date = DateTime.Now.AddHours(-1);
                        
                        var dealsList = await dealCollector.CollectDeals(uow, date, true);
                        foreach (var deal in dealsList){
                            try{
                                var order = await cor.Process(deal);
                                await dealCollector.SendSuccessDealFromBitrixToDB(uow, deal.Id, order);

                            }
                            catch (Exception e){
                                dealCollector.SendFailedDealFromBitrixToDB(uow, deal.Id,
                                    e.Message + "\n" + e.InnerException?.Message);

                                logger.Error(e);
                            }
                        }
                        
                        SavedHour = DateTime.Now.Hour;
                    }
                    //Каждая минута
                    else
                    {
                        logger.Info($" {DateTime.Now}, event number{++eventCount}");
                        var date = DateTime.Now;
                        
                        var dealsList = await dealCollector.CollectDeals(uow, date, false);
                        foreach (var deal in dealsList){
                            try{
                                var order = await cor.Process(deal);
                                await dealCollector.SendSuccessDealFromBitrixToDB(uow, deal.Id, order);
                            }
                            catch (Exception e){
                                dealCollector.SendFailedDealFromBitrixToDB(uow, deal.Id,
                                    e.Message + "\n" + e.InnerException?.Message);

                                logger.Error(e);
                            }
                        }
                    }
                });
                await Task.Delay(TimeSpan.FromSeconds(MINDEALWAITSEC));
            }
        }
    }
}