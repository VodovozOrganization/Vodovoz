using System.Runtime.Serialization;

namespace VodovozSalesReceiptsService.DTO
{
	[DataContract]
	public class FinscalizatorStatusResponseDTO
	{
		#pragma warning disable CS0169, CS0649
		
		[DataMember]
		string dateTime;
		[DataMember]
		string status;
		[DataMember]
		string message;
		
		#pragma warning restore CS0169, CS0649
		
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