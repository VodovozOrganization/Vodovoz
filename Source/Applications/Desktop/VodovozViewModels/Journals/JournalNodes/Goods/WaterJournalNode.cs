using System.Linq;
using QS.Project.Journal;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
    public class WaterJournalNode: JournalEntityNodeBase<Nomenclature>
    {
        public override string Title => Name;
        public string Name { get; set; }
        public string UnitName { get; set; }
        public short UnitDigits { get; set; }

        private string Format(decimal value)
        {
            return string.Format("{0:F" + UnitDigits + "} {1}", value, UnitName);
        }
    }
}
