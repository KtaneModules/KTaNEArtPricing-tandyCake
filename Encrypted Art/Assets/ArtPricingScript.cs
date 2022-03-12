using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using KModkit;
using Rnd = UnityEngine.Random;

public class ArtPricingScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMSelectable leftArrow, rightArrow, upArrow, downArrow, submitButton;
    public TextMesh tag, submissionScreen;
    public GameObject[] tiles;
    public Material[] tileColors;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    int selectedBaseVal, selectedMultiplier;
    int targetBaseVal, targetMultiplier;
    int targetAnswer;

    int[] displayedPicture = new int[16];
    int[] tagPicture = new int[16];
    int[] decryptedPicture = new int[16];

    void Awake()
    {
        moduleId = moduleIdCounter++;
        leftArrow.OnInteract += delegate () { ButtonPress(leftArrow, 0.2f); Left(); UpdateDisplay(); return false; };
        rightArrow.OnInteract += delegate () { ButtonPress(rightArrow, 0.2f); Right(); UpdateDisplay(); return false; };
        upArrow.OnInteract += delegate () { ButtonPress(upArrow, 0.2f); Up(); UpdateDisplay(); return false; };
        downArrow.OnInteract += delegate () { ButtonPress(downArrow, 0.2f); Down(); UpdateDisplay(); return false; };
        submitButton.OnInteract += delegate () { ButtonPress(submitButton, 1); Submit(); return false; };
    }

    void ButtonPress(KMSelectable button, float iPunch)
    {
        button.AddInteractionPunch(iPunch);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
    }
    void Left()
    {
        if (selectedBaseVal != 1)
            selectedBaseVal--;
    }
    void Right()
    {
        if (selectedBaseVal != 12)
            selectedBaseVal++;
    }
    void Up()
    {
        if (selectedMultiplier != 4)
            selectedMultiplier++;
    }
    void Down()
    {
        if (selectedMultiplier != 1)
            selectedMultiplier--;
    }
    void UpdateDisplay()
    {
        if (!moduleSolved)
            submissionScreen.text = MoneyFormat(selectedBaseVal * (int)Math.Pow(10, selectedMultiplier));
    }

    void Submit()
    {
        if (moduleSolved)
            return;
        Debug.LogFormat("[Art Pricing #{0}] You submitted {1}.", moduleId, MoneyFormat(selectedBaseVal * (int)Math.Pow(10, selectedMultiplier)));
        if (selectedBaseVal*(int)Math.Pow(10, selectedMultiplier) == targetAnswer)
        {
            moduleSolved = true;
            Audio.PlaySoundAtTransform("cha ching", transform);
            submissionScreen.color = Color.green;
            Module.HandlePass();
            Debug.LogFormat("[Art Pricing #{0}] Module solved.", moduleId);
        }
        else
        {
            Module.HandleStrike();
            Debug.LogFormat("[Art Pricing #{0}] Incorrect, strike incurred.", moduleId);
        }
    }

    void Start()
    {
        GetPictures();
        DisplayInfo();
        GetBaseVal();
        GetMultiplier();
        LogInfo();
    }

    void GetPictures()
    {
        for (int i = 0; i < 16; i++)
        {
            displayedPicture[i] = Rnd.Range(0, 2);
            tagPicture[i] = Rnd.Range(0, 2);
            decryptedPicture[i] = displayedPicture[i] ^ tagPicture[i];
        }
    }
    void DisplayInfo()
    {
        tag.text = string.Empty;
        for (int row = 0; row < 4; row++)
        {
            string binary = string.Empty;
            for (int i = 0; i < 4; i++)
                binary += tagPicture[4 * row + i];
            if (binary == "0001")
                tag.text += " "; //1's are especially thin, so this will add a space before and after to ensure things don't look weirdly kerned.
            tag.text += Convert.ToString(Convert.ToInt32(binary, 2), 16).ToUpper(); //Converts the binary into hex and then appends it to the tag label
            if (binary == "0001")
                tag.text += " ";
        }
        for (int i = 0; i < 16; i++)
            tiles[i].GetComponent<MeshRenderer>().material = tileColors[displayedPicture[i]];
        selectedBaseVal = Rnd.Range(1, 13);
        selectedMultiplier = Rnd.Range(1, 5);
        UpdateDisplay();
    }
    void LogInfo()
    {
        Debug.LogFormat("[Art Pricing #{0}] The displayed grid is:", moduleId);
        LogGrid(displayedPicture, 4, 4, "░▓");
        Debug.LogFormat("[Art Pricing #{0}] The label says {1}, which corresponds to the grid:", moduleId, tag.text.Where(x => x != ' ').Join(""));
        LogGrid(tagPicture, 4, 4, "░▓");
        Debug.LogFormat("[Art Pricing #{0}] The decrypted grid:", moduleId);
        LogGrid(decryptedPicture, 4, 4, "░▓");
        Debug.LogFormat("[Art Pricing #{0}] The starting position on the edge is cell {1}.", moduleId, DateTime.Now.Month);
        Debug.LogFormat("[Art Pricing #{0}] The ending position after moving around the edges is cell {1}.", moduleId, targetBaseVal);
        Debug.LogFormat("[Art Pricing #{0}] The multiplier derived from the center cells is {1}.", moduleId, MoneyFormat((int)Math.Pow(10, targetMultiplier)));
        Debug.LogFormat("[Art Pricing #{0}] The correct price is {1}.", moduleId, MoneyFormat(targetAnswer));
    }
    void GetBaseVal()
    {
        int[] edgeValues = new int[12] 
        { 0, 1, 2, 3, 7, 11, 15, 14, 13, 12, 8, 4 }
        .Select(x => decryptedPicture[x]).ToArray(); //Gets the region of cells around the border.
        bool[] visited = new bool[12];
        int currentPos = DateTime.Now.Month - 1; //Months are indexed at Jan == 1
        visited[currentPos] = true;
        while (!visited.All(x => x))
        {
            int offset = edgeValues[currentPos] == 1 ? 1 : 11; //cw : ccw
            do
                currentPos = (currentPos + offset) % 12;
            while (visited[currentPos]);
            visited[currentPos] = true;
        }
        targetBaseVal = currentPos + 1;
    }
    void GetMultiplier()
    {
        int[] center = new int[] { 5, 6, 9, 10 }.Select(x => decryptedPicture[x]).ToArray();
        int binary = Convert.ToInt32( center.Join(""), 2 );
        int[] multiplierTable = new int[16] { 1, 1, 2, 2, 3, 4, 4, 3, 3, 4, 4, 3, 2, 2, 1, 1 };
        targetMultiplier = multiplierTable[binary]; //The above table works opposite from the one in the manual. It uses the center converted to binary as a key to obtain the correct exponent for 10.
        targetAnswer = targetBaseVal * (int)Math.Pow(10, targetMultiplier);
    }

    void LogGrid(int[] grid, int height, int length, string charSet, int shift = 0)
    {
        string logger = string.Empty;
        for (int i = 0; i < height * length; i++)
        {
            logger += charSet[grid[i] + shift];
            if (i % length == length - 1)
            {
                Debug.LogFormat("[Art Pricing #{0}] {1}", moduleId, logger);
                logger = string.Empty;
            }
        }
    }

    string MoneyFormat(int input)
    { 
        return "$" + input.ToString("N0"); 
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} submit $3,000> to submit that number into the module.";
    #pragma warning restore 414

    IEnumerator Press(KMSelectable btn, float wait)
    {
        btn.OnInteract();
        yield return new WaitForSeconds(wait);
    }
    IEnumerator Submit(int submittedBase, int submittedMultiplier)
    {
        while (selectedBaseVal != submittedBase)
            yield return Press(selectedBaseVal < submittedBase ? rightArrow : leftArrow, 0.1f);
        while (selectedMultiplier != submittedMultiplier)
            yield return Press(selectedMultiplier < submittedMultiplier ? upArrow : downArrow, 0.1f);
        yield return Press(submitButton, 0.1f);
    }

    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        Match m = Regex.Match(command, @"^(?:PAY|SUBMIT)\s+\$?([\d,]+)$");
        int baseVal;
        int mult;

        if (m.Success)
        {
            string val = int.Parse( m.Groups[1].Value.Replace(",", "") ).ToString();
            if (val == "10")
            {
                baseVal = 1;
                mult = 1;
            }
            else if (val.Length <= 1 || val.Length > 6)
            {
                yield return "sendtochaterror Invalid number length " + val.Length;
                yield break;
            }
            else if (val.StartsWith("10") || val.StartsWith("11") || val.StartsWith("12"))
            {
                baseVal = int.Parse(val.Substring(0, 2));
                mult = val.Substring(2).Length;
            }
            else
            {
                baseVal = val.First() - '0';
                mult = val.Length - 1;
            }
            if (baseVal < 1 || baseVal > 12)
                yield return "sendtochaterror Unexpected value of submitted base " + baseVal;
            else if (mult < 1 || mult > 4)
                yield return "sendtochaterror Unexpected value of submitted multiplier " + Math.Pow(10, mult) + " (" + mult + ")";
            else
            {
                yield return null;
                yield return Submit(baseVal, mult);
            }
        }

    }
    IEnumerator TwitchHandleForcedSolve ()
    {
        yield return Submit(targetBaseVal, targetMultiplier);
    }
}
