using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TickTakToeGameController : GameController
{
    private Field[,] gameBoard;
    public int winLength;
    public int maxDepth;

    public Text EndText;

    void Start()
    {
        Initialize(FieldSize, winLength);
        CurrentPlayer = Player1;
        EndText.gameObject.SetActive(false);
    }

    public override void Initialize(int fieldSize, int winLength)
    {
        FieldSize = fieldSize;
        this.winLength = winLength;
        gameBoard = new Field[FieldSize, FieldSize];
        CreateField(FieldSize);
    }

    public override void CreateField(int size)
    {
        float singleFieldSize = FieldPrefab.GetComponent<RectTransform>().rect.width;
        Vector2 startPoint = new Vector2(singleFieldSize * (-(size - 1) / 2f), singleFieldSize * (-(size - 1) / 2f));

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                GameObject newField = Instantiate(FieldPrefab, transform, false);
                newField.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPoint.x + x * singleFieldSize, startPoint.y + y * singleFieldSize);
                Field field = newField.GetComponent<Field>();
                field.Initialize(x, y);
                gameBoard[x, y] = field;
            }
        }
    }

    public override void Move(Field field)
    {
        if (field.IsEmpty() && !gameOver)
        {
            field.SetFieldState(CurrentPlayer);
            Player winner = CheckVictory(field);
            if (winner != null)
            {
                OnGameEnd();
                OnWin(winner);
            }
            else if (CountEmpty())
            {
                OnGameEnd();
                OnDraw();
            }
            else
            {
                NextPlayer();
                if (CurrentPlayer == Player2 && Player2.IsComputer)
                {
                    StartCoroutine(ComputerMove());
                }
            }
        }
    }

    private IEnumerator ComputerMove()
    {
        yield return new WaitForSeconds(0.5f);

        GameState currentState = GetGameState();
        (int bestMoveX, int bestMoveY) = FindBestMove(currentState);
        Field bestMoveField = gameBoard[bestMoveX, bestMoveY];
        Move(bestMoveField);
    }




    public override (int, int) FindBestMove(GameState state)
    {
        int bestMoveX = -1;
        int bestMoveY = -1;
        int bestValue = int.MinValue;

        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                if (state.Board[x, y] == 0)
                {
                    state.Board[x, y] = 1; 
                    int moveValue = MiniMax(state, maxDepth, int.MinValue, int.MaxValue, false);
                    state.Board[x, y] = 0;
                    if (moveValue > bestValue)
                    {
                        bestMoveX = x;
                        bestMoveY = y;
                        bestValue = moveValue;
                    }
                }
            }
        }
        return (bestMoveX, bestMoveY);
    }

    public override int MiniMax(GameState state, int depth, int alpha, int beta, bool isMaximizingPlayer)
    {
        int score = CheckVictoryState(state);
        if (score != 0)
        {
            return score * (depth + 1); 
        }

        if (depth == 0 || IsFull(state))
        {
            return 0;
        }

        if (isMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            for (int x = 0; x < FieldSize; x++)
            {
                for (int y = 0; y < FieldSize; y++)
                {
                    if (state.Board[x, y] == 0)
                    {
                        state.Board[x, y] = 1; 
                        int eval = MiniMax(state, depth - 1, alpha, beta, false);
                        state.Board[x, y] = 0;
                        maxEval = Mathf.Max(maxEval, eval);
                        alpha = Mathf.Max(alpha, eval);
                        if (beta <= alpha)
                        {
                            break;
                        }
                    }
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            for (int x = 0; x < FieldSize; x++)
            {
                for (int y = 0; y < FieldSize; y++)
                {
                    if (state.Board[x, y] == 0)
                    {
                        state.Board[x, y] = -1; 
                        int eval = MiniMax(state, depth - 1, alpha, beta, true);
                        state.Board[x, y] = 0;
                        minEval = Mathf.Min(minEval, eval);
                        beta = Mathf.Min(beta, eval);
                        if (beta <= alpha)
                        {
                            break;
                        }
                    }
                }
            }
            return minEval;
        }
    }





    public override bool CountEmpty()
    {
        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                if (gameBoard[x, y].IsEmpty())
                {
                    return false;
                }
            }
        }
        return true;
    }

    public override bool IsFull(GameState state)
    {
        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                if (state.Board[x, y] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public override GameState GetGameState()
    {
        GameState state = new GameState();
        state.Board = new int[FieldSize, FieldSize];
        for (int i = 0; i < FieldSize; i++)
        {
            for (int j = 0; j < FieldSize; j++)
            {
                state.Board[i, j] = gameBoard[i, j].PlacedPawn == null ? 0 : (gameBoard[i, j].PlacedPawn.Owner == Player1 ? -1 : 1);
            }
        }
        return state;
    }




    private int CheckLineState(int player, int startX, int startY, int stepX, int stepY, GameState state)
    {
        int count = 1;


        for (int i = 1; i < winLength; i++)
        {
            int newX = startX + i * stepX;
            int newY = startY + i * stepY;
            if (newX >= 0 && newX < FieldSize && newY >= 0 && newY < FieldSize && state.Board[newX, newY] == player)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        for (int i = 1; i < winLength; i++)
        {
            int newX = startX - i * stepX;
            int newY = startY - i * stepY;
            if (newX >= 0 && newX < FieldSize && newY >= 0 && newY < FieldSize && state.Board[newX, newY] == player)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        return count;
    }

    private int CheckLine(Player player, int startX, int startY, int stepX, int stepY)
    {
        int count = 1;
        for (int i = 1; i < winLength; i++)
        {
            int newX = startX + i * stepX;
            int newY = startY + i * stepY;
            if (newX >= 0 && newX < FieldSize && newY >= 0 && newY < FieldSize && gameBoard[newX, newY].PlacedPawn?.Owner == player)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        for (int i = 1; i < winLength; i++)
        {
            int newX = startX - i * stepX;
            int newY = startY - i * stepY;
            if (newX >= 0 && newX < FieldSize && newY >= 0 && newY < FieldSize && gameBoard[newX, newY].PlacedPawn?.Owner == player)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        return count;
    }


    public override int CheckVictoryState(GameState state)
    {
        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                int player = state.Board[x, y];
                if (player != 0)
                {
                    if (CheckLineState(player, x, y, 1, 0, state) >= winLength) return player;
                    if (CheckLineState(player, x, y, 0, 1, state) >= winLength) return player;
                    if (CheckLineState(player, x, y, 1, 1, state) >= winLength) return player;
                    if (CheckLineState(player, x, y, 1, -1, state) >= winLength) return player;
                }
            }
        }
        return 0;
    }

    public override Player CheckVictory(Field lastPlaced)
    {
        int x = lastPlaced.x;
        int y = lastPlaced.y;
        Player player = lastPlaced.PlacedPawn.Owner;

        if (CheckLine(player, x, y, 0, 1) >= winLength) return player;
        if (CheckLine(player, x, y, 1, 0) >= winLength) return player;
        if (CheckLine(player, x, y, 1, 1) >= winLength) return player;
        if (CheckLine(player, x, y, 1, -1) >= winLength) return player;

        return null;
    }











    public override void OnWin(Player winner)
    {
        EndText.text = winner.name + "  Won!";
    }

    public override void OnDraw()
    {
        EndText.text = " Draw! ";
    }

    public override void OnGameEnd()
    {
        gameOver = true;

        EndText.transform.SetAsLastSibling();
        EndText.gameObject.SetActive(true);

        Image[] childrenImages = GetComponentsInChildren<Image>();

        foreach (var image in childrenImages)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);
        }

        //Clear();
    }

    public override void Clear()
    {
        Field[] fields = GetComponentsInChildren<Field>();
        foreach (var field in fields)
        {
            Destroy(field.gameObject);
        }
    }
}
