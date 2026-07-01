using UnityEngine;

public class VRKnob : MonoBehaviour
{
    [Tooltip("Текущее значение крутилки")]
    [SerializeField] private float _currentValue = 1f;

    public float Value => _currentValue;
}