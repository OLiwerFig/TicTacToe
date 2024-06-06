using UnityEngine;

public class TickTakToeGameController : GameController
{
    private Field[,] gameBoard;
    private int winLength;

    void Start()
    {
        Initialize(FieldSize, 3);  // Domyślna długość warunku zwycięstwa to 3 dla gry 3x3
        CurrentPlayer = Player1;
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
        if (field.IsEmpty())
        {
            field.SetFieldState(CurrentPlayer);
            Player winner = CheckVictory(field);
            if (winner != null)
            {
                Debug.Log($"{winner.name} wygrywa!");
                // Obsłuż zwycięstwo (np. pokaż wiadomość, zresetuj grę, itp.)
            }
            else
            {
                NextPlayer();
                if (CurrentPlayer.IsComputer)
                {
                    MoveComputer();
                }
            }
        }
    }

    public override Player CheckVictory(Field lastPlaced)
    {
        int x = lastPlaced.x;
        int y = lastPlaced.y;
        Player player = lastPlaced.PlacedPawn.Owner;

        // Sprawdź wiersz
        if (CheckLine(player, x, y, 0, 1) >= winLength) return player;

        // Sprawdź kolumnę
        if (CheckLine(player, x, y, 1, 0) >= winLength) return player;

        // Sprawdź przekątną (z góry-lewo do dołu-prawo)
        if (CheckLine(player, x, y, 1, 1) >= winLength) return player;

        // Sprawdź przekątną (z góry-prawo do dołu-lewo)
        if (CheckLine(player, x, y, 1, -1) >= winLength) return player;

        return null;
    }

    private int CheckLine(Player player, int startX, int startY, int stepX, int stepY)
    {
        int count = 1;

        // Sprawdź w jedną stronę
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

        // Sprawdź w drugą stronę
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



    public override void MoveComputer()
    {
        GameState currentState = GetGameState();
        int bestMoveIndex = FindBestMove(currentState);
        Field bestMoveField = gameBoard[bestMoveIndex / FieldSize, bestMoveIndex % FieldSize];
        Move(bestMoveField);
    }

    

    public override int FindBestMove(GameState state)
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
                    int moveValue = MiniMax(state, 0, -int.MaxValue, int.MaxValue, false);
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
        return bestMoveX * FieldSize + bestMoveY;
    }

    public override int MiniMax(GameState state, int depth, int alpha, int beta, bool isMaximizingPlayer)
    {
        Player winner = CheckVictoryState(state);
        if (winner != null)
        {
            return winner == Player2 ? 1 : -1;
        }
        if (depth >= 7) return 0;

        if (isMaximizingPlayer)
        {
            int bestValue = int.MinValue;
            for (int x = 0; x < FieldSize; x++)
            {
                for (int y = 0; y < FieldSize; y++)
                {
                    if (state.Board[x, y] == 0)
                    {
                        state.Board[x, y] = 1;
                        int value = MiniMax(state, depth + 1, alpha, beta, false);
                        state.Board[x, y] = 0;
                        bestValue = Mathf.Max(bestValue, value);
                        alpha = Mathf.Max(alpha, bestValue);
                        if (beta <= alpha)
                        {
                            break; // Beta cut-off
                        }
                    }
                }
            }
            return bestValue;
        }
        else
        {
            int bestValue = int.MaxValue;
            for (int x = 0; x < FieldSize; x++)
            {
                for (int y = 0; y < FieldSize; y++)
                {
                    if (state.Board[x, y] == 0)
                    {
                        state.Board[x, y] = -1;
                        int value = MiniMax(state, depth + 1, alpha, beta, true);
                        state.Board[x, y] = 0;
                        bestValue = Mathf.Min(bestValue, value);
                        beta = Mathf.Min(beta, bestValue);
                        if (beta <= alpha)
                        {
                            break; // Alpha cut-off
                        }
                    }
                }
            }
            return bestValue;
        }
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
                Debug.Log(state.Board[i, j]);
            }
        }
        return state;
    }



    public override Player CheckVictoryState(GameState state)
    {
        // Sprawdzanie wierszy
        for (int i = 0; i < FieldSize; i++)
        {
            if (state.Board[i, 0] != 0 && state.Board[i, 0] == state.Board[i, 1] && state.Board[i, 1] == state.Board[i, 2])
            {
                return state.Board[i, 0] == 1 ? Player2 : Player1;
            }
        }

        // Sprawdzanie kolumn
        for (int i = 0; i < FieldSize; i++)
        {
            if (state.Board[0, i] != 0 && state.Board[0, i] == state.Board[1, i] && state.Board[1, i] == state.Board[2, i])
            {
                return state.Board[0, i] == 1 ? Player2 : Player1;
            }
        }

        // Sprawdzanie przekątnych
        if (state.Board[0, 0] != 0 && state.Board[0, 0] == state.Board[1, 1] && state.Board[1, 1] == state.Board[2, 2])
        {
            return state.Board[0, 0] == 1 ? Player2 : Player1;
        }

        if (state.Board[0, 2] != 0 && state.Board[0, 2] == state.Board[1, 1] && state.Board[1, 1] == state.Board[2, 0])
        {
            return state.Board[0, 2] == 1 ? Player2 : Player1;
        }

        return null;
    }
}
