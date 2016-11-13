using UnityEngine;

public class Controlls : MonoBehaviour {
    [SerializeField] private Transform chain;
    [SerializeField] private Transform ball;

    private Physics.PhysicsController _controller;
    private Transform[] _test_subjects;
    private Transform[] _test_subjects3;
    private float _timer = 0;
    private int _implode = -1;
    private float _strenth = 0.1f;
    private System.Random _rand = new System.Random();
    private int current_index = 0;

    public Physics.PhysicsController.Constraint[] Constraints { get { return _controller.Constraints; } }
    public Physics.PhysicsController.Entity[] Entities { get { return _controller.Entities; } }
    public int ChainCount { get { return _test_subjects.Length; } }

    private void Start() {
        _controller = new Physics.PhysicsController();
        _test_subjects = new Transform[chain.childCount];
        for (int i = 0; i < chain.childCount; ++i) {
            _test_subjects[i] = chain.GetChild(i).transform;
        }

        _test_subjects3 = new Transform[ball.childCount];
        for(int i = 0; i < ball.childCount; ++i) {
            _test_subjects3[i] = ball.GetChild(i).transform;
        }

        for (int i = 0; i < _test_subjects.Length; ++i) {
            _controller.add_entity(_test_subjects[i], 1888);

            if (i == Mathf.RoundToInt(_test_subjects.Length / 2)) {
                _controller.set_static(_test_subjects[i].GetInstanceID(), true);
            }

            if (i > 0) {
                _controller.create_constraint(
                    _test_subjects[i - 1].transform.GetInstanceID(),
                    _test_subjects[i].transform.GetInstanceID(),
                    2f);
            }
        }

        for (int i = 0; i < _test_subjects3.Length; ++i) {
            _test_subjects3[i].rotation = new Quaternion(Random.Range(-1, 1f), Random.Range(-1, 1f), Random.Range(-1, 1f), Random.Range(-1, 1f));
            _controller.add_entity(_test_subjects3[i], 1);
            if (i > _test_subjects3.Length / 2f) {
                _controller.create_constraint(
                    _test_subjects[_test_subjects.Length - 1].transform.GetInstanceID(),
                    _test_subjects3[i].transform.GetInstanceID(),
                    7f);
            }
            else {
                _controller.create_constraint(
                    _test_subjects[0].transform.GetInstanceID(),
                    _test_subjects3[i].transform.GetInstanceID(),
                    7f);
            }
            
        }
    }

    private void Update() {
        _controller.add_force(new Vector3(1, 0, 0), 0);
        _controller.add_force(new Vector3(-1, 0, 0), _test_subjects.Length - 1);

        for (int i = 0; i < _test_subjects3.Length; ++i) {
            if (i > _test_subjects3.Length / 2f) {
                _controller.add_force(new Vector3(
                Mathf.Sin(Time.realtimeSinceStartup),
                Mathf.Sin(_rand.Next(-10, 10) * _strenth),
                Mathf.Sin(_rand.Next(-10, 10) * _strenth)),
                i + _test_subjects.Length);
            }
            else {
                _controller.add_force(new Vector3(
                Mathf.Cos(-Time.realtimeSinceStartup),
                Mathf.Sin(_rand.Next(-10, 10) * _strenth),
                Mathf.Sin(_rand.Next(-10, 10) * _strenth)),
                i + _test_subjects.Length);
            }
        }

        _controller.update();
    }
}
