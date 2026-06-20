using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Drags a weapon item or equipped slot entry in the loadout panel.
    /// </summary>
    [RequireComponent(typeof(LoadoutItemView))]
    public class LoadoutDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private GraphicRaycaster raycaster;

        private LoadoutItemView itemView;
        private GameObject dragGhost;
        private RectTransform dragGhostRect;
        private CanvasGroup sourceCanvasGroup;

        private void Awake()
        {
            itemView = GetComponent<LoadoutItemView>();
            if (rootCanvas == null)
                rootCanvas = GetComponentInParent<Canvas>();

            if (raycaster == null && rootCanvas != null)
                raycaster = rootCanvas.GetComponent<GraphicRaycaster>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemView == null || !itemView.CanDrag)
                return;

            if (itemView.IsEquippedWeapon)
                LoadoutDragSession.BeginEquippedWeapon(itemView.WeaponId, itemView.EquippedSlotIndex);
            else
                LoadoutDragSession.BeginWeapon(itemView.WeaponId);

            sourceCanvasGroup = GetComponent<CanvasGroup>();
            if (sourceCanvasGroup == null)
                sourceCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            sourceCanvasGroup.alpha = 0.55f;
            sourceCanvasGroup.blocksRaycasts = false;

            dragGhost = Instantiate(gameObject, rootCanvas != null ? rootCanvas.transform : transform);
            dragGhost.name = "LoadoutDragGhost";
            Destroy(dragGhost.GetComponent<LoadoutDragHandle>());
            Destroy(dragGhost.GetComponent<LoadoutItemView>());

            dragGhostRect = dragGhost.GetComponent<RectTransform>();
            var ghostGroup = dragGhost.GetComponent<CanvasGroup>() ?? dragGhost.AddComponent<CanvasGroup>();
            ghostGroup.alpha = 0.85f;
            ghostGroup.blocksRaycasts = false;

            if (raycaster != null)
                raycaster.enabled = false;

            UpdateGhostPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData) => UpdateGhostPosition(eventData);

        public void OnEndDrag(PointerEventData eventData)
        {
            if (sourceCanvasGroup != null)
            {
                sourceCanvasGroup.alpha = 1f;
                sourceCanvasGroup.blocksRaycasts = true;
            }

            if (raycaster != null)
                raycaster.enabled = true;

            if (dragGhost != null)
                Destroy(dragGhost);

            LoadoutDragSession.Clear();
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            if (dragGhostRect == null || rootCanvas == null)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                eventData.position,
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
                out Vector2 localPoint);

            dragGhostRect.localPosition = localPoint;
        }
    }
}
