using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DischargeMazeScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> arrows;
    public KMSelectable flipswitch;
    public Transform[] switches;
    public Transform fluid;
    public Transform[] markrots;
    public GameObject[] walls;
    public Renderer[] mleds;
    public Renderer fcol;
    public Renderer[] mrends;
    public Material[] mcols;
    public Material[] markmats;
    public Light bulbon;
    public TextMesh timer;
    public GameObject matstore;

    private readonly string[] bigmaze = new string[31]
    {
        "XXXXXXX XXXXX XXXXXXX X XXXXX X",
        "  X  O  X       X     XO    X X",
        "X X X XXX XXXXX X X XXXXX X X X",
        "X   X X   X   X   X X     X X X",
        "XXX X X X X X XXXXX X X XXX X X",
        "X   X X X   X     XO  X X     X",
        "X XXX X XXX XXXXX X XXX X XXXXX",
        "X   X     XOX       X   X    O ",
        "X X XXXXX X X XXXXX X X XXXXX X",
        "X X       X   X     X XO      X",
        "X X XXX XXX XXX X XXX XXXXXXXXX",
        "  XOX   X   X   X   X       X  ",
        "XXX X XXX XXX X XXX X XXXXX X X",
        "X   X    O    X   X   X       X",
        "X XXXXXXX XXXXXXX X XXX XXXXXXX",
        "X     X   X       X X   XO    X",
        "XXXXX X X X XXXXX X X X X XXX X",
        " O      X X   X   X   X X   X X",
        "XXX XXXXX X X X X X X X X X X X",
        "X       X   X X XO  X X   X X  ",
        "X XXXXX XXXXX X XXXXX XXXXX XXX",
        "X   X  O      X X       X  O  X",
        "XXX X XXXXX XXX X XXXXX X XXX X",
        "  X X   X    O      X     X   X",
        "X X X X X XXX XXXXX X XXXXX X X",
        "X X   X   X   X     XO  X   X X",
        "X XXX X XXX X X X XXXXX X X X X",
        "X X   X     X   X       X X X  ",
        "X X XXXXX XXXXXXXXX X X X X XXX",
        "X    O  X      O    X X   X   X",
        "XXXXXXX XXXXX XXXXXXX XXXXXXX X",
    };
    private string[] maze = new string[15];
    private int[] pos = new int[2];
    private int[] cols = new int[2];
    private int rot;
    private bool active;
    private bool held;
    private int charge;
    private int[] chrecord = new int[2] { -1, -1};
    private bool tp;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        rot = Random.Range(0, 4);
        for (int i = 0; i < 2; i++)
        {
            pos[i] = 2 * Random.Range(0, 9);
            cols[i] = Random.Range(0, 4);
        }
        fcol.material = mcols[cols[0]];
        for (int i = 0; i < 15; i++)
            for (int j = 0; j < 15; j++)
            {
                switch (rot) {
                    case 0: maze[i] += bigmaze[pos[0] + i][pos[1] + j].ToString(); break;
                    case 1: maze[i] += bigmaze[pos[1] + j][pos[0] + (14 - i)].ToString(); break;
                    case 2: maze[i] += bigmaze[pos[0] + (14 - i)][pos[1] + (14 - j)].ToString(); break;
                    default: maze[i] += bigmaze[pos[1] + (14 - j)][pos[0] + i].ToString(); break;
                }
            }
        for (int i = 0; i < 49; i++)
            if(maze[(2 * (i / 7)) + 1][(2 * (i % 7) + 1)] == 'O')
            {
                mrends[i].enabled = true;
                mrends[i].material = markmats[Random.Range(0, 3)];
                markrots[i].localEulerAngles = new Vector3(Random.Range(0, 360), 90, 90);
            }
        int edge = (rot + cols[1]) % 4;
        if (edge != 0)
            maze[14] = "XXXXXXXXXXXXXXX";
        if (edge != 1)
            for (int i = 0; i < 15; i++)
                maze[i] = new string(maze[i].Take(14).ToArray()) + "X";
        if (edge != 2)
            maze[0] = "XXXXXXXXXXXXXXX";
        if (edge != 3)
            for (int i = 0; i < 15; i++)
                maze[i] = "X" + new string(maze[i].Skip(1).ToArray());
        string[,] testmaze = new string[7, 7];
        int iter = 0;
        switch (edge){
            case 0:
                for (int i = 0; i < 7; i++)
                    if (maze[14][(2 * i) + 1] != 'X')
                        testmaze[6, i] = "+";
                break;
            case 1:
                for (int i = 0; i < 7; i++)
                    if (maze[(2 * i) + 1][14] != 'X')
                        testmaze[i, 6] = "+";
                break;
            case 2:
                for (int i = 0; i < 7; i++)
                    if (maze[0][(2 * i) + 1] != 'X')
                        testmaze[0, i] = "+";
                break;
            default:
                for (int i = 0; i < 7; i++)
                    if (maze[(2 * i) + 1][0] != 'X')
                        testmaze[i, 0] = "+";
                break;
        }
        while (Enumerable.Range(0, 49).Select(x => testmaze[x / 7, x % 7]).Contains("+"))
        {
            for (int i = 0; i < 49; i++)
                if (testmaze[i / 7, i % 7] == "+")
                    testmaze[i / 7, i % 7] = iter > 6 ? "O" : "X";
            for(int i = 0; i < 49; i++)
            {
                if (testmaze[i / 7, i % 7] != null)
                    continue;
                if (i / 7 > 0 && (testmaze[(i / 7) - 1, i % 7] == "X" || testmaze[(i / 7) - 1, i % 7] == "O") && maze[2 * (i / 7)][(2 * (i % 7)) + 1] != 'X')
                {
                    testmaze[i / 7, i % 7] = "+";
                    continue;
                }
                if (i % 7 < 6 && (testmaze[i / 7, (i % 7) + 1] == "X" || testmaze[i / 7, (i % 7) + 1] == "O") && maze[(2 * (i / 7)) + 1][(2 * (i % 7)) + 2] != 'X')
                {
                    testmaze[i / 7, i % 7] = "+";
                    continue;
                }
                if (i / 7 < 6 && (testmaze[(i / 7) + 1, i % 7] == "X" || testmaze[(i / 7) + 1, i % 7] == "O") && maze[(2 * (i / 7)) + 2][(2 * (i % 7)) + 1] != 'X')
                {
                    testmaze[i / 7, i % 7] = "+";
                    continue;
                }
                if (i % 7 > 0 && (testmaze[i / 7, (i % 7) - 1] == "X" || testmaze[i / 7, (i % 7) - 1] == "O") && maze[(2 * (i / 7)) + 1][2 * (i % 7)] != 'X')
                    testmaze[i / 7, i % 7] = "+";
            }           
            iter++;
        }
        pos[0] = Enumerable.Range(0, 49).Where(x => testmaze[x / 7, x % 7] == "O").PickRandom();
        pos[1] = pos[0] % 7;
        pos[0] /= 7;
        Debug.LogFormat("[Discharge Maze #{0}] The starting position of the {1} light is {2}{3}.", moduleID, new string[] { "Green", "Yellow", "Blue", "Red"}[cols[1]], "ABCDEFG"[pos[1]], pos[0] + 1);
        Logmaze(maze);
        Debug.LogFormat("[Discharge Maze #{0}] The gauge fluid is {1}.", moduleID, new string[] { "Green", "Yellow", "Blue", "Red" }[cols[0]]);
        matstore.SetActive(false);
        flipswitch.OnInteract = delegate ()
        {
            if (!active)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, switches[4]);
                flipswitch.AddInteractionPunch();
                switches[4].localEulerAngles = new Vector3(0, 0, 120);
                mleds[(7 * pos[0]) + pos[1]].material = mcols[cols[1]];
                fcol.enabled = true;
                active = true;
            }
            return false;
        };
        foreach(KMSelectable arrow in arrows)
        {
            int k = arrows.IndexOf(arrow);
            arrow.OnInteract = delegate ()
            {
                held = true;
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, arrow.transform);
                arrow.AddInteractionPunch(0.2f);
                StartCoroutine(Lever(k));
                return false;
            };
            arrow.OnInteractEnded = delegate ()
            {
                held = false;
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, arrow.transform);
                bulbon.intensity = 0;
                if (active)
                {
                    string move = new string[] { "Down", "Right", "Up", "Left" }[k] + " at " + charge.ToString() + "%. ";
                    switch (k)
                    {
                        case 0:
                            if (maze[(2 * pos[0]) + 2][(2 * pos[1]) + 1] == 'X')
                            {
                                move += "Hit wall south of ";
                                Debug.LogFormat("[Discharge Maze #{0}] {1}{2}{3}.", moduleID, move, "ABCDEFG"[pos[1]], pos[0] + 1);
                                walls[(15 * (pos[0] + 1)) + pos[1]].SetActive(true);
                                Struck();
                                return;
                            }
                            break;
                        case 1:
                            if (maze[(2 * pos[0]) + 1][(2 * pos[1]) + 2] == 'X')
                            {
                                move += "Hit wall east of ";
                                Debug.LogFormat("[Discharge Maze #{0}] {1}{2}{3}.", moduleID, move, "ABCDEFG"[pos[1]], pos[0] + 1);
                                walls[(15 * pos[0]) + pos[1] + 8].SetActive(true);
                                Struck();
                                return;
                            }
                            break;
                        case 2:
                            if (maze[2 * pos[0]][(2 * pos[1]) + 1] == 'X')
                            {
                                move += "Hit wall north of ";
                                Debug.LogFormat("[Discharge Maze #{0}] {1}{2}{3}.", moduleID, move, "ABCDEFG"[pos[1]], pos[0] + 1);
                                walls[(15 * pos[0]) + pos[1]].SetActive(true);
                                Struck();
                                return;
                            }
                            break;
                        default:
                            if (maze[(2 * pos[0]) + 1][2 * pos[1]] == 'X')
                            {
                                move += "Hit wall west of ";
                                Debug.LogFormat("[Discharge Maze #{0}] {1}{2}{3}.", moduleID, move, "ABCDEFG"[pos[1]], pos[0] + 1);
                                walls[(15 * pos[0]) + pos[1] + 7].SetActive(true);
                                Struck();
                                return;
                            }
                            break;
                    }
                    chrecord[0] = chrecord[1];
                    chrecord[1] = charge;
                    if (chrecord[0] >= 0)
                    {
                        switch (cols[0])
                        {
                            case 0:
                                if (Mathf.Abs(chrecord[0] - chrecord[1]) < 45)
                                {
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Fails Green release condition.", moduleID, move);
                                    Struck();
                                    return;
                                }
                                break;
                            case 1:
                                if (chrecord[1] - chrecord[0] < 30 && chrecord[0] - chrecord[1] < 60)
                                {
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Fails Yellow release condition.", moduleID, move);
                                    Struck();
                                    return;
                                }
                                break;
                            case 2:
                                if (Mathf.Abs(chrecord[0] - chrecord[1]) < 20 || Mathf.Abs(chrecord[0] - chrecord[1]) > 40)
                                {
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Fails Blue release condition.", moduleID, move);
                                    Struck();
                                    return;
                                }
                                break;
                            default:
                                if (chrecord[1] - chrecord[0] < 80 && chrecord[0] - chrecord[1] < 10)
                                {
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Fails Red release condition.", moduleID, move);
                                    Struck();
                                    return;
                                }
                                break;
                        }
                    }
                    mleds[(7 * pos[0]) + pos[1]].material = mcols[4];
                    switch (k)
                    {
                        case 0:
                            if (pos[0] > 5)
                            {
                                if (chrecord[1] < 90)
                                {
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Fails exit condition.", moduleID, move);
                                    Struck();
                                }
                                else
                                {
                                    moduleSolved = true;
                                    active = false;
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Exits the maze.", moduleID, move);
                                    module.HandlePass();
                                }
                            }
                            else
                            {
                                pos[0]++;
                                mleds[(7 * pos[0]) + pos[1]].material = mcols[cols[1]];
                                Debug.LogFormat("[Discharge Maze #{0}] {1} Moves to {2}{3}.", moduleID, move, "ABCDEFG"[pos[1]], pos[0] + 1);
                            }
                            return;
                        case 1:
                            if (pos[1] > 5)
                            {
                                if (chrecord[1] < 90)
                                {
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Fails exit condition.", moduleID, move);
                                    Struck();
                                }
                                else
                                {
                                    moduleSolved = true;
                                    active = false;
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Exits the maze.", moduleID, move);
                                    module.HandlePass();
                                }
                            }
                            else
                            {
                                pos[1]++;
                                mleds[(7 * pos[0]) + pos[1]].material = mcols[cols[1]];
                                Debug.LogFormat("[Discharge Maze #{0}] {1} Moves to {2}{3}.", moduleID, move, "ABCDEFG"[pos[1]], pos[0] + 1);
                            }
                            return;
                        case 2:
                            if (pos[0] < 1)
                            {
                                if (chrecord[1] < 90)
                                {
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Fails exit condition.", moduleID, move);
                                    Struck();
                                }
                                else
                                {
                                    moduleSolved = true;
                                    active = false;
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Exits the maze.", moduleID, move);
                                    module.HandlePass();
                                }
                            }
                            else
                            {
                                pos[0]--;
                                mleds[(7 * pos[0]) + pos[1]].material = mcols[cols[1]];
                                Debug.LogFormat("[Discharge Maze #{0}] {1} Moves to {2}{3}.", moduleID, move, "ABCDEFG"[pos[1]], pos[0] + 1);
                            }
                            return;
                        default:
                            if (pos[1] < 1)
                            {
                                if (chrecord[1] < 90)
                                {
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Fails exit condition.", moduleID, move);
                                    Struck();
                                }
                                else
                                {
                                    moduleSolved = true;
                                    active = false;
                                    Debug.LogFormat("[Discharge Maze #{0}] {1} Exits the maze.", moduleID, move);
                                    module.HandlePass();
                                }
                            }
                            else
                            {
                                pos[1]--;
                                mleds[(7 * pos[0]) + pos[1]].material = mcols[cols[1]];
                                Debug.LogFormat("[Discharge Maze #{0}] {1} Moves to {2}{3}.", moduleID, move, "ABCDEFG"[pos[1]], pos[0] + 1);
                            }
                            return;
                    }                   
                }
            };
        }
        StartCoroutine(Charge());
    }

    private void Start()
    {
        float scalar = transform.lossyScale.x;
	    bulbon.range *= scalar;
    }

    private void Logmaze(string[] m)
    {
        string[][] logmaze = new string[15][];
        for (int i = 0; i < 15; i++)
            logmaze[i] = m[i].Select(x => x == 'X' ? "\u25a0" : "\u25a1").ToArray();
        for (int i = 0; i < 49; i++)
            if (m[(2 * (i / 7)) + 1][(2 * (i % 7)) + 1] == 'O')
                logmaze[(2 * (i / 7)) + 1][(2 * (i % 7)) + 1] = "\u25ef";
        logmaze[(2 * pos[0]) + 1][(2 * pos[1]) + 1] = m[(2 * pos[0]) + 1][(2 * pos[1]) + 1] == 'O' ? "\u25c9" : "\u25a3";
        Debug.LogFormat("[Discharge Maze #{0}] The maze to navigate through is:\n[Discharge Maze #{0}] {1}", moduleID, string.Join("\n[Discharge Maze #" + moduleID + "] ", logmaze.Select(x => string.Join("", x.ToArray())).ToArray()));
    }

    private IEnumerator Lever(int h)
    {
        float e = switches[h].localEulerAngles.z;
        e -= 46;
        e /= 180;
        while(e < 0.5f && held)
        {
            e += Time.deltaTime;
            switches[h].localEulerAngles = new Vector3(0, -90, Mathf.Lerp(46, 136, 2 * e));
            yield return null;
        }
        switches[h].localEulerAngles = new Vector3(0, -90, 136);
        while (held)
            yield return null;
        while(e > 0f && !held)
        {
            e -= Time.deltaTime;
            switches[h].localEulerAngles = new Vector3(0, -90, Mathf.Lerp(46, 136, 2 * e));
            yield return null;
        }
    }

    private IEnumerator Charge()
    {
        float ch = 0;
        while (!moduleSolved)
        {
            if (active)
            {
                if(TwitchPlaysActive && !tp)
                    tp = true;
                timer.text = (charge < 10 ? "0" : "") + charge.ToString();
                if (held)
                {
                    ch -= (tp && ch <= 0.5f) ? (Time.deltaTime / 2) : Time.deltaTime;
                    Gauge(ch);
                    bulbon.intensity = ch * 10;
                    if(ch <= 0f)
                    {
                        Debug.LogFormat("[Discharge Maze #{0}] Capacitor Depleted!", moduleID);
                        Struck();
                    }
                }
                else
                {
                    ch += (tp && ch >= 9f) ? (Time.deltaTime / 5) : Time.deltaTime;
                    Gauge(ch);
                    if (ch >= 10f)
                    {
                        Debug.LogFormat("[Discharge Maze #{0}] Capacitor Overload!", moduleID);
                        Struck();
                    }
                }
            }
            else
            {
                if(ch > 0f)
                {
                    ch -= Time.deltaTime;
                    Gauge(ch);
                }
            }
            yield return null;
        }
    }

    private void Gauge(float x)
    {
        charge = (int)(x * 10);
        fluid.localPosition = new Vector3(0, (x / 10) - 1, -0.1f);
        fluid.localScale = new Vector3(2, x / 5, 0.15f);
    }

    private void Struck()
    {
        active = false;
        timer.text = "";
        chrecord = new int[2] { -1, -1 };
        switches[4].localEulerAngles = new Vector3(0, 0, 0);
        mleds[(7 * pos[0]) + pos[1]].material = mcols[4];
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CapacitorPop, transform);
        module.HandleStrike();
    }

    bool TwitchPlaysActive;

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} U/R/D/L # [Holds down the specified lever and releases it at the specified charge percentage] | !{0} activate";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        if (command == "ACTIVATE")
        {
            if (active)
            {
                yield return "sendtochat!f Maze is already active.";
                yield break;
            }
            yield return null;
            flipswitch.OnInteract();
            yield break;
        }
        if (!active)
        {
            yield return "sendtochaterror!f Maze is not active.";
            yield break;
        }
        string[] commands = command.Split(' ');
        if(commands.Length < 2)
        {
            yield return "sendtochaterror!f Invalid command length.";
            yield break;
        }
        int d = "DRUL".IndexOf(commands[0]);
        if (commands[0].Length > 1)
            d = new List<string> { "DOWN", "RIGHT", "UP", "LEFT" }.IndexOf(commands[0]);
        if (d < 0)
        {
            yield return "sendtochaterror!f Invalid direction.";
            yield break;
        }
        int t = 0;
        if (commands[1].Last() == '%')
            commands[1] = commands[1].Remove(commands[1].Length - 1);
        if (int.TryParse(commands[1], out t))
        {
            if(t > 99 || t < 1)
            {
                yield return "sendtochaterror!f Charge percentages must be in the range 01-99.";
                yield break;
            }
            while (charge < t)
                yield return null;
            yield return null;
            arrows[d].OnInteract();
            while (charge > t)
                yield return null;
            arrows[d].OnInteractEnded();
            yield return null;
            if (active)
                yield return "sendtochat!f Light is now positioned at " + "ABCDEFG"[pos[1]] + (pos[0] + 1).ToString() + ".";
        }
        else
            yield return "sendtochaterror!f Invalid charge percentage.";
    }
}
