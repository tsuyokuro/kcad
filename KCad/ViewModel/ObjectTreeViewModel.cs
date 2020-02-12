using KCad.Controls;
using KCad.Dialogs;
using Plotter;
using Plotter.Settings;

namespace KCad.ViewModel
{
    public class ObjectTreeViewModel
    {
        ViewModelContext mVMContext;

        CadObjectTreeView mCadObjectTreeView;

        public CadObjectTreeView ObjectTreeView
        {
            set
            {
                if (mCadObjectTreeView != null)
                {
                    mCadObjectTreeView.CheckChanged -= ObjectTreeView_CheckChanged;
                    mCadObjectTreeView.ItemCommand -= ObjectTreeView_ItemCommand;
                }

                mCadObjectTreeView = value;

                if (mCadObjectTreeView != null)
                {
                    mCadObjectTreeView.CheckChanged += ObjectTreeView_CheckChanged;
                    mCadObjectTreeView.ItemCommand += ObjectTreeView_ItemCommand;
                }
            }

            get => mCadObjectTreeView;
        }

        public ObjectTreeViewModel(ViewModelContext context)
        {
            mVMContext = context;

            mVMContext.Controller.Observer.UpdateObjectTree = UpdateTreeView;

            mVMContext.Controller.Observer.SetObjectTreePos = SetTreeViewPos;

            mVMContext.Controller.Observer.FindObjectTreeItemIndex = FindTreeViewItemIndex;
        }

        private void ObjectTreeView_CheckChanged(CadObjTreeItem item)
        {
            mVMContext.Controller.CurrentFigure =
                TreeViewUtil.GetCurrentFigure(item, mVMContext.Controller.CurrentFigure);

            mVMContext.Redraw();
        }

        private void UpdateTreeView(bool remakeTree)
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                UpdateTreeViewProc(remakeTree);
            }, true);
        }

        private void UpdateTreeViewProc(bool remakeTree)
        {
            if (mCadObjectTreeView == null)
            {
                return;
            }

            if (SettingsHolder.Settings.FilterObjectTree)
            {
                CadLayerTreeItem item = new CadLayerTreeItem();
                item.AddChildren(mVMContext.Controller.CurrentLayer, fig =>
                {
                    return fig.HasSelectedPointInclueChild();
                });

                mCadObjectTreeView.AttachRoot(item);
                mCadObjectTreeView.Redraw();
            }
            else
            {
                if (remakeTree)
                {
                    CadLayerTreeItem item = new CadLayerTreeItem(mVMContext.Controller.CurrentLayer);
                    mCadObjectTreeView.AttachRoot(item);
                    mCadObjectTreeView.Redraw();
                }
                else
                {
                    mCadObjectTreeView.Redraw();
                }
            }
        }

        private void SetTreeViewPos(int index)
        {
            if (mCadObjectTreeView == null)
            {
                return;
            }

            ThreadUtil.RunOnMainThread(() => {
                mCadObjectTreeView.SetVPos(index);
            }, true);
        }

        private int FindTreeViewItemIndex(uint id)
        {
            int idx = mCadObjectTreeView.Find((item) =>
            {
                if (item is CadFigTreeItem)
                {
                    CadFigTreeItem figItem = (CadFigTreeItem)item;

                    if (figItem.Fig.ID == id)
                    {
                        return true;
                    }
                }

                return false;
            });

            return idx;
        }

        public void ObjectTreeView_ItemCommand(CadObjTreeItem treeItem, string cmd)
        {
            if (!(treeItem is CadFigTreeItem))
            {
                return;
            }

            CadFigTreeItem figItem = (CadFigTreeItem)treeItem;

            if (cmd == CadFigTreeItem.ITEM_CMD_CHANGE_NAME)
            {
                CadFigure fig = figItem.Fig;

                InputStringDialog dlg = new InputStringDialog();

                dlg.Message = KCad.Properties.Resources.string_input_fig_name;

                if (fig.Name != null)
                {
                    dlg.InputString = fig.Name;
                }

                bool? dlgRet = dlg.ShowDialog();

                if (dlgRet.Value)
                {
                    fig.Name = dlg.InputString;
                    UpdateTreeView(false);
                }
            }
        }
    }
}
