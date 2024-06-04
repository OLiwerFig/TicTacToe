using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameController : MonoBehaviour {


    private static GameController Instance;

    public static GameController GetInstance() {

        if (Instance == null) {
            Instance = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }
        return Instance;
    }

    public Player Player1;

    public Player Player2;

    protected Player CurrentPlayer;

    public int FieldSize;

    public GameObject FieldPrefab;

    public abstract Player CheckVictory (Field lastPlaced);

 
    protected void NextPlayer() {

        CurrentPlayer = (CurrentPlayer == Player1 ? Player2 : Player1);
    }

    public abstract void Initialize(int fieldSize, int winLength);

    public abstract void CreateField(int size);

    public abstract void Move(Field field); 

}
