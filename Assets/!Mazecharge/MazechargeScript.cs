using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class MazechargeScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> arrows;
    public Transform[] transistors;
    public Renderer[] trends;
    public Material[] tmats;
    public Transform bulb;
    public Renderer brend;
    public Material[] bmats;
    public Light blight;
    public Transform fluid;
    public GameObject[] walls;
    public TextMesh timer;
    public Renderer[] crends;
    public Material[] cmats;
    public Light[] clights;
    public GameObject matstore;

    private readonly string[,] mazes = new string[36, 9]
    {
        {
            "   X   X ",
            "XX X XXX ",
            "     X   ",
            " XXX X X ",
            " X     X ",
            "XXXXXX XX",
            "     X   ",
            "XX XXX X ",
            "       X ",
        },
        {
            " X X X   ",
            " X X XXX ",
            "     X   ",
            "XXXX XXX ",
            " X       ",
            " X X XXXX",
            "   X     ",
            "XX X XXX ",
            "   X   X ",
        },
        {
            "     X X ",
            "XX X X X ",
            " X X     ",
            " XXX X XX",
            "     X   ",
            "XX XXXXX ",
            "     X   ",
            " XXX X XX",
            " X   X   ",
        },
        {
            "   X     ",
            "XX X XXX ",
            " X   X X ",
            " XXX X XX",
            " X   X   ",
            " XXX X X ",
            "   X   X ",
            "XX XXX X ",
            "       X ",
        },
        {
            "     X   ",
            " XXX X X ",
            " X     X ",
            "XX X X XX",
            " X X X   ",
            " XXXXX X ",
            "       X ",
            " X XXXXX ",
            " X X     ",
        },
        {
            " X   X   ",
            " XXX XXX ",
            "         ",
            "XX XXX X ",
            "   X   X ",
            "XX XXXXX ",
            " X   X   ",
            " X X XXX ",
            "   X X   ",
        },
        {
            "   X   X ",
            " XXXXX X ",
            "   X   X ",
            "XX X XXX ",
            " X       ",
            " X XXXXX ",
            "       X ",
            " X XXX XX",
            " X   X   ",
        },
        {
            " X X   X ",
            " X XXX X ",
            "   X     ",
            "XX X XXXX",
            "       X ",
            " XXX X X ",
            " X   X   ",
            "XX X XXX ",
            "   X   X ",
        },
        {
            "   X     ",
            "XX XXX XX",
            "         ",
            "XXXX XXXX",
            "         ",
            "XX X XXX ",
            "   X   X ",
            " XXXXXXX ",
            "     X   ",
        },
        {
            "     X   ",
            " X XXXXX ",
            " X   X   ",
            "XX XXXXX ",
            " X       ",
            " XXX X X ",
            " X   X X ",
            " X X X X ",
            "   X X X ",
        },
        {
            "     X   ",
            "XXXX X X ",
            "       X ",
            "XX X XXXX",
            " X X     ",
            " XXX X X ",
            "     X X ",
            " X XXX X ",
            " X X   X ",
        },
        {
            "       X ",
            "XXXX XXX ",
            "   X X   ",
            " XXX X XX",
            "         ",
            "XXXX X XX",
            "     X   ",
            " XXX XXX ",
            " X     X ",
        },
        {
            "     X   ",
            " XXX XXX ",
            " X   X X ",
            "XXXX X X ",
            " X       ",
            " XXXXX X ",
            "       X ",
            " XXX X XX",
            " X   X   ",
        },
        {
            "     X   ",
            " XXX XXX ",
            " X       ",
            " X XXX XX",
            " X   X   ",
            "XX XXXXXX",
            "     X   ",
            " XXX XXX ",
            "   X     ",
        },
        {
            "   X     ",
            " X XXX XX",
            " X   X   ",
            "XXXX XXX ",
            "     X   ",
            " XXX X X ",
            " X     X ",
            " XXX X XX",
            "   X X   ",
        },
        {
            "     X X ",
            "XXXX X X ",
            "   X     ",
            "XX X XXX ",
            " X     X ",
            " XXXXX XX",
            "     X   ",
            "XXXX X X ",
            "       X ",
        },
        {
            "     X   ",
            " XXX X XX",
            " X     X ",
            "XX XXX X ",
            " X X     ",
            " XXX XXXX",
            "       X ",
            "XX XXX X ",
            "   X     ",
        },
        {
            "       X ",
            " X X XXX ",
            " X X     ",
            " XXXXX X ",
            " X X   X ",
            "XX X XXX ",
            "       X ",
            " X XXX XX",
            " X X     ",
        },
        {
            "   X     ",
            "XX XXX XX",
            " X   X   ",
            " XXX X X ",
            "   X   X ",
            "XX X XXXX",
            "   X     ",
            " X X XXX ",
            " X     X ",
        },
        {
            "   X   X ",
            " XXXXX X ",
            "       X ",
            " XXXXX X ",
            "   X     ",
            "XXXX X XX",
            "     X   ",
            " X XXXXX ",
            " X   X   ",
        },
        {
            " X X     ",
            " X XXX XX",
            "   X     ",
            " X XXXXX ",
            " X       ",
            "XXXXXX X ",
            "       X ",
            " XXX X XX",
            " X   X   ",
        },
        {
            " X X X   ",
            " X X X XX",
            "     X   ",
            "XX XXX XX",
            " X       ",
            " XXX XXX ",
            "       X ",
            " X XXXXX ",
            " X     X ",
        },
        {
            "   X     ",
            "XX XXX XX",
            " X X   X ",
            " X XXX X ",
            "         ",
            " XXXXX XX",
            " X X X   ",
            "XX X X X ",
            "       X ",
        },
        {
            "     X   ",
            " X X X X ",
            " X X   X ",
            "XXXXXX XX",
            "         ",
            "XX X XXX ",
            " X X   X ",
            " X X XXX ",
            "   X X   ",
        },
        {
            "   X     ",
            "XX X XXXX",
            "     X X ",
            "XXXX X X ",
            " X X     ",
            " X X XXX ",
            "     X   ",
            "XXXX XXX ",
            "     X   ",
        },
        {
            "     X   ",
            " X X XXX ",
            " X X   X ",
            "XX XXXXX ",
            " X   X   ",
            " XXX X X ",
            "       X ",
            " X XXX XX",
            " X X     ",
        },
        {
            "     X   ",
            "XXXX X XX",
            " X       ",
            " XXX XXX ",
            "       X ",
            "XX X X XX",
            "   X X X ",
            " XXX XXX ",
            " X       ",
        },
        {
            " X     X ",
            " XXX XXX ",
            "         ",
            "XX XXX XX",
            "     X X ",
            " XXXXX X ",
            " X       ",
            " X XXX XX",
            " X   X   ",
        },
        {
            "   X     ",
            "XX X XXX ",
            "       X ",
            "XX XXX XX",
            " X X     ",
            " XXXXX XX",
            " X       ",
            " X X XXXX",
            "   X     ",
        },
        {
            "   X     ",
            "XX XXXXX ",
            "       X ",
            "XX XXX X ",
            "   X     ",
            "XXXX X XX",
            "     X   ",
            " XXX XXX ",
            " X   X   ",
        },
        {
            "   X   X ",
            "XX X XXX ",
            " X       ",
            " XXXXX X ",
            "       X ",
            "XX XXX XX",
            " X X     ",
            " XXX X X ",
            "     X X ",
        },
        {
            " X     X ",
            " XXX XXX ",
            "   X X   ",
            "XX X X XX",
            "   X     ",
            "XX X X XX",
            "     X   ",
            " X XXX X ",
            " X X   X ",
        },
        {
            "   X     ",
            " X XXX XX",
            " X   X   ",
            "XX X X XX",
            " X X     ",
            " X XXX XX",
            "   X   X ",
            "XXXX X X ",
            "     X   ",
        },
        {
            "     X   ",
            " XXX XXX ",
            "   X     ",
            "XXXX X XX",
            "     X   ",
            "XX XXXXXX",
            "     X   ",
            " XXX XXX ",
            "   X     ",
        },
        {
            "     X X ",
            "XX XXX X ",
            "   X     ",
            "XX X XXX ",
            "     X   ",
            " XXX XXX ",
            " X   X X ",
            "XX XXX X ",
            "       X ",
        },
        {
            " X       ",
            " XXXXX XX",
            " X X X   ",
            " X X X X ",
            "       X ",
            "XXXX XXXX",
            "   X     ",
            "XX X X X ",
            "     X X ",
        },
    };
    private int loc = 12;
    private List<Vector2> targets = new List<Vector2> { };
    private float time = 60f;
    private int[] index = new int[2];
    private bool strikanim;
    private int[,] strgrid = new int[4, 25] { { -1, -1, -1, -1, -1, 4, 5, 6, 7, 8, 13, 14, 15, 16, 17, 22, 23, 24, 25, 26, 31, 32, 33, 34, 35 }, { -1, 0, 1, 2, 3, -1, 9, 10, 11, 12, -1, 18, 19, 20, 21, -1, 27, 28, 29, 30, -1, 36, 37, 38, 39 }, { 4, 5, 6, 7, 8, 13, 14, 15, 16, 17, 22, 23, 24, 25, 26, 31, 32, 33, 34, 35, -1, -1, -1, -1, -1 }, { 0, 1, 2, 3, -1, 9, 10, 11, 12, -1, 18, 19, 20, 21, -1, 27, 28, 29, 30, -1, 36, 37, 38, 39, -1 } };
    private int strikwall;
    private float dspeed = 1f;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        Generate();
        float scale = module.transform.lossyScale.x;
        blight.range *= scale;
        foreach (Light l in clights)
            l.range *= scale;
        StartCoroutine("Discharge");
        foreach(KMSelectable button in arrows)
        {
            int b = arrows.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved && !strikanim)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform);
                    button.AddInteractionPunch(0.25f);
                    switch (b)
                    {
                        case 0:
                            if (loc > 4)
                            {
                                if (mazes[index[1], ((loc / 5) * 2) - 1][(loc % 5) * 2] == 'X')
                                {
                                    StartCoroutine("StrikAnim", 0);
                                    Debug.LogFormat("[Mazecharge #{0}] Hit wall north of {1}{2}", moduleID, "ABCDE"[loc % 5], (loc / 5) + 1);
                                }
                                else
                                    StartCoroutine("MovAnim", 0);
                            }
                            break;
                        case 1:
                            if (loc % 5 > 0)
                            {
                                if (mazes[index[1], (loc / 5) * 2][((loc % 5) * 2) - 1] == 'X')
                                {
                                    StartCoroutine("StrikAnim", 1);
                                    Debug.LogFormat("[Mazecharge #{0}] Hit wall west of {1}{2}", moduleID, "ABCDE"[loc % 5], (loc / 5) + 1);
                                }
                                else
                                    StartCoroutine("MovAnim", 1);
                            }
                            break;
                        case 2:
                            if (loc < 20)
                            {
                                if (mazes[index[1], ((loc / 5) * 2) + 1][(loc % 5) * 2] == 'X')
                                {
                                    StartCoroutine("StrikAnim", 2);
                                    Debug.LogFormat("[Mazecharge #{0}] Hit wall south of {1}{2}", moduleID, "ABCDE"[loc % 5], (loc / 5) + 1);
                                }
                                else
                                    StartCoroutine("MovAnim", 2);
                            }
                            break;
                        default:
                            if (loc % 5 < 4)
                            {
                                if (mazes[index[1], (loc / 5) * 2][((loc % 5) * 2) + 1] == 'X')
                                {
                                    StartCoroutine("StrikAnim", 3);
                                    Debug.LogFormat("[Mazecharge #{0}] Hit wall east of {1}{2}", moduleID, "ABCDE"[loc % 5], (loc / 5) + 1);
                                }
                                else
                                    StartCoroutine("MovAnim", 3);
                            }
                            break;
                    }
                }
                return false;
            };
        }
        matstore.SetActive(false);
	}

    private void Generate()
    {
        index[1] = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(info.GetSerialNumber()[index[0]].ToString());
        int n = Mathf.CeilToInt(time / 20);
        for(int i = 0; i < n; i++)
        {
            int[] spaces = new int[25];
            spaces[loc] = 1;
            for (int k = 0; k < i; k++)
                spaces[(int)targets[k].x] = 1;
            for(int j = 1; j < 6; j++)
            {
                for (int k = 0; k < 25; k++)
                    if (spaces[k] == 0 && ((k > 5 && spaces[k - 5] == j && mazes[index[1], ((k / 5) * 2) - 1][(k % 5) * 2] != 'X') || (k % 5 > 0 && spaces[k - 1] == j && mazes[index[1], (k / 5) * 2][((k % 5) * 2) - 1] != 'X') || (k < 20 && spaces[k + 5] == j && mazes[index[1], ((k / 5) * 2) + 1][(k % 5) * 2] != 'X') || (k % 5 < 4 && spaces[k + 1] == j && mazes[index[1], (k / 5) * 2][((k % 5) * 2) + 1] != 'X')))
                        spaces[k] = j;
            }
            targets.Add(new Vector2(Enumerable.Range(0, 25).Where(x => spaces[x] == 0).PickRandom(), i == 0 ? 12f : Random.Range(7f, 15f)));
        }
        for (int i = 0; i < 25; i++)
            trends[i].material = tmats[targets.Select(x => (int)x.x).Contains(i) ? 0 : 1];
        string[,] lgrid = new string[11, 11];
        for(int i = 0; i < 11; i++)
            for(int j = 0; j < 11; j++)
            {
                if (i < 1 || i > 9 || j < 1 || j > 9)
                    lgrid[i, j] = "\u25a0";
                else if (i % 2 == 1 && j % 2 == 1)
                    lgrid[i, j] = targets.Select(x => x.x).Any(x => (((i - 1) / 2) * 5) + ((j - 1) / 2) == x) ? "O" : "I";
                else
                    lgrid[i, j] = mazes[index[1], i - 1][j - 1] == 'X' ? "\u25a0" : "\u25a1";
            }
        Debug.LogFormat("[Mazecharge #{0}] {2} seconds remain:\n[Mazecharge #{0}] {1}", moduleID, string.Join("\n[Mazecharge #" + moduleID + "] ", Enumerable.Range(0, 11).Select(x => string.Join("", Enumerable.Range(0, 11).Select(y => lgrid[x, y].ToString()).ToArray())).ToArray()), Mathf.CeilToInt(time));
    }

    private IEnumerator Discharge()
    {
        while (!moduleSolved)
        {
            float d = Time.deltaTime;
            if (!strikanim)
            {
                List<int> m = targets.Select(x => (int)x.x).ToList();
                if (m.Contains(loc))
                {
                    int i = m.IndexOf(loc);
                    targets[i] -= new Vector2(0, d);
                    time -= d;
                    brend.material = bmats[0];
                    blight.enabled = false;
                    if (time <= 0)
                    {
                        time = 0;
                        moduleSolved = true;
                    }
                    else if (targets[i].y <= 0)
                    {
                        targets.RemoveAt(i);
                        trends[loc].material = tmats[1];
                        if (!targets.Any())
                        {
                            index[0]++;
                            if(index[0] > 5)
                            {
                                index[0] = 0;
                                if (dspeed > 0.5f)
                                    dspeed -= 0.1f;
                            }
                            Generate();
                        }
                    }
                }
                else
                {
                    brend.material = bmats[1];
                    blight.enabled = true;
                    time += d * dspeed;
                    if (time >= 60)
                        time = 60;
                }
            }
            else
            {
                if(time < 60)
                {
                    time += d * 10;
                    float i = Random.Range(0, 15f);
                    blight.intensity = i;
                    walls[strikwall].SetActive(i < 5);
                }
                else
                {
                    module.HandleStrike();
                    time = 60;
                    brend.material = bmats[1];
                    blight.intensity = 5;
                    blight.color = new Color(1, 1, 1);
                    Generate();
                    strikanim = false;
                    brend.material = bmats[1];
                    blight.color = new Color(1, 1, 1);
                    walls[strikwall].SetActive(false);
                    yield return new WaitForSeconds(0.5f);
                }
            }
            int t = Mathf.CeilToInt(time);
            timer.text = (t < 10 ? "0" : "") + t.ToString();
            fluid.localPosition = new Vector3(0, -(time / 60), -0.1f);
            fluid.localScale = new Vector3(2, 2 - (time / 30), 0.15f);
            yield return null;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CapacitorPop, transform);
        brend.material = bmats[2];
        blight.color = new Color(0, 1, 0);
        for (int j = 0; j < 25; j++)
            trends[j].material = tmats[j == loc ? 1 : 0];
        module.HandlePass();
        for (int i = 0; i < 2; i++)
            crends[i].material = cmats[i];
        foreach (Light l in clights)
            l.enabled = true;
        float e = 1;
        while(e > 0)
        {
            float d = Time.deltaTime;
            e -= d * 4;
            foreach (Light l in clights)
                l.intensity *= 1 - d;
        }
        foreach (Light l in clights)
            l.enabled = false;
    }

    private IEnumerator MovAnim(int t)
    {
        int l = new int[] { loc - 5, loc - 1, loc + 5, loc + 1 }[t];
        Vector3 u = bulb.localPosition;
        Vector3 v = transistors[l].localPosition;
        float e = 0;
        while(e < 1)
        {
            float d = Time.deltaTime;
            e += d * 6;
            bulb.localPosition = new Vector3(Mathf.Lerp(u.x, v.x, e), Mathf.Lerp(u.y, v.y, e), u.z);
            yield return null;
        }
        bulb.localPosition = new Vector3(v.x, v.y, u.z);
        loc = l;
    }

    private IEnumerator StrikAnim(int t)
    {
        strikwall = strgrid[t, loc];
        targets.Clear();
        foreach (Renderer tr in trends)
            tr.material = tmats[1];
        int l = new int[] { loc - 5, loc - 1, loc + 5, loc + 1 }[t];
        Vector3 u = bulb.localPosition;
        Vector3 v = transistors[l].localPosition;
        float e = 0;
        while (e < 0.5f)
        {
            float d = Time.deltaTime;
            e += d * 6;
            bulb.localPosition = new Vector3(Mathf.Lerp(u.x, v.x, e), Mathf.Lerp(u.y, v.y, e), u.z);
            yield return null;
        }
        strikanim = true;
        e = 0.5f;
        bulb.localPosition = new Vector3(Mathf.Lerp(u.x, v.x, e), Mathf.Lerp(u.y, v.y, e), u.z);
        Audio.PlaySoundAtTransform("Strike", bulb);
        while (strikanim)
            yield return null;
        u = bulb.localPosition;
        v = transistors[loc].localPosition;
        while (e > 0)
        {
            float d = Time.deltaTime;
            e -= d * 6;
            bulb.localPosition = new Vector3(Mathf.Lerp(u.x, v.x, e), Mathf.Lerp(u.y, v.y, e), u.z);
            yield return null;
        }
        bulb.localPosition = new Vector3(v.x, v.y, u.z);
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} <U/R/D/L> [Moves one space in each direction. Chain without separators.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        List<int> dir = new List<int> { };
        for(int i = 0; i < command.Length; i++)
        {
            int d = "ULDR".IndexOf(command[i].ToString());
            if(d < 0)
            {
                yield return "sendtochaterror!f Invalid direction: " + command[i].ToString();
                yield return null;
            }
            dir.Add(d);
        }
        if (targets.Any(x => x.x == loc))
        {
            yield return "sendtochat Moving " + command + " once the light turns on.";
            while (targets.Any(x => x.x == loc))
            {
                yield return "trycancel";
                yield return null;
            }
        }
        for(int i = 0; i < command.Length; i++)
        {
            arrows[dir[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
