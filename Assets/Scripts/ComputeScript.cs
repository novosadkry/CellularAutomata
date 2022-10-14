using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ComputeScript : MonoBehaviour
{
    public GridScript grid;

    [Header("Properties")] 
    public float zoom = 1;
    public float zoomSpeed = 2;
    public float freeLook = 0.1f;
    public Vector2 offset;

    [Header("Shader Settings")] 
    public ComputeShader computeShader;
    public Vector2Int textureScale;

    private Vector3 _mousePos;
    
    private RenderTexture _renderTexture;
    private ComputeBuffer _cellsIn;
    private ComputeBuffer _cellsOut;
    private ComputeBuffer _cellsSwap;

    private void Awake()
    {
        grid.Scale = textureScale;
    }

    private void Start()
    {
        UpdateTexture();

        _cellsIn   = new ComputeBuffer(grid.Size.x * grid.Size.y, 4);
        _cellsOut  = new ComputeBuffer(grid.Size.x * grid.Size.y, 4);
        _cellsSwap = new ComputeBuffer(grid.Size.x * grid.Size.y, 4);

        var cellScale = grid.GetCellScale();

        computeShader.SetTexture(0, "Result", _renderTexture);
        computeShader.SetBuffer(0, "CellsIn", _cellsIn);
        computeShader.SetBuffer(0, "CellsOut", _cellsOut);
        computeShader.SetBuffer(0, "CellsSwap", _cellsSwap);

        computeShader.SetTexture(1, "Result", _renderTexture);
        computeShader.SetBuffer(1, "CellsIn", _cellsIn);
        computeShader.SetBuffer(1, "CellsOut", _cellsOut);
        computeShader.SetBuffer(1, "CellsSwap", _cellsSwap);

        computeShader.SetFloats("cell_scale", cellScale.x, cellScale.y);
        computeShader.SetInts("grid_size", grid.Size.x, grid.Size.y);

        grid.OnGameUpdate += OnGameUpdate;
        grid.OnInputUpdate += OnInputUpdate;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        Vector2 mouseDelta = Input.mousePosition - _mousePos;
        mouseDelta *= Time.deltaTime * freeLook * zoom;
        
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.mouseScrollDelta.y == 0)
                return;
            
            zoom -= Input.mouseScrollDelta.y * Time.deltaTime * zoomSpeed;
            zoom = Mathf.Clamp01(zoom);
        }

        if (Input.GetMouseButton(2))
        {
            offset -= mouseDelta;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }

        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        var maxOffset = 1 - zoom;
        offset.x = Mathf.Clamp(offset.x, 0, maxOffset);
        offset.y = Mathf.Clamp(offset.y, 0, maxOffset);

        UpdateGridEditValues();
        _mousePos = Input.mousePosition;
    }

    private void OnValidate()
    {
        UpdateGridEditValues();
    }

    private void UpdateGridEditValues()
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

    private void OnGameUpdate()
    {
        computeShader.Dispatch(0, _renderTexture.width / 16, _renderTexture.height / 16, 1);

        (_cellsIn, _cellsOut, _cellsSwap) = (_cellsOut, _cellsSwap, _cellsIn);
        computeShader.SetBuffer(0, "CellsIn", _cellsIn);
        computeShader.SetBuffer(0, "CellsOut", _cellsOut);
        computeShader.SetBuffer(0, "CellsSwap", _cellsSwap);
    }

    private void OnInputUpdate(Vector2Int cell, bool fill)
    {
        computeShader.SetBool("fill", fill);
        computeShader.SetInts("fill_pos", cell.x, cell.y);
        computeShader.SetFloat("fill_radius", grid.editRadius);

        computeShader.Dispatch(1, _renderTexture.width / 16, _renderTexture.height / 16, 1);

        (_cellsIn, _cellsOut, _cellsSwap) = (_cellsOut, _cellsSwap, _cellsIn);
        computeShader.SetBuffer(1, "CellsIn", _cellsIn);
        computeShader.SetBuffer(1, "CellsOut", _cellsOut);
        computeShader.SetBuffer(1, "CellsSwap", _cellsSwap);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_renderTexture, dest, new Vector2(zoom, zoom), offset);
    }

    private void OnDestroy()
    {
        _cellsIn.Release();
        _cellsOut.Release();
        _cellsSwap.Release();
    }
}
