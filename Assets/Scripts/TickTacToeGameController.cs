using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickTakToeGameController : GameController  {


    void Start()  {

        CreateField  (FieldSize);
		CurrentPlayer = Player1;

    }

    public override void Initialize(int fieldSize, int winLength) {
    }

    public override void CreateField(int size) {

		float singleFieldSize = FieldPrefab.GetComponent<RectTransform>().rect.width;
		Vector2 startPoint = new Vector2(singleFieldSize * (-(size - 1) / 2f), singleFieldSize * (-(size - 1) / 2f));

		for (int y = 0; y < size; y++)  {

			for (int x = 0; x < size; x++) {

				GameObject newField = Instantiate(FieldPrefab, transform, false);
				newField.GetComponent<RectTransform> ().anchoredPosition 
					= new Vector2 (startPoint.x + x * singleFieldSize, startPoint.y + y * singleFieldSize);
				newField.GetComponent<Field>().Initialize(x, y);
			}
		}
	}



    public override Player CheckVictory(Field lastPlaced) {

		Field[] fields = GetComponentsInChildren<Field> ();
		Player player = lastPlaced.PlacedPawn.Owner;

		return Player1;
    }

    public override void Move(Field field){

		if (field.IsEmpty()) {
			field.SetFieldState(CurrentPlayer);
			NextPlayer();

		}

    }




}


