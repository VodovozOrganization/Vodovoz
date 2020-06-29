using System.Runtime.Serialization;

namespace VodovozSalesReceiptsService.DTO
{
	[DataContract]
	public class FinscalizatorStatusResponseDTO
	{
		[DataMember]
		string dateTime;
		[DataMember]
		string status;
		[DataMember]
		string message;

		public FiscalRegistratorStatus Status {
			get {
				switch(status.ToLower()) {
					case "ready": return FiscalRegistratorStatus.Ready;
					case "failed": return FiscalRegistratorStatus.Failed;
					case "associated": return FiscalRegistratorStatus.Associated;
					default: return FiscalRegistratorStatus.Unknown;
				}
			}
		}

		public string Message => message;
	}
}