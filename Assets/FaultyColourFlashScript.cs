using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

public class FaultyColourFlashScript : MonoBehaviour {

    public KMBombInfo bomb;
    public KMSelectable yesButton;
    public KMSelectable noButton;
    public TextMesh display;

    private Coroutine flashCoroutine;
    private List<char> chosenLetters = new List<char>();
    private readonly string[][] tableList = new string[][]
    {
        new string[] { "1d", "3d", "perspective", "orientation" },
        new string[] { "bravo", "kilo", "golf", "x-ray" },
        new string[] { "red", "blue", "green", "yellow" },
        new string[] { "crazy", "insane", "mad", "wild" },
        new string[] { "press", "tap", "hold", "mash" },
        new string[] { "word", "letter", "character", "symbol" },
        new string[] { "boolean", "logic", "gate", "and" },
        new string[] { "alpha", "omega", "eta", "xi" },
        new string[] { "morse", "flash", "light", "led" },
        new string[] { "binary", "ternary", "zero", "one" },
        new string[] { "sound", "audio", "pitch", "listen" },
        new string[] { "who", "what", "why", "when" },
        new string[] { "line", "triangle", "square", "hexagon" },
        new string[] { "talk", "say", "shout", "scream" },
        new string[] { "color", "colour", "rainbow", "art" },
        new string[] { "password", "code", "cipher", "encrypt" },
        new string[] { "guitar", "piano", "music", "song" },
        new string[] { "&", "#", "?", "!" },
        new string[] { "math", "equation", "calculus", "derivative" },
        new string[] { "faulty", "cruel", "ultimate", "bamboozling" },
        new string[] { "wire", "button", "maze", "simon" },
        new string[] { "chess", "knight", "pawn", "king" },
        new string[] { "time", "clock", "date", "day" },
        new string[] { "balance", "order", "harmony", "rule" },
        new string[] { "forget", "memory", "amnesia", "elder" },
        new string[] { "sugar", "candy", "taco", "snack" },
    };
    private Color[] textColors = { new Color(1.0f, 0.0f, 0.0f, 1.0f), new Color(0.0f, 1.0f, 0.0f, 1.0f), new Color(0.0f, 0.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 0.0f, 1.0f), new Color(1.0f, 0.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f) };
    private bool[] modPresence = new bool[26];
    private bool[] fasterSpeed = new bool[8];
    private int[] wordIndexes = new int[8];
    private int[] colourIndexes = new int[8];
    private int pressIndex;
    private readonly string[] colours = { "R--", "-G-", "--B", "RG-", "R-B", "RGB" };
    private readonly string[] colourNames = { "Red", "Green", "Blue", "Yellow", "Magenta", "White" };
    private readonly int[] tapCode = { 11, 12, 13, 14, 15, 21, 22, 23, 24, 25, 0, 31, 32, 33, 34, 35, 41, 42, 43, 44, 45, 51, 52, 53, 54, 55 };
    private readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        yesButton.OnInteract += delegate () { PressButton(true); return false; };
        noButton.OnInteract += delegate () { PressButton(false); return false; };
    }

    void Start ()
    {
        List<string> mods = bomb.GetModuleNames();
        if (Application.isEditor && mods.Count == 0)
            mods.Add("Faulty Colour Flash");
        for (int mod = 0; mod < mods.Count; mod++)
        {
            for (int i = 0; i < 26; i++)
            {
                if (modPresence[i] != true)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (mods[mod].ToLower().Replace(" ", "").Contains(tableList[i][j]))
                        {
                            modPresence[i] = true;
                            break;
                        }
                    }
                }
            }
        }
        redo:
        if (modPresence.Contains(false))
        {
            for (int i = 0; i < 4; i++)
            {
                int rando = UnityEngine.Random.Range(0, 2);
                if (rando == 0)
                {
                    int choice = UnityEngine.Random.Range(0, 26);
                    while (modPresence[choice] == true)
                        choice = UnityEngine.Random.Range(0, 26);
                    chosenLetters.Add(alphabet[choice]);
                }
                else
                {
                    int choice = UnityEngine.Random.Range(0, 26);
                    while (modPresence[choice] == false)
                        choice = UnityEngine.Random.Range(0, 26);
                    chosenLetters.Add(alphabet[choice]);
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
                chosenLetters.Add(alphabet[UnityEngine.Random.Range(0, 26)]);
        }
        chosenLetters = chosenLetters.Shuffle();
        int[] nums = new int[8];
        int[] tempNumsLog = new int[8];
        for (int i = 0; i < 8; i+=2)
        {
            int index = chosenLetters[i / 2] - 65;
            if (tapCode[index] == 0)
            {
                int rando = UnityEngine.Random.Range(0, 2);
                if (rando == 0)
                {
                    nums[i] = 0;
                    nums[i + 1] = UnityEngine.Random.Range(0, 6);
                }
                else
                {
                    nums[i] = UnityEngine.Random.Range(0, 6);
                    nums[i + 1] = 0;
                }
            }
            else
            {
                nums[i] = tapCode[index] / 10;
                nums[i + 1] = tapCode[index] % 10;
            }
            tempNumsLog[i] = nums[i];
            tempNumsLog[i + 1] = nums[i + 1];
        }
        if (nums.Count(x => x.EqualsAny(0, 1)) == 8 || nums.Count(x => x.EqualsAny(4, 5)) == 8)
        {
            chosenLetters.Clear();
            goto redo;
        }
        redo2:
        for (int i = 0; i < 8; i++)
        {
            int rando = UnityEngine.Random.Range(0, 2);
            if ((nums[i] == 2 && rando == 0) || nums[i] > 3)
                fasterSpeed[i] = true;
        }
        if (fasterSpeed.Count(x => x) < 1 || fasterSpeed.Count(x => x) > 7)
        {
            fasterSpeed = new bool[8];
            goto redo2;
        }
        for (int i = 0; i < 8; i++)
        {
            if (fasterSpeed[i])
                nums[i] -= 2;
        }
        List<int> speedLog = new List<int>();
        string log1 = "", log2 = "", log3 = "";
        int[] prevTemp = new int[2];
        for (int i = 0; i < 8; i++)
        {
            int[] temp = GetPuzzleColours(nums[i]);
            while (temp[0] == prevTemp[0] && temp[1] == prevTemp[1])
                temp = GetPuzzleColours(nums[i]);
            prevTemp[0] = temp[0];
            prevTemp[1] = temp[1];
            wordIndexes[i] = temp[0];
            colourIndexes[i] = temp[1];
            if (i != 8)
            {
                log1 += colourNames[temp[0]] + ", ";
                log2 += colourNames[temp[1]] + ", ";
            }
            else
            {
                log1 += colourNames[temp[0]];
                log2 += colourNames[temp[1]];
            }
            if (i % 2 == 0)
                log3 += modPresence[chosenLetters[i / 2] - 65] ? "Yes " : "No ";
            if (fasterSpeed[i])
                speedLog.Add(i + 1);
        }
        Debug.LogFormat("[Faulty Colour Flash #{0}] Words: {1}", moduleId, log1);
        Debug.LogFormat("[Faulty Colour Flash #{0}] Colours: {1}", moduleId, log2);
        Debug.LogFormat("[Faulty Colour Flash #{0}] Fast Displays: {1}", moduleId, speedLog.Join(", "));
        Debug.LogFormat("[Faulty Colour Flash #{0}] Number Pairs: {1} {2} {3} {4}", moduleId, tempNumsLog[0].ToString() + tempNumsLog[1].ToString(), tempNumsLog[2].ToString() + tempNumsLog[3].ToString(), tempNumsLog[4].ToString() + tempNumsLog[5].ToString(), tempNumsLog[6].ToString() + tempNumsLog[7].ToString());
        Debug.LogFormat("[Faulty Colour Flash #{0}] Letters: {1}", moduleId, chosenLetters.Join(", "));
        Debug.LogFormat("[Faulty Colour Flash #{0}] Correct Presses: {1}", moduleId, log3.Trim());
        GetComponent<KMBombModule>().OnActivate += Activate;
    }

    void Activate()
    {
        flashCoroutine = StartCoroutine(DisplaySequence());
    }

    int[] GetPuzzleColours(int dif)
    {
        redo:
        int choice1 = UnityEngine.Random.Range(0, 6);
        int choice2 = UnityEngine.Random.Range(0, 6);
        int check = 0;
        for (int i = 0; i < 3; i++)
        {
            if (colours[choice1][i] != colours[choice2][i])
                check++;
        }
        if (check != dif)
            goto redo;
        return new int[] { choice1, choice2 };
    }

    void PressButton(bool yesPress)
    {
        if (moduleSolved != true)
        {
            Debug.LogFormat("[Faulty Colour Flash #{0}] Pressed {1}", moduleId, yesPress ? "Yes" : "No");
            if ((yesPress && modPresence[chosenLetters[pressIndex] - 65]) || (!yesPress && !modPresence[chosenLetters[pressIndex] - 65]))
            {
                pressIndex++;
                if (pressIndex == 4)
                {
                    Debug.LogFormat("[Faulty Colour Flash #{0}] Module disarmed", moduleId);
                    moduleSolved = true;
                    GetComponent<KMBombModule>().HandlePass();
                    StopCoroutine(flashCoroutine);
                }
            }
            else
            {
                Debug.LogFormat("[Faulty Colour Flash #{0}] Strike: Incorrect button pressed", moduleId);
                pressIndex = 0;
                GetComponent<KMBombModule>().HandleStrike();
            }
        }
    }

    IEnumerator DisplaySequence()
    {
        while (true)
        {
            for (int colourSequenceIndex = 0; colourSequenceIndex < 8; ++colourSequenceIndex)
            {
                display.text = colourNames[wordIndexes[colourSequenceIndex]];
                display.color = textColors[colourIndexes[colourSequenceIndex]];
                if (fasterSpeed[colourSequenceIndex])
                    yield return new WaitForSeconds(.4f);
                else
                    yield return new WaitForSeconds(.75f);
            }

            display.text = "";
            yield return new WaitForSeconds(2f);
        }
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <yes/y/no/n> [Presses the Yes or No button] | Presses can be chained with spaces";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify at least one button to press!";
            }
            else
            {
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (!parameters[i].ToLower().EqualsAny("yes", "y", "no", "n"))
                    {
                        yield return "sendtochaterror!f The specified button to press '" + parameters[i] + "' is invalid!";
                        yield break;
                    }
                }
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (parameters[i].ToLower().EqualsAny("yes", "y"))
                        yesButton.OnInteract();
                    else
                        noButton.OnInteract();
                    yield return new WaitForSeconds(.25f);
                }
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = pressIndex; i < 4; i++)
        {
            if (modPresence[chosenLetters[i] - 65])
                yesButton.OnInteract();
            else
                noButton.OnInteract();
            yield return new WaitForSeconds(.25f);
        }
    }
}