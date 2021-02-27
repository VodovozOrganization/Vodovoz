using System;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace BitrixIntegration {
    public class MainCycle {
        private static int a = 0;
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
        
        private static async Task EventAAsync()
        {
            while (true)
            {
                Task.Run(() =>
                {
                    Console.WriteLine("The Elapsed event A was raised at {0}, a{1}", DateTime.Now, ++a);
                });
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}