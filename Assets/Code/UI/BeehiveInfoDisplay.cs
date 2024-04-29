using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class BeehiveInfoDisplay : MonoBehaviour
{
    const int NUMBER_OF_PANELs = 3;
    public static event Action<int> PanelNumberChanged;
    int _currentPanel;
    [SerializeField] Vector3 _offset;
    [SerializeField] TMP_Text _textField;
    Transform _mainCameraTransform;
    // Start is called before the first frame update

    void Start()
    {
        _mainCameraTransform = Camera.main.transform;
    }

    void Update()
    {
        transform.LookAt(_mainCameraTransform.position);
    }

    public void UpdateText(string newText)
    {
        _textField.text = newText;
    }

    public void UpdatePosition(float3 newPosition)
    {
        transform.position = (Vector3)newPosition + _offset;
    }

    public void OnDisplayPanelChangeLeft()
    {
        _currentPanel = (_currentPanel - 1 + NUMBER_OF_PANELs) % NUMBER_OF_PANELs;
        PanelNumberChanged?.Invoke(_currentPanel);
    }

    public void OnDisplayPanelChangeRight()
    {
        _currentPanel = (_currentPanel + 1) % NUMBER_OF_PANELs;
        PanelNumberChanged?.Invoke(_currentPanel);
    }

    void OnEnable()
    {
        ReportingSystem.PanelPositionUpdated += UpdatePosition;
        ReportingSystem.PanelTextUpdated += UpdateText;
    }

    void OnDisable()
    {
        ReportingSystem.PanelPositionUpdated -= UpdatePosition;
        ReportingSystem.PanelTextUpdated -= UpdateText;
    }
}
