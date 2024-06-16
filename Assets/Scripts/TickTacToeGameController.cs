using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TickTakToeGameController : GameController
{
    private Field[,] gameBoard;
    public int winLength;
    public int maxDepth;

    public Text EndText;

    private Dictionary<string, int> transpositionTable = new Dictionary<string, int>();

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
                // Switch to the computer player and let it think about its move.
                NextPlayer();
                if (CurrentPlayer.IsComputer)
                {
                    StartCoroutine(ComputerMove());
                }
            }
        }
    }


    public override void MoveComp()
    {
        StartCoroutine(ComputerMove());
    }

    private IEnumerator ComputerMove()
    {
        // Wait for a brief moment to simulate thinking time
        yield return new WaitForSeconds(0.5f);

        GameState currentState = GetGameState();
        (int bestMoveX, int bestMoveY) = FindBestMove(currentState);
        Field bestMoveField = gameBoard[bestMoveX, bestMoveY];
        Move(bestMoveField);
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

    public override (int, int) FindBestMove(GameState state)
    {
        int bestMoveX = -1;
        int bestMoveY = -1;
        int bestValue = int.MinValue;

        int dynamicDepth = Math.Min(maxDepth, FieldSize * FieldSize - CountFilled(state));

        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                if (state.Board[x, y] == 0)
                {
                    state.Board[x, y] = 1;
                    int moveValue = MiniMax(state, dynamicDepth, int.MinValue, int.MaxValue, false);
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

    private int CountFilled(GameState state)
    {
        int count = 0;
        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                if (state.Board[x, y] != 0)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public override int MiniMax(GameState state, int depth, int alpha, int beta, bool isMaximizingPlayer)
    {
        Player winner = CheckVictoryState(state);
        if (winner != null)
        {
            return winner == Player2 ? 1 : -1;
        }
        if (depth == 0 || IsFull(state))
        {
            return 0;
        }

        string stateHash = GetStateHash(state);
        if (transpositionTable.ContainsKey(stateHash))
        {
            return transpositionTable[stateHash];
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
            transpositionTable[stateHash] = maxEval;
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
            transpositionTable[stateHash] = minEval;
            return minEval;
        }
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
                state.Board[i, j] = gameBoard[i, j].PlacedPawn == null ? 0 : gameBoard[i, j].PlacedPawn.Owner == Player1 ? -1 : 1;
            }
        }
        return state;
    }

    public override Player CheckVictoryState(GameState state)
    {
        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                if (state.Board[x, y] != 0)
                {
                    int player = state.Board[x, y];

                    if (CheckLineState(player, x, y, 0, 1, state) >= winLength ||
                        CheckLineState(player, x, y, 1, 0, state) >= winLength ||
                        CheckLineState(player, x, y, 1, 1, state) >= winLength ||
                        CheckLineState(player, x, y, 1, -1, state) >= winLength)
                    {
                        return player == 1 ? Player2 : Player1;
                    }
                }
            }
        }
        return null;
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

    private string GetStateHash(GameState state)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < FieldSize; i++)
        {
            for (int j = 0; j < FieldSize; j++)
            {
                sb.Append(state.Board[i, j]);
            }
        }
        return sb.ToString();
    }
}
