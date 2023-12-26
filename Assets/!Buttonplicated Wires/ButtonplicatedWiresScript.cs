using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class ButtonplicatedWiresScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> wires;
    public List<KMSelectable> buttons;
    public GameObject[] wcuts;
    public GameObject[] bpush;
    public Transform[] bpos;
    public Renderer[] wrends;
    public Renderer[] brends;
    public Renderer[] lrends;
    public Material[] wcols;
    public Material[] bcols;
    public Material[] lcols;
    public TextMesh[] labels;
    public Light[] leds;

    private readonly Color[] lightcols = new Color[4] { new Color(1, 0.3f, 0.3f), new Color(0.1f, 0.45f, 1), new Color(0.7f, 0.7f, 0), new Color(0.7f, 0.7f, 0.7f)};
    private readonly string[] collog = new string[5] { "Red", "Blue", "Yellow", "White", "Black" };
    private readonly string[] symblog = new string[11] { "\u263c", "\u263e", "\u2606", "\u2661", "\u2662", "\u2664", "\u2667", "\u2724", "\u272a", "\u273f", "\u2744"};
    private readonly int[] venn = new int[128] { 5, 1, 3, 0, 2, 1, 4, 6, 3, 5, 6, 0, 5, 2, 4, 3, 1, 0, 6, 4, 5, 1, 2, 6, 4, 2, 3, 6, 0, 1, 2, 1, 4, 3, 1, 4, 5, 2, 6, 2, 1, 2, 4, 1, 0, 5, 2, 4, 6, 6, 2, 5, 2, 3, 1, 0, 3, 5, 4, 3, 5, 6, 4, 2, 6, 4, 5, 6, 4, 5, 2, 3, 5, 1, 2, 0, 1, 4, 0, 6, 0, 3, 1, 5, 6, 4, 6, 0, 6, 2, 0, 1, 2, 3, 5, 4, 0, 0, 3, 3, 3, 1, 6, 5, 4, 4, 2, 5, 1, 4, 3, 2, 3, 4, 5, 4, 5, 6, 0, 6, 0, 5, 2, 2, 6, 1, 5, 3 };
    private readonly int[,,] table = new int[5, 3, 3] { { { 2, 7, 6}, { 9, 5, 1}, { 4, 3, 8} }, { { 8, 1, 4}, {6, 9, 7}, { 5, 2, 3} }, { { 7, 6, 3}, { 1, 2, 5}, {8, 4, 9} }, { { 9, 3, 1}, { 4, 6, 8}, { 2, 5, 7} }, { { 1, 4, 9}, { 5, 3, 2}, { 6, 7, 8} } };
    private readonly int[] venntwo = new int[16] { 0, 1, 2, 3, 2, 0, 3, 1, 3, 0, 1, 2, 1, 3, 2, 0};
    private int[,] wircols = new int[6, 2];
    private int[,] binfo = new int[6, 3];
    private bool[] ledon = new bool[6];
    private int[] conds = new int[6];
    private bool[] cuts = new bool[6];
    private bool[] hold = new bool[2];
    private int tphold = -1;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        float scale = module.transform.lossyScale.x;
        foreach (Light l in leds)
            l.range *= scale;
        for (int i = 0; i < 6; i++)
        {
            wcuts[i + 6].SetActive(false);
            ledon[i] = Random.Range(0, 2) == 0;
            if (ledon[i])
            {
                leds[i].color = lightcols[1];
                lrends[i].material = lcols[1];
            }
            else
            {
                leds[i].enabled = false;
                lrends[i].material = lcols[4];
            }
            int wr = Random.Range(0, 30);
            wircols[i, 1] = wr % 5;
            wircols[i, 0] = wr > 24 ? wircols[i, 1] : wr / 5;
            for (int j = 0; j < 2; j++)
                wrends[(6 * j) + i].material = wcols[wr];
            binfo[i, 0] = Random.Range(0, 5);
            for (int j = 1; j < 3; j++)
                binfo[i, j] = Random.Range(0, 3);
            for (int j = 0; j < 3; j++)
                if (j == binfo[i, 1])
                {
                    brends[(3 * i) + j].material = bcols[binfo[i, 0] + (5 * j)];
                    if (binfo[i, 0] == 4)
                        labels[i].color = new Color(0, 0.4f, 1);
                }
                else
                    bpush[(3 * i) + j].SetActive(false);
            labels[i].text = "HAD"[binfo[i, 2]].ToString();
            Debug.LogFormat("[Buttonplicated Wires #{0}] The {1} button is a {2} {3} labelled \"{4}\" connected to an {5}active LED through a {6}{7} wire.", moduleID, new string[] { "first", "second", "third", "fourth", "fifth", "sixth" }[i], collog[binfo[i, 0]], new string[] { "Circle", "Hexagon", "Square" }[binfo[i, 1]], new string[] { "Hold", "Abort", "Detonate" }[binfo[i, 2]], ledon[i] ? "" : "in", collog[wircols[i, 0]], wircols[i, 0] != wircols[i, 1] ? " & " + collog[wircols[i, 1]] : "");
            int index = 0;
            string c = "";
            if (binfo[i, 0] < 2)
            {
                index = 1;
                c = "A";
            }
            if (binfo[i, 0] == wircols[i, 0] || binfo[i, 0] == wircols[i, 1])
            {
                index += 2;
                c += "B";
            }
            if (binfo[i, 1] == 0)
            {
                index += 4;
                c += "C";
            }
            if (binfo[i, 2] < 2)
            {
                index += 8;
                c += "D";
            }
            if (wircols[i, 0] > 2 || wircols[i, 1] > 2)
            {
                index += 16;
                c += "E";
            }
            if (info.GetSerialNumberNumbers().Contains(i))
            {
                index += 32;
                c += "F";
            }
            if (ledon[i])
            {
                index += 64;
                c += "G";
            }
            if (c == "")
                c = "O";
            Debug.LogFormat("[Buttonplicated Wires #{0}] {1} = {2}", moduleID, c, symblog[venn[index]]);
            conds[i] = new int[] { 1, 2, 4, 3, 5, 6, 8 }[venn[index]];
        }
        foreach (KMSelectable wire in wires)
        {
            int i = wires.IndexOf(wire);
            wire.OnInteract += delegate ()
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, wcuts[i].transform);
                wcuts[i].SetActive(false);
                wcuts[i + 6].SetActive(true);
                cuts[i] = true;
                if (conds[i] % 2 == 1)
                {
                    conds[i]--;
                    Debug.LogFormat("[Buttonplicated Wires #{0}] Wire {1} cut.", moduleID, i + 1);
                }
                else
                {
                    module.HandleStrike();
                    Debug.LogFormat("[Buttonplicated Wires #{0}] Wire {1} should not be cut at this point.", moduleID, i + 1);
                    if (conds[i] > 4)
                        conds[i] -= 4;
                }
                if (cuts.All(x => x))
                    for (int j = 0; j < 6; j++)
                        if (conds[j] > 0)
                            conds[j] = 2;
                if (conds.All(x => x == 0))
                {
                    module.HandlePass();
                    moduleSolved = true;
                }
                return false;
            };
        }
        foreach (KMSelectable button in buttons)
        {
            int i = buttons.IndexOf(button);
            if (bpush[i].activeSelf)
            {
                i /= 3;
                buttons[(3 * i) + binfo[i, 1]].OnInteract += delegate ()
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, bpos[i]);
                    bpos[i].localPosition = new Vector3(0, -0.495f, 0);
                    hold[0] = true;
                    leds[i].enabled = false;
                    lrends[i].material = lcols[4];
                    if (!moduleSolved)
                        StartCoroutine(Hold(i));
                    return false;
                };
                buttons[(3 * i) + binfo[i, 1]].OnInteractEnded += delegate ()
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, bpos[i]);
                    if (!moduleSolved)
                    {
                        if (!hold[1])
                        {
                            bpos[i].localPosition = new Vector3(0, 0, 0);
                            if (conds[i] % 2 == 0 && (conds[i] / 2) % 2 == 1)
                            {
                                conds[i] = conds[i] > 4 ? 1 : 0;
                                Debug.LogFormat("[Buttonplicated Wires #{0}] Button {1} tapped.", moduleID, i + 1);
                            }
                            else
                            {
                                module.HandleStrike();
                                Debug.LogFormat("[Buttonplicated Wires #{0}] Button {1} should not be tapped at this point.", moduleID, i + 1);
                            }
                            if (conds.All(x => x == 0))
                            {
                                module.HandlePass();
                                moduleSolved = true;
                            }
                        }
                        hold[0] = false;
                        hold[1] = false;
                        leds[i].intensity = 50;
                        if (ledon[i])
                        {
                            leds[i].enabled = true;
                            leds[i].color = new Color(0, 0.4f, 1);
                            lrends[i].material = lcols[1];
                        }
                        else
                        {
                            leds[i].enabled = false;
                            lrends[i].material = lcols[4];
                        }
                        if (conds.All(x => x == 0))
                        {
                            module.HandlePass();
                            moduleSolved = true;
                            StartCoroutine(SolveAnim());
                        }
                    }
                };
            }    
        }
    }

    private IEnumerator Hold(int b)
    {
        float e = 0;
        while(e < 0.5f)
        {
            e += Time.deltaTime;
            yield return null;
            if (!hold[0])
                yield break;
        }
        if(cuts.All(x => x))
        {
            hold[1] = true;
            Debug.LogFormat("[Buttonplicated Wires #{0}] All wires cut. Do not hold buttons.", moduleID);
            module.HandleStrike();
        }
        int r = Random.Range(0, 4);
        leds[b].color = lightcols[r];
        lrends[b].material = lcols[r];
        leds[b].enabled = true;
        hold[1] = true;
        Debug.LogFormat("[Buttonplicated Wires #{0}] Button {1} held; LED is flashing {2}.", moduleID, b + 1, collog[r]);
        while (hold[1])
        {
            e += Time.deltaTime * Mathf.PI;
            float a = Mathf.Abs(Mathf.Cos(e));
            leds[b].intensity = 100 * (1 - a);
            yield return null;
        }
        bpos[b].localPosition = new Vector3(0, 0, 0);
        if(conds[b] % 4 == 0)
        {
            Debug.LogFormat("[Buttonplicated Wires #{0}] Button {1} released at {2}.", moduleID, b + 1, info.GetFormattedTime());
            int d = table[binfo[b, 0], binfo[b, 2], binfo[b, 1]];
            int v = 0;
            string c = "";
            if(r > 1)
            {
                v = 1;
                c = "a";
            }
            if(wircols[b, 0] == r || wircols[b, 1] == r)
            {
                v += 2;
                c += "b";
            }
            if(cuts.Count(x => x) % 2 == 0)
            {
                v += 4;
                c += "c";
            }
            if((b > 0 && ledon[b - 1]) ^ (b < 5 && ledon[b + 1]))
            {
                v += 8;
                c += "d";
            }
            if (c == "")
                c = "O";
            v = venntwo[v];
            Debug.LogFormat("[Buttonplicated Wires #{0}] Button {1} has a value of {2}.", moduleID, b + 1, d);
            Debug.LogFormat("[Buttonplicated Wires #{0}] {1} = {2}.", moduleID, c, symblog[v + 7]);
            bool success = false;
            switch (v)
            {
                case 0: success = d == ((info.GetFormattedTime().TakeWhile(x => x != '.').Where(x => "0123456789".Contains(x.ToString())).Select(x => x - '0').Sum() - 1) % 9) + 1 ; break;
                case 1: success = 0 == (int)info.GetTime() % (d + 10); break;
                case 2: success = info.GetFormattedTime().TakeWhile(x => x != '.').Count(x => x - '0' < d) % 2 == 1; break;
                default:
                    for (int i = 0; i < 3; i++)
                        d += Enumerable.Range(0, 6).Count(x => x != b && binfo[x, i] == binfo[b, i]);
                    success = (int)info.GetTime() % 30 == d; break;
            }
            if (success)
            {
                conds[b] /= 8;
                Debug.LogFormat("[Buttonplicated Wires #{0}] Button {1} release condition met.", moduleID, b + 1);
                if (conds.All(x => x == 0))
                {
                    module.HandlePass();
                    moduleSolved = true;
                }
            }
            else
            {
                module.HandleStrike();
                Debug.LogFormat("[Buttonplicated Wires #{0}] Button {1} release condition failed.", moduleID, b + 1);
            }
        }
        else
        {
            module.HandleStrike();
            Debug.LogFormat("[Buttonplicated Wires #{0}] Button {1} should not be held at this point.", moduleID, b + 1);
        }
    }

    private IEnumerator SolveAnim()
    {
        for(int i = 0; i < 6; i++)
        {
            leds[i].enabled = false;
            leds[i].color = new Color(0, 1, 0);
            lrends[i].material = lcols[4];
        }
        for(int c = 0; c > -1; c = (c + 1) % 6)
        {
            leds[c].enabled = true;
            lrends[c].material = lcols[5];
            leds[(c + 5) % 6].enabled = false;
            lrends[(c + 5) % 6].material = lcols[4];
            yield return new WaitForSeconds(0.16f);
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} cut 1-6 [Wire position] | !{0} tap/hold 1-6 [Button interaction and position] | !{0} release ##:## [Releases held button at the specified minutes and seconds.]";
#pragma warning restore 414

    bool ZenModeActive;

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if(commands.Length != 2)
        {
            yield return "sendtochaterror!f Invalid command length.";
            yield break;
        }
        int d = 0;
        switch (commands[0])
        {
            case "cut":
                if (int.TryParse(commands[1], out d))
                {
                    if (d > 0 && d < 7)
                    {
                        if (cuts[d - 1])
                            yield return "sendtochaterror!f Wire " + d.ToString() + " is already cut.";
                        else
                        {
                            yield return null;
                            wires[d - 1].OnInteract();
                        }
                    }
                    else
                        yield return "sendtochaterror!f Position must be in the range 1-6.";
                }
                else
                    yield return "sendtochaterror!f Position NaN entered.";
                yield break;
            case "tap":
                if (int.TryParse(commands[1], out d))
                {
                    if (hold[0])
                        yield return "sendtochaterror!f Held button must be released before another can be interacted with.";
                    else if (d > 0 && d < 7)
                    {
                        d--;
                        tphold = (3 * d) + binfo[d, 1];
                        yield return null;
                        buttons[tphold].OnInteract();
                        yield return new WaitForSeconds(0.16f);
                        buttons[tphold].OnInteractEnded();
                    }
                    else
                        yield return "sendtochaterror!f Position must be in the range 1-6.";
                }
                else
                    yield return "sendtochaterror!f Position NaN entered.";
                yield break;
            case "hold":
                if (int.TryParse(commands[1], out d))
                {
                    if (hold[0])
                        yield return "sendtochaterror!f Held button must be released before another can be interacted with.";
                    else if (d > 0 && d < 7)
                    {
                        d--;
                        yield return null;
                        tphold = (3 * d) + binfo[d, 1];
                        buttons[tphold].OnInteract();
                    }
                    else
                        yield return "sendtochaterror!f Position must be in the range 1-6.";
                }
                else
                    yield return "sendtochaterror!f Position NaN entered.";
                yield break;
            case "release":
                if(!hold[0])
                {
                    yield return "sendtochaterror!f No buttons are held.";
                    yield break;
                }
                if (commands[1].Length < 3 || commands[1][commands[1].Length - 3] != ':')
                {
                    yield return "sendtochaterror!f Invalid timer format.";
                    yield break;
                }
                int s = 0;
                if (int.TryParse(new string(commands[1].TakeWhile(x => x != ':').ToArray()), out d) && int.TryParse(new string(commands[1].TakeLast(2).ToArray()), out s))
                {
                    d *= 60;
                    d += s;
                    if (ZenModeActive ^ info.GetTime() < d)
                        yield return "sendtochaterror Bomb time has exceeded the given release time.";
                    else
                    {
                        Debug.Log("Releasing button with " + d.ToString() + " seconds remaining.");
                        yield return null;
                        while ((int)info.GetTime() != d)
                            yield return "trycancel";
                        buttons[tphold].OnInteractEnded();
                    }
                }
                else
                    yield return "sendtochaterror NaN minutes or seconds entered.";
                yield break;
            default:
                yield return "sendtochaterror!f Invalid command received.";
                yield break;
        }
    }
}
