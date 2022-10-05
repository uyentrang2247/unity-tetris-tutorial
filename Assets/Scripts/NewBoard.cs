using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewBoard : Board
{
    public List<Piece> nextPieces { get; private set; }
    public Piece savedPiece { get; private set; }

    public Vector3Int previewPosition = new Vector3Int(-1, 12, 0);
    public Vector3Int holdPosition = new Vector3Int(-1, 16, 0);

    private int previewAmount = 3;
    private int previewMargin = 4;

    private bool Swaped { get; set; } = false;

    public GameObject pausePanel;
    public GameObject gameOverPanel;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log(activePiece);
        nextPieces = new List<Piece>();

        savedPiece = gameObject.AddComponent<Piece>();
        savedPiece.enabled = false;

        GameObject nextBox = GameObject.FindGameObjectWithTag("Next Box");
        // align position for item
        Vector3 nextBoxPiecePosition = nextBox.transform.position + new Vector3(-1, 0, 0);
        if (nextBox != null) previewPosition = Vector3Int.FloorToInt(nextBoxPiecePosition);

        GameObject holdBox = GameObject.FindGameObjectWithTag("Hold Box");
        // align position for item
        Vector3 holdBoxPiecePosition = holdBox.transform.position + new Vector3(-1, -1, 0);
        if (holdBox != null) holdPosition = Vector3Int.FloorToInt(holdBoxPiecePosition);

        Debug.Log($"previewPosition {previewPosition}");
        Debug.Log($"holdPosition {holdPosition}");
    }

    protected override void Start()
    {
        for (int i = 0; i < previewAmount; i++)
        {
            SetNextPiece();
        }
        base.Start();
    }

    private void SetNextPiece()
    {
        Piece nextPiece = gameObject.AddComponent<Piece>();
        nextPiece.enabled = false;

        // Pick a random tetromino to use
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        // Initialize the next piece with the random data
        // Draw it at the "preview" position on the board
        nextPiece.Initialize(this, previewPosition, data);
        Set(nextPiece);
        nextPieces.Add(nextPiece);
        AlignNextPiece();
    }

    private void AlignNextPiece()
    {
        // Clear existing preview piece
        for(int i = 0; i < nextPieces.Count; i++)
        {
            Clear(nextPieces[i]);
        }

        // Draw new preview piece
        for (int i = 0; i < nextPieces.Count; i++)
        {
            Piece piece = nextPieces[i];

            Vector3Int alignPosition = previewPosition + new Vector3Int(0, (1 - i) * previewMargin - 1, 0);
            piece.ChangePosition(alignPosition);

            Set(piece);
        }
    }

    public override void SpawnPiece()
    {
        // Clear the next piece from the next box to make it active piece
        Piece nextPiece = nextPieces[0];
        TetrominoData nextPieceData = nextPiece.data;
        Debug.Log("Clear " + nextPiece.cells);
        Clear(nextPiece);
        Destroy(nextPiece);
        nextPieces.Remove(nextPiece);


        // Initialize the active piece with the next piece data
        if(nextPiece.data.tetromino.Equals(Tetromino.I))
            activePiece.Initialize(this, spawnPosition + Vector3Int.down, nextPieceData);
        else
            activePiece.Initialize(this, spawnPosition, nextPieceData);

        // Only spawn the piece if valid position otherwise game over
        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver(gameOverPanel);
        }

        // Set the next random piece
        SetNextPiece();

        // Reset swap flag when spawn new piece
        Swaped = false;
    }

    public void SwapPiece()
    {
        //Return when swaped once or swap with the same tetromino
        if (Swaped || activePiece.data.tetromino.Equals(savedPiece.data.tetromino)) return;

        // Temporarily store the current saved data so we can swap
        TetrominoData savedData = activePiece.data;

        // Clear the active piece before swaping
        Clear(activePiece);

        // Clear the existing saved piece from the board then swap with the active piece
        if (savedPiece.cells != null)
        {
            Clear(savedPiece);
            activePiece.Initialize(this, spawnPosition, savedPiece.data);
        }
        // If not swaped yet then spawn next
        else
        {
            SpawnPiece();
        }

        // Create saved piece in hold box
        savedPiece.Initialize(this, holdPosition, savedData);
        Set(savedPiece);

        Swaped = true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Toggle pause");
            IsStop = !IsStop;
            Time.timeScale = 1;
            pausePanel.SetActive(!pausePanel.activeSelf);
        }

        if (IsStop)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                IsStop = false;
                Time.timeScale = 1;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }else{
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                SwapPiece();
            }
        }

    }
}
