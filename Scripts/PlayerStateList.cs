using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateList : MonoBehaviour
{
    public bool jumping = false;
    public bool dashing = false;
    public bool recoilingX, recoilingY;
    public bool lookingRight;
    public bool invincible; //cue title card
    public bool cutscene = false;
    public bool alive;
    public bool idle = false;
    public bool isDefault = true;
    public bool canMove = true;
}
