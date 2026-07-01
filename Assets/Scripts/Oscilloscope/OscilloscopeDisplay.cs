using UnityEngine;

public class OscilloscopeDisplay : MonoBehaviour
{
    [SerializeField] private OscilloscopeCore core;

    [Header("Рендереры графиков")]
    [SerializeField] private LineRenderer lineRendererCH1;
    [SerializeField] private LineRenderer lineRendererCH2;

    [Header("Рендереры Курсоров")]
    [SerializeField] private LineRenderer cursor1Line;
    [SerializeField] private LineRenderer cursor2Line;

    // Константы размеров нашего экрана (10 клеток в ширину, 8 в высоту)
    private const float HALF_WIDTH = 5f;
    private const float HALF_HEIGHT = 4f;

    public void Initialize()
    {
        SetupLine(lineRendererCH1, 0.05f);
        SetupLine(lineRendererCH2, 0.05f);

        // Курсоры делаем чуть тоньше, чтобы они не перекрывали график
        SetupLine(cursor1Line, 0.03f);
        SetupLine(cursor2Line, 0.03f);

        if (core != null)
            core.OnGraphReady += DrawGraph;
    }

    private void SetupLine(LineRenderer lr, float width)
    {
        if (lr == null) return;
        lr.useWorldSpace = false;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.positionCount = 0; // Изначально прячем
    }

    // ИЗМЕНЕНО: Принимаем новые параметры курсоров
    private void DrawGraph(Vector3[] pointsCH1, Vector3[] pointsCH2, CursorMode cMode, float c1Pos, float c2Pos)
    {
        // 1. Отрисовка графиков
        if (pointsCH1 != null && pointsCH1.Length > 0 && lineRendererCH1 != null)
        {
            lineRendererCH1.positionCount = pointsCH1.Length;
            lineRendererCH1.SetPositions(pointsCH1);
        }

        if (pointsCH2 != null && pointsCH2.Length > 0 && lineRendererCH2 != null)
        {
            lineRendererCH2.positionCount = pointsCH2.Length;
            lineRendererCH2.SetPositions(pointsCH2);
        }

        // 2. Отрисовка Курсоров
        DrawCursors(cMode, c1Pos, c2Pos);
    }

    private void DrawCursors(CursorMode mode, float c1Pos, float c2Pos)
    {
        if (cursor1Line == null || cursor2Line == null) return;

        if (mode == CursorMode.Off)
        {
            // Прячем курсоры
            cursor1Line.positionCount = 0;
            cursor2Line.positionCount = 0;
            return;
        }

        cursor1Line.positionCount = 2;
        cursor2Line.positionCount = 2;

        if (mode == CursorMode.TimeAxis)
        {
            // Режим Времени: Вертикальные линии (X фиксирован, Y от низа до верха)
            // Ограничиваем X краями экрана (-5 .. +5)
            float x1 = Mathf.Clamp(c1Pos, -HALF_WIDTH, HALF_WIDTH);
            float x2 = Mathf.Clamp(c2Pos, -HALF_WIDTH, HALF_WIDTH);

            cursor1Line.SetPosition(0, new Vector3(x1, -HALF_HEIGHT, 0));
            cursor1Line.SetPosition(1, new Vector3(x1, HALF_HEIGHT, 0));

            cursor2Line.SetPosition(0, new Vector3(x2, -HALF_HEIGHT, 0));
            cursor2Line.SetPosition(1, new Vector3(x2, HALF_HEIGHT, 0));
        }
        else if (mode == CursorMode.VoltageAxis)
        {
            // Режим Напряжения: Горизонтальные линии (Y фиксирован, X от лева до права)
            // Ограничиваем Y краями экрана (-4 .. +4)
            float y1 = Mathf.Clamp(c1Pos, -HALF_HEIGHT, HALF_HEIGHT);
            float y2 = Mathf.Clamp(c2Pos, -HALF_HEIGHT, HALF_HEIGHT);

            cursor1Line.SetPosition(0, new Vector3(-HALF_WIDTH, y1, 0));
            cursor1Line.SetPosition(1, new Vector3(HALF_WIDTH, y1, 0));

            cursor2Line.SetPosition(0, new Vector3(-HALF_WIDTH, y2, 0));
            cursor2Line.SetPosition(1, new Vector3(HALF_WIDTH, y2, 0));
        }
    }

    void OnDestroy()
    {
        if (core != null) core.OnGraphReady -= DrawGraph;
    }
}