using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Complaints;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.EntityRepositories.Complaints
{
	public class OksDailyReportComplaintDataNode
	{
		public int Id { get; set; }
		public DateTime CreationDate { get; set; }
		public string ComplaintText { get; set; }
		public IEnumerable<OksDailyReportComplaintResultCommentsData> ComplaintResults { get; set; }
		public ComplaintWorkWithClientResult? WorkWithClientResult { get; set; }
		public ComplaintStatuses Status { get; set; }
		public ComplaintKind ComplaintKind { get; set; }
		public ComplaintObject ComplaintObject { get; set; }
		public ComplaintSource ComplaintSource { get; set; }
		public string ClientName { get; set; }
		public string DeliveryPointAddress { get; set; }
		public IEnumerable<DiscussionSubdivisionData> DiscussionSubdivisions { get; set; }
	}
}
