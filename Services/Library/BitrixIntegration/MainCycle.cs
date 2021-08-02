using System;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace BitrixIntegration {
    public class MainCycle 
	{
        private static int eventCount;
        
        private readonly IUnitOfWork uow;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly DealCollector dealCollector;
        private readonly DealProcessor dealProcessor;
        private const int MINDEALWAITSEC = 60;

        public MainCycle(IUnitOfWork _uow, DealCollector _dealCollector, DealProcessor dealProcessor)
        {
            uow = _uow ?? throw new ArgumentNullException(nameof(_uow));
            dealCollector = _dealCollector ?? throw new ArgumentNullException(nameof(_dealCollector));
            this.dealProcessor = dealProcessor ?? throw new ArgumentNullException(nameof(dealProcessor));
        }
        public async Task RunProcessCycle()
        {
            await EventAAsync();
        }
        
        
        private int savedDay;
        private int savedHour;
        
        private async Task EventAAsync()
        {
			/*
            savedDay = DateTime.Now.Day;
            savedHour = DateTime.Now.Hour;
            while (true)
            {
                await Task.Run(async () =>
                {
                    //Сменился день
                    if (DateTime.Now.Day != savedDay)
                    {
                        Console.Out.WriteLine("Day Changed");
                        var date = DateTime.Now.AddDays(-1);
                        
                        var dealsList = await dealCollector.CollectDeals(uow, date, true);
                        foreach (var deal in dealsList){
                            try{
                                var order = await dealProcessor.ProcessDeal(deal);
                                await dealCollector.SendSuccessDealFromBitrixToDB(uow, deal.Id, order);

                            }
                            catch (Exception e){
                                dealCollector.SendFailedDealFromBitrixToDB(uow, deal.Id,
                                    e.Message + "\n" + e.InnerException?.Message);

                                logger.Error(e);
                            }
                        }
                        
                        savedDay = DateTime.Now.Day;
                    }
                    //Сменился час
                    else if (DateTime.Now.Hour != savedHour)
                    {
                        var date = DateTime.Now.AddHours(-1);
                        
                        var dealsList = await dealCollector.CollectDeals(uow, date, true);
                        foreach (var deal in dealsList){
                            try{
                                var order = await dealProcessor.ProcessDeal(deal);
                                await dealCollector.SendSuccessDealFromBitrixToDB(uow, deal.Id, order);

                            }
                            catch (Exception e){
                                dealCollector.SendFailedDealFromBitrixToDB(uow, deal.Id,
                                    e.Message + "\n" + e.InnerException?.Message);

                                logger.Error(e);
                            }
                        }
                        
                        savedHour = DateTime.Now.Hour;
                    }
                    //Каждая минута
                    else
                    {
                        logger.Info($" {DateTime.Now}, event number{++eventCount}");
                        var date = DateTime.Now;
                        
                        var dealsList = await dealCollector.CollectDeals(uow, date, false);
                        foreach (var deal in dealsList){
                            try{
                                var order = await dealProcessor.ProcessDeal(deal);
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
			*/
        }
    }
}