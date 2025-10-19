using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class TouchVFXAtTouchPointSimple : MonoBehaviour
{
    [Header("Setup")]
    public VisualEffect vfxPrefab;          // your VFX Graph prefab
    public string touchTag = "Hand";        // tag on hand/finger
    public bool useTrigger = true;          // TRUE if collider is trigger
    public float surfaceOffset = 0.003f;    // offset to avoid clipping

    private Collider _surface;
    private VisualEffect _vfx;
    private readonly ContactPoint[] _contacts = new ContactPoint[8];
    private int _touchCount = 0;
    private Coroutine _deactivateCoroutine;

    void Awake()
    {
        _surface = GetComponent<Collider>();

        if (!vfxPrefab)
        {
            Debug.LogError("[TouchVFX] Please assign a VisualEffect prefab.");
            enabled = false;
            return;
        }

        // Instantiate once (world-space)
        _vfx = Instantiate(vfxPrefab, null);
        _vfx.gameObject.SetActive(false);
        _vfx.Stop();
    }

    // -------- Trigger path --------
    void OnTriggerEnter(Collider other)
    {
        if (!useTrigger || !IsToucher(other)) return;
        _touchCount++;
        PlaceAtClosestPoint(other.transform.position);
        PlayVFX();
    }

    void OnTriggerStay(Collider other)
    {
        if (!useTrigger || !IsToucher(other)) return;
        PlaceAtClosestPoint(other.transform.position);
    }

    void OnTriggerExit(Collider other)
    {
        if (!useTrigger || !IsToucher(other)) return;
        _touchCount = Mathf.Max(0, _touchCount - 1);
        if (_touchCount == 0) StopVFX();
    }

    // -------- Collision path --------
    void OnCollisionEnter(Collision collision)
    {
        if (useTrigger || !IsToucher(collision.collider)) return;

        Vector3 p = GetContactPoint(collision, collision.collider.bounds.center);
        Vector3 n = GetContactNormal(collision, p);
        PlaceAtPointWithNormal(p, n);
        _touchCount++;
        PlayVFX();
    }

    void OnCollisionStay(Collision collision)
    {
        if (useTrigger || !IsToucher(collision.collider)) return;

        Vector3 p = GetContactPoint(collision, collision.collider.bounds.center);
        Vector3 n = GetContactNormal(collision, p);
        PlaceAtPointWithNormal(p, n);
    }

    void OnCollisionExit(Collision collision)
    {
        if (useTrigger || !IsToucher(collision.collider)) return;

        _touchCount = Mathf.Max(0, _touchCount - 1);
        if (_touchCount == 0) StopVFX();
    }

    // -------- Helpers --------
    bool IsToucher(Collider c) => c != null && c.CompareTag(touchTag);

    void PlayVFX()
    {
        // cancel any pending deactivate
        if (_deactivateCoroutine != null)
        {
            StopCoroutine(_deactivateCoroutine);
            _deactivateCoroutine = null;
        }

        if (!_vfx.gameObject.activeSelf)
            _vfx.gameObject.SetActive(true);

        _vfx.Play();
    }

    void StopVFX()
    {
        _vfx.Stop(); // stop emission but keep visible particles
        if (_deactivateCoroutine == null)
            _deactivateCoroutine = StartCoroutine(DeactivateAfterDelay(0.7f));
    }

    IEnumerator DeactivateAfterDelay(float delay)
    {
        // simple fallback delay instead of checking .alive
        yield return new WaitForSeconds(delay);
        _vfx.gameObject.SetActive(false);
        _deactivateCoroutine = null;
    }

    void PlaceAtClosestPoint(Vector3 fromWorldPos)
    {
        Vector3 cp = _surface.ClosestPoint(fromWorldPos);
        Vector3 approxNormal = (cp - fromWorldPos).sqrMagnitude > 1e-8f
            ? (cp - fromWorldPos).normalized
            : (cp - _surface.bounds.center).normalized;

        _vfx.transform.position = cp + approxNormal * surfaceOffset;
    }

    void PlaceAtPointWithNormal(Vector3 p, Vector3 n)
    {
        _vfx.transform.position = p + n.normalized * surfaceOffset;
    }

    Vector3 GetContactPoint(Collision col, Vector3 fallback)
    {
        int count = col.GetContacts(_contacts);
        return count > 0 ? _contacts[0].point : fallback;
    }

    Vector3 GetContactNormal(Collision col, Vector3 fallbackPoint)
    {
        int count = col.GetContacts(_contacts);
        if (count > 0) return _contacts[0].normal;
        return (fallbackPoint - _surface.bounds.center).normalized;
    }
}
