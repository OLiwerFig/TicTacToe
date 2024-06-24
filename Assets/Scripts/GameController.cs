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

    public abstract bool CountEmpty();

        public Player Player1;

        public Player Player2;

        protected bool gameOver = false;

        protected Player CurrentPlayer;

        public int FieldSize;

        public GameObject FieldPrefab;


        public abstract Player CheckVictory (Field lastPlaced);

        public abstract void OnWin (Player winner);

        public abstract void OnDraw ();

        public abstract void OnGameEnd ();

        protected void NextPlayer() {

            CurrentPlayer = (CurrentPlayer == Player1 ? Player2 : Player1);
            //Debug.Log("nastopila zmiana gracza z gracza na komputer !");
        }

        //public abstract bool CountEmpty ();

        public abstract void Initialize(int fieldSize, int winLength);

        public abstract void CreateField(int size);

        public abstract void Move(Field field);

        public abstract int CheckVictoryState(GameState state);

        public abstract GameState GetGameState();

        public abstract int MiniMax(GameState state, int depth, int alpha, int beta, bool isMaximizingPlayer);

        public abstract (int, int) FindBestMove(GameState state);

        public abstract bool IsFull(GameState state);



        public abstract void Clear();


}
