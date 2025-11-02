using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskProblemDescriptionSourceEntity : IEquatable<EdoTaskProblemDescriptionSourceEntity>
	{
		public virtual string Name { get; set; }
		public virtual EdoTaskProblemType Type { get; set; }
		public virtual EdoProblemImportance Importance { get; set; }
		public virtual string Description { get; set; }
		public virtual string Recommendation { get; set; }

		public override bool Equals(object obj)
		{
			return Equals(obj as EdoTaskProblemDescriptionSourceEntity);
		}

		public virtual bool Equals(EdoTaskProblemDescriptionSourceEntity other)
		{
			return !(other is null) &&
				   Name == other.Name &&
				   Type == other.Type &&
				   Importance == other.Importance &&
				   Description == other.Description &&
				   Recommendation == other.Recommendation;
		}

		public override int GetHashCode()
		{
			int hashCode = 369692617;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + Type.GetHashCode();
			hashCode = hashCode * -1521134295 + Importance.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Recommendation);
			return hashCode;
		}

		public override string ToString()
		{
			return $"EdoTaskDescriptionSource: {Name}";
		}
	}
}
