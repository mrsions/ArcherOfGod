using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    public class UIMoveController : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IDragHandler, IEndDragHandler
    {
        enum EType
        {
            None,
            Left,
            Right
        }

        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string m_ControlPath;

        [SerializeField]
        private Image m_ImgLeft;

        [SerializeField]
        private Image m_ImgRight;

        private RectTransform m_Rect;
        private int m_ActivateId = -1;
        private EType m_State = EType.None;

        protected override string controlPathInternal { get => m_ControlPath; set => m_ControlPath = value; }

        private void Awake()
        {
            m_Rect = (RectTransform)transform;
        }

        private void LateUpdate()
        {
            Vector2 val = GameSettings.main.GetPlayerMoveAction().ReadValue<Vector2>();
            if (val != default)
            {
                UpdateUI(val);
            }
            else
            {
                UpdateUI(Vector2.zero);

            }
        }

        private Color Transparent = new(0, 0, 0, 0);
        private void UpdateUI(Vector2 pos)
        {
            EType needState = pos.x == 0 ? EType.None : (pos.x < 0 ? EType.Left : EType.Right);
            if (needState == m_State) return;

            m_State = needState;
            switch (needState)
            {
                case EType.None:
                    m_ImgLeft.color = Transparent;
                    m_ImgRight.color = Transparent;
                    break;
                case EType.Left:
                    m_ImgLeft.color = Color.white;
                    m_ImgRight.color = Transparent;
                    break;
                case EType.Right:
                    m_ImgLeft.color = Transparent;
                    m_ImgRight.color = Color.white;
                    break;
            }
        }

        private void PerformAction(bool activate, PointerEventData eventData)
        {
            if (activate)
            {
                if (eventData == null)
                {
                    throw new System.ArgumentNullException(nameof(eventData));
                }

                m_ActivateId = eventData.pointerId;

                Vector2 pos;
                Camera eventCamera = eventData.pressEventCamera;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Rect, eventData.position, eventCamera, out pos))
                {
                    return;
                }

                pos.x = pos.x > 0 ? 1 : -1;
                pos.y = 0;

                SendValueToControl(pos);
            }
            else
            {
                m_ActivateId = -1;
                SendValueToControl(Vector2.zero);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PerformAction(true, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (m_ActivateId != eventData.pointerId) return;
            PerformAction(false, eventData);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (m_ActivateId != eventData.pointerId) return;
            PerformAction(true, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_ActivateId != eventData.pointerId) return;
            PerformAction(true, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (m_ActivateId != eventData.pointerId) return;
            PerformAction(false, eventData);
        }
    }
}