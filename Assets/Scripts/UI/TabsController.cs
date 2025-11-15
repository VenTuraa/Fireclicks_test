using UnityEngine;

namespace Fireclicks.UI
{
    public class TabsController : MonoBehaviour
    {
        [SerializeField] private GameObject _listTab;
        [SerializeField] private GameObject _groupedListTab;
        [SerializeField] private GameObject _requestTab;

        private VirtualizedList _virtualizedList;
        private GroupedList _groupedList;
        private VirtualizedList _groupedVirtualizedList;

        private void Awake()
        {
            InitializeComponents();
            InitializeLists();
        }

        private void InitializeComponents()
        {
            if (_listTab != null)
            {
                _virtualizedList = _listTab.GetComponentInChildren<VirtualizedList>();
            }

            if (_groupedListTab != null)
            {
                _groupedList = _groupedListTab.GetComponentInChildren<GroupedList>();
                _groupedVirtualizedList = _groupedListTab.GetComponentInChildren<VirtualizedList>();
            }
        }

        private void InitializeLists()
        {
            if (_virtualizedList != null)
            {
                _virtualizedList.Initialize();
            }
            else if (_listTab != null)
            {
                Debug.LogWarning($"[TabsController] VirtualizedList not found in {_listTab.name}!", this);
            }

            if (_groupedList != null)
            {
                _groupedList.Initialize();

                if (_groupedVirtualizedList != null)
                {
                    int totalCount = _groupedList.GetTotalCount();
                    Debug.Log($"[TabsController] GroupedList total count: {totalCount}");
                    if (totalCount > 0)
                    {
                        _groupedVirtualizedList.SetVisibleItemCount(3);
                        
                        _groupedVirtualizedList.SetTotalCount(totalCount);
                        
                        _groupedVirtualizedList.SetItemCreatedCallback((rect, index) =>
                        {
                            _groupedList.UpdateItemVisual(rect, index);
                        });
                        
                        _groupedVirtualizedList.Initialize();
                    }
                    else
                    {
                        Debug.LogWarning($"[TabsController] GroupedList has 0 items!", this);
                    }
                }
                else
                {
                    Debug.LogWarning($"[TabsController] VirtualizedList not found in {_groupedListTab.name}!", this);
                }
            }
            else if (_groupedListTab != null)
            {
                Debug.LogWarning($"[TabsController] GroupedList not found in {_groupedListTab.name}!", this);
            }
        }

        public void ShowListTab()
        {
            HideAllTabs();
            if (_listTab != null)
            {
                _listTab.SetActive(true);
            }
        }

        public void ShowGroupedListTab()
        {
            HideAllTabs();
            if (_groupedListTab != null)
            {
                _groupedListTab.SetActive(true);
            }
        }

        public void ShowRequestTab()
        {
            HideAllTabs();
            if (_requestTab != null)
            {
                _requestTab.SetActive(true);
            }
        }

        private void HideAllTabs()
        {
            if (_listTab != null)
                _listTab.SetActive(false);

            if (_groupedListTab != null)
                _groupedListTab.SetActive(false);

            if (_requestTab != null)
                _requestTab.SetActive(false);
        }

        private void Start()
        {
            ShowListTab();
        }
    }
}
