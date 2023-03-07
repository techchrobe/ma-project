using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireframeCompute : MonoBehaviour
{
    [SerializeField] ComputeShader compute;
    [SerializeField] private int width = 1024;
    [SerializeField] private int height = 1024;
    private RenderTexture _rTexture;
    private int _kernelHandle = -1;
    private int _kernelHandleSetup = -1;
    private Vector2Int _groupSize;
    private Vector2Int _groupSizeSetup;

    private void Start() {
        _kernelHandleSetup = compute.FindKernel("CSMain");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if(_rTexture == null) {
            _rTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _rTexture.enableRandomWrite = true;
            _rTexture.Create();
            compute.SetTexture(0, "Result", _rTexture);
            compute.GetKernelThreadGroupSizes(_kernelHandleSetup, out var x, out var y, out _);
            _groupSizeSetup.x = Mathf.CeilToInt((float)width / x);
            _groupSizeSetup.y = Mathf.CeilToInt((float)height / y);
            compute.Dispatch(_kernelHandleSetup, _groupSizeSetup.x, _groupSizeSetup.y, 1);
            Graphics.Blit(_rTexture, destination);
        }
    }
}
