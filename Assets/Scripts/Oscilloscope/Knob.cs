using UnityEngine;

public class VRKnob : MonoBehaviour
{
    [SerializeField] private float _minValue = 1f;
    [SerializeField] private float _maxValue = 5f;

    [SerializeField] private float _fallbackValue;

    [SerializeField] private RotationInteractable _associatedRotationIteractable;

    public float Value => 
        _associatedRotationIteractable == null ? 
        _fallbackValue : 
        _minValue + ((_maxValue - _minValue) * _associatedRotationIteractable.CalculateRotationProgress());
}