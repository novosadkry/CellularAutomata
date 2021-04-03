using System;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    public event Action OnGameUpdate;
    public event Action OnInputUpdate;
    
    [field: Header("Dimensions")]
    [field: SerializeField] public Vector2Int Size { get; set; }
    [field: SerializeField] public Vector2Int Scale { get; set; }
    
    public uint[,] Cells { get; set; }

    [Header("Settings")] 
    public int editRadius;
    
    [Header("Timings")]
    public ClockScript inputClock;
    public ClockScript gameClock;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;

        inputClock.OnTick += OnInputTick;
        gameClock.OnTick += OnGameTick;
        
        Initialize();
    }

    private void Update()
    {
        editRadius += Mathf.RoundToInt(Input.mouseScrollDelta.y);
        
        if (Input.GetKeyDown(KeyCode.Space))
            gameClock.paused = !gameClock.paused;
    }

    private void OnInputTick()
    {
        if (Input.GetMouseButton(0))
        {
            var cell = GetCellFromMousePos();

            Fill(cell, editRadius);
            OnInputUpdate?.Invoke();
        }

        if (Input.GetMouseButton(1))
        {
            var cell = GetCellFromMousePos();
            
            Fill(cell, editRadius, 0);
            OnInputUpdate?.Invoke();
        }
    }
    
    private void OnGameTick()
    {
        OnGameUpdate?.Invoke();
    }

    private void OnValidate()
    {
        Initialize();
        OnInputUpdate?.Invoke();
    }

    private void Initialize()
    {
        Cells = new uint[Size.x, Size.y];
    }

    private void Fill(Func<int, int, bool> predicate, uint value = 1)
    {
        for (var x = 0; x < Cells.GetLength(0); x++)
        {
            for (var y = 0; y < Cells.GetLength(1); y++)
            {
                if (predicate(x, y))
                    Cells[x, y] = value;
            }
        }
    }

    private void Fill(Vector2Int pos, int radius, uint value = 1)
    {
        for (var x = pos.x - radius; x <= pos.x + radius; x++)
        {
            for (var y = pos.y - radius; y <= pos.y + radius; y++)
            {
                if (x < 0 || x >= Size.x) continue;
                if (y < 0 || y >= Size.y) continue;
                
                Cells[x, y] = value;
            }
        }
    }

    private Vector2Int GetCellFromMousePos()
    {
        float xRatio = (float) _cam.pixelWidth / Scale.x;
        float yRatio = (float) _cam.pixelHeight / Scale.y;
        
        var cellScale = GetCellScale();
        
        int x = Mathf.FloorToInt(Input.mousePosition.x / (cellScale.x * xRatio));
        int y = Mathf.FloorToInt(Input.mousePosition.y / (cellScale.y * yRatio));

        return new Vector2Int(x, y);
    }

    public Vector2 GetCellScale()
    {
        float x = (float) Scale.x / Size.x;
        float y = (float) Scale.y / Size.y;

        return new Vector2(x, y);
    }
}