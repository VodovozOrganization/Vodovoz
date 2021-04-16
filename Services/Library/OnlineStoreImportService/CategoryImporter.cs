using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace OnlineStoreImportService
{
    public class CategoryImporter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IUnitOfWork uow;
        private Dictionary<string, ProductGroup> productGroups = new Dictionary<string, ProductGroup>();
        private OnlineStore onlineStore;
        public CategoryImporter(IUnitOfWork uow)
        {
            this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Dictionary<string, ProductGroup> ImportCategories(JObject categoryObject, int onlineStoreId, ProductGroup root)
        {
            if (categoryObject == null) throw new ArgumentNullException(nameof(categoryObject));
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (onlineStoreId <= 0) throw new ArgumentOutOfRangeException(nameof(onlineStoreId));
            onlineStore = uow.GetById<OnlineStore>(onlineStoreId);
            if (onlineStore == null) {
                throw new ArgumentException($"Не возможно загрузить сущность онлайн магазина из бд (id={onlineStoreId})"); 
            }
            
            LoadProductGroup(onlineStoreId);

            ReadRoot(categoryObject, root);

            return productGroups;
        }

        private void LoadProductGroup(int onlineStoreId)
        {
             var tempProductGroups = uow.Session.QueryOver<ProductGroup>()
                .Where(x => x.OnlineStore.Id == onlineStoreId)
                .Where(Restrictions.IsNotNull(Projections.Property<ProductGroup>(x => x.OnlineStoreExternalId)))
                .List();
            
             productGroups = tempProductGroups.ToDictionary(x => x.OnlineStoreExternalId);;
        }

        private void ReadRoot(JObject childs, ProductGroup parent)
        {
            foreach (JProperty child in childs.Properties()) {
                if (child.HasValues) {
                    ReadCategory((JObject)child.Value, parent);
                }
            }
        }

        private void ReadChilds(JObject childs, string parentId)
        {
            foreach (JProperty child in childs.Properties()) {
                if (child.HasValues) {
                    ReadCategory((JObject)child.Value, productGroups[parentId]);
                }
            }
        }

        private void ReadCategory(JObject category, ProductGroup parent)
        {
            string id = category.Property("ID").Value.Value<string>();
            string name = category.Property("NAME").Value.Value<string>();
            JProperty childs = category.Property("CHILDS");

            AddOrUpdateProductGroup(id, name, parent);

            if (childs != null && childs.HasValues) {
                ReadChilds((JObject) childs.Value, id);
            }
        }

        public void AddOrUpdateProductGroup(string id, string name, ProductGroup parent)
        {
            if (productGroups.TryGetValue(id, out ProductGroup productGroup)) {
                UpdateExistingProductGroup(productGroup, name, parent);
            }
            else {
                AddNewProductGroup(id, name, parent);
            }
        }

        private void UpdateExistingProductGroup(ProductGroup productGroup, string name, ProductGroup parent)
        {
            productGroup.Name = name;
            productGroup.Parent = parent;

            uow.Save(productGroup);
        }

        private void AddNewProductGroup(string id, string name, ProductGroup parent)
        {
            ProductGroup newProductGroup = new ProductGroup();
            newProductGroup.Name = name;
            newProductGroup.OnlineStore = onlineStore;
            newProductGroup.OnlineStoreExternalId = id;
            newProductGroup.Parent = parent;
            productGroups.Add(id, newProductGroup);
            
            uow.Save(newProductGroup);
        }
    }
}