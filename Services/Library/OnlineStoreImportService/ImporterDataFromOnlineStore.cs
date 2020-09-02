using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Services;
using Timer = System.Timers.Timer;

namespace OnlineStoreImportService
{
    public class ImporterDataFromOnlineStore : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly INomenclatureParametersProvider nomenclatureParametersProvider;
        private readonly INomenclatureRepository nomenclatureRepository;
        private readonly int onlineStoreId;
        private const string unknownProductGroupId = "UNKNOWN_01";

        private readonly Timer timer = new Timer();
        private bool loadIsProgress;
        
        public ImporterDataFromOnlineStore(INomenclatureParametersProvider nomenclatureParametersProvider, INomenclatureRepository nomenclatureRepository)
        {
            this.nomenclatureParametersProvider = nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
            this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
            onlineStoreId = nomenclatureParametersProvider.CurrentOnlineStoreId;
        }

        public void Start()
        {
            Stop();
            timer.Elapsed += (s, e) => Load();
            //2 часа
            timer.Interval = 7200000;
            timer.Start();
            Load();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void Load()
        {
            if(loadIsProgress) {
                return;
            }
            
            loadIsProgress = true;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
            
                LoadFromOnlineStore();
            
                sw.Stop();
                string loadTime = $"{sw.Elapsed.Minutes.ToString()}мин. {sw.Elapsed.Seconds.ToString()}сек.";
                logger.Info($"Загружены номенклатуры из интернет-магазина. Время загрузки: {loadTime}");
            }
            finally
            {
                loadIsProgress = false;
            }
        }

        private void LoadFromOnlineStore()
        {
            string importData = null;
            using (WebClient client = new WebClient()) {
                try
                {
                    importData = client.DownloadString(nomenclatureParametersProvider.OnlineStoreExportFileUrl);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Не удалось загрузить файл с данными о продуктах");
                    return;
                }
            }

            try
            {
                var productsData = JsonConvert.DeserializeObject<Dictionary<object, object>>(importData);
                LoadNomenclatures(productsData);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Произошла ошибка при чтении выгрузки.");
            }
        }

        private void LoadNomenclatures(IDictionary<object, object> productsData)
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                CategoryImporter categoryImporter = new CategoryImporter(uow);
                var rootGroupId = nomenclatureParametersProvider.RootProductGroupForOnlineStoreNomenclatures;
                var rootGroup = uow.GetById<ProductGroup>(rootGroupId);
                var unknownProductGroup = GetUnknownProductGroup(uow, rootGroup);
                
                var categories = (JObject) productsData["category"];
                var resultGroups = categoryImporter.ImportCategories(categories, onlineStoreId, rootGroup);
                uow.Commit();
                
                OnlineStoreNomenclatureFactory onlineStoreNomenclatureFactory = new OnlineStoreNomenclatureFactory(nomenclatureParametersProvider, nomenclatureRepository);
                ProductImporter productImporter = new ProductImporter(uow, onlineStoreNomenclatureFactory, resultGroups, unknownProductGroup);
                var products = (JArray) productsData["product"];
                productImporter.LoadProducts(products, onlineStoreId);
            } 
        }
        
        private ProductGroup GetUnknownProductGroup(IUnitOfWork uow, ProductGroup onlineStoreRootGroup)
        {
            if (uow == null) throw new ArgumentNullException(nameof(uow));
            if (onlineStoreRootGroup == null) throw new ArgumentNullException(nameof(onlineStoreRootGroup));
            
            var unknownProductGroup = uow.Session.QueryOver<ProductGroup>()
                .Where(x => x.OnlineStoreExternalId == unknownProductGroupId)
                .SingleOrDefault();
            
            if (unknownProductGroup == null) {
                unknownProductGroup = new ProductGroup();
                unknownProductGroup.Name = "Товары без группы";
                unknownProductGroup.OnlineStoreExternalId = unknownProductGroupId;
                unknownProductGroup.Parent = onlineStoreRootGroup;
                
                uow.Save(unknownProductGroup);
            }

            return unknownProductGroup;
        }
        
        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}