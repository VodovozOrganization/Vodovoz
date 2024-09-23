using System;
using Vodovoz.Domain.Complaints;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.EntityRepositories.Complaints
{
	public class OksDailyReportComplaintDataNode
	{
		public int Id { get; set; }
		public DateTime CreationDate { get; set; }
		public string ComplaintText { get; set; }
		public string ComplaintResults { get; set; }
		public ComplaintWorkWithClientResult? WorkWithClientResult { get; set; }
		public ComplaintStatuses Status { get; set; }
		public ComplaintKind ComplaintKind { get; set; }
		public ComplaintObject ComplaintObject { get; set; }
		public ComplaintSource ComplaintSource { get; set; }
		public ComplaintDiscussionStatuses OksDiskussionStatuse { get; set; }
		public string ClientName {  get; set; }
		public string DeliveryPointAddress { get; set; }
	}
}
