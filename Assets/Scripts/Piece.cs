using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay;
    public float moveDelay = 0.05f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;

    private bool locking;

    private int level;
    private bool possibleTspin;
    private bool wallKicked;

    public void Start()
    {
        
    }

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0;

        level = board.levelManager.Level;
        stepDelay = Mathf.Pow((0.8f - ((level - 1) * 0.007f)), (level - 1));

        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        locking = false;
        lockTime = 0f;

        if (cells == null) {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++) {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    public void ChangePosition(Vector3Int position)
    {
        this.position = position;   
    }

    private void Update()
    {
        stepDelay = Mathf.Pow((0.8f - ((level - 1) * 0.007f)), (level - 1));

        if (!board.IsStop && board.IsPlay)
        {
            board.Clear(this);

            // We use a timer to allow the player to make adjustments to the piece
            // before it locks in place
            lockTime += Time.deltaTime;

            // Handle rotation
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Rotate(1);
            }

            // Handle rotation
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Rotate(-1);
            }

            // Handle sonic drop
            if (Input.GetKeyDown(KeyCode.X))
            {
                SonicDrop();
            }

            // Handle hard drop
            if (Input.GetKeyDown(KeyCode.Space))
            {
                HardDrop();
            }

            // Allow the player to hold movement keys but only after a move delay
            // so it does not move too fast
            if (Time.time > moveTime)
            {
                HandleMoveInputs();
            }

            // Advance the piece to the next row every x seconds
            if (Time.time > stepTime)
            {
                Step();
            }

            board.Set(this);
        }
    }

    private void HandleMoveInputs()
    {
        // Soft drop movement
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (Move(Vector2Int.down)) {
                board.scoreManager.score += Data.BaseActionScore[ScoreAction.SoftDrop];
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
            }
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.LeftArrow)) {
            Move(Vector2Int.left);
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            Move(Vector2Int.right);
        }
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Step down to the next row
        if (Move(Vector2Int.down))
        {
            locking = false;
        } else
        {
            locking = true;
        }

        // Once the piece has been inactive for too long it becomes locked
        if (lockTime >= lockDelay) {
            Lock();
        }
    }

    private void SonicDrop()
    {
        while (Move(Vector2Int.down)) {
            board.scoreManager.score += Data.BaseActionScore[ScoreAction.HardDrop];
            continue;
        }
    }

    private void HardDrop()
    {
        SonicDrop();
        Lock();
    }

    private void Lock()
    {
        board.Set(this);

        possibleTspin = CheckPossibleTspin();

        int lineClear = board.ClearLines();

        if (lineClear > 0) { 
            board.TryLevelUp(lineClear);
        }

        Score(lineClear);

        board.SpawnPiece();
    }

    public void Score(int lineClear)
    {
        int score = 0;

        if (lineClear == 0) return;

        Debug.Log($"tetromino: {data.tetromino.Equals(Tetromino.T)} posible tspin: {possibleTspin} walkick: {wallKicked}");
        if (data.tetromino.Equals(Tetromino.T) && possibleTspin)
        {
            switch (lineClear)
            {
                case 0:
                    if(wallKicked) score = Data.BaseActionScore[ScoreAction.TSpinMiniNoLines] * level;
                    else score = Data.BaseActionScore[ScoreAction.TSpinNoLines] * level;
                    break;
                case 1:
                    if (wallKicked) score = Data.BaseActionScore[ScoreAction.TSpinMiniSingle] * level;
                    else score = Data.BaseActionScore[ScoreAction.TSpinSingle] * level;
                    break;
                case 2:
                    if (wallKicked) score = Data.BaseActionScore[ScoreAction.TspinMiniDouble] * level;
                    else score = Data.BaseActionScore[ScoreAction.TspinDouble] * level;
                    break;
                case 3:
                    score = Data.BaseActionScore[ScoreAction.TSpinTriple] * level;
                    break;
            }
        }
        else
        {
            switch (lineClear)
            {
                case 1:
                    score = Data.BaseActionScore[ScoreAction.Single] * level;
                    break;
                case 2:
                    score = Data.BaseActionScore[ScoreAction.Double] * level;
                    break;
                case 3:
                    score = Data.BaseActionScore[ScoreAction.Triple] * level;
                    break;
                case 4:
                    score = Data.BaseActionScore[ScoreAction.Tetris] * level;
                    break;
            }
        }

        board.scoreManager.score += score;
    }

    public bool CheckPossibleTspin()
    {
        if (!data.tetromino.Equals(Tetromino.T)) return false;
        Debug.Log($"down: {CheckMoveValid(Vector2Int.down)} up: {CheckMoveValid(Vector2Int.up)} left: {CheckMoveValid(Vector2Int.left)} right: {CheckMoveValid(Vector2Int.right)}");
        if (CheckMoveValid(Vector2Int.down) 
            || CheckMoveValid(Vector2Int.up) 
            || CheckMoveValid(Vector2Int.left) 
            || CheckMoveValid(Vector2Int.right))
        {
            return false;
        }

        return true;
    }
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            if(!locking) lockTime = 0f; // reset
        }

        return valid;
    }

    private bool CheckMoveValid(Vector2Int translation)
    {
        Debug.Log(string.Join(",", cells));
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        return board.IsValidPosition(this, newPosition);
    }

    private void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = rotationIndex;

        // Rotate all of the cells using a rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(originalRotation, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation)) {
                //the first wallkick0 translation is no kick (0,0)
                if (i != 0) wallKicked = true;
                else wallKicked = false;
                return true;
            }
        }

        return false;
    }


    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0) {
            wallKickIndex--;
        }

        Debug.Log($"wallKickIndex {wallKickIndex}");

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return (max - (min - input) % (max - min)) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }

}
