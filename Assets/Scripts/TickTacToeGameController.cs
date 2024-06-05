using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TickTakToeGameController : GameController {

    private Field[,] gameBoard;
    private int winLength;

    void Start() {
        Initialize(FieldSize, 3);  // Domyślna długość warunku zwycięstwa to 3 dla gry 3x3
        CurrentPlayer = Player1;
    }

    public override void Initialize(int fieldSize, int winLength) {
        FieldSize = fieldSize;
        this.winLength = winLength;
        gameBoard = new Field[FieldSize, FieldSize];
        CreateField(FieldSize);
    }

    public override void CreateField(int size) {
        float singleFieldSize = FieldPrefab.GetComponent<RectTransform>().rect.width;
        Vector2 startPoint = new Vector2(singleFieldSize * (-(size - 1) / 2f), singleFieldSize * (-(size - 1) / 2f));

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                GameObject newField = Instantiate(FieldPrefab, transform, false);
                newField.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPoint.x + x * singleFieldSize, startPoint.y + y * singleFieldSize);
                Field field = newField.GetComponent<Field>();
                field.Initialize(x, y);
                gameBoard[x, y] = field;
            }
        }
    }

    public override Player CheckVictory(Field lastPlaced) {
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

    private int CheckLine(Player player, int startX, int startY, int stepX, int stepY) {
        int count = 1;

        // Sprawdź w jedną stronę
        for (int i = 1; i < winLength; i++) {
            int newX = startX + i * stepX;
            int newY = startY + i * stepY;
            if (newX >= 0 && newX < FieldSize && newY >= 0 && newY < FieldSize && gameBoard[newX, newY].PlacedPawn?.Owner == player) {
                count++;
            } else {
                break;
            }
        }

        // Sprawdź w drugą stronę
        for (int i = 1; i < winLength; i++) {
            int newX = startX - i * stepX;
            int newY = startY - i * stepY;
            if (newX >= 0 && newX < FieldSize && newY >= 0 && newY < FieldSize && gameBoard[newX, newY].PlacedPawn?.Owner == player) {
                count++;
            } else {
                break;
            }
        }

        return count;
    }

    public override void Move(Field field) {
        if (field.IsEmpty()) {
            field.SetFieldState(CurrentPlayer);
            if (CheckVictory(field) != null) {
                Debug.Log($"{CurrentPlayer.name} wygrywa!");
                // Obsłuż zwycięstwo (np. pokaż wiadomość, zresetuj grę, itp.)
            } else {
                NextPlayer();
            }
        }
    }
}
