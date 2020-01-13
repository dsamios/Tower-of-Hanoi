using UnityEngine;
using UnityEditor;

public class HanoiDisk : Object
{

    public enum PEG { LEFT, MIDDLE, RIGHT}

    private int rank;
    private int peg;
    private SpriteRenderer spr;
    private Vector2 leftBase;
    private Vector2 midBase;
    private Vector2 rightBase;

    public GameObject gameObject;

    // scale factor
    public float scaleFact = 0.4f;

    public HanoiDisk(int rank, Sprite disk, Vector2 leftPeg, Vector2 middlePeg, Vector2 rightPeg, SpriteDrawMode drawMode)
    {
        this.rank = rank;

        peg = (int)PEG.LEFT;
        gameObject = new GameObject("Hanoi Block");

        leftBase = leftPeg; midBase = middlePeg; rightBase = rightPeg;

        spr = gameObject.AddComponent<SpriteRenderer>();
        spr.drawMode = drawMode;
        spr.sprite = disk;

        spr.size += Vector2.right * (rank-1) * scaleFact;

    }

    public int GetRank()
    {
        return rank;
    }

    public int GetPeg()
    {
        return peg;
    }


}
