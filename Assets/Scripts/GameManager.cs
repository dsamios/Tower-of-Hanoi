using UnityEngine.Jobs;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;

public class GameManager : MonoBehaviour
{
    #region Variables

    public int mode;
    
    [SerializeField]
    public Sprite disk;

    public GameObject leftRod,middleRod,rightRod,guiObj,envObj,hanoiObj;

    public Sprite rod;

    public SpriteDrawMode drawMode=SpriteDrawMode.Sliced;

    public TMP_Text counterText,statusText;
    
    public float winDuration = 5f;

    public float helpDuration = 5f;

    public Slider speedSlider;
    private Stack<HanoiDisk> lRod = new Stack<HanoiDisk>();

    private Stack<HanoiDisk> mRod = new Stack<HanoiDisk>();

    private Stack<HanoiDisk> rRod = new Stack<HanoiDisk>();

    private Button undoButton;

    [SerializeField]
    private float scaleFact;

    private Stack<Tuple<Stack<HanoiDisk>, Stack<HanoiDisk>>> moveHistory = new Stack<Tuple<Stack<HanoiDisk>, Stack<HanoiDisk>>>();

    private Dictionary<Stack<HanoiDisk>, GameObject> stackDict = new Dictionary<Stack<HanoiDisk>, GameObject>();

    private float targetTime = 1.0f;

    private float speed = 1.0f;

    private bool isPaused  = true;

    private List<Move> moveList = new List<Move>();
    
    private int noDisks,moveCount;

    private bool enableUndo,enableSolutionTip;

    private Stack<HanoiDisk> fromStack = null;

    private Stack<HanoiDisk> targetStack = null;

    private HanoiDisk selectDisk = null;

    private Vector3 initial= Vector3.zero;  

    private float timer;

    private int helpLevel;

    private float helpTimer;

    private bool solved = false;

    private bool initialize =false;

    private bool assembled = false;
    private JobHandle simpleJobHandle;
    
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        noDisks = PlayerPrefs.GetInt("disks");
        enableUndo = PlayerPrefsX.GetBool("undoButton");
        enableSolutionTip= PlayerPrefsX.GetBool("solutionTip");      
    }

    // Update is called once per frame
    void Update()
    {
        // 0 : Playing Mode
        // 1 : Simulate Mode

        if (mode == 0) {
            if (WonGame())
            {
                if(timer < float.Epsilon)
                    statusText.text = "You won! Going back to Menu...";
                timer += Time.deltaTime;
                if (timer > winDuration)
                {
                    statusText.text = "";
                    SceneManager.LoadScene("MainMenu");
                }
            }
            else
            {
                if (helpTimer < float.Epsilon)
                {
                    if (helpLevel == 0)
                        statusText.text = "Welcome to Towers of Hanoi!\n Try to move all the disks to the rightmost rod by dragging them.";
                    if (helpLevel == 1)
                    {
                        if(enableSolutionTip)
                        {
                            statusText.text = "You can only place disks on top!\n You cannot place a bigger disk on top of a smaller one.";      
                        }else{
                            statusText.text = "You can only place disks on top!";
                        }

                    }                                        
                    if (helpLevel == 2)
                    {
                        if(enableSolutionTip)
                        {
                            statusText.text = "The best solution is done in " + (Mathf.Pow(2,noDisks) -1) + " moves.";      
                        }else{
                            statusText.text = "You cannot place a bigger disk on top of a smaller one.";
                        }
                    }                   
                    if (helpLevel == 3) 
                    {
                        AssembleGame();
                    }                    

                }
                if (helpLevel < 4)
                    helpTimer += Time.deltaTime;
                if (helpTimer > helpDuration)
                {
                    helpTimer = 0;
                    statusText.text = "";
                    helpLevel++;
                }

                if (Input.GetMouseButton(0) && fromStack == null)
                {
                    fromStack = GetStackFromDisk();
                    if (fromStack != null)
                    {
                        selectDisk = fromStack.Peek();
                        initial = selectDisk.gameObject.transform.position;
                    }
                }
                if (Input.GetMouseButton(0) && fromStack != null)
                {
                    Vector3 loc = Input.mousePosition;
                    loc.z = selectDisk.gameObject.transform.position.z;
                    loc = Camera.main.ScreenToWorldPoint(loc);
                    loc.z = selectDisk.gameObject.transform.position.z;
                    selectDisk.gameObject.transform.position = loc;
                }
                if (!Input.GetMouseButton(0) && fromStack != null)
                {
                    targetStack = GetStackFromRod();
                    if (targetStack != null && targetStack != fromStack && AllowedMove(fromStack,targetStack))
                    {
                        if(AllowedMove(fromStack, targetStack))
                        {
                            MoveDisk(fromStack, targetStack);
                            moveHistory.Push(new Tuple<Stack<HanoiDisk>, Stack<HanoiDisk>>(fromStack, targetStack));
                            IncrementMove();
                            fromStack = null; targetStack = null; selectDisk = null;
                        }
                    }
                    else
                    {
                        selectDisk.gameObject.transform.position = initial;
                        fromStack = null; targetStack = null; selectDisk = null; 
                    }
                }
            } 

        }
        else
        {
            if (WonGame())
            {
                
            }
            else
            {
                if(!solved){
                    if (helpTimer < float.Epsilon)
                    {
                        if (helpLevel == 0)
                        {
                            statusText.text = "Calculating moves .";
                        }
                        else if (helpLevel == 1)
                        {
                            statusText.text = "Calculating moves ..";
                        }                                        
                        else if (helpLevel == 2)
                        {
                            statusText.text = "Calculating moves ...";
                        }                   
                        else 
                        {
                            if (!initialize) 
                            {
                                CalculateMoves();
                                Debug.Log("pass");
                                initialize = true;
                            }
                            helpLevel = 0;
                            Debug.Log("pass2");
                        } 
                    }

                    if (helpLevel < 3)
                        helpTimer += Time.deltaTime;

                    if (helpTimer > helpDuration)
                    {
                        helpTimer = 0;
                        statusText.text = "";
                        helpLevel++;
                    } 
                }
                else if(solved && helpLevel < 4)
                {
                    statusText.text = "";
                    helpLevel = 5;                            
                    AssembleGame();
                    isPaused = false;
                }                                
                                
                if(!isPaused)
                {
                    targetTime -= Time.deltaTime;
                    if (targetTime <= 0.0f)                
                    {
                        Move move = moveList[moveCount]; 
                        MoveDisk(GetRodFromInt(move.start),GetRodFromInt(move.end));                   
                        targetTime = speed;
                        IncrementMove();
                    }                
                }
                
            }
        }  
    }

    private Stack<HanoiDisk> GetRodFromInt(int value){    
        if(value==1)
        {
            return mRod;
        }else if(value==2)
        {
            return rRod;
        }else{
            return lRod;
        }
    }

    private void AssembleGame()
    {
        hanoiObj.SetActive(true);
        envObj.SetActive(true);
        guiObj.SetActive(true);

        if(!enableUndo)
        {
            undoButton.gameObject.SetActive(false);
        }

        for (int i =1; i < noDisks + 1; i++)
        {
            HanoiDisk h = new HanoiDisk(noDisks - i, disk, leftRod.transform.position, middleRod.transform.position, rightRod.transform.position, drawMode);
            h.gameObject.transform.SetParent(this.gameObject.transform);

            //fix world positions
            h.gameObject.transform.position = leftRod.transform.position + Vector3.up * (i - 1) + Vector3.back;

            if (i > 1)
                h.gameObject.transform.position = GetTop(lRod.Peek().gameObject);

            BottomToCenter(h.gameObject);

            lRod.Push(h);
        }

        // initialize rods.
        SpriteRenderer lRodSpr = leftRod.AddComponent<SpriteRenderer>();
        SpriteRenderer mRodSpr = middleRod.AddComponent<SpriteRenderer>();
        SpriteRenderer rRodSpr = rightRod.AddComponent<SpriteRenderer>();

        lRodSpr.sprite = mRodSpr.sprite = rRodSpr.sprite = rod;
        lRodSpr.drawMode = mRodSpr.drawMode = rRodSpr.drawMode = drawMode;

        lRodSpr.size = mRodSpr.size = rRodSpr.size += Vector2.up * noDisks * scaleFact;
        Vector2 tempBase = leftRod.transform.position;

        BottomToCenter(leftRod);
        BottomToCenter(middleRod);
        BottomToCenter(rightRod);

        stackDict.Add(lRod, leftRod);
        stackDict.Add(mRod, middleRod);
        stackDict.Add(rRod, rightRod);
    }    

    private void BottomToCenter(GameObject g)
    {
        Vector3 tempBase = g.transform.position;
        Vector3 btm = tempBase; 
        btm.y = g.GetComponent<SpriteRenderer>().bounds.min.y;
        g.transform.position += tempBase - btm;
    }

    private Vector3 GetTop(GameObject g)
    {
        Vector3 v = g.transform.position;
        v.y = g.GetComponent<SpriteRenderer>().bounds.max.y;
        return v;
    }

    private Vector3 GetBottom(GameObject g)
    {
        Vector3 v = g.transform.position;
        v.y = g.GetComponent<SpriteRenderer>().bounds.min.y;
        return v;
    }

    private bool AllowedMove(Stack<HanoiDisk> former, Stack<HanoiDisk> target)
    {
        if (target.Count == 0)
            return true;
        else
            return former.Peek().GetRank() < target.Peek().GetRank();
    }

    private void MoveDisk(Stack<HanoiDisk> former, Stack<HanoiDisk> target)
    {       
        HanoiDisk h = former.Pop();
        HanoiDisk top;
        if (target.Count == 0)
        {
            GameObject g;
            stackDict.TryGetValue(target, out g);
            h.gameObject.transform.position = GetBottom(g);
        }
        else
        {
            top = target.Peek();
            h.gameObject.transform.position = GetTop(top.gameObject);
        }
        target.Push(h);
        BottomToCenter(h.gameObject);
        h.gameObject.transform.position += Vector3.back;
    }

    async void CalculateMoves(){
        await SolveTowers(noDisks,0,2,1);        
        solved = true;
    }

    async Task SolveTowers(int n, int from, int to, int via) {
        
        if (n == 1) {    
            // #if UNITY_EDITOR    
            //     Debug.Log("["+ ( moveList.Count + 1 ) +"] Move disk from rod " + from + " to rod " + to);
            // #endif
            moveList.Add(new Move(from,to));                
        } else {
            await SolveTowers(n - 1, from, via, to);
            await SolveTowers(1, from, to, via);
            await SolveTowers(n - 1, via, to, from);
        }
    }

    private bool WonGame()
    {
        return rRod.Count == noDisks;
    }

    private Stack<HanoiDisk> GetStackFromDisk()
    {

        foreach (KeyValuePair<Stack<HanoiDisk>, GameObject> p in stackDict)
        {
            if (p.Key.Count > 0)
            {
                Vector3 loc = Input.mousePosition;
                Bounds b = p.Key.Peek().gameObject.GetComponent<SpriteRenderer>().bounds;
                loc.z = b.center.z;
                loc = Camera.main.ScreenToWorldPoint(loc);
                loc.z = b.center.z;
                if (b.Contains(loc))
                    return p.Key;
            }
        }
        return null;
    }

    private Stack<HanoiDisk> GetStackFromRod()
    {
        foreach (KeyValuePair<Stack<HanoiDisk>, GameObject> p in stackDict)
        {
            Vector3 loc = Input.mousePosition;
            Bounds b = p.Value.GetComponent<SpriteRenderer>().bounds;
            loc.z = b.center.z;
            loc = Camera.main.ScreenToWorldPoint(loc);
            loc.z = b.center.z;
            if (b.Contains(loc))
                return p.Key;
        }
        return null;
    }

    private void IncrementMove()
    {
        moveCount += 1;
        counterText.text = moveCount.ToString();
    }

    private void DecrementMove()
    {
        moveCount -= 1;
        counterText.text = moveCount.ToString();
    }

    public void UndoMove(){
        if(moveHistory.Count > 0 && !WonGame())
        {
            Tuple<Stack<HanoiDisk>, Stack<HanoiDisk>> move = moveHistory.Pop();
            MoveDisk(move.Second, move.First);
            DecrementMove();
        }
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Pause(){
        isPaused=true;
    }

    public void Continue(){
        isPaused=false;   
    }

    public void ChangeSpeed()
    {
        speed=1.0f/speedSlider.value;
    }
}
