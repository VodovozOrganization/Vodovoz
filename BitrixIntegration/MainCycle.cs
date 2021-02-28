using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitrixApi.DTO;
using QS.DomainModel.UoW;

namespace BitrixIntegration {
    public class MainCycle {
        // Для фиксации того что сменился день

        private static int a;
        
        private readonly IUnitOfWork uow;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public MainCycle(IUnitOfWork _uow)
        {
            uow = _uow ?? throw new ArgumentNullException(nameof(_uow));
        }
        public async Task RunProcessCycle(CoR cor, DealCollector dealCollector)
        {
            var date = DateTime.Parse("25.02.2021");
            // var a = await bitrixApi.GetDealsBetweenDates(uow,date.StartOfDay(), date.EndOfDay());
            var dealsList = await dealCollector.CollectDeals(uow, date);
            dealsList = new List<Deal>();
            Console.Out.WriteLine("Sas");
            await EventAAsync();
            
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
        
        
        private int SavedDay;
        private async Task EventAAsync()
        {
            SavedDay = DateTime.Today.Minute;
            while (true)
            {
                Task.Run(() =>
                {
                    if (DateTime.Now.Minute != SavedDay)
                    {
                        Console.Out.WriteLine("Day Changed");
                        //обработка всех за день

                        SavedDay = DateTime.Now.Minute;
                    }
                    else
                    {
                        Console.WriteLine("The Elapsed event A was raised at {0}, a{1}", DateTime.Now, ++a);
                        //обработка всех за час

                    }
                });
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}