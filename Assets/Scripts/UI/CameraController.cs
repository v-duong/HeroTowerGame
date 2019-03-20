using UnityEngine;

public class CameraController : MonoBehaviour
{
    private bool isDragging; 
    private static float dragspeed = 0.35f;
#if UNITY_STANDALONE
    
    private bool isRotating;    // Is the camera being rotated?
    private bool isZooming;     // Is the camera zooming?
    private static float speed = 3.0f;

#endif

    // Update is called once per frame
    private void LateUpdate()
    {
        if (!GameManager.Instance.isInBattle)
            return;
#if UNITY_STANDALONE

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }


        if (!Input.GetMouseButton(0)) isPanning = false;

        if (isDragging)
        {
            Vector3 move = new Vector3(Input.GetAxis("Mouse X") * dragspeed, Input.GetAxis("Mouse Y") * dragspeed, 0);
            transform.Translate(-move, Space.Self);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
        }
#endif
#if UNITY_ANDROID


#endif
    }
}