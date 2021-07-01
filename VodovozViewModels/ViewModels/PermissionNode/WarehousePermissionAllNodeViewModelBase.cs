using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
    public class WarehousePermissionAllNodeViewModelBase : PropertyChangedBase
    {
        
        private string title;
        public string Title
        {
            get => title; 
            set => SetField(ref title, value);
        }

        private List<WarehousePermissionNodeViewModel> subNodeViewModel;
        public List<WarehousePermissionNodeViewModel> SubNodeViewModel
        {
            get => subNodeViewModel;
            set => SetField(ref subNodeViewModel, value);
        }

        private bool? permissionValue;
        public bool? PermissionValue
        {
            get => permissionValue;
            set
            {
                if (UnSetAll) SetField(ref permissionValue, value);
                else if (SetField(ref permissionValue, value))
                {
                    if (permissionValue.HasValue || !value.HasValue)
                    {
                        foreach (var subNode in SubNodeViewModel)
                        {
                            subNode.UnSubscribe = true;
                            subNode.PermissionValue = value;
                            subNode.UnSubscribe = false;
                        }
                    }
                    else if (permissionValue == value)
                    {
                        foreach (var subNode in SubNodeViewModel)
                        {
                            subNode.UnSubscribe = true;
                            subNode.PermissionValue = value;
                            subNode.UnSubscribe = false;
                        }
                    }
                }
                ItemChangeSelectAll?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool UnSetAll = false;
        public bool UnSubscribeAll = false;

        public EventHandler ItemChangeSelectAll;
    }
}