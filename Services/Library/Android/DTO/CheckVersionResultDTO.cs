using System;
using System.Runtime.Serialization;

namespace Android
{
	[DataContract]
	public class CheckVersionResultDTO
	{
		public enum ResultType{
			Ok,
			CanUpdate,
			NeedUpdate
		}

		[DataMember]
		public ResultType Result = ResultType.Ok;

		[DataMember]
		public string DownloadUrl;

		[DataMember]
		public string NewVersion;
	}
}
