using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace OnlineStoreImportService
{
    public class ProductImporter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IUnitOfWork uow;
        private readonly OnlineStoreNomenclatureFactory onlineStoreNomenclatureFactory;

        private IDictionary<string, Nomenclature> nomenclatures = new Dictionary<string, Nomenclature>();
        private readonly Dictionary<string, ProductGroup> productGroups;
        
        private readonly ProductGroup unknownProductGroup;
        private OnlineStore onlineStore;
        
        public ProductImporter(IUnitOfWork uow, OnlineStoreNomenclatureFactory onlineStoreNomenclatureFactory, Dictionary<string, ProductGroup> productGroups, ProductGroup unknownGroup)
        {
            this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
            this.onlineStoreNomenclatureFactory = onlineStoreNomenclatureFactory ?? throw new ArgumentNullException(nameof(onlineStoreNomenclatureFactory));
            this.productGroups = productGroups ?? throw new ArgumentNullException(nameof(productGroups));
            this.unknownProductGroup = unknownGroup ?? throw new ArgumentNullException(nameof(unknownGroup));

            LoadNomenclatures();
        }

        private void LoadNomenclatures()
        {
            nomenclatures = uow.Session.QueryOver<Nomenclature>()
                .Where(Restrictions.IsNotNull(Projections.Property<Nomenclature>(x => x.OnlineStoreExternalId)))
                .List().ToDictionary(x => x.OnlineStoreExternalId);
        }

        public void LoadProducts(JArray productsObject, int onlineStoreId)
        {
            onlineStore = uow.GetById<OnlineStore>(onlineStoreId);
            if (onlineStore == null) {
                throw new ArgumentException($"Не возможно загрузить сущность онлайн магазина из бд (id={onlineStoreId})"); 
            }
            
            var products = JsonConvert.DeserializeObject<IEnumerable<Product>>(productsObject.ToString());
            foreach (Product product in products) {
                AddOrUpdateNomenclature(product);
            }
        }

        private void AddOrUpdateNomenclature(Product product)
        {
            logger.Info($"Обработка товара: ExternalId: {product.Id}, Name: {product.Name}, Price: {product.Price}");

            try {
                if (nomenclatures.TryGetValue(product.Id, out Nomenclature nomenclature)) {
                    UpdateNomenclature(nomenclature, product);
                }
                else {
                    AddNewNomenclature(product);
                }
                uow.Commit();
                
                if (nomenclatures.TryGetValue(product.Id, out Nomenclature nom)) {
                    var price = nom.NomenclaturePrice.FirstOrDefault();
                    logger.Info($"Сохранена номенклатура: Id: {nom.Id}, ExternalId: {nom.OnlineStoreExternalId}, Name: {nom.Name}, Price: {price?.Price}");
                }
                else {
                    logger.Info($"Номенклатура не была создана по ExternalId: {product.Id}");
                }
            }
            catch (Exception ex) {
                logger.Error(ex, "Не удалось загрузить номенклатуру");
            }
        }

        private void UpdateNomenclature(Nomenclature nomenclature, Product product)
        {
            if(nomenclature.OnlineStoreExternalId != product.Id) {
                return;
            }
            FillNomenclatureFromProduct(nomenclature, product);
            uow.Save(nomenclature);
        }

        private void AddNewNomenclature(Product product)
        {
            var nomenclature = onlineStoreNomenclatureFactory.CreateNewNomenclature(uow);
            nomenclature.OnlineStore = onlineStore;
            FillNomenclatureFromProduct(nomenclature, product);
            nomenclatures.Add(product.Id, nomenclature);
            uow.Save(nomenclature);
        }

        private void FillNomenclatureFromProduct(Nomenclature nomenclature, Product product)
        {
            if(!productGroups.TryGetValue(product.GroupId, out ProductGroup productGroup)) {
                productGroup = unknownProductGroup;
            }
            
            nomenclature.Name = product.Name;
            nomenclature.OfficialName = product.Name;
            nomenclature.OnlineStoreExternalId = product.Id;
            nomenclature.ProductGroup = productGroup;
                
            SetPrice(nomenclature, product.Price);
        }

        private void SetPrice(Nomenclature nomenclature, string price)
        {
            bool havePrice = false;
            decimal priceValue = 0;
            if(!string.IsNullOrWhiteSpace(price)) {
                price = price.Replace(",", ".");
                var culture = CultureInfo.CreateSpecificCulture("ru-RU");
                culture.NumberFormat.NumberDecimalSeparator = ".";
                havePrice = decimal.TryParse(price, NumberStyles.AllowDecimalPoint, culture, out priceValue);
            }
            
            NomenclaturePrice nomenclaturePrice = nomenclature.NomenclaturePrice.FirstOrDefault();
            if (nomenclaturePrice == null) {
                if(!havePrice) {
                    return;
                }
                nomenclaturePrice = new NomenclaturePrice();
                nomenclaturePrice.Nomenclature = nomenclature;
                nomenclaturePrice.Price = priceValue;
                nomenclaturePrice.MinCount = 1;
                nomenclature.NomenclaturePrice.Add(nomenclaturePrice);
            }
            else {
                nomenclaturePrice.Price = havePrice ? priceValue : 0;
                nomenclaturePrice.MinCount = 1;
            }
        }
    }
}