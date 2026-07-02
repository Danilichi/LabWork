using System;
using UnityEngine;

[Serializable]
public class OscilloscopeChannel
{
    [Header("Железо")]
    [SerializeField] private DummySignalGenerator source;
    [SerializeField] private VRKnob voltsDivKnob;
    [SerializeField] private VRKnob yPosKnob;

    [Header("Настройки")]
    public CouplingMode coupling = CouplingMode.DC;

    public bool HasSource => source != null;

    // Эти свойства читаются ОДИН РАЗ за кадр Ядром
    public float VoltsPerDiv => (voltsDivKnob != null && voltsDivKnob.Value != 0) ? voltsDivKnob.Value : 1f;
    public float YOffset => yPosKnob != null ? yPosKnob.Value : 0f;

    public float GetRawVoltage(float time)
    {
        if (!HasSource || coupling == CouplingMode.GND) return 0f;
        return source.GetVoltage(time);
    }

    // ИСПРАВЛЕНИЕ: Передали vDiv и yOffset сюда, чтобы не вызывать крутилки 2000 раз
    public float GetScreenYPosition(float rawVoltage, float frameAverageVoltage, float vDiv, float yOffset)
    {
        float finalVolts = rawVoltage;

        if (coupling == CouplingMode.AC)
        {
            finalVolts -= frameAverageVoltage;
        }

        return (finalVolts / vDiv) + yOffset;
    }
}