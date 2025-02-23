using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    public Music music;
    public MainMenu mainMenu;

    public LevelManager levelManager;
    public ScoreManager scoreManager;

    protected int totalLineCleard = 0;

    //field for stop or over mode
    public bool IsStop { get; set; } = false;
    public bool IsPlay { get; set; } = false;

    public RectInt Bounds {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    protected virtual void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();
        music = FindObjectOfType<Music>();

        for (int i = 0; i < tetrominoes.Length; i++) {
            tetrominoes[i].Initialize();
        }
    }

    protected virtual void Start()
    {
        SpawnPiece();
    }

    public virtual void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver(null);
        }

    }

    public virtual void GameOver(GameObject overPanel)
    {
        tilemap.ClearAllTiles();
        music?.playGameOverMusic();
        // mainMenu.NewGame();
        if (overPanel != null)
        {
            IsStop = true;
            Time.timeScale = Time.timeScale > 0 ? 0f : 1f;
            overPanel.SetActive(true);
        }
        else
        {

        }
        
        // Do anything else you want on game over here..
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;

    }

    public int ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int lineClear = 0;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row))
            {
                LineClear(row);
                music?.playScorePointMusic();
                lineClear++;
            }
            else
            {
                row++;
            }
        }

        return lineClear;
    }

    //public void Score(int lineClear)
    //{
    //    if (lineClear == 0) return;

    //    switch (lineClear)
    //    {
    //        case 1:
    //            ScoreManager.score += 40 * (LevelManager.level);
    //            break;
    //        case 2:
    //            ScoreManager.score += 100 * (LevelManager.level);
    //            break;
    //        case 3:
    //            ScoreManager.score += 300 * (LevelManager.level);
    //            break;
    //        case 4:
    //            ScoreManager.score += 1200 * (LevelManager.level);
    //            break;
    //    }

    //    totalLineCleard += lineClear;

    //    Debug.Log(totalLineCleard);
    //    Debug.Log(totalLineCleard > LevelManager.level * 10);

    //    if (LevelManager.level != 15 && totalLineCleard > LevelManager.level * 10)
    //    {
    //        Debug.Log("Level up");
    //        LevelManager.level++;
    //    }
    //}

    public bool IsLineFull(int row)
    {
    RectInt bounds = Bounds;

    for (int col = bounds.xMin; col < bounds.xMax; col++)
    {
        Vector3Int position = new Vector3Int(col, row, 0);

        // The line is not full if a tile is missing
        if (!tilemap.HasTile(position)) {
            return false;
        }
    }

    return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    public void TryLevelUp(int newLineClear)
    {
        totalLineCleard += newLineClear;
        levelManager.TryLevelUp(totalLineCleard);
    }
}
