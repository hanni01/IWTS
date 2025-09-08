using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(ReflectionProbe))]
public class RuntimeReflectionProbeBake : MonoBehaviour
{
    private ReflectionProbe _probe;

    void Awake()
    {
        _probe = GetComponent<ReflectionProbe>();

        // Baked ���� ����
        _probe.mode = ReflectionProbeMode.Baked;

        // ��Ÿ�� Bake
        _probe.RenderProbe();
    }
}
