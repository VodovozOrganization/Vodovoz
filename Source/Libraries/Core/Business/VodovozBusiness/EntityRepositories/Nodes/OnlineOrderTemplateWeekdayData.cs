namespace VodovozBusiness.EntityRepositories.Nodes
{
	public class OnlineOrderTemplateWeekdayData
	{
		public int TemplateId { get; set; }
		public string Weekday { get; set; }

		public static OnlineOrderTemplateWeekdayData Create(int templateId, string weekday) =>
			new OnlineOrderTemplateWeekdayData
			{
				TemplateId = templateId,
				Weekday = weekday
			};
	}
}
