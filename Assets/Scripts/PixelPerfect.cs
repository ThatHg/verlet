using UnityEngine;
public class PixelPerfect : MonoBehaviour {
    [SerializeField] private Sprite m_reference;

    // Use this for initialization
    void Start () {
        float size = m_reference.bounds.size.y;
        if(m_reference.bounds.size.x < m_reference.bounds.size.y) {
            size = m_reference.bounds.size.x;
        }

        var cam = Camera.main;
        cam.orthographicSize  = Screen.width / (((Screen.width / Screen.height) * 2) * size);
    }
}
