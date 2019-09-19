using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
    [RequireComponent(typeof(BookUI))]
    public class PageSelector : MonoBehaviour
    {
        void Start()
        {
            var layout = GetComponentInChildren<ILayoutElement>() as Component;
            if (layout != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.transform as RectTransform);

            var book = GetComponent<BookUI>();
            foreach (var target in GetComponentsInChildren<Selectable>())
            {
                var matrix = book.transform.localToWorldMatrix.inverse * target.transform.localToWorldMatrix;
                float x = matrix.m03 / book.Resolution.x;

                var trigger = target.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Select;
                entry.callback.AddListener((eventData) => { book.CurrentPage = Mathf.FloorToInt(x + 0.5f); });
                trigger.triggers.Add(entry);
            }
        }
    }
}
