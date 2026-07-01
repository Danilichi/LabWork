using System;
using UnityEngine;

public enum DisplayMode { YT, XY }
public enum CouplingMode { DC, AC, GND }
public enum TriggerEdge { Rising, Falling }
public enum CursorMode { Off, TimeAxis, VoltageAxis }

public class OscilloscopeCore : MonoBehaviour
{
    private float lastSweepTime = 0f;

    [Header("Каналы")]
    [SerializeField] private OscilloscopeChannel ch1 = new OscilloscopeChannel();
    [SerializeField] private OscilloscopeChannel ch2 = new OscilloscopeChannel();

    [Header("Горизонтальная развертка")]
    [SerializeField] private VRKnob timeDivKnob;
    [SerializeField] private VRKnob xPosKnob;

    [Header("Блок Триггера (Синхронизация)")]
    [SerializeField] private VRKnob triggerLevelKnob; // Ручка уровня синхронизации (в Вольтах!)
    [SerializeField] public TriggerEdge triggerEdge = TriggerEdge.Rising; // Кнопка переключения фронта
    // Временно жестко привяжем триггер к Каналу 1 (обычно есть переключатель Source)

    [Header("Режим дисплея")]
    [SerializeField] private DisplayMode currentMode = DisplayMode.YT;

    [Header("Курсоры")]
    public CursorMode cursorMode = CursorMode.Off;
    [SerializeField] private VRKnob cursor1Knob; // Ползунок 1 (от -5 до +5)
    [SerializeField] private VRKnob cursor2Knob; // Ползунок 2

    [Header("Настройки экрана")]
    [SerializeField] private int resolution = 512;
    [SerializeField] private float screenGridWidth = 10f;

    public event Action<Vector3[], Vector3[], CursorMode, float, float> OnGraphReady;

    private Vector3[] bufferCH1;
    private Vector3[] bufferCH2;
    private float[] rawBufferCH1;
    private float[] rawBufferCH2;

    public void Initialize()
    {
        bufferCH1 = new Vector3[resolution];
        bufferCH2 = new Vector3[resolution];
        rawBufferCH1 = new float[resolution];
        rawBufferCH2 = new float[resolution];
    }

    void Update()
    {
        if (!ch1.HasSource) return;

        if (currentMode == DisplayMode.YT)
            GenerateYTBuffer();
        else if (currentMode == DisplayMode.XY)
            GenerateXYBuffer();

        OnGraphReady?.Invoke(bufferCH1, bufferCH2, cursorMode,
            cursor1Knob != null ? cursor1Knob.Value : 0f,
            cursor2Knob != null ? cursor2Knob.Value : 0f);
    }

    private void GenerateYTBuffer()
    {
        float tDiv = timeDivKnob != null ? timeDivKnob.Value : 1f;
        float xTimeOffset = xPosKnob != null ? xPosKnob.Value * tDiv : 0f;
        float trigLevel = triggerLevelKnob != null ? triggerLevelKnob.Value : 0f;

        float totalTimeOnScreen = screenGridWidth * tDiv;
        float timeStep = totalTimeOnScreen / (resolution - 1);
        float startX = -screenGridWidth / 2f;
        float xStep = screenGridWidth / (resolution - 1);

        float startTime = lastSweepTime;
        float searchStep = totalTimeOnScreen / 200f;
        bool triggerFound = false;

        // Поиск триггера (смотрим в реальное время)
        float currentRealTime = Time.time - xTimeOffset;

        for (int i = 0; i < 300; i++)
        {
            float t1 = currentRealTime - (i * searchStep);
            float t2 = currentRealTime - ((i + 1) * searchStep);

            float v1 = ch1.GetRawVoltage(t1);
            float v2 = ch1.GetRawVoltage(t2);

            if (triggerEdge == TriggerEdge.Rising)
            {
                if (v2 < trigLevel && v1 >= trigLevel)
                {
                    float fraction = (trigLevel - v2) / (v1 - v2);
                    startTime = t2 + (t1 - t2) * fraction;
                    triggerFound = true;
                    break;
                }
            }
            else
            {
                if (v2 > trigLevel && v1 <= trigLevel)
                {
                    float fraction = (trigLevel - v2) / (v1 - v2);
                    startTime = t2 + (t1 - t2) * fraction;
                    triggerFound = true;
                    break;
                }
            }
        }

        // ИСПРАВЛЕНИЕ YT: Если триггер не найден (или выключен), 
        // имитируем реальный аналоговый "свободный бег" луча (Free Run).
        if (!triggerFound)
        {
            // Если предыдущий луч дорисовал экран, стартуем новый
            if (Time.time >= lastSweepTime + totalTimeOnScreen)
            {
                lastSweepTime = Time.time;
            }
            startTime = lastSweepTime - xTimeOffset;
        }
        else
        {
            lastSweepTime = startTime; // Синхронизируем внутренний таймер с триггером
        }

        float sumCH1 = 0f, sumCH2 = 0f;
        for (int i = 0; i < resolution; i++)
        {
            float t = startTime + (i * timeStep);

            rawBufferCH1[i] = ch1.GetRawVoltage(t);
            sumCH1 += rawBufferCH1[i];

            rawBufferCH2[i] = ch2.GetRawVoltage(t);
            sumCH2 += rawBufferCH2[i];
        }

        float avgCH1 = sumCH1 / resolution;
        float avgCH2 = sumCH2 / resolution;

        for (int i = 0; i < resolution; i++)
        {
            float xPos = startX + (i * xStep);
            bufferCH1[i] = new Vector3(xPos, ch1.GetScreenYPosition(rawBufferCH1[i], avgCH1), 0f);
            bufferCH2[i] = ch2.HasSource ? new Vector3(xPos, ch2.GetScreenYPosition(rawBufferCH2[i], avgCH2), 0f) : Vector3.zero;
        }
    }

    private void GenerateXYBuffer()
    {
        if (!ch2.HasSource) return;

        float tDiv = timeDivKnob != null ? timeDivKnob.Value : 1f;
        float totalTimeToDraw = screenGridWidth * tDiv;

        float timeStep = totalTimeToDraw / (resolution - 1);
        float startTime = Time.time;

        for (int i = 0; i < resolution; i++)
        {
            float t = startTime + (i * timeStep);

            float rawCH1 = ch1.GetRawVoltage(t);
            float rawCH2 = ch2.GetRawVoltage(t);

            float yPos = ch1.GetScreenYPosition(rawCH1, 0f);
            float xPos = ch2.GetScreenYPosition(rawCH2, 0f);

            bufferCH1[i] = new Vector3(xPos, yPos, 0f);
            bufferCH2[i] = Vector3.zero;
        }
    }
}