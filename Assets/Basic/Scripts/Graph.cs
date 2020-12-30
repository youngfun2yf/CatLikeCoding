using System;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public enum TransitionMode { Cycle, Random}
    
    [SerializeField] private Transform pointPrefab = default;

    [SerializeField] [Range(10, 200)] private int resolution = 10;

    [SerializeField] private FunctionLibrary.FunctionName function = default;

    [SerializeField] private TransitionMode transitionMode = TransitionMode.Cycle;

    [SerializeField] [Min(0f)] private float functionDuration = 1f, transitionDuration = 1f;

    private Transform[] _points;

    private float _duration;

    private bool _transitioning;

    private FunctionLibrary.FunctionName _transitionFunction;

    private void Awake()
    {
        var step = 2f / resolution;
        var scale = Vector3.one * step;
        _points = new Transform[resolution * resolution];
        for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
        {
            var point = Instantiate(pointPrefab, transform, false);
            point.localScale = scale;
            _points[i] = point;
        }
    }

    private void Update()
    {
        _duration += Time.deltaTime;
        if (_transitioning)
        {
            if (_duration >= transitionDuration)
            {
                _duration -= transitionDuration;
                _transitioning = false;
            }
        }
        else if (_duration >= functionDuration)
        {
            _duration -= functionDuration;
            _transitioning = true;
            _transitionFunction = function;
            PickNextFunction();
        }

        if (_transitioning)
        {
            UpdateFunctionTransition();
        }
        else
            UpdateFunction();
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ? 
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }
    
    private void UpdateFunctionTransition()
    {
        var from = FunctionLibrary.GetFunction(_transitionFunction);
        var to = FunctionLibrary.GetFunction(function);
        float progress = _duration / transitionDuration;
        float time = Time.time;
        float step = 2f / resolution;
        float v = .5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z++;
                v = (z + .5f) * step - 1f;
            }

            float u = (x + .5f) * step - 1f;
            var point = _points[i];
            _points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
        }
    }

    private void UpdateFunction()
    {
        var func = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        float step = 2f / resolution;
        float v = .5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z++;
                v = (z + .5f) * step - 1f;
            }

            float u = (x + .5f) * step - 1f;
            var point = _points[i];
            _points[i].localPosition = func.Invoke(u, v, time);
        }
    }
}