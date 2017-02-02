using UnityEngine;

public class Controlls : MonoBehaviour {
    [SerializeField] private Transform nucleus;
    [SerializeField] private Transform atoms;
    [SerializeField] private Vector3 gravity;
    [SerializeField] private float strenth = 0.001f;
    [SerializeField] private float atom_gravity;

    private Physics.PhysicsController _controller;
    private Transform[] _test_atoms;
    private float _timer = 0;
    private int _implode = -1;
    private System.Random _rand = new System.Random();
    private int current_index = 0;

    public Physics.PhysicsController.Constraint[] Constraints { get { return _controller.Constraints; } }
    public Physics.PhysicsController.Entity[] Entities { get { return _controller.Entities; } }

    private void Start() {
        _controller = new Physics.PhysicsController();

        _test_atoms = new Transform[atoms.childCount];
        for(int i = 0; i < atoms.childCount; ++i) {
            _test_atoms[i] = atoms.GetChild(i).transform;
        }
        
        _controller.add_entity(nucleus, 1);
        _controller.set_static(nucleus.GetInstanceID(), true);

        for (int i = 0; i < _test_atoms.Length; ++i) {
            _test_atoms[i].rotation = new Quaternion(Random.Range(-1, 1f), Random.Range(-1, 1f), Random.Range(-1, 1f), Random.Range(-1, 1f));
            _controller.add_entity(_test_atoms[i], 1);
            _controller.create_constraint( nucleus.GetInstanceID(), _test_atoms[i].transform.GetInstanceID(), 7f);
        }
        //_controller.update_rest_len();
        _controller.prepare();
    }

    private void Update() {
        var rend = nucleus.GetComponent<Renderer>();
        rend.sharedMaterial.SetVector("_WorldPos", nucleus.transform.position);
        _controller._gravity = gravity;
        //_controller.update_rest_len();
        for (int i = 0; i < _test_atoms.Length; ++i) {
            _controller.add_force(new Vector3(
            _rand.Next(-100, 100) * strenth,
            _rand.Next(-100, 100) * strenth,
            _rand.Next(-100, 100) * strenth),
            i + 1);

            var dir = (Vector3.zero - _controller.Entities[i + 1]._transform.position);
            dir.Normalize();
            _controller.add_force(dir * atom_gravity, i + 1);
        }
        _controller.update();
    }
}
