using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class RotationInteractable : MonoBehaviour
{
    private Transform _referenceParent;
    private Vector3 _initialLocalUp;
    private Vector3 _initialLocalForward;

    private float _limitMin;
    private float _limitMax;
    private float _amplitude;

    private void Awake()
    {
        HingeJoint hinge = GetComponent<HingeJoint>();

        _limitMin = hinge.limits.min;
        _limitMax = hinge.limits.max;
        _amplitude = _limitMax - _limitMin;

        if (_amplitude == 0) _amplitude = 1f; // Защита от деления на ноль

        // ЗАПОМИНАЕМ КОРПУС ОСЦИЛЛОГРАФА (Родителя)
        // Чтобы всегда сверять угол относительно него, даже если XR вырвет ручку
        _referenceParent = transform.parent;

        if (_referenceParent != null)
        {
            // Запоминаем, куда смотрели оси ручки относительно корпуса в самом начале
            _initialLocalUp = _referenceParent.InverseTransformDirection(transform.up);
            _initialLocalForward = _referenceParent.InverseTransformDirection(transform.forward);
        }
        else
        {
            _initialLocalUp = transform.up;
            _initialLocalForward = transform.forward;
        }
    }

    public float CalculateRotationProgress()
    {
        Vector3 refUp;
        Vector3 refForward;

        if (_referenceParent != null)
        {
            // Восстанавливаем стартовые оси в мировых координатах (на случай, если осциллограф двигали)
            refUp = _referenceParent.TransformDirection(_initialLocalUp);
            refForward = _referenceParent.TransformDirection(_initialLocalForward);
        }
        else
        {
            refUp = _initialLocalUp;
            refForward = _initialLocalForward;
        }

        // Берем текущее направление ручки (мировое)
        Vector3 currentUp = transform.up;

        // ИДЕАЛЬНЫЙ УГОЛ: Вычисляем угол между изначальным и текущим положением вокруг оси Z (Forward)
        float currentAngle = Vector3.SignedAngle(refUp, currentUp, refForward);

        // Считаем прогресс по лимитам
        float angleProgressDegrees = currentAngle - _limitMin;
        float progress = angleProgressDegrees / _amplitude;

        // Если при вращении ВПРАВО график идет ВЛЕВО, раскомментируй строку ниже:
        // progress = 1f - progress;

        return Mathf.Clamp01(progress);
    }
}