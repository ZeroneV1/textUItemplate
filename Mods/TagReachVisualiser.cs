using UnityEngine;
using System.Collections;

namespace TextUITemplate.Mods
{
    public class TagReachVisualizer : MonoBehaviour
    {
        private static TagReachVisualizer _instance;
        public static TagReachVisualizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("TagReachVisualizerInstance");
                    _instance = go.AddComponent<TagReachVisualizer>();
                    // DontDestroyOnLoad(go); // Optional: if it should persist scenes
                }
                return _instance;
            }
        }

        // Keep track of active visualization spheres to prevent stacking
        private GameObject leftHandSphere = null;
        private GameObject rightHandSphere = null;
        private Coroutine leftSphereCoroutine = null;
        private Coroutine rightSphereCoroutine = null;

        public void ShowVisualization(Vector3 handPosition, float range, Color color, bool isLeftHand)
        {
            if (isLeftHand)
            {
                if (leftSphereCoroutine != null) StopCoroutine(leftSphereCoroutine);
                if (leftHandSphere != null) Destroy(leftHandSphere);
                leftHandSphere = CreateSphere(handPosition, range, color);
                leftSphereCoroutine = StartCoroutine(DestroySphereAfterDelay(leftHandSphere, 2.0f, true));
            }
            else
            {
                if (rightSphereCoroutine != null) StopCoroutine(rightSphereCoroutine);
                if (rightHandSphere != null) Destroy(rightHandSphere);
                rightHandSphere = CreateSphere(handPosition, range, color);
                rightSphereCoroutine = StartCoroutine(DestroySphereAfterDelay(rightHandSphere, 2.0f, false));
            }
        }

        private GameObject CreateSphere(Vector3 position, float range, Color color)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            Collider col = sphere.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

            sphere.transform.position = position;
            // The range parameter is the desired radius, scale is diameter for CreatePrimitive.Sphere
            sphere.transform.localScale = new Vector3(range * 2, range * 2, range * 2);

            Renderer rend = sphere.GetComponent<Renderer>();
            if (rend != null)
            {
                // It's better to create an instance of the material if you're changing its properties
                // to avoid modifying the shared material asset.
                Material matInstance = new Material(Shader.Find("GUI/Text Shader")); // Or a transparent shader
                Color displayColor = color;
                displayColor.a = 0.25f; // Set transparency
                matInstance.color = displayColor;

                // For GUI/Text Shader to be transparent, you might need to set properties for blending
                // Or use a shader designed for transparency like "Legacy Shaders/Transparent/Diffuse"
                // For GUI/Text Shader, transparency is usually handled by the font texture itself.
                // Using a standard transparent shader is more reliable for geometry.
                // Let's try a standard transparent material setup
                matInstance.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse"); // A common transparent shader
                matInstance.color = displayColor;

                rend.material = matInstance;
            }
            return sphere;
        }

        private IEnumerator DestroySphereAfterDelay(GameObject sphereToDestroy, float delay, bool isLeft)
        {
            yield return new WaitForSeconds(delay);
            if (sphereToDestroy != null)
            {
                Destroy(sphereToDestroy);
            }
            if (isLeft) leftHandSphere = null; else rightHandSphere = null;
            if (isLeft) leftSphereCoroutine = null; else rightSphereCoroutine = null;
        }

        void OnDestroy() // Cleanup if this GameObject is destroyed
        {
            if (leftHandSphere != null) Destroy(leftHandSphere);
            if (rightHandSphere != null) Destroy(rightHandSphere);
            if (leftSphereCoroutine != null) StopCoroutine(leftSphereCoroutine);
            if (rightSphereCoroutine != null) StopCoroutine(rightSphereCoroutine);
            if (_instance == this) _instance = null;
        }
    }
}