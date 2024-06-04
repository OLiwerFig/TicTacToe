using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{

    public int x;
    public int y;

    public GameObject PawnPrefab;


    public Pawn PlacedPawn {
        get;
        private set;
    }


    public void Initialize(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public void Click () {
        GameController.GetInstance().Move(this);
        //PrintDebug();
    }


    public bool IsEmpty() {

        return PlacedPawn == null;
    }


    public void PrintDebug() {

        Debug.Log(x + "    " + y);
    }

    public void SetFieldState(Player owner) {
        if (owner != null) {
            PlacedPawn = Instantiate(PawnPrefab, transform, false).GetComponent<Pawn>();
            PlacedPawn.Initialize (owner, this);
        }
        else if(PlacedPawn != null) {
            Destroy(PlacedPawn);
            }
    }


}
