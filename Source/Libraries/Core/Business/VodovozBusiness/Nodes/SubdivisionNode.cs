using QS.DomainModel.Entity;
using Vodovoz;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Переместить ноду во Views
	/// </summary>
	public class SubdivisionNode : PropertyChangedBase
	{
		public SubdivisionNode(Subdivision subdivison)
		{
			Subdivision = subdivison;
		}

		private bool _selected;
		public virtual bool Selected
		{
			get => _selected;
			set => SetField(ref _selected, value);
		}
		public Subdivision Subdivision { get; }
		public string SubdivisionName { get => Subdivision.Name; }
		public int SubdivisionId { get => Subdivision.Id; }
	}
}
