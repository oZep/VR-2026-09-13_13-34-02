using UnityEngine;

// Attach to the ripple prefab: a flat, unlit, transparent ring or quad
// (e.g. a Quad with an Unlit/Transparent material, or a ring mesh) sized
// ~0.05 units by default, since this script scales it up from there.
public class TapRipple : MonoBehaviour
{
    float _duration = 0.6f;
    float _elapsed;
    float _maxRadius = 0.4f;

    Renderer _renderer;
    MaterialPropertyBlock _block;
    Color _baseColor;

    public void Configure(Color color, float radius, float duration)
    {
        _maxRadius = Mathf.Max(radius, 0.01f);
        _duration = Mathf.Max(duration, 0.05f);
        _baseColor = color;
        _renderer = GetComponent<Renderer>();
        _block = new MaterialPropertyBlock();
        transform.localScale = Vector3.one * 0.05f;
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        // Ease-out cubic: quick pop, then slows as it spreads and fades.
        float easedT = 1f - Mathf.Pow(1f - t, 3f);
        float s = Mathf.Lerp(0.05f, _maxRadius, easedT);
        transform.localScale = Vector3.one * s;

        if (_renderer != null)
        {
            var c = _baseColor;
            c.a = Mathf.Lerp(_baseColor.a, 0f, t);
            _block.SetColor("_BaseColor", c);   // URP Unlit/Lit property name
            _renderer.SetPropertyBlock(_block);
        }

        if (t >= 1f) Destroy(gameObject);
    }
}