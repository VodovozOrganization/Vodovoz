namespace Vodovoz.Services {
    public interface IBitrixServiceSettings {
        int MaxStatusesInQueueForWorkingService { get; }
        int EmployeeForOrderCreate { get; }
    }
}