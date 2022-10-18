using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
    public class WarehousePermissionAllNodeViewModelBase : PropertyChangedBase, IPermissionNodeViewModel
    {
	    private string _title;
        public string Title
        {
            get => _title; 
            set => SetField(ref _title, value);
        }
        
        private List<WarehousePermissionNodeViewModel> _subNodeViewModel;
        public List<WarehousePermissionNodeViewModel> SubNodeViewModel
        {
            get => _subNodeViewModel;
            set => SetField(ref _subNodeViewModel, value);
        }

        private bool? _permissionValue;
        public bool? PermissionValue
        {
            get => _permissionValue;
            set
            {
                if (UnSetAll) SetField(ref _permissionValue, value);
                else if (SetField(ref _permissionValue, value))
                {
                    if (_permissionValue.HasValue || !value.HasValue)
                    {
                        foreach (var subNode in SubNodeViewModel)
                        {
                            subNode.Unsubscribed = true;
                            subNode.PermissionValue = value;
                            subNode.Unsubscribed = false;
                        }
                    }
                    else if (_permissionValue == value)
                    {
                        foreach (var subNode in SubNodeViewModel)
                        {
                            subNode.Unsubscribed = true;
                            subNode.PermissionValue = value;
                            subNode.Unsubscribed = false;
                        }
                    }
                }
                ItemSelectAllChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool UnSetAll = false;
        public bool UnsubscribedAll = false;

        public EventHandler ItemSelectAllChanged;
    }
}
