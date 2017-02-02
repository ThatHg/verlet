using UnityEngine;
using Physics;

public class LeafGenerator : MonoBehaviour {
    [SerializeField] private Leaf[] m_gradient;
    [SerializeField] private Vector3 m_wind_force = new Vector3(0.1f, 0.01f, 0.01f);
    [SerializeField] private GameObject m_leaf_object;
    [SerializeField] private float m_rate = 1 / 60f;
    [SerializeField] private int m_iterations = 3;

    private static readonly int m_leaf_per_branch = 40;
    private Transform[] m_objects = new Transform[m_leaf_per_branch * 8];
    private PhysicsController _controller = new PhysicsController();

    private void Start() {
        m_objects = new Transform[400 * m_leaf_per_branch];
        for (int i = 0; i < 200; ++i) {
            for (int j = 0; j < m_leaf_per_branch; ++j) {
                int number = i * m_leaf_per_branch + j;
                var go = Instantiate(m_leaf_object);
                var sprite_rend = go.GetComponent<SpriteRenderer>();
                go.name = string.Format("Leaf:{0}", number);
                m_objects[number] = go.transform;
                go.transform.SetParent(transform);
                go.transform.position = new Vector3(Random.Range(-10,10f), Random.Range(-10, -7f),0);
                float scale = (m_leaf_per_branch - j) / (float)m_leaf_per_branch;
                go.transform.localScale = new Vector3(scale, scale, scale);
                _controller.add_entity(go.transform, scale);
                sprite_rend.color = (1-scale) * m_gradient[0].m_color + (scale * m_gradient[1].m_color);
                if (j == 0) {
                    _controller.set_static(go.transform.GetInstanceID(), true);
                }
                else {
                    _controller.create_constraint(
                        m_objects[number - 1].transform.GetInstanceID(),
                        m_objects[number].transform.GetInstanceID(),
                        0.1f);
                }
            }
        }
    }

    private void Update() {
        for(int i = 0; i < m_objects.Length; ++i) {
            _controller.add_force(m_wind_force * Random.Range(-1f,1f), i);
        }

        _controller.update(m_rate, m_iterations);
    }
}
