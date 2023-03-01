using UnityEngine;
using UnityEngine.UI;

public class PlayerMarker : MonoBehaviour {
    [SerializeField]
    private Vector2 padding = new Vector2(0.05f, 0.05f);

    [SerializeField] GameObject player;
    private Image uiImage;

    private void Start() {
        uiImage = GetComponent<Image>();
    }

    private void Update() {
        if(player != null) {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            if(GeometryUtility.TestPlanesAABB(planes, player.GetComponent<Collider>().bounds)) {
                uiImage.enabled = false;
            }
            else {
                Vector3 pos = CalculateViewPosition(player.transform.position,  Camera.main);

                Transform camTransform = Camera.main.transform;

                // Check if target is behind camera
                if(Vector3.Dot(player.transform.position - camTransform.position, camTransform.forward) < 0) {
                    pos.x = pos.x < 0.5 ? 1 : 0;
                    pos.y = 0;
                }

                pos.x = Mathf.Clamp(pos.x, padding.x, 1.0f - padding.x);
                pos.y = Mathf.Clamp(pos.y, padding.y, 1.0f - padding.y);
                pos.z = 0.0f;

                transform.position = CalculateScreenPosition(pos,  Camera.main);
                uiImage.enabled = true;
            }
        }
    }

    private static Vector3 CalculateScreenPosition(Vector3 viewPos, Camera camera) {
        return camera.ViewportToScreenPoint(viewPos);
    }

    private static Vector3 CalculateViewPosition(Vector3 worldPos, Camera camera) {
        return camera.WorldToViewportPoint(worldPos);
    }
}
