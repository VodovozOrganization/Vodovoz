using QS.Project.Journal;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarEventTypeJournalNode : JournalEntityNodeBase<CarEventType>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string ShortName { get; set; }
		public bool NeedComment { get; set; }
		public bool IsArchive { get; set; }
		public bool IsDoNotShowInOperation { get; set; }
		public bool IsAttachWriteOffDocument { get; set; }
		public AreaOfResponsibility? AreaOfResponsibility { get; set; }

		public string AreaOfResponsibilityValue
		{
			get
			{
				if(!AreaOfResponsibility.HasValue)
				{
					return string.Empty;
				}

				var member = typeof(AreaOfResponsibility).GetMember(AreaOfResponsibility.Value.ToString()).FirstOrDefault();
				if(member != null)
				{
					var displayAttr = member.GetCustomAttribute<DisplayAttribute>();
					if(displayAttr != null && !string.IsNullOrWhiteSpace(displayAttr.ShortName))
					{
						return displayAttr.ShortName;
					}
				}
				return AreaOfResponsibility.Value.ToString();
			}
		}
	}
}
