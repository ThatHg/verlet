using UnityEngine;
using System.Collections.Generic;
namespace Physics {
    public class PhysicsController {
        private class Force {
            public Force(int i, Vector3 f) {
                _index = i;
                _force = f;
            }

            public int _index;
            public Vector3 _force;
        }

        public class Entity {
            public Entity(Transform t, float m, bool is_static = false) {
                _transform = t;
                _id = t.GetInstanceID();
                _mass = m;
                _inv_mass = 1 / _mass;
                _is_static = is_static;
                _force = Vector3.zero;
            }

            public Transform _transform;
            public Vector3 _force;
            public float _inv_mass;
            public float _mass;
            public int _id;
            public bool _is_static;
        }

        public class Constraint {
            public Constraint(int e1_key, int e2_key, int e1_idx, int e2_idx, float rl) {
                _entity_a_key = e1_key;
                _entity_b_key = e2_key;
                _entity_a_idx = e1_idx;
                _entity_b_idx = e2_idx;
                _rest_len = rl;
            }

            public float _offset;
            public float _rest_len;
            public int _entity_a_key;
            public int _entity_b_key;
            public int _entity_a_idx;
            public int _entity_b_idx;
        }

        private static readonly int MAX_ENTITY_COUNT = 20000;
        public Vector3 _gravity = new Vector3(0, 2, 0);

        private Dictionary<int, int> _entity_map = new Dictionary<int, int>();
        private List<Constraint> _constraints = new List<Constraint>();
        private Queue<Force> _add_forces = new Queue<Force>();
        private Entity[] _entity_pool = new Entity[MAX_ENTITY_COUNT];
        private Vector3[] _old_pos = new Vector3[MAX_ENTITY_COUNT];
        private int _current_pool_index = 0;

        public Constraint[] Constraints { get { return _constraints.ToArray(); } }
        public Entity[] Entities { get { return _entity_pool; } }

        public void set_static(int id, bool is_static) {
            if (_entity_map.ContainsKey(id) == false) {
                Debug.LogErrorFormat("[Error] Entity map does not contain InstanceId: {0}", id);
                return;
            }

            if(_entity_pool[_entity_map[id]] == null) {
                Debug.LogErrorFormat("[Error] Entity pool does not contain InstanceId with Index: {0} - {1}", id, _entity_map[id]);
                return;
            }

            _entity_pool[_entity_map[id]]._is_static = is_static;
        }

        public void prepare(int iterations = 10) {
            update_constraints_bend(iterations); // Iterations = 3
        }

        public void update(float rate = 0.03f, int iterations = 3) {
            accumulate_forces();
            verlet(rate); // rate = frames/sec
            update_constraints_bend(iterations); // Iterations = 3
        }

        // Add force on a point in skeleton
        public void add_force(Vector3 force, int index) {
            _add_forces.Enqueue(new Force(index, force));
        }

        public void add_entity(Transform t, float mass) {
            if(mass <= 0) {
                Debug.LogErrorFormat("[Error] Invalid mass: {0}", mass);
                return;
            }

            if(t == null) {
                Debug.LogError("[Error] Transform is null");
                return;
            }

            if (_entity_map.ContainsKey(t.GetInstanceID())) {
                Debug.LogErrorFormat("[Error] GameObject with id already exist: {0} - {1} ", t.GetInstanceID(), t.name);
                return;
            }

            if(_current_pool_index >= _entity_pool.Length - 1) {
                Debug.LogErrorFormat("[Error] Entity pool reach its limit: {0}", _entity_pool.Length);
                return;
            }
            Entity e = new Entity(t, mass);
            _entity_pool[_current_pool_index] = e;
            _entity_map.Add(t.GetInstanceID(), _current_pool_index);
            _current_pool_index++;
        }

        public void create_constraint(int e1, int e2, float len) {
            if(!_entity_map.ContainsKey(e1) ||
                !_entity_map.ContainsKey(e2)) {
                Debug.LogErrorFormat(
                    "[Error] Enities not in enity dictionary: Entity1 {0} - Entity2 {1} ",
                    _entity_map.ContainsKey(e1),
                    _entity_map.ContainsKey(e2));
                return;
            }

            if(len <= 0) {
                Debug.LogErrorFormat("[Error] Invalid rest length: {0}", len);
                return;
            }

            if(e1 == e2) {
                Debug.LogError("[Error] Entity1 and Entity2 must be two different objects");
            }

            Constraint c = new Constraint(e1, e2, _entity_map[e1], _entity_map[e2], len);
            c._offset = Random.Range(-10f, 10f);
            _constraints.Add(c);
        }

        private void verlet(float rate) {
            for (int i = 0; i < MAX_ENTITY_COUNT; ++i) {
                if (_entity_pool[i] == null) {
                    continue;
                }

                if (_entity_pool[i]._transform == null) {
                    continue;
                }

                if (_entity_pool[i]._is_static) {
                    continue;
                }

                Transform t = _entity_pool[i]._transform;
                Vector3 old_pos = t.position;

                if (_old_pos[i] == null) {
                    _old_pos[i] = old_pos;
                }

                // Verlet integration
                t.position += t.position - _old_pos[i] + _entity_pool[i]._force * (rate * rate);

                // Store the old position until next calculation
                _old_pos[i] = old_pos;
            }
        }

        public void update_rest_len() {
            int NUM_CONSTR = _constraints.Count;
            for (int i = NUM_CONSTR - 1; i >= 0; --i) {
                float f = Time.time;
                float roc = 0.1f;
                float amp = 10f;
                float phase = _constraints[i]._offset;
                _constraints[i]._rest_len = amp * Mathf.Sin(roc * f + phase);
            }
        }

        private void update_constraints(int iterations) {
            for (int j = 0; j < iterations; ++j) {
                // Keep bone lengts and restrain freedom of movement
                int NUM_CONSTR = _constraints.Count;
                for (int i = NUM_CONSTR - 1; i >= 0; --i) {
                    Entity entity_a = _entity_pool[_constraints[i]._entity_a_idx];
                    Entity entity_b = _entity_pool[_constraints[i]._entity_b_idx];

                    if (entity_a == null || entity_b == null) {
                        Debug.LogError("[Error] Entity a or b is null");
                        continue;
                    }

                    // Just skip to next constraint if both inv_masses are 0
                    if (entity_a._inv_mass + entity_b._inv_mass == 0) {
                        continue;
                    }

                    Vector3 delta = entity_b._transform.position - entity_a._transform.position;

                    float diff = 0;
                    float rest_length = _constraints[i]._rest_len;

                    // contstraint with masses from Jacobsen http://graphics.cs.cmu.edu/nsp/course/15-869/2006/papers/jakobsen.htm
                    float deltalength = Vector3.Distance(entity_b._transform.position, entity_a._transform.position);
                    float val = (deltalength * (entity_a._inv_mass + entity_b._inv_mass));
                    if (val == 0) {
                        diff = val;
                    }
                    else {
                        diff = (deltalength - rest_length) / val;
                    }
                    if(entity_a._is_static == false) {
                        entity_a._transform.position += entity_a._inv_mass * delta * diff;
                    }
                    if (entity_b._is_static == false) {
                        entity_b._transform.position -= entity_b._inv_mass * delta * diff;
                    }
                }
            }
        }

        private void update_constraints_bend(int iterations) {
            for (int j = 0; j < iterations; ++j) {
                // Keep bone lengts and restrain freedom of movement
                int NUM_CONSTR = _constraints.Count;
                for (int i = NUM_CONSTR - 1; i >= 0; --i) {
                    Entity entity_a = _entity_pool[_constraints[i]._entity_a_idx];
                    Entity entity_b = _entity_pool[_constraints[i]._entity_b_idx];

                    if (entity_a == null || entity_b == null) {
                        Debug.LogError("[Error] Entity a or b is null");
                        continue;
                    }

                    // Just skip to next constraint if both inv_masses are 0
                    if (entity_a._inv_mass + entity_b._inv_mass == 0) {
                        continue;
                    }

                    Vector3 delta = entity_b._transform.position - entity_a._transform.position;

                    float diff = 0;
                    float rest_length = _constraints[i]._rest_len;

                    float curr_length = Vector3.Distance(entity_a._transform.position, entity_b._transform.position);
                    if (curr_length >= rest_length) {
                        float deltalength = Vector3.Distance(entity_b._transform.position, entity_a._transform.position);
                        float val = (deltalength * (entity_a._inv_mass + entity_b._inv_mass));
                        if (val == 0) {
                            diff = val;
                        }
                        else {
                            diff = (deltalength - rest_length) / val;
                        }
                        if (entity_a._is_static == false) {
                            entity_a._transform.position += entity_a._inv_mass * delta * diff;
                        }
                        if (entity_b._is_static == false) {
                            entity_b._transform.position -= entity_b._inv_mass * delta * diff;
                        }
                    }
                }
            }
        }

        private void accumulate_forces() {
            // Gravity, this step also remove old forces
            for (int i = 0; i < MAX_ENTITY_COUNT; ++i) {
                if (_entity_pool[i] == null || _entity_pool[i]._transform == null) {
                    continue;
                }

                _entity_pool[i]._force = _gravity * _entity_pool[i]._mass;
            }
            while (_add_forces.Count != 0) {
                Force f = _add_forces.Dequeue();

                if (f._index < 0 || f._index >= MAX_ENTITY_COUNT) {
                    Debug.LogErrorFormat("[Error] Entity index for force is invalid: {0}", f._index);
                }

                if (_entity_pool[f._index] == null || _entity_pool[f._index]._transform == null) {
                    continue;
                }

                _entity_pool[f._index]._force += f._force;
            }
        }
    }
}

