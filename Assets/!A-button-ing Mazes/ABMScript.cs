using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ABMScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Transform[] bpos;
    public Renderer[] cells;
    public Renderer[] walls;
    public Renderer[] markers;
    public Renderer[] bulbs;
    public Material[] lmats;
    public Light[] lights;

    private readonly int[,] movetable = new int[6, 6]
    {
        { 0, 1, 2, 5, 4, 3},
        { 1, 5, 0, 3, 2, 4},
        { 5, 3, 1, 4, 0, 2},
        { 3, 4, 5, 2, 1, 0},
        { 4, 2, 3, 0, 5, 1},
        { 2, 0, 4, 1, 3, 5}
    };
    private readonly Color[] cols = new Color[6] { new Color(1, 0, 0), new Color(0, 0, 1), new Color(1, 1, 0), new Color(1, 0, 1), new Color(0, 1, 1), new Color(1, 1, 1)};
    private string[][] mazes = new string[4][];
    private int[,] pos = new int[4,3];
    private bool[] pressable = new bool[4] { true, true, true, true};
    private bool hold;
    private int col;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        cells[12].material = lmats[0];
        cells[62].material = lmats[0];
        foreach (Renderer b in bulbs)
            b.material = lmats[0];
        float scale = module.transform.lossyScale.x;
        foreach (Light l in lights)
            l.range *= scale;
        int[][,] mprox = new int[4][,];
        int[] mselect = Enumerable.Range(0, 12).ToArray().Shuffle().Take(4).ToArray();
        for(int i = 0; i < 4; i++)
        {
            mazes[i] = ABMMazes.mazes[mselect[i]];
            if (Random.Range(0, 2) > 0)
                mazes[i] = mazes[i].Reverse().ToArray();
            if (Random.Range(0, 2) > 0)
                for(int j = 0; j < 11; j++)
                mazes[i][j] = new string(mazes[i][j].Reverse().ToArray());
            if (Random.Range(0, 2) > 0)
            {
                string[] m = mazes[i];
                for (int j = 0; j < 11; j++)
                {
                    string n = "";
                    for (int k = 0; k < 11; k++)
                        n += m[j][k].ToString();
                    mazes[i][j] = n;
                }
            }           
            Debug.LogFormat("[A-button-ing Mazes #{0}] {1} maze:\n[A-button-ing Mazes #{0}] {2}", moduleID, new string[] { "Up", "Right", "Down", "Left"}[i], string.Join("\n[A-button-ing Mazes #" + moduleID + "] ", mazes[i]));
            int s = Random.Range(0, 25);
            pos[i, 0] = s;
            pos[i, 1] = s % 5;
            pos[i, 2] = s / 5;
            cells[(25 * i) + s].material = lmats[2];
            for (int j = 0; j < 16; j++)
                if (mazes[i][(2 * (j % 4)) + 2][(2 * (j >> 2)) + 2] == 'X')
                    markers[(16 * i) + j].enabled = true;
            mprox[i] = new int[5, 5];
            mprox[i][s % 5, s / 5] = 1;
            int iter = 1;
            while (Enumerable.Range(0, 25).Select(x => mprox[i][x / 5, x % 5] < 1).Any(x => x))
            {
                for (int j = 0; j < 5; j++)
                    for (int k = 0; k < 5; k++)
                        if (mprox[i][j, k] < 1 && ((j > 0 && mprox[i][j - 1, k] == iter && mazes[i][2 * j][(2 * k) + 1] != '#') || (k > 0 && mprox[i][j, k - 1] == iter && mazes[i][(2 * j) + 1][2 * k] != '#') || (j < 4 && mprox[i][j + 1, k] == iter && mazes[i][(2 * j) + 2][(2 * k) + 1] != '#') || (k < 4 && mprox[i][j, k + 1] == iter && mazes[i][(2 * j) + 1][(2 * k) + 2] != '#')))
                            mprox[i][j, k] = iter + 1;
                iter++;
            }
        }
        Debug.LogFormat("[A-button-ing Mazes #{0}] Goal positions: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 4).Select(x => "URDL"[x] + " = " + "ABCDE"[pos[x, 2]] + (pos[x, 1] + 1).ToString()).ToArray()));
        int[] mspos = new int[] { 1, 1, 1, 1 };
        while(mspos.Any(x => x < 6))
        {
            bool find = false;
            int r = Random.Range(0, 4);
            List<int> rdir = new List<int> { };
            if (pos[r, 1] > 0 && mazes[r][2 * pos[r, 1]][(2 * pos[r, 2]) + 1] != '#')
                rdir.Add(0);
            if (pos[r, 2] > 0 && mazes[r][(2 * pos[r, 1]) + 1][2 * pos[r, 2]] != '#')
                rdir.Add(1);
            if (pos[r, 1] < 4 && mazes[r][(2 * pos[r, 1]) + 2][(2 * pos[r, 2]) + 1] != '#')
                rdir.Add(2);
            if (pos[r, 2] < 4 && mazes[r][(2 * pos[r, 1]) + 1][(2 * pos[r, 2]) + 2] != '#')
                rdir.Add(3);
            rdir.Shuffle();
            int s = 0;
            for(int i = 0; i < rdir.Count(); i++)
            {
                int d = rdir[i];
                int[] scheck = Enumerable.Range(0, 4).Where(x => x != r).ToArray().Shuffle().ToArray();
                for(int j = 0; j < 3; j++)
                {
                    s = scheck[j];
                    switch (d)
                    {
                        case 0:
                            if (pos[s, 1] > 0 && mazes[s][2 * pos[s, 1]][(2 * pos[s, 2]) + 1] != '#')
                                find = true;
                            break;
                        case 1:
                            if (pos[s, 2] > 0 && mazes[s][(2 * pos[s, 1]) + 1][2 * pos[s, 2]] != '#')
                                find = true;
                            break;
                        case 2:
                            if (pos[s, 1] < 4 && mazes[s][(2 * pos[s, 1]) + 2][(2 * pos[s, 2]) + 1] != '#')
                                find = true;
                            break;
                        default:
                            if (pos[s, 2] < 4 && mazes[s][(2 * pos[s, 1]) + 1][(2 * pos[s, 2]) + 2] != '#')
                                find = true;
                            break;
                    }
                    if (find)
                        break;
                }
                if (find)
                {
                    switch (d)
                    {
                        case 0:
                            pos[r, 1]--;
                            pos[s, 1]--;
                            break;
                        case 1:
                            pos[r, 2]--;
                            pos[s, 2]--;
                            break;
                        case 2:
                            pos[r, 1]++;
                            pos[s, 1]++;
                            break;
                        default:
                            pos[r, 2]++;
                            pos[s, 2]++;
                            break;
                    }
                    break;
                }
            }
            if (find)
            {
                mspos[r] = mprox[r][pos[r, 1], pos[r, 2]];
                mspos[s] = mprox[s][pos[s, 1], pos[s, 2]];
            }
        }
        for(int i = 0; i < 4; i++)
            cells[(25 * i) + pos[i, 1] + (pos[i, 2] * 5)].material = lmats[1];
        Debug.LogFormat("[A-button-ing Mazes #{0}] Start positions: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 4).Select(x => "URDL"[x] + " = " + "ABCDE"[pos[x, 2]] + (pos[x, 1] + 1).ToString()).ToArray()));
        foreach (KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                    StartCoroutine(Push(b, true));
                }
                return false;
            };
            button.OnInteractEnded += delegate ()
            {
                if (!moduleSolved)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform);
                    StartCoroutine(Push(b, false));
                    if (!hold)
                        return;
                    hold = false;
                    int t = (int)info.GetTime();
                    Debug.LogFormat("[A-button-ing Mazes #{0}] Released at {1}.", moduleID, info.GetFormattedTime());
                    t = t % 10 > 5 ? (t % 60) / 10 : t % 10;
                    int v = movetable[col, t];
                    int[] p = new int[2];
                    if(v > 2)
                    {
                        p[0] = 3;
                        p[1] = v - 3;
                    }
                    else if(v < 1)
                    {
                        p[0] = 0;
                        p[1] = 1;
                    }
                    else
                    {
                        p[0] = 2;
                        p[1] = v - 1;
                    }
                    Debug.Log("(" + col + "," + t + ") = " + v + "=>" + p[0] + "&" + p[1]);
                    if (Move(p[0], b) & Move(p[1], b))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int x = pos[p[i], 1] + (5 * pos[p[i], 2]);
                            cells[(25 * p[i]) + x].material = lmats[pos[p[i], 0] == x ? 2 : 0];
                            switch (b)
                            {
                                case 0:
                                    pos[p[i], 1]--;
                                    break;
                                case 1:
                                    pos[p[i], 2]++;
                                    break;
                                case 2:
                                    pos[p[i], 1]++;
                                    break;
                                default:
                                    pos[p[i], 2]--;
                                    break;
                            }
                            cells[(25 * p[i]) + pos[p[i], 1] + (5 * pos[p[i], 2])].material = lmats[1];
                            Debug.LogFormat("[A-button-ing Mazes #{0}] Moved to {1}{2} in the {3} maze.", moduleID, "ABCDE"[pos[p[i], 2]], pos[p[i], 1] + 1, new string[] { "Up", "Right", "Down", "Left" }[p[i]]);
                        }
                        if(Enumerable.Range(0, 4).All(x => pos[x, 0] == pos[x, 1] + (5 * pos[x, 2])))
                        {
                            moduleSolved = true;
                            module.HandlePass();
                            for (int i = 0; i < 4; i++)
                                cells[(25 * i) + pos[i, 0]].material = lmats[0];
                        }
                    }
                    else
                        module.HandleStrike();
                }
            };
        }
    }

    private IEnumerator Push(int d, bool down)
    {
        pressable[d] = !down;
        float e = (0.0561f - bpos[d].localPosition.y) / 0.0161f;
        float c = down ? 9 : -9;
        while(e <= 1 && e >= 0)
        {
            if (pressable[d] == down)
                yield break;
            e += c * Time.deltaTime;
            bpos[d].localPosition = new Vector3(0, Mathf.Lerp(0.0561f, 0.04f, e), Mathf.Lerp(0.0675f, 0.0613f, e));
            yield return null;
        }
        if(down)
        {
            bpos[d].localPosition = new Vector3(0, 0.04f, 0.0613f);
            col = Random.Range(0, 6);
            StartCoroutine(Flash(d, col));
        }
        else
            bpos[d].localPosition = new Vector3(0, 0.0561f, 0.0675f);
    }

    private IEnumerator Flash(int d, int r)
    {
        Debug.LogFormat("[A-button-ing Mazes #{0}] {1} selected. Displaying {2}.", moduleID, new string[] { "Up", "Right", "Down", "Left"}[d], new string[] { "Red", "Blue", "Yellow", "Magenta", "Cyan", "White"}[r]);
        hold = true;
        bulbs[d].material = lmats[3];
        bulbs[d].material.color = cols[r];
        for(int i = d * 4; i < (d * 4) + 4; i++)
        { 
            lights[i].enabled = true;
            lights[i].color = cols[r];
        }
        float e = 0;
        while (!pressable[d])
        {
            e += Time.deltaTime * Mathf.PI;
            float a = 20 * (1 - Mathf.Abs(Mathf.Sin(e)));
            foreach (Light l in lights)
                l.intensity = a;
            yield return null;
        }
        bulbs[d].material = lmats[0];
        for (int i = d * 4; i < (d * 4) + 4; i++)
            lights[i].enabled = false;
    }

    private bool Move(int m, int d)
    {
        Debug.Log(pos[m, 1] + ", " + pos[m, 2]);
        switch (d)
        {
            case 0:
                if(pos[m, 1] < 1 || mazes[m][2 * pos[m, 1]][(2 * pos[m, 2]) + 1] == '#')
                {
                    walls[(60 * m) + pos[m, 1] + (6 * pos[m, 2])].enabled = true;
                    Debug.LogFormat("[A-button-ing Mazes #{0}] Hit wall north of {1}{2} in the {3} maze.", moduleID, "ABCDE"[pos[m, 2]], pos[m, 1] + 1, new string[] { "Up", "Right", "Down", "Left"}[m]);
                    return false;
                }
                break;
            case 1:
                if (pos[m, 2] > 3 || mazes[m][(2 * pos[m, 1]) + 1][(2 * pos[m, 2]) + 2] == '#')
                {
                    walls[(60 * m) + 35 + pos[m, 1] + (5 * pos[m, 2])].enabled = true;
                    Debug.LogFormat("[A-button-ing Mazes #{0}] Hit wall east of {1}{2} in the {3} maze.", moduleID, "ABCDE"[pos[m, 2]], pos[m, 1] + 1, new string[] { "Up", "Right", "Down", "Left" }[m]);
                    return false;
                }
                break;
            case 2:
                if (pos[m, 1] > 3 || mazes[m][(2 * pos[m, 1]) + 2][(2 * pos[m, 2]) + 1] == '#')
                {
                    walls[(60 * m) + pos[m, 1] + (6 * pos[m, 2]) + 1].enabled = true;
                    Debug.LogFormat("[A-button-ing Mazes #{0}] Hit wall south of {1}{2} in the {3} maze.", moduleID, "ABCDE"[pos[m, 2]], pos[m, 1] + 1, new string[] { "Up", "Right", "Down", "Left" }[m]);
                    return false;
                }
                break;
            default:
                if (pos[m, 2] < 1 || mazes[m][(2 * pos[m, 1]) + 1][2 * pos[m, 2]] == '#')
                {
                    walls[(60 * m) + 30 + pos[m, 1] + (5 * pos[m, 2])].enabled = true;
                    Debug.LogFormat("[A-button-ing Mazes #{0}] Hit wall west of {1}{2} in the {3} maze.", moduleID, "ABCDE"[pos[m, 2]], pos[m, 1] + 1, new string[] { "Up", "Right", "Down", "Left" }[m]);
                    return false;
                }
                break;
        }
        return true;
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} U/L/D/R [Hold directional button.] | !{0} release # [Releases button at specified digit.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        if(command.Length == 1)
        {
            int h = "URDL".IndexOf(command);
            if(h < 0)
            {
                yield return "sendtochaterror \"" + command + "\" is not a valid direction.";
                yield break;
            }
            if (pressable.Any(x => !x))
            {
                yield return "sendtochaterror Button must be released before another is held.";
                yield break;
            }
            yield return null;
            buttons[h].OnInteract();
            yield break;
        }
        string[] commands = command.Split(' ');
        if(commands.Length == 2 && commands[0] == "RELEASE")
        {
            if (pressable.All(x => x))
            {
                yield return "sendtochaterror No held buttons.";
                yield break;
            }
            int d = 0;
            if (int.TryParse(commands[1], out d))
            { 
                if(d >= 0 && d < 6)
                {
                    int h = 0;
                    for(int i = 0; i < 4; i++)
                        if (!pressable[i])
                        {
                            h = i;
                            break;
                        }
                    yield return null;
                    while ((int)info.GetTime() % 10 != d && ((int)info.GetTime() % 60) / 10 != d)
                        yield return null;
                    buttons[h].OnInteractEnded();
                }
                else
                    yield return "sendtochaterror Digits must be in the range 0-5.";
            }
            else
                yield return "sendtochaterror \"" + commands[1] + "\" is not a valid digit.";
            yield break;
        }
        yield return "sendtochaterror \"" + command + "\"is invalid.";
    }
}
