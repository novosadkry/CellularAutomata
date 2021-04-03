using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ComputeScript : MonoBehaviour
{
    public GridScript grid;
    
    [Header("Properties")]
    public float zoom = 1;
    public Vector2 offset;

    [Header("Shader Settings")]
    public ComputeShader computeShader;
    public Vector2Int textureScale;
    
    private RenderTexture _renderTexture;
    private ComputeBuffer _cellsIn;
    private ComputeBuffer _cellsOut;

    private void Awake()
    {
        grid.Scale = textureScale;
    }

    private void Start()
    {
        UpdateTexture();
        
        UpdateBuffer(ref _cellsIn, grid.Cells);
        UpdateBuffer(ref _cellsOut, grid.Cells);

        grid.OnGameUpdate += OnGameUpdate;
        grid.OnInputUpdate += OnInputUpdate;
        
        OnInputUpdate();
    }

    private void OnValidate()
    {
        grid.editZoom = zoom;
        grid.editOffset = offset;
    }

    private void UpdateTexture()
    {
        if (_renderTexture != null)
            _renderTexture.Release();
        
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
        
        var cellScale = grid.GetCellScale();
        
        computeShader.SetTexture(0, "Result", _renderTexture);
        computeShader.SetBuffer(0, "CellsIn", _cellsIn);
        computeShader.SetBuffer(0, "CellsOut", _cellsOut);
        computeShader.SetFloats("cell_scale", cellScale.x, cellScale.y);
        computeShader.SetInts("grid_size", grid.Size.x, grid.Size.y);
        computeShader.SetBool("compute", true);
        
        computeShader.Dispatch(0, _renderTexture.width / 8, _renderTexture.height / 8, 1);
        
        _cellsOut.GetData(grid.Cells);
    }
    
    private void OnInputUpdate()
    {
        UpdateBuffer(ref _cellsIn, grid.Cells);
        UpdateBuffer(ref _cellsOut, grid.Cells);

        var cellScale = grid.GetCellScale();
        
        computeShader.SetTexture(0, "Result", _renderTexture);
        computeShader.SetBuffer(0, "CellsIn", _cellsIn);
        computeShader.SetBuffer(0, "CellsOut", _cellsOut);
        computeShader.SetFloats("cell_scale", cellScale.x, cellScale.y);
        computeShader.SetInts("grid_size", grid.Size.x, grid.Size.y);
        computeShader.SetBool("compute", false);
        
        computeShader.Dispatch(0, _renderTexture.width / 8, _renderTexture.height / 8, 1);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_renderTexture, dest, new Vector2(zoom, zoom), offset);
    }

    private void OnDestroy()
    {
        _cellsIn.Release();
        _cellsOut.Release();
    }
}
