using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Linq;

namespace VodovozMobileService.DTO
{
	[DataContract]
	public class MobileOrderDTO
	{
		[DataMember]
		public int OrderId { get; private set; }

		[DataMember]
		public string UuidRaw { get; private set; }

		[DataMember]
		public decimal OrderSum { get; private set; }

		[DataMember]
		public string Created { get; private set; }

		public MobileOrderDTO(int id, string uuid, decimal sum)
		{
			OrderId = id;
			UuidRaw = uuid;
			OrderSum = sum;
		}

		public MobileOrderDTO() { }

		public string GetUuid() => UuidRaw.Replace("-", "").Replace(" ", "").ToUpper();

		public bool IsUuidValid()
		{
			var uuid = GetUuid();
			return uuid.Length <= 32 && !uuid.All(c => c == '0') && Regex.IsMatch(GetUuid(), @"\A\b[0-9a-fA-F]+\b\Z");
		}

		public bool IsOrderSumValid() => OrderSum > 0;
	}
}