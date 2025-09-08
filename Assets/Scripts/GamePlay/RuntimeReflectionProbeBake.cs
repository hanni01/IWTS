using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(ReflectionProbe))]
public class RuntimeReflectionProbeBake : MonoBehaviour
{
    private ReflectionProbe _probe;

    void Awake()
    {
        _probe = GetComponent<ReflectionProbe>();

        // Baked 모드로 설정
        _probe.mode = ReflectionProbeMode.Baked;

        // 런타임 Bake
        _probe.RenderProbe();
    }
}
