using System.Threading.Tasks;

namespace BitrixIntegration {
    public interface ICoR {
        Task Process(uint id);
    }
}