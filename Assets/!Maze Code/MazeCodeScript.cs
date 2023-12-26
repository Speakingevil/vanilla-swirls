using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeCodeScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> buttons;
    public Renderer bulb;
    public Material[] io;
    public Light flash;
    public Transform pointer;
    public TextMesh disp;

    private readonly string alph = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly string[][] mazes = new string[10][]
    {
        new string[13]{
            "#############",
            "#     #     #",
            "### ####### #",
            "#   #       #",
            "# ##### # ###",
            "# #   # #   #",
            "# # # # ### #",
            "#   # # #   #",
            "# # # ### # #",
            "# # # #   # #",
            "### # # # # #",
            "#   #   # # #",
            "#############",
        },
        new string[13]{
            "#############",
            "# #     #   #",
            "# # ##### # #",
            "# #   #   # #",
            "# # # # #####",
            "#   #   #   #",
            "##### ### # #",
            "#   #     # #",
            "### # # #####",
            "#   # # #   #",
            "# ##### ### #",
            "#           #",
            "#############",
        },
        new string[13]{
            "#############",
            "#   # #   # #",
            "# ### # ### #",
            "#     # #   #",
            "### ### # # #",
            "# #       # #",
            "# # ### # # #",
            "#   #   # # #",
            "# ### #######",
            "#   #   # # #",
            "# # # ### # #",
            "# # #       #",
            "#############",
        },
        new string[13]{
            "#############",
            "#     #     #",
            "# ##### #####",
            "#       #   #",
            "##### # ### #",
            "#   # #   # #",
            "### ##### # #",
            "# #   # #   #",
            "# # # # # # #",
            "# # #     # #",
            "# ### ##### #",
            "#     #     #",
            "#############",
        },
        new string[13]{
            "#############",
            "# #     #   #",
            "# # ##### # #",
            "#     # # # #",
            "# # # # # ###",
            "# # # #     #",
            "# # ### # ###",
            "# #     #   #",
            "### ### #####",
            "#   # #     #",
            "# ### # #####",
            "#     #     #",
            "#############",
        },
        new string[13]{
            "#############",
            "#   #       #",
            "# # ##### ###",
            "# #   #   # #",
            "# ### ### # #",
            "# # #   #   #",
            "### # # # # #",
            "# #   #   # #",
            "# # ### # # #",
            "# # # # # # #",
            "# # # # #####",
            "#     #     #",
            "#############",
        },
        new string[13]{
            "#############",
            "#   #       #",
            "# ##### ### #",
            "#         # #",
            "# ### ### # #",
            "#   #   # # #",
            "### ##### ###",
            "#   #   # # #",
            "# ##### ### #",
            "#   #     # #",
            "### ### ### #",
            "#           #",
            "#############",
        },
        new string[13]{
            "#############",
            "# #   #     #",
            "# # ### #####",
            "# # #   #   #",
            "# # # ##### #",
            "# #     #   #",
            "# # ### # ###",
            "#   # #   # #",
            "##### ### # #",
            "# # #     # #",
            "# # # # ### #",
            "#     #     #",
            "#############",
        },
        new string[13]{
            "#############",
            "#         # #",
            "##### ##### #",
            "#   #   #   #",
            "### # ### ###",
            "# # #   # # #",
            "# # # # # # #",
            "#   # #     #",
            "# ### ### # #",
            "#       # # #",
            "# ### # #####",
            "#   # #     #",
            "#############",
        },
        new string[13]{
            "#############",
            "#     #     #",
            "# # ### ### #",
            "# # #     # #",
            "### # ##### #",
            "#     # #   #",
            "####### # # #",
            "# #     # # #",
            "# ### ### ###",
            "#   # #     #",
            "# # # # ### #",
            "# #     #   #",
            "#############",
        }
    };
    private int length = 9;
    private int[,] grid = new int[6, 6];
    private char[] freq = new char[3];
    private int hz;
    private int deltahz = 1;
    private string[] codes = new string[] { "#", "#-#", "###", "#-#-#", "#-###", "###-#"};
    private string tx = "";
    private List<int> moves = new List<int> { };
    private bool movetick;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        codes = codes.Shuffle().Take(4).ToArray();
        Generate();
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved)
                {
                    button.AddInteractionPunch(0.2f);
                    switch (b)
                    {
                        case 0:
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                            if (hz - deltahz >= 0)
                            {
                                hz -= deltahz;
                                disp.text = "3." + alph[hz / 1296].ToString() + alph[(hz / 36) % 36].ToString() + alph[hz % 36].ToString();
                                if (!movetick)
                                    StartCoroutine("MoveTick");
                            }
                            break;
                        case 1:
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                            if (hz + deltahz < 12960)
                            {
                                hz += deltahz;
                                disp.text = "3." + alph[hz / 1296].ToString() + alph[(hz / 36) % 36].ToString() + alph[hz % 36].ToString();
                                if (!movetick)
                                    StartCoroutine("MoveTick");
                            }
                            break;
                        default:
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);                          
                            if (deltahz < 1296)
                                deltahz *= 6;
                            else
                            {
                                Debug.LogFormat("[Maze Code #{0}] Submitted a frequency of 3.{1}{2}{3} MHz.", moduleID, alph[hz / 1296], alph[(hz / 36) % 36], alph[hz % 36]);
                                if (disp.text.Skip(2).ToArray().SequenceEqual(freq))
                                {
                                    moduleSolved = true;
                                    module.HandlePass();
                                    Flicker(false);
                                }
                                else
                                {
                                    module.HandleStrike();
                                    deltahz = 1;
                                }
                            }
                            break;
                    }
                }
                return false;
            };
        }
    }

    private void Start()
    {
        float scale = module.transform.lossyScale.x;
        flash.range *= scale;
    }

    private void Generate()
    {
        for (int i = 0; i < 36; i++)
            grid[i / 6, i % 6] = -1;
        moves.Clear();
        int[] m = new int[3];
        m[0] = Random.Range(0, 10);
        m[2] = Random.Range(0, 36);
        grid[m[2] / 6, m[2] % 6] = 0;
        int iter = 0;
        while(Enumerable.Range(0, 36).Any(x => grid[x / 6, x % 6] < 0))
        {
            for(int i = 0; i < 36; i++)
            {
                if (grid[i / 6, i % 6] >= 0)
                    continue;
                if ((i % 6 > 0 && grid[i / 6, (i % 6) - 1] == iter && mazes[m[0]][(2 * (i / 6)) + 1][2 * (i % 6)] != '#') || (i > 5 && grid[(i / 6) - 1, i % 6] == iter && mazes[m[0]][2 * (i / 6)][(2 * (i % 6)) + 1] != '#') || (i % 6 < 5 && grid[i / 6, (i % 6) + 1] == iter && mazes[m[0]][(2 * (i / 6)) + 1][(2 * (i % 6)) + 2] != '#') || (i < 30 && grid[(i / 6) + 1, i % 6] == iter && mazes[m[0]][(2 * (i / 6)) + 2][(2 * (i % 6)) + 1] != '#'))
                    grid[i / 6, i % 6] = iter + 1;
            }
            iter++;
        }
        int[] mp = Enumerable.Range(0, 36).Where(x => grid[x / 6, x % 6] == length).ToArray();
        if(mp.Length < 1)
        {
            Generate();
            return;
        }
        m[1] = mp.PickRandom();
        for (int i = 0; i < 3; i++)
            freq[i] = alph[m[i]];
        int p = m[1];
        for(int i = 0; i < length; i++)
        {
            if(p % 6 > 0 && grid[p / 6, p % 6] - grid[p / 6, (p % 6) - 1] == 1 && mazes[m[0]][(2 * (p / 6)) + 1][2 * (p % 6)] != '#')
            {
                moves.Add(0);
                p--;
            }
            else if (p / 6 > 0 && grid[p / 6, p % 6] - grid[(p / 6) - 1, p % 6] == 1 && mazes[m[0]][2 * (p / 6)][(2 * (p % 6)) + 1] != '#')
            {
                moves.Add(1);
                p -= 6;
            }
            else if (p % 6 < 5 && grid[p / 6, p % 6] - grid[p / 6, (p % 6) + 1] == 1 && mazes[m[0]][(2 * (p / 6)) + 1][(2 * (p % 6)) + 2] != '#')
            {
                moves.Add(2);
                p++;
            }
            else
            {
                moves.Add(3);
                p += 6;
            }
        }
        if (!Check())
        {
            length++;
            Generate();
            return;
        }
        Debug.Log(string.Join("\n", Enumerable.Range(0, 6).Select(x => string.Join("", Enumerable.Range(0, 6).Select(y => alph[grid[x, y]].ToString()).ToArray())).ToArray()));
        tx = "-------" + string.Join("---", moves.Select(x => codes[x]).ToArray());
        StartCoroutine("Transmit");
        Debug.LogFormat("[Maze Code #{0}] The transmission is: {1}", moduleID, tx);
        Debug.LogFormat("[Maze Code #{0}] Using the assignment: {1}; the sequence of moves is {2}.", moduleID, string.Join(", ", Enumerable.Range(0, 4).Select(x => "LURD"[x].ToString() + " = " + codes[x]).ToArray()), string.Join("", moves.Select(x => "LURD"[x].ToString()).ToArray()));
        Debug.LogFormat("[Maze Code #{0}] Set the frequency to 3.{1} MHz.", moduleID, string.Join("", freq.Select(x => x.ToString()).ToArray()));
    }

    private bool Check()
    {
        string[] assignment = new string[8] { "LURD", "RULD", "LDRU", "RDLU", "ULDR", "DLUR", "URDL", "DRUL" };
        for(int i = 0; i < 8; i++)
        {
            string[] m = moves.Select(x => assignment[i][x].ToString()).ToArray();
            for (int j = 0; j < 10; j++)
                for (int k = 0; k < 36; k++)
                {
                    if (i == 0 && alph[j] == freq[0] && alph[k] == freq[1])
                        continue;
                    if (Path(mazes[j], k / 6, k % 6, m))
                        return false;
                }
        }
        return true;
    }

    private bool Path(string[] maze, int x, int y, string[] m)
    {
        for(int i = 0; i < m.Length; i++)
        {
            switch (m[i])
            {
                case "L":
                    if (maze[(2 * x) + 1][2 * y] == '#')
                        return false;
                    else
                        y--;
                    break;
                case "U":
                    if (maze[2 * x][(2 * y) + 1] == '#')
                        return false;
                    else
                        x--;
                    break;
                case "R":
                    if (maze[(2 * x) + 1][(2 * y) + 2] == '#')
                        return false;
                    else
                        y++;
                    break;
                default:
                    if (maze[(2 * x) + 2][(2 * y) + 1] == '#')
                        return false;
                    else
                        x++;
                    break;
            }
        }
        return true;
    }

    private IEnumerator Transmit()
    {
        int t = Random.Range(0, tx.Length);
        while (!moduleSolved)
        {
            Flicker(tx[t] == '#');
            yield return new WaitForSeconds(0.2f);
            t++;
            t %= tx.Length;
        }
    }

    private void Flicker(bool i)
    {
        flash.enabled = i;
        bulb.material = io[i ? 0 : 1];
    }

    private IEnumerator MoveTick()
    {
        movetick = true;
        float t = 4.547f - (9.094f * hz / 12960);
        while(Mathf.Abs(pointer.localPosition.x - t) > 0.03f)
        {
            t = 4.547f - (9.094f * hz / 12960);
            float d = Time.deltaTime;
            if (pointer.localPosition.x < t)
                pointer.localPosition += new Vector3(d * 3, 0, 0);
            else
                pointer.localPosition -= new Vector3(d * 3, 0, 0);
            yield return null;
        }
        pointer.localPosition = new Vector3(t, 0.183f, 0);
        movetick = false;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} +/- (1-9) [Increases/decreases frequency by the product of the sensitivity and the given amount.] | !{0} tx [Increases sensitivity and submits displayed frequency.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if(command.ToLowerInvariant() == "tx")
        {
            yield return null;
            buttons[2].OnInteract();
            yield break;
        }
        string[] commands = command.ToLowerInvariant().Split(' ');
        if (commands.Length != 2)
        {
            yield return "sendtochaterror!f Invalid command length.";
            yield break;
        }
        if(commands[0] == "+" || commands[0] == "-")
        {
            int d = 0;
            if (int.TryParse(commands[1], out d))
            {
                if(d < 1)
                {
                    yield return "sendtochaterror!f Given amount must be positive.";
                    yield break;
                }
                if(d > 9)
                {
                    yield return "sendtochaterror!f Given amount must be a single digit. Increase sensitivity to shift by a greater amount.";
                    yield break;
                }
                int b = commands[0] == "+" ? 1 : 0;
                for(int i = 0; i < d; i++)
                {
                    yield return null;
                    buttons[b].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                yield break;
            }
            else
            {
                yield return "sendtochaterror!f Invalid digit.";
                yield break;
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        deltahz = 1;
        int b = hz % 6 < alph.IndexOf(freq[2]) % 6 ? 1 : 0;
        while (hz % 6 != alph.IndexOf(freq[2]) % 6)
        {
            yield return null;
            buttons[b].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
        buttons[2].OnInteract();
        yield return new WaitForSeconds(0.1f);
        b = (hz / 6) % 6 < alph.IndexOf(freq[2]) / 6 ? 1 : 0;
        while((hz / 6) % 6 != alph.IndexOf(freq[2]) / 6)
        {
            yield return null;
            buttons[b].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
        buttons[2].OnInteract();
        yield return new WaitForSeconds(0.1f);
        b = (hz / 36) % 6 < alph.IndexOf(freq[1]) % 6 ? 1 : 0;
        while((hz / 36) % 6 != alph.IndexOf(freq[1]) % 6)
        {
            yield return null;
            buttons[b].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
        buttons[2].OnInteract();
        yield return new WaitForSeconds(0.1f);
        b = (hz / 216) % 6 < alph.IndexOf(freq[1]) / 6 ? 1 : 0;
        while((hz / 216) % 6 != alph.IndexOf(freq[1]) / 6)
        {
            yield return null;
            buttons[b].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
        buttons[2].OnInteract();
        yield return new WaitForSeconds(0.1f);
        b = hz / 1296 < freq[0] - '0' ? 1 : 0;
        while(hz / 1296 != freq[0] - '0')
        {
            yield return null;
            buttons[b].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
        buttons[2].OnInteract();
    }
}
