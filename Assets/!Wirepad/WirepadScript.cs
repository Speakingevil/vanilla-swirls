using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class WirepadScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> wires;
    public GameObject[] wobj;
    public Renderer[] wrends;
    public Material[] wmats;
    public TextMesh[] symbols;

    private readonly int[,] csel = new int[15, 4] { { 0, 1, 2, 3}, { 0, 1, 2, 4}, { 0, 1, 2, 5}, { 0, 1, 3, 4}, { 0, 1, 3, 5}, { 0, 1, 4, 5}, { 0, 2, 3, 4}, { 0, 2, 3, 5}, { 0, 2, 4, 5}, { 0, 3, 4, 5}, { 1, 2, 3, 4}, { 1, 2, 3, 5}, { 1, 2, 4, 5}, { 1, 3, 4, 5}, { 2, 3, 4, 5} };
    private readonly int[,] blocks = new int[14, 6] { { 7, 3, 5, 1, 2, 9 }, { 4, 7, 11, 6, 2, 8 }, { 1, 5, 0, 10, 3, 4 }, { 0, 7, 3, 8, 11, 5 }, { 5, 9, 6, 11, 8, 1 }, { 3, 10, 0, 2, 11, 9 }, { 8, 9, 1, 7, 0, 10 }, { 6, 10, 0, 2, 7, 5 }, { 2, 3, 8, 0, 6, 1 }, { 6, 9, 1, 2, 10, 4 }, { 1, 10, 3, 6, 7, 11 }, { 10, 7, 9, 5, 11, 4 }, { 7, 6, 3, 9, 4, 0 }, { 9, 8, 4, 0, 5, 2} };
    private readonly string[] symb = new string[12] { "\u0541", "\u054b", "\u0a35", "\u0a72", "\u0b07", "\u0b30", "\u0b95", "\u0b9a", "\u0a21", "\u0a2d", "\u0b37", "\u0b2f"};
    private readonly int[,][] cports = new int[3, 9][] { 
    { new int[] {0,3,4,8,9,10,11}, new int[] {0,1,3,5,6,8,9,11}, new int[] {3,5,7,9,10}, new int[] {0,2,8,9,10}, new int[] {3,4,6,7,10}, new int[] {0,2,3,4,7,8,9,10}, new int[] {0,2,3,6}, new int[] {1,2,7,9,11}, new int[] {1,2,4,5,6,8,9}},
    { new int[] {0,1,2,3,6,8,9,10}, new int[] {0,1,5,6,7,9,10,11}, new int[] {0,1,4,5,6,7,8,11}, new int[] {0,3,4,6,8,10}, new int[] {0,1,4,5,6,9}, new int[] {1,2,3,4,6,7,9}, new int[] {0,1,2,3,5,7,9,11}, new int[] {2,4,5,7,8,9,10}, new int[] {1,2,5,6,7,8,9}},
    { new int[] {1,5,6,7}, new int[] {3,5,7,9,11}, new int[] {0,2,3,6,7,9,10}, new int[] {0,1,2,6,8,9,10,11}, new int[] {3,4,5,6,7,8,10}, new int[] {2,3,5,7}, new int[] {0,2,3,4,6,7,8,11}, new int[] {2,3,4,5,6,7,9}, new int[] {0,3,5,6,8}},};
    private List<int>[] winfo = new List<int>[2] { new List<int> { -1, -1, -1 }, new List<int> { } };
    private List<int>[] cut = new List<int>[4] { new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }};
    private int panelog;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        int[] order = new int[] { 0, 1, 2 }.Shuffle().ToArray();
        int[] breaks = new int[] { Random.Range(0, 9), 0 };
        breaks[1] = Random.Range(breaks[0], 9);
        for (int i = 0; i < 9; i++)
            winfo[0].Add(i < breaks[0] ? order[0] : i < breaks[1] ? order[1] : order[2]);
        winfo[0].Shuffle();
        order.Shuffle();
        breaks[0] = Random.Range(0, 6);
        breaks[1] = Random.Range(breaks[0], 12);
        for (int i = 0; i < 12; i++)
            winfo[1].Add(i < breaks[0] ? order[0] : i < breaks[1] ? order[1] : order[2]);
        winfo[1].Shuffle();
    }

    private void Start()
    {
        int[] panelorder = new int[] { 0, 1, 2, 3}.Shuffle().ToArray();
        int[] col = new int[2] { Random.Range(0, 14), Random.Range(0, 15)};
        int[] w = new int[4];
        bool[] cinfo = new bool[2];
        cinfo[0] = info.GetSerialNumberNumbers().Sum() % 3 == 0;
        cinfo[1] = info.GetSerialNumberNumbers().Contains(0);
        for(int i = 0; i < 4; i++)
        {
            int t = panelorder[i];
            symbols[i].text = symb[blocks[col[0], csel[col[1], t]]];
            for(int j = t * 3; j < (t + 1) * 3; j++)
            {
                if(winfo[0][j] >= 0)
                {
                    int p = t * 3 + winfo[0][j];
                    int h = winfo[1][j];
                    bool c = cports[h, w[h]].Contains(p);
                    Debug.Log((c ? "Cut wire " : "Do not cut wire ") + (j + 1).ToString());
                    c ^= (cinfo[0] && w[3] % 3 == 2) || (cinfo[1] && info.GetSerialNumberNumbers().Contains(w[h] + 1)) || info.GetOnIndicators().Any(x => x.Any(y => y - 'A' == p));
                    if (cinfo[0] && w[3] % 3 == 2)
                        Debug.Log("Override 1 applies.");
                    if (info.GetOnIndicators().Any(x => x.Any(y => y - 'A' == p)))
                        Debug.Log("Override 2 applies.");
                    if (cinfo[1] && info.GetSerialNumberNumbers().Contains(w[h] + 1))
                        Debug.Log("Override 3 applies.");
                    if(c)
                      cut[i].Add(j);
                    w[h]++;
                    w[3]++;
                }
            }
        }
        foreach (GameObject wo in wobj)
            wo.SetActive(false);
        for (int i = 0; i < 12; i++)
            if (winfo[0][i] >= 0)
            {
                int j = (i * 3) + winfo[0][i];
                wobj[j].SetActive(true);
                int wi = winfo[1][i];
                wrends[j].material = wmats[wi];
                wrends[j + 36].material = wmats[wi];
            }
            else
                winfo[1][i] = -1;
        if (cut.All(x => !x.Any()))
            for (int i = 0; i < 3; i++)
                if (winfo[1].Contains(i))
                {
                    int[] q = new int[2];
                    for (int j = 0; j < 4; j++)
                    {
                        int t = panelorder[i] * 3;
                        for (int k = t; k < t + 3; k++)
                            if (winfo[1][k] == i)
                            {
                                q[0] = j;
                                q[1] = k;
                            }
                    }
                    cut[q[0]].Add(q[1]);
                }
        Debug.LogFormat("[Wirepad #{0}] The panels have the symbols: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 4).Select(x => symbols[x].text).ToArray()));
        for(int i = 0; i < 4; i++)
            if (cut[i].Any())
            {
                panelog = i;
                Debug.LogFormat("[Wirepad #{0}] First cut: {1}", moduleID, string.Join(", ", cut[i].Select(x => (x + 1).ToString()).ToArray()));
                break;
            }
        foreach (KMSelectable wire in wires)
        {
            int b = wires.IndexOf(wire);
            int p = b / 3;
            wire.OnInteract += delegate ()
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, wire.transform);
                wire.AddInteractionPunch(0.3f);
                wobj[b].SetActive(false);
                wobj[b + 36].SetActive(true);
                Debug.LogFormat("[Wirepad #{0}] Wire {1} cut.", moduleID, p + 1);
                if (moduleSolved)
                    module.HandleStrike();
                else
                {
                    if (!cut[panelog].Contains(p))
                        module.HandleStrike();
                    for (int i = 0; i < 4; i++)
                        if (cut[i].Contains(p))
                        {
                            cut[i].Remove(p);
                            break;
                        }
                    if (cut.All(x => !x.Any()))
                    {
                        moduleSolved = true;
                        Debug.LogFormat("[Wirepad #{0}] All required wires cut.", moduleID);
                        module.HandlePass();
                    }
                    else if (!cut[panelog].Any())
                    {
                        while (!cut[panelog].Any())
                            panelog++;
                        Debug.LogFormat("[Wirepad #{0}] Next cut: {1}", moduleID, string.Join(", ", cut[panelog].Select(x => (x + 1).ToString()).ToArray()));                        
                    }
                }
                return false;
            };
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} cut 1-12 [Cuts wire connected at the specified number.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if (commands[0] == "cut" && commands.Length > 1)
        {
            int d = -1;
            if(int.TryParse(commands[1], out d))
            {
                if(d >= 1 && d < 13)
                {
                    d--;
                    if (winfo[0][d] >= 0)
                    {
                        d = winfo[0][d] + d * 3;
                        yield return null;
                        wires[d].OnInteract();
                    }
                    else
                        yield return "sendtochaterror No uncut wires at position " + (d + 1).ToString() + ".";

                }
                else
                    yield return "sendtochaterror Invalid wire position.";
            }
            else
                yield return "sendtochaterror Invalid wire position.";
        }
        else
            yield return "sendtochaterror Invalid command.";
        yield break;
    }
}
