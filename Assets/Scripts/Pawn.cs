using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Pawn : MonoBehaviour
{


    public Player Owner {
        get;
        private set;
    }

    public Field ParentField {
        get;
        private set;
    }

    public void Initialize(Player owner, Field parent) {
        this.Owner = owner;
        this.ParentField = parent;
        ColorBlock newColorBlock = this.GetComponent<Button>().colors;
        newColorBlock.normalColor = owner.PlayerColor;
        this.GetComponent<Button>().colors = newColorBlock;
    }


}
    