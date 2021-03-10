using System.Threading.Tasks;
using BitrixApi.DTO;
using Vodovoz.Domain.Orders;

namespace BitrixIntegration {
    public interface ICoR {
        Task<Order> Process(Deal deal);
    }
}