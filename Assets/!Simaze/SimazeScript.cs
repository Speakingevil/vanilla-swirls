using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class SimazeScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Light[] lights;
    public Renderer[] walls;
    public Renderer[] markers;
    public Material[] marmats;
    public Transform[] mrots;

    private readonly string[] collog = new string[] { "Blue", "Yellow", "Green", "Red"};
    private readonly string[] dirlog = new string[] { "Up", "Right", "Down", "Left" };
    private readonly int[,,] colarr = new int[18, 4, 4] { { { 29, 25, 12, 2 }, { 8, 6, 19, 35 }, { 1, 0, 30, 16 }, { 14, 20, 31, 3 } }, { { 20, 27, 23, 5 }, { 10, 17, 13, 7 }, { 8, 2, 29, 15 }, { 25, 4, 12, 1 } }, { { 1, 17, 31, 16 }, { 5, 14, 27, 34 }, { 20, 21, 10, 6 }, { 22, 7, 29, 30 } }, { { 33, 9, 30, 14 }, { 31, 20, 11, 4 }, { 25, 12, 18, 27 }, { 0, 5, 24, 28 } }, { { 10, 4, 15, 21 }, { 26, 0, 23, 18 }, { 35, 24, 9, 14 }, { 6, 8, 16, 17 } }, { { 28, 22, 13, 35 }, { 2, 25, 29, 24 }, { 34, 11, 33, 32 }, { 10, 19, 21, 9 } }, { { 6, 32, 7, 24 }, { 15, 12, 3, 9 }, { 22, 26, 17, 31 }, { 2, 11, 34, 18 } }, { { 8, 3, 19, 0 }, { 32, 28, 21, 33 }, { 5, 23, 13, 4 }, { 35, 26, 27, 15 } }, { { 26, 34, 18, 11 }, { 22, 16, 30, 1 }, { 28, 19, 7, 3 }, { 13, 32, 33, 23 } }, { { 26, 13, 9, 12 }, { 14, 6, 7, 35 }, { 18, 19, 30, 34 }, { 1, 2, 23, 17 } }, { { 1, 19, 28, 15 }, { 31, 24, 17, 26 }, { 29, 22, 20, 21 }, { 9, 14, 27, 6 } }, { { 23, 16, 4, 27 }, { 10, 20, 13, 8 }, { 17, 11, 2, 33 }, { 22, 35, 15, 21 } }, { { 17, 24, 25, 30 }, { 2, 18, 11, 16 }, { 12, 35, 0, 28 }, { 26, 13, 4, 20 } }, { { 20, 22, 34, 0 }, { 19, 5, 21, 4 }, { 32, 9, 8, 25 }, { 11, 30, 31, 12 } }, { { 29, 5, 7, 14 }, { 28, 27, 3, 0 }, { 23, 15, 4, 13 }, { 8, 32, 18, 25 } }, { { 33, 31, 3, 35 }, { 25, 12, 32, 22 }, { 6, 10, 5, 1 }, { 19, 7, 16, 0 } }, { { 21, 32, 10, 18 }, { 30, 9, 29, 33 }, { 16, 31, 24, 14 }, { 34, 3, 28, 5 } }, { { 8, 2, 11, 6 }, { 1, 34, 23, 15 }, { 27, 3, 7, 26 }, { 10, 24, 29, 33 } } };
    private readonly int[,,] flasharr = new int[6, 2, 6] { { { 5, 1, 2, 0, 4, 3 }, { 0, 2, 1, 4, 3, 5 } }, { { 4, 5, 2, 1, 3, 0 }, { 3, 5, 4, 0, 2, 1 } }, { { 2, 0, 1, 4, 3, 5 }, { 1, 5, 0, 2, 3, 4 } }, { { 3, 4, 1, 2, 0, 5 }, { 2, 5, 4, 0, 3, 1 } }, { { 2, 5, 1, 3, 4, 0 }, { 2, 0, 3, 5, 1, 4 } }, { { 1, 2, 3, 0, 4, 5 }, { 1, 3, 4, 0, 5, 2 } } };
    private string[][] maze = new string[11][]
    {
        new string[11] { "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[11] { "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[11] { "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[11] { "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[11] { "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[11] { "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[11] { "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[11] { "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[11] { "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[11] { "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[11] { "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
    };
    private int[] points = new int[6];
    private string[] fullseq = new string[5];
    private List<int>[] dirseq = new List<int>[5];
    private List<int>[] colseq = new List<int>[5];
    private List<int>[] pressseq = new List<int>[5];
    private int[,] flashseq = new int[12,3];
    private int[] stage = new int[2];
    private IEnumerator flash;
    private bool sound;
    private bool wait;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        maze[2 * Random.Range(0, 6)][2 * Random.Range(0, 6)] = "+";
        while (maze.Any(r => r.Contains("-")))
        {
            int[] select = new int[2];
            for (int i = 0; i < 2; i++)
                select[i] = Random.Range(0, 6);
            while (maze[2 * select[0]][2 * select[1]] != "+")
            {
                for (int i = 0; i < 2; i++)
                    select[i] = Random.Range(0, 6);
            }
            int del = Random.Range(0, 4);
            while ((del == 0 && (select[1] == 0 || maze[2 * select[0]][(2 * select[1]) - 1] != "X" || maze[2 * select[0]][(2 * select[1]) - 2] != "-")) || (del == 1 && (select[0] == 0 || maze[(2 * select[0]) - 1][2 * select[1]] != "X" || maze[(2 * select[0]) - 2][2 * select[1]] != "-")) || (del == 2 && (select[1] == 5 || maze[2 * select[0]][(2 * select[1]) + 1] != "X" || maze[2 * select[0]][(2 * select[1]) + 2] != "-")) || (del == 3 && (select[0] == 5 || maze[(2 * select[0]) + 1][2 * select[1]] != "X" || maze[(2 * select[0]) + 2][2 * select[1]] != "-")))
                del = Random.Range(0, 4);
            switch (del)
            {
                case 0:
                    maze[2 * select[0]][(2 * select[1]) - 1] = "/";
                    maze[2 * select[0]][(2 * select[1]) - 2] = "+";
                    break;
                case 1:
                    maze[(2 * select[0]) - 1][2 * select[1]] = "/";
                    maze[(2 * select[0]) - 2][2 * select[1]] = "+";
                    break;
                case 2:
                    maze[2 * select[0]][(2 * select[1]) + 1] = "/";
                    maze[2 * select[0]][(2 * select[1]) + 2] = "+";
                    break;
                case 3:
                    maze[(2 * select[0]) + 1][2 * select[1]] = "/";
                    maze[(2 * select[0]) + 2][2 * select[1]] = "+";
                    break;
            }
            for (int i = 0; i < 11; i += 2)
                for (int j = 0; j < 11; j += 2)
                    if (maze[i][j] == "+" && (j == 0 || maze[i][j - 2] != "-") && (i == 0 || maze[i - 2][j] != "-") && (j == 10 || maze[i][j + 2] != "-") && (i == 10 || maze[i + 2][j] != "-"))
                        maze[i][j] = "o";
        }
        maze = maze.Select(i => i.Select(j => "+o/".Contains(j) ? " " : "X").ToArray()).ToArray();
        int msel = Random.Range(0, 18);
        int[] mar = new int[4] { 0, 1, 2, 3 };
        int[] marsel = new int[2];
        do
        {
            mar = mar.Shuffle().ToArray();
            for (int i = 0; i < 2; i++)
                marsel[i] = colarr[msel, mar[i], Random.Range(0, 4)];
        } while (Enumerable.Range(0, 18).Count(x => marsel.Count(y => Enumerable.Range(0, 4).Select(z => colarr[x, mar[0], z]).Contains(y)) > 1) != 1);
        for (int i = 0; i < 30; i++)
        {
            if (maze[2 * (i / 5)][(2 * (i % 5)) + 1] == " ")
                walls[i].enabled = false;
            if (maze[(2 * (i / 6)) + 1][2 * (i % 6)] == " ")
                walls[i + 30].enabled = false;
        }
        foreach (Renderer m in markers)
            m.enabled = false;
        for (int i = 0; i < 2; i++)
        {
            int j = marsel[i];
            markers[j].enabled = true;
            markers[j].material = marmats[mar[i]];
            mrots[j].transform.localEulerAngles = new Vector3(90 * Random.Range(0, 4), 90, 90);
        }
        Debug.LogFormat("[Simaze #{0}] The marked spaces are: {1} at {2}{3}, and {4} at {5}{6}", moduleID, collog[mar[0]], "ABCDEF"[marsel[0] % 6], (marsel[0] / 6) + 1, collog[mar[1]], "ABCDEF"[marsel[1] % 6], (marsel[1] / 6) + 1);
        List<int> av = Enumerable.Range(0, 36).ToList();
        for (int i = 0; i < 16; i++)
            av.Remove(colarr[msel, i / 4, i % 4]);
        List<int> dead = new List<int> { };
        for (int i = 0; i < 36; i++)
        {
            int w = 0;
            if (i < 6 || maze[(2 * (i / 6)) - 1][2 * (i % 6)] == "X")
                w++;
            if (i % 6 < 1 || maze[2 * (i / 6)][(2 * (i % 6)) - 1] == "X")
                w++;
            if (i > 29 || maze[(2 * (i / 6)) + 1][2 * (i % 6)] == "X")
                w++;
            if (i % 6 > 4 || maze[2 * (i / 6)][(2 * (i % 6)) + 1] == "X")
                w++;
            if (w > 2)
                dead.Add(i);
        }
        if (av.Any(x => dead.Contains(x)))
            points[0] = Enumerable.Range(0, 36).Where(x => av.Contains(x) && dead.Contains(x)).PickRandom();
        else
            points[0] = av.PickRandom();
        for(int i = 0; i < 5; i++)
        {
            av.Remove(points[i]);
            int[][] grid = new int[6][];
            for (int j = 0; j < 6; j++)
                grid[j] = new int[6];
            int p = 1;
            grid[points[i] / 6][points[i] % 6] = 1;
            while(grid.Any(x => x.Any(y => y < 1)))
            {
                for (int j = 0; j < 36; j++)
                    if (grid[j / 6][j % 6] < 1 && ((j > 5 && grid[(j / 6) - 1][j % 6] == p && maze[(2 * (j / 6)) - 1][2 * (j % 6)] == " ") || (j % 6 > 0 && grid[j / 6][(j % 6) - 1] == p && maze[2 * (j / 6)][(2 * (j % 6)) - 1] == " ") || (j < 30 && grid[(j / 6) + 1][j % 6] == p && maze[(2 * (j / 6)) + 1][2 * (j % 6)] == " ") || (j % 6 < 5 && grid[j / 6][(j % 6) + 1] == p && maze[2 * (j / 6)][(2 * (j % 6)) + 1] == " ")))
                        grid[j / 6][j % 6] = p + 1;
                p++;
            }
            int[] far = Enumerable.Range(0, 36).Where(x => grid[x / 6][x % 6] >= Mathf.Min((2 * i) + 7, grid.Max(y => y.Max()) - 1)).ToArray();
            if(av.Any(x => far.Contains(x)))
            {
                if (av.Any(x => far.Contains(x) && dead.Contains(x)))
                    points[i + 1] = Enumerable.Range(0, 36).Where(x => av.Contains(x) && dead.Contains(x) && far.Contains(x)).PickRandom();
                else
                    points[i + 1] = Enumerable.Range(0, 36).Where(x => av.Contains(x) && far.Contains(x)).PickRandom();
            }
            else if (av.Any(x => dead.Contains(x)))
                points[i + 1] = Enumerable.Range(0, 36).Where(x => av.Contains(x) && dead.Contains(x)).PickRandom();
            else
                points[i + 1] = av.PickRandom();
            av.Remove(points[i + 1]);
            int c = points[i + 1];
            int d = 0;
            List<int> dr = new List<int> { };
            List<int> cl = new List<int> { };
            while (c != points[i])
            {
                int q = grid[c / 6][c % 6] - 1;
                if (c % 6 > 0 && grid[c / 6][(c % 6) - 1] == q && maze[2 * (c / 6)][(2 * (c % 6)) - 1] == " ")
                    d = 1;
                else if (c > 5 && grid[(c / 6) - 1][c % 6] == q && maze[(2 * (c / 6)) - 1][2 * (c % 6)] == " ")
                    d = 2;
                else if (c % 6 < 5 && grid[c / 6][(c % 6) + 1] == q && maze[2 * (c / 6)][(2 * (c % 6)) + 1] == " ")
                    d = 3;
                else
                    d = 0;
                fullseq[i] = "URDL"[d].ToString() + fullseq[i];
                if(Enumerable.Range(0, 16).Select(x => colarr[msel, x / 4, x % 4]).Contains(c))
                {
                    dr.Insert(0, d);
                    for(int j = 0; j < 4; j++)
                        if(Enumerable.Range(0, 4).Select(x => colarr[msel, j, x]).Contains(c))
                        {
                            cl.Insert(0, j);
                            break;
                        }
                }
                switch (d)
                {
                    case 1: c--; break;
                    case 2: c -= 6; break;
                    case 3: c++; break;
                    default: c += 6; break;
                }
            }
            if (dr.Any())
            {
                dirseq[i] = dr;
                colseq[i] = cl;
                List<int> pr = new List<int> { };
                for (int j = 0; j < i; j++)
                    for (int k = 0; k < colseq[j].Count(); k++)
                        pr.Add(colseq[j][k]);
                for (int k = 0; k < dirseq[i].Count(); k++)
                    pr.Add(dirseq[i][k]);
                pressseq[i] = pr;
            }
            else
                i--;
        }
        for (int i = 0; i < 121; i++)
            maze[i / 11][i % 11] = maze[i / 11][i % 11] == "X" ? "\u25a0" : "\u25a1";
        for (int i = 0; i < 16; i++)
        {
            int p = colarr[msel, i / 4, i % 4];
            maze[2 * (p / 6)][2 * (p % 6)] = "BYGR"[i / 4].ToString();
        }
        Debug.LogFormat("[Simaze #{0}] The complete maze:\n[Simaze #{0}] \u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\n[Simaze #{0}] {1}\n[Simaze #{0}] \u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0\u25a0", moduleID, string.Join("\n[Simaze #" + moduleID + "] ", maze.Select(x => "\u25a0" + string.Join("", x) + "\u25a0").ToArray()));
        module.OnActivate = Activate;
        flash = Flash();
    }

    private void Activate()
    {
        float scale = module.transform.lossyScale.x;
        foreach (Light l in lights)
            l.range *= scale;
        int ft = info.GetSerialNumber().Any(x => "AEIOU".Contains(x)) ? 1 : 4;
        if (info.GetOffIndicators().Count() > info.GetOnIndicators().Count())
            ft--;
        else if (info.GetOffIndicators().Count() < info.GetOnIndicators().Count())
            ft++;
        for (int i = 0; i < 6; i++)
        {
            int p = points[i];
            flashseq[2 * i, 0] = flasharr[ft, 0, p / 6];
            flashseq[(2 * i) + 1, 0] = flasharr[ft, 1, p % 6];
            for (int j = 2 * i; j < (2 * i) + 2; j++)
            {
                int f = flashseq[j, 0];
                flashseq[j, 1] = new int[] { 0, 0, 0, 1, 1, 2 }[f];
                flashseq[j, 2] = new int[] { 1, 2, 3, 2, 3, 3 }[f];
            }
        }
        StartCoroutine(flash);
        Debug.LogFormat("[Simaze #{0}] The sequence flashes: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 4).Select(x => collog[flashseq[x, 1]] + " & " + collog[flashseq[x, 2]]).ToArray()));
        Debug.LogFormat("[Simaze #{0}] {1}{2} \u2192 {3} \u2192 {4}{5}", moduleID, "ABCDEF"[points[0] % 6], (points[0] / 6) + 1, fullseq[0], "ABCDEF"[points[1] % 6], (points[1] / 6) + 1);
        Debug.LogFormat("[Simaze #{0}] Press the buttons: {1}", moduleID, string.Join(", ", dirseq[0].Select(x => dirlog[x]).ToArray()));
        foreach (KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                sound = true;
                Audio.PlaySoundAtTransform("Beep" + (b + 1), button.transform);
                button.AddInteractionPunch(0.3f);
                if (!moduleSolved)
                {
                    StopCoroutine(flash);
                    if (wait)
                        StopCoroutine("Wait");
                    StartCoroutine("Wait");
                    StartCoroutine("Press", b);
                    if (pressseq[stage[0]][stage[1]] == b)
                    {
                        stage[1]++;
                        if (stage[1] >= pressseq[stage[0]].Count())
                        {
                            if (stage[0] > 3 || stage[1] > 11)
                            {
                                moduleSolved = true;
                                module.HandlePass();
                            }
                            else
                            {
                                stage[0]++;
                                stage[1] = 0;
                                Debug.LogFormat("[Simaze #{0}] {1} & {2}, and {3} & {4} flash next.", moduleID, collog[flashseq[(2 * stage[0]) + 2, 1]], collog[flashseq[(2 * stage[0]) + 2, 2]], collog[flashseq[(2 * stage[0]) + 3, 1]], collog[flashseq[(2 * stage[0]) + 3, 2]]);
                                Debug.LogFormat("[Simaze #{0}] {1}{2} \u2192 {3} \u2192 {4}{5}", moduleID, "ABCDEF"[points[stage[0]] % 6], (points[stage[0]] / 6) + 1, fullseq[stage[0]], "ABCDEF"[points[stage[0] + 1] % 6], (points[stage[0] + 1] / 6) + 1);
                                Debug.LogFormat("[Simaze #{0}] Press the buttons: {1}. Then press: {2}", moduleID, string.Join(", ", Enumerable.Range(0, stage[0]).Select(x => string.Join(", ", colseq[x].Select(y => collog[y]).ToArray())).ToArray()), string.Join(", ", dirseq[stage[0]].Select(x => dirlog[x]).ToArray()));
                            }
                        }
                    }
                    else
                    {
                        module.HandleStrike();
                        stage[1] = 0;
                    }
                }
                return false;
            };
        }
    }

    private IEnumerator Flash()
    {
        int i = 0;
        while (!moduleSolved)
        {
            if (sound)
                Audio.PlaySoundAtTransform("Beep" + flashseq[i, 0], transform);
            lights[flashseq[i, 1]].enabled = true;
            lights[flashseq[i, 2]].enabled = true;
            yield return new WaitForSeconds(0.75f);
            foreach (Light l in lights)
                l.enabled = false;
            yield return new WaitForSeconds(0.375f);
            i++;
            if (i >= (stage[0] * 2) + 4)
            {
                i = 0;
                yield return new WaitForSeconds(1.025f);
            }
        }
    }

    private IEnumerator Wait()
    {
        wait = true;
        yield return new WaitForSeconds(3.075f);
        if (!moduleSolved)
            StartCoroutine(flash);
        wait = false;
        stage[1] = 0;
    }

    private IEnumerator Press(int b)
    {
        foreach (Light l in lights)
            l.enabled = false;
        lights[b].enabled = true;
        yield return new WaitForSeconds(0.3f);
        lights[b].enabled = false;
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} <B/Y/G/R> [Presses buttons, by colour, in order specified. Chain without separators.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        List<int> p = new List<int> { };
        for (int i = 0; i < command.Length; i++)
        {
            p.Add("BYGR".IndexOf(command[i].ToString()));
            if (p.Last() < 0)
            {
                yield return "sendtochaterror!f \"" + command[i].ToString() + "\" is an invalid colour.";
                yield break;
            }
        }
        yield return null;
        for(int i = 0; i < command.Length; i++)
        {
            buttons[p[i]].OnInteract();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
