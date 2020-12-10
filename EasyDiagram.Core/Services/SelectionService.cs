using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyDiagram.Core
{
    public class SelectionService
    {
        #region Constructors
        public SelectionService(DesignerCanvas canvas)
        {
            _designerCanvas = canvas;
        }
        #endregion

        #region Properties
        public List<ISelectable> CurrentSelection
        {
            get => _currentSelection;
        }
        #endregion

        #region Members
        DesignerCanvas _designerCanvas;
        List<ISelectable> _currentSelection = new List<ISelectable>();
        #endregion

        #region Methods
        public void SelectItem(ISelectable item)
        {
            ClearSelection();
            AddToSelection(item);
        }

        public void AddToSelection(ISelectable item)
        {
            if (item is IGroupable)
            {
                List<IGroupable> groupItems = GetGroupMembers(item as IGroupable);
                foreach (ISelectable groupItem in groupItems)
                {
                    groupItem.IsSelected = true;
                    _currentSelection.Add(groupItem);
                }
            }
            else
            {
                item.IsSelected = true;
                _currentSelection.Add(item);
            }
        }

        public void RemoveFromSelection(ISelectable item)
        {
            if (item is IGroupable)
            {
                List<IGroupable> groupItems = GetGroupMembers(item as IGroupable);
                foreach (ISelectable groupItem in groupItems)
                {
                    groupItem.IsSelected = false;
                    _currentSelection.Remove(groupItem);
                }
            }
            else
            {
                item.IsSelected = false;
                _currentSelection.Remove(item);
            }
        }

        public void ClearSelection()
        {
            _currentSelection.ForEach(item => item.IsSelected = false);
            _currentSelection.Clear();
        }

        public void SelectAll()
        {
            ClearSelection();
            CurrentSelection.AddRange(_designerCanvas.Children.OfType<ISelectable>());
            CurrentSelection.ForEach(item => item.IsSelected = true);
        }

        public IGroupable GetGroupRoot(IGroupable item)
        {
            IEnumerable<IGroupable> list = _designerCanvas.Children.OfType<IGroupable>();
            return GetRoot(list, item);
        }

        public IGroupable GetRoot(IEnumerable<IGroupable> list, IGroupable node)
        {
            if (node == null || node.ParentId == Guid.Empty)
            {
                return node;
            }
            else
            {
                foreach (IGroupable item in list)
                {
                    if (item.Id == node.ParentId)
                    {
                        return GetRoot(list, item);
                    }
                }
                return null;
            }
        }

        public List<IGroupable> GetGroupMembers(IGroupable item)
        {
            IEnumerable<IGroupable> list = _designerCanvas.Children.OfType<IGroupable>();
            IGroupable rootItem = GetRoot(list, item);
            return GetGroupMembers(list, rootItem);
        }

        public List<IGroupable> GetGroupMembers(IEnumerable<IGroupable> list, IGroupable parent)
        {
            List<IGroupable> groupMembers = new List<IGroupable>();
            groupMembers.Add(parent);

            var children = list.Where(node => node.ParentId == parent.Id);

            foreach (IGroupable child in children)
            {
                groupMembers.AddRange(GetGroupMembers(list, child));
            }

            return groupMembers;
        }
        #endregion
    }
}
