using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SequentialButtonScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> wires;
    public KMSelectable cover;
    public KMSelectable button;
    public GameObject[] wobjs;
    public Transform[] hatch;
    public Transform[] cpos;
    public Transform bpos;
    public Renderer[] wrends;
    public Renderer brend;
    public Renderer[] lrends;
    public Material[] wmats;
    public Material[] bmats;
    public Material[] lmats;
    public Light[] lights;
    public TextMesh[] labels;
    public GameObject matstore;

    private readonly string[] blabels = new string[4] { "ABORT", "PUSH", "HOLD", "DETONATE"};
    private readonly string[] collog = new string[5] { "Red", "Blue", "Yellow", "White", "Black" };
    private readonly Color[] cols = new Color[4] { new Color(1, 0, 0), new Color(0, 0, 1), new Color(0.78f, 0.6f, 0), new Color(0.55f, 0.55f, 0.55f)};
    private readonly int[,] locc = new int[4, 16] { { 2, 7, 11, 13, 16, 22, 28, 31, 38, 41, 46, 48, 51, 53, 59, 63 }, { 4, 8, 10, 12, 14, 17, 32, 35, 40, 44, 47, 49, 52, 56, 57, 61 }, { 1, 5, 18, 20, 21, 24, 25, 26, 27, 29, 37, 39, 43, 45, 60, 64 }, { 3, 6, 9, 15, 19, 23, 30, 33, 34, 36, 42, 50, 54, 55, 58, 62 } };
    private int[] warrange = new int[36];
    private bool[] wcut = new bool[36];
    private int[] lcols = new int[16];
    private int[] cut = new int[4];
    private int[] rel = new int[4];
    private int[] binfo = new int[2];
    private int stage;
    private int panelind;
    private bool hold;
    private bool pressable;
    private string[] logs = new string[4];
    private KMAudio.KMAudioRef mechanism;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        float scale = module.transform.lossyScale.x;
        foreach (Light l in lights)
            l.range *= scale;
        matstore.SetActive(false);
        binfo[0] = Random.Range(0, 5);
        binfo[1] = Random.Range(0, 4);
        brend.material = bmats[binfo[0]];
        labels[3].text = blabels[binfo[1]];
        labels[3].fontSize = 1600 / blabels[binfo[1]].Length;
        if (binfo[0] == 4)
            labels[3].color = new Color(1, 1, 1);
        Debug.LogFormat("[Sequential Button #{0}] The button is {1} and labelled \"{2}\".", moduleID, collog[binfo[0]], blabels[binfo[1]]);
        List<int> choose = Enumerable.Range(4, 95).Select(x => (int)Mathf.Sqrt(x)).ToList();
        for (int i = 0; i < 4; i++)
        {
            int wirenum = choose.PickRandom();
            choose.RemoveAll(x => x == wirenum);
            warrange[9 * i] = Random.Range(0, 5);
            for(int j = 1; j < 9; j++)
            {
                List<int> c = new List<int> { 0, 1, 2, 3, 4 };
                for (int k = 0; k < j / 3; k++)
                    c.Remove(warrange[(9 * i) + (k * 3) + (j % 3)]);
                for (int k = 0; k < j % 3; k++)
                    c.Remove(warrange[(9 * i) + ((j / 3) * 3) + (k % 3)]);
                warrange[(9 * i) + j] = c.PickRandom();
            }
            int[] gaps = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }.Shuffle().Take(9 - wirenum).ToArray();
            foreach (int w in gaps)
                warrange[(9 * i) + w] = -1;
            List<int> panel = warrange.Skip(9 * i).Take(9).ToList();
            logs[i] = string.Format("[Sequential Button #{0}] Stage {1}:\n[Sequential Button #{0}] {2} wires: ", moduleID, i + 1, wirenum);
            logs[i] += string.Join(", ", Enumerable.Range(0, 9).Where(x => warrange[(9 * i) + x] >= 0).Select(x => ((x / 3) + 1).ToString() + "-" + "ABC"[x % 3] + " is " + collog[warrange[(9 * i) + x]]).ToArray());
            Vector2Int cinfo = Panelcut(panel, wirenum);
            cut[i] = cinfo.y;
            Debug.Log(wirenum + ", " + cinfo.x + ", " + cinfo.y);
            logs[i] += string.Format("\n[Sequential Button #{0}] The {1} rule is satisfied; cut wire {2}-{3}.", moduleID, new string[] { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "nineth"}[(int)cinfo.x], (3 * i) + (cinfo.y / 3) + 1, "ABC"[cinfo.y % 3]);
        }
        do
        {
            for (int i = 0; i < 16; i++)
                lcols[i] = Random.Range(0, 4);
            for (int i = 0; i < 4; i++)
            {
                rel[i] = 0;
                int[] plcols = lcols.Take(4 * (i + 1)).ToArray();
                for (int j = 0; j < 4; j++)
                    if (plcols.Any(x => x == j))
                        rel[i] += locc[j, plcols.Count(x => x == j) - 1];
            }
        } while (rel.Any(x => x / 10 > 5 && x % 10 > 5));
        for (int i = 0; i < 4; i++)
        {
            logs[i] += string.Format("\n[Sequential Button #{0}] The colours of the lights are: {1}.", moduleID, string.Join(", ", lcols.Skip(4 * i).Take(4).Select(x => collog[x]).ToArray()));
            logs[i] += string.Format("\n[Sequential Button #{0}] Release the button when the timer contains the digits: {1}.", moduleID, string.Join(" & ", rel[i].ToString().Select(x => x.ToString()).Distinct().ToArray()));
        }
        Panelset(0);
        Debug.Log(logs[0]);
        module.OnActivate += delegate () { mechanism = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform); StartCoroutine(Hatchmove(true, true)); };
        foreach(KMSelectable wire in wires)
        {
            int w = wires.IndexOf(wire);
            wire.OnInteract += delegate ()
            {
                if (!moduleSolved && pressable)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, wobjs[w].transform);
                    wire.AddInteractionPunch(0.1f);
                    Debug.LogFormat("[Sequential Button #{0}] Wire {1}-{2} cut.", moduleID, (panelind * 3) + (w / 3) + 1, "ABC"[w % 3]);
                    wcut[(9 * panelind) + w] = true;
                    wobjs[w].SetActive(false);
                    wobjs[w + 9].SetActive(true);
                    wrends[w + 9].material = wrends[w].material;
                    if (w != cut[panelind])
                        module.HandleStrike();
                }
                return false;
            };
        }
        IEnumerator covermove = null;
        cover.OnInteract += delegate ()
        {
            if (covermove != null)
                StopCoroutine(covermove);
            covermove = Covermove(true);
            StartCoroutine(covermove);
            return true;
        };
        button.OnDeselect += delegate ()
        {
            if (covermove != null)
                StopCoroutine(covermove);
            covermove = Covermove(false);
            StartCoroutine(covermove);
        };
        button.OnInteract += delegate ()
        {
            if(!moduleSolved && pressable)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, bpos);
                StartCoroutine("Press");
            }
            return false;
        };
        button.OnInteractEnded += delegate ()
        {
            if (!moduleSolved && pressable)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, bpos);
                button.AddInteractionPunch(0.5f);
                bpos.localPosition = new Vector3(0, 0.6f, 0);
                if (!hold)
                {
                    StopCoroutine("Press");
                    if (panelind < 1)
                    {
                        Debug.LogFormat("[Sequential Button #{0}] There are no previous panels to acces.", moduleID);
                        module.HandleStrike();
                    }
                    else
                    {
                        Debug.LogFormat("[Sequential Button #{0}] Returning to panel {1}.", moduleID, panelind);
                        mechanism = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
                        StartCoroutine(Hatchmove(false, false));
                    }
                }
                else
                {
                    hold = false;
                    if(panelind < stage)
                    {
                        Debug.LogFormat("[Sequential Button #{0}] Advancing to panel {1}.", moduleID, panelind + 2);
                        mechanism = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
                        StartCoroutine(Hatchmove(false, true));
                    }
                    else if(wcut[(9 * stage) + cut[stage]])
                    {
                        string t = info.GetFormattedTime();
                        Debug.LogFormat("[Sequential Button #{0}] Button released at {1}.", moduleID, t);
                        string[] digits = rel[stage].ToString().Select(x => x.ToString()).Distinct().ToArray();
                        if (digits.All(d => t.Contains(d)))
                        {
                            if (stage < 3)
                            {
                                stage++;
                                Debug.LogFormat("[Sequential Button #{0}] Advancing to panel {1}.", moduleID, stage + 1);
                                Debug.Log(logs[stage]);
                            }
                            else
                            {
                                stage++;
                                moduleSolved = true;
                                Debug.LogFormat("[Sequential Button #{0}] All panels completed.", moduleID);
                            }
                            mechanism = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
                            StartCoroutine(Hatchmove(false, true));
                        }
                        else
                            module.HandleStrike();
                    }
                }
            }
        };
    }

    private Vector2Int Panelcut(List<int> panel, int wirenum)
    {
        int rule = 0;
        int p = 0;
        int[][] ports = Ports(panel);
        switch (wirenum)
        {
            case 2:
                if (panel.Count(w => w == binfo[0]) == 1)
                    p = panel.FindIndex(w => w >= 0 && w != binfo[0]);
                else if (Enumerable.Range(0, 5).Any(x => panel.Count(w => w == x) > 1))
                {
                    rule = 1;
                    p = Placement(panel, 1);
                }
                else if (ports.Any(x => x.Count(w => w >= 0) > 1))
                {
                    rule = 2;
                    p = Placement(panel, 2);
                }
                else if (binfo[1] == 0)
                {
                    rule = 3;
                    p = Placement(panel, 1);
                }
                else if (ports[4].All(w => w < 0))
                {
                    rule = 4;
                    for (int j = 2; j < 9; j += 3)
                        if (panel[j] >= 0)
                        {
                            p = j;
                            break;
                        }
                }
                else if (binfo[0] == 0)
                {
                    rule = 5;
                    p = Placement(panel, 2);
                }
                else
                {
                    rule = 6;
                    p = Placement(panel, 1);
                }
                break;
            case 3:
                if (ports.All(x => x.Any(w => w >= 0)))
                {
                    for (int j = 0; j < 9; j += 3)
                        if (panel[j] >= 0)
                        {
                            p = j;
                            break;
                        }
                }
                else if (panel.Count(w => w == binfo[0]) > 1)
                {
                    rule = 1;
                    p = Placement(panel, 2);
                }
                else if (ports[5].Count(w => w >= 0) == 1)
                {
                    rule = 2;
                    p = Placement(panel, 1);
                }
                else if (panel.Count(w => w == 4) == 1)
                {
                    rule = 3;
                    p = panel.IndexOf(4);
                }
                else if (binfo[1] == 1)
                {
                    rule = 4;
                    p = Placement(panel, 3);
                }
                else if (Enumerable.Range(0, 5).Any(x => panel.Count(w => x == w) == 2))
                {
                    rule = 5;
                    for (int j = 0; j < 5; j++)
                        if (panel.Count(w => j == w) == 2)
                        {
                            p = panel.FindLastIndex(w => w == j);
                            break;
                        }
                }
                else if (panel.All(w => w != 3))
                {
                    rule = 6;
                    p = Placement(panel, 1);
                }
                else
                {
                    rule = 7;
                    p = Placement(panel, 2);
                }
                break;
            case 4:
                if (Enumerable.Range(0, 5).All(x => panel.Count(w => w == x) < 2))
                    p = Placement(panel, 3);
                else if (panel.All(w => w != binfo[0]))
                {
                    rule = 1;
                    p = Placement(panel, 4);
                }
                else if (ports.Any(x => x.All(w => w >= 0)))
                {
                    rule = 2;
                    for (int j = 0; j < 6; j++)
                        if (ports[j].All(w => w >= 0))
                        {
                            if (j < 3)
                                p = panel.Select((w, k) => k / 3 == j ? -1 : w).ToList().FindIndex(w => w >= 0);
                            else
                                p = panel.Select((w, k) => k % 3 == j - 3 ? -1 : w).ToList().FindIndex(w => w >= 0);
                            break;
                        }
                }
                else if (panel.Count(w => w == 1) == 2)
                {
                    rule = 3;
                    p = panel.FindIndex(w => w == 1);
                }
                else if (ports.All(x => x.Any(w => w >= 0)))
                {
                    rule = 4;
                    for (int j = 1; j < 9; j += 3)
                        if(panel[j] >= 0)
                             p = j;
                }
                else if (binfo[1] == 2)
                {
                    rule = 5;
                    p = Placement(panel, 2);
                }
                else if (Enumerable.Range(0, 5).Any(x => panel.Count(w => w == x) > 2))
                {
                    rule = 6;
                    p = Placement(panel, 1);
                }
                else
                {
                    rule = 7;
                    p = panel.FindLastIndex(w => w == binfo[0]);
                }
                break;
            case 5:
                if (ports.Count(x => x.All(w => w >= 0)) == 1)
                {
                    for (int j = 0; j < 6; j++)
                        if (ports[j].All(w => w >= 0))
                        {
                            if (j < 3)
                                p = panel.Select((w, k) => k / 3 != j || k % 3 == 0 ? -1 : w).ToList().FindIndex(w => w >= 0);
                            else
                                p = panel.Select((w, k) => k % 3 != j - 3 || k < 3 ? -1 : w).ToList().FindIndex(w => w >= 0);
                            break;
                        }
                }
                else if (panel.All(w => w != 2))
                {
                    rule = 1;
                    p = Placement(panel, 2);
                }
                else if (Enumerable.Range(0, 5).Where(x => panel.Count(w => w == x) == 1).Any(x => ports.Where(y => y.Contains(x)).Any(y => y.Count(w => w < 0) > 1)))
                {
                    rule = 2;
                    p = Placement(panel, 5);
                }
                else if (panel.Count(w => w == binfo[0]) > 1)
                {
                    rule = 3;
                    p = panel.FindIndex(w => w == binfo[0]);
                }
                else if (binfo[1] == 3)
                {
                    rule = 4;
                    p = Placement(panel, 4);
                }
                else if (panel.Count(w => w == 4) == 1)
                {
                    rule = 5;
                    p = Placement(panel, 1);
                }
                else if (panel.Count(w => w == 0) > 1)
                {
                    rule = 6;
                    p = panel.FindLastIndex(w => w == 0);
                }
                else
                {
                    rule = 7;
                    p = Placement(panel, 3);
                }
                break;
            case 6:
                if (Enumerable.Range(0, 5).All(x => panel.Count(w => w == x) != 1))
                {
                    p = Placement(panel, 5);
                }
                else if (ports.Any(x => x.All(w => w < 0)))
                {
                    rule = 1;
                    p = panel.FindIndex(x => panel.Count(w => w == x) == 1);
                }
                else if (Enumerable.Range(3, 2).All(x => panel.Contains(x)))
                {
                    rule = 2;
                    p = panel.FindIndex(w => w == 3);
                }
                else if (panel.All(w => w != binfo[0]))
                {
                    rule = 3;
                    p = Placement(panel, 3);
                }
                else if (binfo[0] == 1)
                {
                    rule = 4;
                    p = Placement(panel, 6);
                }
                else if (panel.Count(w => w == 2) > 1)
                {
                    rule = 5;
                    p = panel.FindLastIndex(w => w == 2);
                }
                else if (ports.Count(x => x.Count(w => w >= 0) == 1) == 1)
                {
                    rule = 6;
                    for (int j = 0; j < 6; j++)
                        if (ports[j].Count(w => w >= 0) == 1)
                        {
                            if (j < 3)
                                p = panel.Select((w, k) => k / 3 != j ? -1 : w).ToList().FindIndex(w => w >= 0);
                            else
                                p = panel.Select((w, k) => k % 3 != j - 3 ? -1 : w).ToList().FindIndex(w => w >= 0);
                            break;
                        }
                }
                else
                {
                    rule = 7;
                    p = panel.FindIndex(w => w == binfo[0]);
                }
                break;
            case 7:
                if (ports.Count(x => x.Count(w => w >= 0) == 1) == 1 && Enumerable.Range(0, 5).Where(x => panel.Count(w => w == x) > 1).Any(x => ports.Where(y => y.Contains(x)).Any(z => z.Count(w => w < 0) > 1)))
                {
                    for (int j = 0; j < 6; j++)
                        if (ports[j].Count(w => w < 0) > 1)
                        {
                            int x = 0;
                            p = ports[j].First(w => w >= 0);
                            for (int k = 0; k < 9; k++)
                                if (panel[k] == p)
                                {
                                    x++;
                                    if (x == 2)
                                    {
                                        p = k;
                                        break;
                                    }
                                }
                            break;
                        }
                }
                else if (panel.All(w => w != 0))
                {
                    rule = 1;
                    p = Placement(panel, 7);
                }
                else if (panel.Count(w => w == 2) > panel.Count(w => w == 3))
                {
                    rule = 2;
                    p = panel.FindIndex(w => w == 2);
                }
                else if (Enumerable.Range(0, 5).Where(x => x != binfo[0]).All(x => panel.Count(w => w == binfo[0]) >= panel.Count(w => w == x)))
                {
                    rule = 3;
                    int x = 0;
                    for (int k = 0; k < 9; k++)
                        if (panel[k] >= 0 && panel[k] != binfo[0])
                        {
                            x++;
                            if (x == 3)
                            {
                                p = k;
                                break;
                            }
                        }
                }
                else if (ports[5].All(w => w >= 0))
                {
                    rule = 4;
                    p = Placement(panel, 4);
                }
                else if (panel.Count(w => w == binfo[0]) < 2)
                {
                    rule = 5;
                    p = Placement(panel, 2);
                }
                else if (Enumerable.Range(0, 5).Count(x => panel.Count(w => w == x) == 1) == 2)
                {
                    rule = 6;
                    p = panel.FindLastIndex(w => panel.Count(x => x == w) == 1);
                }
                else
                {
                    rule = 7;
                    List<int> counts = new List<int> { };
                    for (int j = 0; j < 5; j++)
                        counts.Add(panel.Count(w => w == j));
                    int u = counts.FindIndex(c => c >= 0 && counts.Count(w => w == c) == 1);
                    p = panel.FindIndex(w => w == u);
                }
                break;
            case 8:
                if (Enumerable.Range(0, 3).All(x => ports[x + 3].Contains(x)))
                {
                    for (int j = 2; j < 9; j += 3)
                        if (panel[j] >= 0)
                        {
                            p = j;
                            break;
                        }
                }
                else if (ports[5].Contains(binfo[0]))
                {
                    rule = 1;
                    p = Placement(panel, 1);
                    for (int j = 2; j < 9; j++)
                        if (panel[Placement(panel, j)] == panel[p])
                        {
                            p = j;
                            break;
                        }
                }
                else if (panel[Placement(panel, 4)] == binfo[0])
                {
                    rule = 2;
                    p = Placement(panel, 1);
                }
                else if (panel.First(w => w >= 0) == panel.Last(w => w >= 0))
                {
                    rule = 3;
                    p = Placement(panel, 6);
                }
                else if (Enumerable.Range(0, 5).Count(x => panel.Count(w => w == x) == 1) == 2)
                {
                    List<int> pantwo = new List<int> { };
                    for (int j = 0; j < 9; j++)
                        pantwo.Add(panel.Count(w => w == panel[j]) != 1 ? -1 : panel[j]);
                    p = Panelcut(pantwo, 2).y;
                    rule = 4;
                }
                else if(panel.Distinct().Count() > 5)
                {
                    rule = 5;
                    p = panel.FindLastIndex(w => w == binfo[0]);
                }
                else if(panel.Distinct().Count() == 4)
                {
                    rule = 6;
                    for(int j = 0; j < 5; j++)
                        if(panel.Count(w => w == j) == 2)
                        {
                            p = panel.FindLastIndex(w => w == j);
                            break;
                        }
                }
                else if(panel.Count(w => w == panel[Placement(panel, 7)]) == 1)
                {
                    rule = 7;
                    p = Placement(panel, 2);
                }
                else
                {
                    rule = 8;
                    p = Placement(panel, 8);
                }
                break;
            default:
                if (Enumerable.Range(0, 5).All(x => panel.Count(w => w == x) < 3))
                    p = panel.IndexOf(binfo[0]);
                else if (Enumerable.Range(0, 5).Count(x => panel.Count(w => w == x) == 1) == 3)
                {
                    rule = 1;
                    for(int j = 0; j < 3; j++)
                        if(panel.Count(w => w == j) == 1)
                        {
                            p = j;
                            break;
                        }
                }
                else if (panel.All(w => w != binfo[0]))
                {
                    rule = 2;
                    p = panel.IndexOf(panel[8]);
                }
                else if (Enumerable.Range(0, 5).Where(x => panel.Count(w => w == x) == 1).Any(w => ports[3].Contains(w)))
                {
                    rule = 3;
                    p = 4;
                }
                else if(panel.Count(w => w == binfo[0]) == 1)
                {
                    rule = 4;
                    p = 6;
                }
                else if (panel[2] == panel[6])
                {
                    rule = 5;
                    p = 0;
                }
                else if (panel.Count(w => w == panel[8]) == 1)
                {
                    rule = 6;
                    p = panel.FindIndex(w => panel.Count(x => x == w) == 1);
                }
                else if (Enumerable.Range(0, 5).Count(x => panel.Count(w => w == x) == 1) == 2)
                {
                    List<int> pantwo = new List<int> { };
                    for (int j = 0; j < 9; j++)
                        pantwo.Add(panel.Count(w => w == panel[j]) != 1 ? -1 : panel[j]);
                    p = Panelcut(pantwo, 2).y;
                    rule = 7;
                }
                else
                {
                    rule = 8;
                    p = 3;
                }
                break;
        }
        return new Vector2Int(rule, p);
    }

    private void OnDestroy()
    {
        if (mechanism != null)
        {
            mechanism.StopSound();
            mechanism = null;
        }
    }

    private IEnumerator Press()
    {
        float e = 0;
        while(e < 0.5f)
        {
            e += Time.deltaTime;
            bpos.localPosition = new Vector3(0, Mathf.Lerp(0.6f, 0.247f, e * 2), 0);
            yield return null;
        }
        bpos.localPosition = new Vector3(0, 0.247f, 0);
        hold = true;
        if (panelind < stage || wcut[(9 * panelind) + cut[panelind]])
        {
            for (int i = 0; i < 4; i++)
            {
                int l = lcols[(panelind * 4) + i];
                lrends[i].material = lmats[l];
                lights[i].color = cols[l];
                lights[i].enabled = true;
            }
            while (hold)
            {
                e += Time.deltaTime;
                float a = 50 * (1 - Mathf.Abs(Mathf.Cos(e * Mathf.PI)));
                for (int i = 0; i < 4; i++)
                    lights[i].intensity = a;
                yield return null;
            }
            for (int i = 0; i < 4; i++)
            {
                lrends[i].material = lmats[i < stage ? 5 : 4];
                lights[i].enabled = false;
            }
        }
        else
        {
            module.HandleStrike();
            Debug.LogFormat("[Sequential Button #{0}] Do not hold the button until the correct wire is cut.", moduleID);
        }
    }

    private IEnumerator Covermove(bool open)
    {
        pressable = false;
        float e = cpos[0].localEulerAngles.z;
        if (e > 0)
            e -= 360;
        if (open)
        {
            while(e > -146)
            {
                e -= Time.deltaTime * 292;
                for (int i = 0; i < 2; i++)
                    cpos[i].localEulerAngles = new Vector3(0, 0, e);
                yield return null;
            }
            for (int i = 0; i < 2; i++)
                cpos[i].localEulerAngles = new Vector3(0, 0, -146);
            bpos.localPosition = new Vector3(0, 0.6f, 0);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, bpos);
        }
        else
        {
            bpos.localPosition = new Vector3(0, 0.494f, 0);
            while (e < 0)
            {
                e += Time.deltaTime * 292;
                for (int i = 0; i < 2; i++)
                    cpos[i].localEulerAngles = new Vector3(0, 0, e);
                yield return null;
            }
            for (int i = 0; i < 2; i++)
                cpos[i].localEulerAngles = new Vector3(0, 0, 0);
        }
        pressable = true;
    }

    private IEnumerator Hatchmove(bool open, bool up)
    {
        pressable = false;
        if (open)
        {
            float e = 0;
            while (e < 0.25f)
            {
                e += Time.deltaTime;
                hatch[0].localEulerAngles = new Vector3(360 * e, 0, 0);
                hatch[1].localEulerAngles = new Vector3(-360 * e, 0, 0);
                hatch[2].localPosition = new Vector3(-0.0227f, Mathf.Lerp(-0.044f, 0.014f, e * 2), 0.0205f);
                yield return null;
            }
            hatch[0].localEulerAngles = new Vector3(90, 0, 0);
            hatch[1].localEulerAngles = new Vector3(-90, 0, 0);
            while (e < 0.5f)
            {
                e += Time.deltaTime;
                hatch[0].localPosition = new Vector3(0, -0.242f * (e - 0.25f), 0.0608f);
                hatch[1].localPosition = new Vector3(0, -0.242f * (e - 0.25f), -0.0608f);
                hatch[2].localPosition = new Vector3(-0.0227f, Mathf.Lerp(-0.044f, 0.014f, e * 2), 0.0205f);
                yield return null;
            }
            hatch[0].localPosition = new Vector3(0, -0.0605f, 0.0608f);
            hatch[1].localPosition = new Vector3(0, -0.0605f, -0.0608f);
            hatch[2].localPosition = new Vector3(-0.0227f, 0.014f, 0.0205f);
            pressable = true;
            mechanism.StopSound();
        }
        else
        {
            float e = 0.5f;
            while (e > 0.25f)
            {
                e -= Time.deltaTime;
                hatch[0].localPosition = new Vector3(0, -0.242f * (e - 0.25f), 0.0608f);
                hatch[1].localPosition = new Vector3(0, -0.242f * (e - 0.25f), -0.0608f);
                hatch[2].localPosition = new Vector3(-0.0227f, Mathf.Lerp(-0.044f, 0.014f, e * 2), 0.0205f);
                yield return null;
            }
            hatch[0].localPosition = new Vector3(0, 0, 0.0608f);
            hatch[1].localPosition = new Vector3(0, 0, -0.0608f);
            while (e > 0)
            {
                e -= Time.deltaTime;
                hatch[0].localEulerAngles = new Vector3(360 * e, 0, 0);
                hatch[1].localEulerAngles = new Vector3(-360 * e, 0, 0);
                hatch[2].localPosition = new Vector3(-0.0227f, Mathf.Lerp(-0.044f, 0.014f, e * 2), 0.0205f);
                yield return null;
            }
            hatch[0].localEulerAngles = new Vector3(0, 0, 0);
            hatch[1].localEulerAngles = new Vector3(0, 0, 0);
            hatch[2].localPosition = new Vector3(-0.0227f, -0.044f, 0.0205f);
            panelind = panelind + (up ? 1 : -1);
            if (moduleSolved)
            {
                mechanism.StopSound();
                module.HandlePass();
            }
            else
            {
                Panelset(panelind);
                yield return new WaitForSeconds(0.25f);
                StartCoroutine(Hatchmove(true, true));
            }
        }
    }

    private int[][] Ports(List<int> w)
    {
        int[][] p = new int[6][];
        for (int i = 0; i < 3; i++)
            p[i] = w.Skip(3 * i).Take(3).ToArray();
        for (int i = 0; i < 3; i++)
            p[i + 3] = w.Where((x, j) => j % 3 == i).ToArray();
        return p;
    }

    private int Placement(List<int> w, int c)
    {
        int x = 0;
        for(int i = 0; i < 9; i++)
            if(w[i] >= 0)
            {
                x++;
                if (x == c)
                    return i;
            }
        return -1;
    }

    private void Panelset(int k)
    {
        for(int i = 0; i < 3; i++)
            labels[i].text = ((k * 3) + i + 1).ToString();
        for (int i = 0; i < 9; i++)
        {
            wobjs[i].SetActive(false);
            wobjs[i + 9].SetActive(false);
            int w = (9 * k) + i;
            if (wcut[w])
            {
                wobjs[i + 9].SetActive(true);
                wrends[i + 9].material = wmats[warrange[w]];
            }
            else if (warrange[w] >= 0)
            {
                wobjs[i].SetActive(true);
                wrends[i].material = wmats[warrange[w]];
            }
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} cut 1-9 [Wire position] | !{0} tap/hold [Presses button] | !{0} release ##:## [Releases button at the time given in munites and seconds]";
#pragma warning restore 414

    bool ZenModeActive;

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if(commands.Length > 2)
        {
            yield return "sendtochaterror!f Invalid command length.";
            yield break;
        }
        switch (commands[0])
        {
            case "cut":
                if(commands.Length < 2)
                {
                    yield return "sendtochaterror!f Invalid command length.";
                    yield break;
                }
                if (hold)
                {
                    yield return "sendtochaterroe!f Cannot cut wire while button is held.";
                    yield break;
                }
                int w = 0;
                if(int.TryParse(commands[1], out w))
                {
                    w = Placement(warrange.Skip(9 * panelind).Take(9).ToList(), w);
                    if(w < 0)
                    {
                        yield return "sendtochaterror!f Invalid wire position.";
                        yield break;
                    }
                    if(wcut[(9 * panelind) + w])
                    {
                        yield return "sendtochaterror!f Wire " + commands[1] + "is already cut.";
                        yield break;
                    }
                    yield return null;
                    wires[w].OnInteract();
                }
                else
                    yield return "sendtochaterror!f Wire position must be a number.";
                yield break;
            case "tap":
                while (!pressable)
                    yield return null;
                if (hold)
                {
                    yield return "sendtochaterror!f Release the button before pressing it again.";
                    yield break;
                }
                yield return null;
                cover.OnInteract();
                yield return null;
                while (!pressable)
                    yield return null;
                button.OnInteract();
                yield return new WaitForSeconds(0.1f);
                button.OnInteractEnded();
                yield return null;
                button.OnDeselect();
                yield break;
            case "hold":
                if (hold)
                {
                    yield return "sendtochaterror!f Release the button before pressing it again.";
                    yield break;
                }
                yield return null;
                cover.OnInteract();
                yield return null;
                while (!pressable)
                    yield return null;
                button.OnInteract();
                yield return new WaitForSeconds(0.5f);
                yield break;
            case "release":
                if (commands.Length < 2)
                {
                    yield return "sendtochaterror!f Invalid command length.";
                    yield break;
                }
                if (!hold)
                {
                    yield return "sendtochaterror!f Hold the button before releasing it.";
                    yield break;
                }
                if (commands[1].Length < 3 || commands[1][commands[1].Length - 3] != ':')
                {
                    yield return "sendtochaterror!f Invalid timer format.";
                    yield break;
                }
                int m = 0;
                int s = 0;
                if (int.TryParse(new string(commands[1].TakeWhile(x => x != ':').ToArray()), out m) && int.TryParse(new string(commands[1].TakeLast(2).ToArray()), out s))
                {
                    m *= 60;
                    m += s;
                    if (ZenModeActive ^ info.GetTime() < m)
                        yield return "sendtochaterror Bomb time has exceeded the given release time.";
                    else
                    {
                        Debug.Log("Releasing button with " + m.ToString() + " seconds remaining.");
                        yield return null;
                        while ((int)info.GetTime() != m)
                            yield return "trycancel";
                        button.OnInteractEnded();
                        yield return null;
                        button.OnDeselect();
                    }
                }
                else
                    yield return "sendtochaterror NaN minutes or seconds entered.";
                yield break;
            default:
                yield return "sendtochaterror!f " + commands[0] + " is not a valid command.";
                yield break;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            while(!pressable)
                yield return true;
            if (hold)
            {
                if (panelind < stage)
                {
                    yield return null;
                    button.OnInteractEnded();                   
                }
                else
                {
                    yield return null;
                    string[] digits = rel[stage].ToString().Select(x => x.ToString()).Distinct().ToArray();
                    while (digits.Any(d => info.GetFormattedTime().All(x => x.ToString() != d)))
                        yield return true;
                    button.OnInteractEnded();
                    yield return null;
                    button.OnDeselect();
                }
            }
            else
            {
                if (!wcut[(9 * panelind) + cut[panelind]])
                    wires[cut[panelind]].OnInteract();
                yield return null;
                cover.OnInteract();
                yield return null;
                while (!pressable)
                    yield return null;
                button.OnInteract();
            }
        }
    }
}
