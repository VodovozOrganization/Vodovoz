using NUnit.Framework;

namespace VodovozBusinessTests.Domain {
    [TestFixture]
    public class NomenclatureFixedPriceControllerTests {

        [Test(Description = "Проверка метода ContainsFixedPrice(OrderBase order, Nomenclature nomenclature)")]
        public void TestContainsFixedPriceMethodFromOrder() {
        }
        
        [Test(Description = "Проверка метода ContainsFixedPrice(Counterparty counterparty, Nomenclature nomenclature)")]
        public void TestContainsFixedPriceMethodFromCounterparty() {
        }
        
        [Test(Description = "Проверка метода ContainsFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature)")]
        public void TestContainsFixedPriceMethodFromDeliveryPoint() {
        }
        
        [Test(Description = "Проверка метода TryGetFixedPrice(OrderBase order, Nomenclature nomenclature, out decimal fixedPrice)")]
        public void TestTryGetFixedPriceMethodFromOrder() {
        }
        
        [Test(Description = "Проверка метода TryGetFixedPrice(Counterparty counterparty, Nomenclature nomenclature, out decimal fixedPrice)")]
        public void TestTryGetFixedPriceMethodFromCounterparty() {
        }
        
        [Test(Description = "Проверка метода TryGetFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, out decimal fixedPrice)")]
        public void TestTryGetFixedPriceMethodFromDeliveryPoint() {
        }
        
        [Test(Description = "Проверка метода AddOrUpdateFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice)")]
        public void TestAddOrUpdateFixedPriceMethodFromDeliveryPoint() {
        }
        
        [Test(Description = "Проверка метода AddOrUpdateFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice)")]
        public void TestAddOrUpdateFixedPriceMethodFromCounterparty() {
        }
        
        [Test(Description = "Проверка метода AddOrUpdateWaterFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice)")]
        public void TestAddOrUpdateWaterFixedPriceMethodFromDeliveryPoint() {
        }
        
        [Test(Description = "Проверка метода AddOrUpdateWaterFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice)")]
        public void TestAddOrUpdateWaterFixedPriceMethodFromCounterparty() {
        }
        
        [Test(Description = "Проверка метода DeleteFixedPrice(DeliveryPoint deliveryPoint, NomenclatureFixedPrice nomenclatureFixedPrice)")]
        public void TestDeleteFixedPriceMethodFromDeliveryPoint() {
        }
        
        [Test(Description = "Проверка метода DeleteFixedPrice(Counterparty counterparty, NomenclatureFixedPrice nomenclatureFixedPrice)")]
        public void TestDeleteFixedPriceMethodFromCounterparty() {
        }
    }
}