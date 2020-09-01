using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Orders
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "интернет-магазины",
        Nominative = "интернет-магазин",
        Prepositional = "интернет-магазине",
        PrepositionalPlural = "интернет-магазинах"
    )]
    public class OnlineStore : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }

        private string name;
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }
    }
}