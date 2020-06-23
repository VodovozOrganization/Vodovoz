using System;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace Android
{
	[DataContract(Namespace = "http://tempuri.org/")]
	[Serializable]
	public class TrackPoint
	{
		[DataMember]
		public string Latitude { get; set; }
		[DataMember]
		public string Longitude { get; set; }
		[DataMember]
		public string TimeStamp { get; set; }
	}

	[CollectionDataContract(Namespace = "http://tempuri.org/")]
	public class TrackPointList : Collection<TrackPoint> {}
}