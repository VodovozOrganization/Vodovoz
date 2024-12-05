using RoboAtsService.Contracts.Requests;

namespace RoboatsService.Handlers
{
	public abstract class GetRequestHandlerBase : RequestHandlerBase
	{
		public override string ErrorMessage => $"ERROR. request={Request}.";
		public abstract string Request { get; }
		public virtual int? AddressId { get; }

		protected GetRequestHandlerBase(RequestDto requestDto) : base(requestDto)
		{
			if(int.TryParse(requestDto.AddressId, out int addressId))
			{
				AddressId = addressId;
			}
		}
	}

}
