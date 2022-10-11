using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.ViewModels.Store
{
    public class MovementWagonViewModel : EntityTabViewModelBase<MovementWagon>
    {
        public MovementWagonViewModel(
            IEntityUoWBuilder ctorParam, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            ICommonServices commonServices)
            : base(ctorParam, unitOfWorkFactory, commonServices)
        {
            if (!CanCreateOrUpdate)
                AbortOpening("У вас недостаточно прав");
            TabName = "Фуры";
        }

        private string name;

        public string Name
        {
            get {
                name = Entity.Name;
                return name;
            }
            set { 
                if (SetField(ref name, value)) {
                    Entity.Name = value;
                }
            }
        }


        public override bool Save(bool close)
        {
            return base.Save(close);
        }

        #region Permissions

        public bool CanCreate => PermissionResult.CanCreate;
        public bool CanRead => PermissionResult.CanRead;
        public bool CanUpdate => PermissionResult.CanUpdate;
        public bool CanDelete => PermissionResult.CanDelete;

        public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;

        #endregion
    }
}
