using UnityEngine;
using UnityEngine.UI;

public class PlayerMarker : MonoBehaviour {
    [SerializeField]
    private Vector2 padding = new Vector2(0.05f, 0.05f);

    [SerializeField] GameObject player;
    private Image uiImage;
    private Camera cam;

    private void Start() {
        uiImage = GetComponent<Image>();
        cam = Camera.main;
    }

    private void Update() {
        if (player != null && player.active) {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            if (GeometryUtility.TestPlanesAABB(planes, player.GetComponent<Collider>().bounds)) {
                //Physics.Raycast(cam.transform.position, (player.transform.position - cam.transform.position), out RaycastHit hit);
                //if (hit.collider != null && hit.collider.tag.Equals("Player")) {
                    uiImage.enabled = false;
                //}
                //else {
                //    Vector3 pos = cam.WorldToViewportPoint(player.transform.position);

                 //   pos.x = Mathf.Clamp(pos.x, padding.x, 1.0f - padding.x);
                 //   pos.y = Mathf.Clamp(pos.y, padding.y, 1.0f - padding.y);
                 //   pos.z = 0.0f;

                 //   transform.position = cam.ViewportToScreenPoint(pos);
                 //   uiImage.enabled = true;
                //}
            }
            else {
                Vector3 pos = cam.WorldToViewportPoint(player.transform.position);

                Transform camTransform = Camera.main.transform;

                // Check if player is behind camera
                if(Vector3.Dot(player.transform.position - camTransform.position, camTransform.forward) < 0) {
                    pos.x = pos.x < 0.5 ? 1 : 0;
                    pos.y = 0;
                }

                pos.x = Mathf.Clamp(pos.x, padding.x, 1.0f - padding.x);
                pos.y = Mathf.Clamp(pos.y, padding.y, 1.0f - padding.y);
                pos.z = 0.0f;

                transform.position = cam.ViewportToScreenPoint(pos);
                uiImage.enabled = true;
            }
        }
    }
}
