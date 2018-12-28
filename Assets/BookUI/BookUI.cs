using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
    [DisallowMultipleComponent]
    public class BookUI : MonoBehaviour
    {
        [SerializeField, Range(0, 3)]
        float TurnTime = 0.5f;
        [SerializeField, Range(-2, 2)]
        float TurnPageTilt = 1f;
        [SerializeField]
        Shader shader;
        [SerializeField]
        public UnityEvent OnPageChanged = new UnityEvent();

        int PageID;
        float _currentPosition = 0;
        float CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                _currentPosition = value;
                material.SetFloat(PageID, _currentPosition);
            }
        }

        int _currentPage;
        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (_currentPage == value) return;
                _currentPage = value;
                startPosition = CurrentPosition;
                currentTime = 0;
                if (OnPageChanged != null)
                    OnPageChanged.Invoke();
            }
        }

        Material _mat;
        Material material
        {
            get
            {
                if (_mat == null)
                {
                    _mat = new Material(shader);
                    var matrix = Matrix4x4.Scale(new Vector3(1f / Resolution.x, 1f / Resolution.y, 1)) * CalcCanvas2LocalMatrix();
                    _mat.SetMatrix("_Canvas2Local", matrix);
                    _mat.SetMatrix("_Local2Canvas", matrix.inverse);
                    _mat.SetFloat("_Tilt", TurnPageTilt);
                }
                return _mat;
            }
        }

        Matrix4x4 CalcCanvas2LocalMatrix()
        {
            var canvaslist = GetComponentsInParent<Canvas>();
            return transform.worldToLocalMatrix * canvaslist[canvaslist.Length - 1].transform.worldToLocalMatrix.inverse;
        }

        Vector2? _resolution = null;
        public Vector2 Resolution
        {
            get
            {
                if (_resolution == null) _resolution = CalcResolution();
                return _resolution.Value;
            }
        }

        Vector2 CalcResolution()
        {
            var scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ConstantPixelSize)
                {
                    var canvas = GetComponent<Canvas>();
                    if (canvas.isRootCanvas)
                        return scaler.referenceResolution;
                }
            var rect = GetComponent<RectTransform>().rect;
            return new Vector2(rect.width, rect.height);
        }

        void Awake()
        {
            PageID = Shader.PropertyToID("_Page");
			foreach (var g in GetComponentsInChildren<Graphic>(true))
                g.material = material;
        }

        float currentTime = -1;
        float startPosition;
        void Update()
        {
            if (currentTime < 0) return;
            currentTime += Time.unscaledDeltaTime;
            float t = currentTime / TurnTime;
            if (currentTime >= TurnTime)
            {
                currentTime = -1;
                t = 1f;
            }
            CurrentPosition = Mathf.SmoothStep(startPosition, CurrentPage, t);
        }
    }
}
