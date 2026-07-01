// --- НОВЫЙ КЛАСС КАНАЛА ---
using System;
using UnityEngine;

[Serializable] // Обязательно, чтобы класс красиво отображался в инспекторе Unity
public class OscilloscopeChannel
{
    [Header("Железо")]
    [SerializeField] private DummySignalGenerator source;
    [SerializeField] private VRKnob voltsDivKnob;
    [SerializeField] private VRKnob yPosKnob;

    [Header("Настройки")]
    public CouplingMode coupling = CouplingMode.DC;

    // Валидация: есть ли источник
    public bool HasSource => source != null;

    // Безопасное чтение крутилок (если забыли привязать в инспекторе — не будет ошибок)
    public float VoltsPerDiv => (voltsDivKnob != null && voltsDivKnob.Value != 0) ? voltsDivKnob.Value : 1f;
    public float YOffset => yPosKnob != null ? yPosKnob.Value : 0f;

    // Получение сырого напряжения с учетом режима GND
    public float GetRawVoltage(float time)
    {
        if (!HasSource || coupling == CouplingMode.GND) return 0f;
        return source.GetVoltage(time);
    }

    // Финальная математика для пикселя на экране (с учетом AC-смещения, масштаба и Y-сдвига)
    public float GetScreenYPosition(float rawVoltage, float frameAverageVoltage)
    {
        float finalVolts = rawVoltage;

        // Удаляем постоянный ток, если включен режим AC
        if (coupling == CouplingMode.AC)
        {
            finalVolts -= frameAverageVoltage;
        }

        // Применяем масштаб (Делим на цену деления) и сдвигаем ручкой Y Position
        return (finalVolts / VoltsPerDiv) + YOffset;
    }
}