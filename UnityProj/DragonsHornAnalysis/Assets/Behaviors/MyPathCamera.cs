using UnityEngine;

public class MyPathCamera : MonoBehaviour
{
    public Transform target;

    public Camera m_Camera;

    public float panSpeed = 5f;

    public float pitchMin = 10f;

    public float pitchMax = 90f;

    public float pitchSpeed = 10f;

    public float distanceMin = 0.1f;

    public float distanceMax = 40.0f;

    public float zoomSpeed = 2f;

    public float pitch = 45f;

    public float yaw = 0f;

    public float distance = 10.0f;

    public float distanceToOrthoSize = 1f;
    public float orthoDistance = 100.0f;

    public Vector3 pan = Vector3.zero;

    private bool panning = false;
    private bool looking = false;

    private void Start()
    {
        m_Camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (panning)
        {
            pan.x += panSpeed * distance * Input.GetAxis("Mouse X");
            pan.z += panSpeed * distance * Input.GetAxis("Mouse Y");
        }

        if (looking)
        {
            pitch += pitchSpeed * Input.GetAxis("Mouse Y");
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            distance *= Mathf.Pow(zoomSpeed, -scroll);
        }

        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        distance = Mathf.Clamp(distance, distanceMin, distanceMax);

        transform.position = (target != null ? target.position : Vector3.zero) + pan;
        transform.eulerAngles = new Vector3(pitch, yaw, 0);
        transform.position -= (m_Camera.orthographic ? orthoDistance : distance) * transform.forward;
        if (m_Camera.orthographic)
        {
            m_Camera.orthographicSize = distanceToOrthoSize * distance;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            panning = true;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            panning = false;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            looking = true;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            looking = false;
        }

        if (looking || panning)
        {
            Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;
        }
    }

    void OnDisable()
    {
        looking = false;
        panning = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}