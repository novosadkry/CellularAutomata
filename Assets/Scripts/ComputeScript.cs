using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ComputeScript : MonoBehaviour
{
    [Header("Properties")]
    public GridScript grid;
    
    [Header("Shader Settings")]
    public ComputeShader computeShader;
    public Vector2Int textureScale;
    
    private RenderTexture _renderTexture;
    private ComputeBuffer _cellsIn;
    private ComputeBuffer _cellsOut;

    private void Start()
    {
        if (_renderTexture == null)
            CreateTexture();

        grid.OnGameUpdate += OnGameUpdate;
        grid.OnInputUpdate += OnInputUpdate;
    }

    private void CreateTexture()
    {
        _renderTexture = new RenderTexture(textureScale.x, textureScale.y, 0);
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();
    }

    private void UpdateBuffer(ref ComputeBuffer buffer, Array data)
    {
        buffer?.Release();
        buffer = new ComputeBuffer(grid.Cells.Length, 4);
        buffer.SetData(data);
    }

    private void OnGameUpdate()
    {
        UpdateBuffer(ref _cellsIn, grid.Cells);
        UpdateBuffer(ref _cellsOut, grid.Cells);
        
        computeShader.SetTexture(0, "Result", _renderTexture);
        computeShader.SetBuffer(0, "CellsIn", _cellsIn);
        computeShader.SetBuffer(0, "CellsOut", _cellsOut);
        computeShader.SetFloats("cell_scale", (float) textureScale.x / grid.Size.x, (float) textureScale.y / grid.Size.y);
        computeShader.SetInts("grid_size", grid.Size.x, grid.Size.y);
        computeShader.SetBool("compute", true);
        
        computeShader.Dispatch(0, _renderTexture.width / 8, _renderTexture.height / 8, 1);
        
        _cellsOut.GetData(grid.Cells);
    }
    
    private void OnInputUpdate()
    {
        UpdateBuffer(ref _cellsIn, grid.Cells);
        UpdateBuffer(ref _cellsOut, grid.Cells);
        
        computeShader.SetTexture(0, "Result", _renderTexture);
        computeShader.SetBuffer(0, "CellsIn", _cellsIn);
        computeShader.SetBuffer(0, "CellsOut", _cellsOut);
        computeShader.SetFloats("cell_scale", (float) textureScale.x / grid.Size.x, (float) textureScale.y / grid.Size.y);
        computeShader.SetInts("grid_size", grid.Size.x, grid.Size.y);
        computeShader.SetBool("compute", false);
        
        computeShader.Dispatch(0, _renderTexture.width / 8, _renderTexture.height / 8, 1);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_renderTexture, dest);
    }

    private void OnDestroy()
    {
        _cellsIn.Release();
        _cellsOut.Release();
    }
}
