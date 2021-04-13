using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
    public class WarehousePermissionAllNodeViewModelBase
    {
        public string Title { get; set; }
        
        public IEnumerable<WarehousePermissionNodeViewModel> SubNodeViewModel { get; set; }
        
        public bool? PermissionValue { get; set; }
    }
}