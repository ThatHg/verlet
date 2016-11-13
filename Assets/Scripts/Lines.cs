using UnityEngine;
public class Lines : MonoBehaviour {
    [SerializeField] private Controlls _controlls;
    static Material lineMaterial;
    static void CreateLineMaterial() {
        if (!lineMaterial) {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    // Will be called after all regular rendering is done
    public void OnRenderObject() {
        CreateLineMaterial();
        // Apply the line material
        lineMaterial.SetPass(0);

        GL.PushMatrix();

        // Draw lines
        Physics.PhysicsController.Constraint[] constraints = _controlls.Constraints;
        Physics.PhysicsController.Entity[] entities = _controlls.Entities;
        int chain_link_count = _controlls.ChainCount;
        GL.Begin(GL.LINES);
        GL.Color(new Color(0.7f, 0.0f, 0.1f, 0.7f));
        for (int i = 0; i < constraints.Length; ++i) {
            if(i == chain_link_count-1) {
                GL.Color(new Color(0.7f, 0.4f, 0.1f, 0.2f));
            }
            var e1_pos = entities[constraints[i]._entity_a_idx]._transform.position;
            var e2_pos = entities[constraints[i]._entity_b_idx]._transform.position;
            GL.Vertex3(e1_pos.x, e1_pos.y,e1_pos.z);
            GL.Vertex3(e2_pos.x, e2_pos.y, e2_pos.z);
        }
        GL.End();
        GL.PopMatrix();
    }
}

