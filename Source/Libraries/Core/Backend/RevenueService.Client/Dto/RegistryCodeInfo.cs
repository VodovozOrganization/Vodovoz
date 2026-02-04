using Dadata.Model;

namespace RevenueService.Client.Dto
{
	public sealed class RegistryCodeInfo
	{
		public int Code { get; }
		public PartyType OrganizationType { get; }
		public PartyStatus Status { get; }
		public string Description { get; }

		public RegistryCodeInfo(int code, PartyType organizationType, PartyStatus status, string description)
		{
			Code = code;
			OrganizationType = organizationType;
			Status = status;
			Description = description;
		}
	}
}
