using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityLibrary
{
    public class DrawLine : MonoBehaviour
    {

        [SerializeField]
        protected LineRenderer m_LineRenderer;
        [SerializeField]
        protected Camera m_Camera;
        protected List<Vector3> m_Points;

        public virtual LineRenderer lineRenderer {
            get {
                return m_LineRenderer;
            }
        }

        public virtual new Camera camera {
            get {
                return m_Camera;
            }
        }

        public virtual List<Vector3> points {
            get {
                return m_Points;
            }
        }

        protected virtual void Awake()
        {
            if (m_LineRenderer == null)
            {
                Debug.LogWarning("DrawLine: Line Renderer not assigned, Adding and Using default Line Renderer.");
                CreateDefaultLineRenderer();
            }
            if (m_Camera == null)
            {
                Debug.LogWarning("DrawLine: Camera not assigned, Using Main Camera or Creating Camera if main not exists.");
                CreateDefaultCamera();
            }
            m_Points = new List<Vector3>();
        }

        protected virtual void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Reset();
            }
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePosition = m_Camera.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = m_LineRenderer.transform.position.z;
                if (!m_Points.Contains(mousePosition))
                {
                    m_Points.Add(mousePosition);
                    m_LineRenderer.positionCount = m_Points.Count;
                    m_LineRenderer.SetPosition(m_LineRenderer.positionCount - 1, mousePosition);
                }
            }
        }

        protected virtual void Reset()
        {
            if (m_LineRenderer != null)
            {
                m_LineRenderer.positionCount = 0;
            }
            if (m_Points != null)
            {
                m_Points.Clear();
            }
        }

        protected virtual void CreateDefaultLineRenderer()
        {
            m_LineRenderer = gameObject.AddComponent<LineRenderer>();
            m_LineRenderer.positionCount = 0;
            m_LineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            m_LineRenderer.startColor = Color.white;
            m_LineRenderer.endColor = Color.white;
            m_LineRenderer.startWidth = 0.3f;
            m_LineRenderer.endWidth = 0.3f;
            m_LineRenderer.useWorldSpace = true;
        }

        protected virtual void CreateDefaultCamera()
        {
            m_Camera = Camera.main;
            if (m_Camera == null)
            {
                m_Camera = gameObject.AddComponent<Camera>();
            }
            m_Camera.orthographic = true;
        }

    }
}
