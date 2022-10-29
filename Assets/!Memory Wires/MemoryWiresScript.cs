using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class MemoryWiresScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public GameObject[] wireobjs;
    public GameObject[] cutwires;
    public Transform[] hubs;
    public List<KMSelectable> wires;
    public Renderer[] wrends;
    public Renderer[] stagelights;
    public Material[] wcols;
    public Material[] lightstates;
    public TextMesh[] labels;

    private readonly string[] logcol = new string[5] { "Red", "Yellow", "Blue", "White", "Black"};
    private int edgerule;
    private int[][] colset = new int[5][];
    private bool[][] cuts = new bool[5][];
    private bool[] docut = new bool[6];
    private int stage;
    private int dispnum;
    private bool cuttable = true;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

	private void Start()
    {
        moduleID = ++moduleIDCounter;
        stagelights[0].material = lightstates[0];
        for (int i = 0; i < 5; i++)
        {
            colset[i] = new int[6];
            cuts[i] = new bool[6];
        }
        if (info.IsPortPresent(Port.Parallel) && (info.IsIndicatorOn(Indicator.FRK) || info.IsIndicatorOn(Indicator.CAR)) && info.GetBatteryCount() > 1)
            edgerule = 2;
        else
            edgerule = info.GetSerialNumberNumbers().Last() % 2;
        module.OnActivate += Activate;
	}

    private void Activate()
    {
        Debug.LogFormat("[Memory Wires #{0}] Stage 1:", moduleID);
        Generate(0, Random.Range(0, 6));
        labels[6].text = dispnum.ToString();
        for (int i = 0; i < 6; i++)
            StartCoroutine(Hubmove(i, true));
        foreach (KMSelectable wire in wires)
        {
            int k = wires.IndexOf(wire);
            wire.OnInteract = delegate ()
            {
                if (cuttable)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, wire.transform);
                    wire.AddInteractionPunch(0.5f);
                    Debug.LogFormat("[Memory Wires #{0}] Wire {1} cut.", moduleID, (6 * stage) + k + 1);
                    wireobjs[k].SetActive(false);
                    cutwires[k].SetActive(true);
                    if (docut[k])
                    {
                        docut[k] = false;
                        if (docut.All(x => !x))
                        {
                            stagelights[stage].material = lightstates[1];
                            stage++;
                            if (stage > 4)
                            {
                                labels[6].text = "";
                                moduleSolved = true;
                                module.HandlePass();
                            }
                            else
                            {
                                Debug.LogFormat("[Memory Wires #{0}] All correct wires cut. Progressing to stage {1}.", moduleID, stage + 1);
                                Generate(stage, Random.Range(0, 6));
                                StartCoroutine(Stageset());
                            }
                        }
                    }
                    else
                    {
                        module.HandleStrike();
                        if (!moduleSolved)
                        {
                            stage = 0;
                            Debug.LogFormat("[Memory Wires #{0}] Incorrect. Resetting to stage 1.", moduleID);
                            for (int i = 0; i < 4; i++)
                                stagelights[i].material = lightstates[0];
                            Generate(0, Random.Range(0, 6));
                            StartCoroutine(Stageset());
                        }
                    }                   
                }
                return false;
            };
        }
    }

    private void Generate(int s, int n)
    {
        bool[] c = new bool[6];
        for (int i = 0; i < 6; i++)
            colset[s][i] = Random.Range(0, 5);
        switch (edgerule)
        {
            case 2:
                switch (n)
                {
                    case 0:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[s].Count(x => x == colset[s][i]) < 2;
                        break;
                    case 1:
                        for (int i = 0; i < 6; i++)
                            c[i] = new int[] { 1, 2, 4, 6, 10, 12, 16, 18, 22, 28, 30}.Contains((6 * s) + i);
                        break;
                    case 2:
                        for (int i = 0; i < 6; i++)
                            c[i] = Enumerable.Range(0, s + 1).Select(x => colset[x][i]).Count(x => x % 4 == 0) % 2 == 0;
                        break;
                    case 3:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[s][i] != 3 && (s < 1 || !cuts[s - 1][i]);
                        break;
                    case 4:
                        for (int i = 0; i < 6; i++)
                            c[i] = s < 1 ? (i > 0 && colset[s][i - 1] == 2) || (i < 5 && colset[s][i + 1] == 2) : ((i > 0 && !cuts[s - 1][i - 1]) || (i < 5 && !cuts[s - 1][i + 1]));
                        break;
                    default:
                        for(int i = 0; i < 6; i++)
                            c[i] = Enumerable.Range(0, s + 1).Select(x => colset[x][i]).Where((x, k) => x == 1 || cuts[k][i]).Count() % 2 == 1;
                        break;
                }
                break;
            case 1:
                switch ((s * 6) + n)
                {
                    case 0:
                        for (int i = 0; i < 6; i++)
                            c[i] = i % 2 == 0 && colset[0][i] != 1;
                        break;
                    case 1:
                        for (int i = 0; i < 6; i++)
                            c[i] = (i > 0 && colset[0][i - 1] == colset[0][i]) || (i < 5 && colset[0][i + 1] == colset[0][i]);
                        break;
                    case 2:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[0].Count(x => x == colset[0][i]) > 2;
                        break;
                    case 3:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[0][i] > 2;
                        break;
                    case 4:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[0].Contains(0) && i > colset[0].TakeWhile(x => x > 0).Count();
                        break;
                    case 5:
                        for(int i = 0; i < 6; i++)
                            c[i] = (i == 0 || colset[0][i - 1] != 2) && (i == 5 || colset[0][i + 1] != 2);
                        break;
                    case 6:
                        for (int i = 0; i < 6; i++)
                            c[i] = (colset[0][i] == 0 ^ colset[1][i] == 0) ^ (colset[0][i] == 2 ^ colset[1][i] == 2);
                        break;
                    case 7:
                        for (int i = 0; i < 6; i++)
                            c[i] = i < cuts[0].TakeWhile(x => !x).Count();
                        break;
                    case 8:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[0][i] < 4 && colset[1][i] < 4;
                        break;
                    case 9:
                        for (int i = 0; i < 6; i++)
                            c[i] = !cuts[0][i] && (colset[1][i] == 1 || colset[1][i] == 2);
                        break;
                    case 10:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[0][i] == colset[1][i];
                        break;
                    case 11:
                        for (int i = 0; i < 6; i++)
                            c[i] = i > 0 && colset[0][i - 1] == 3;
                        break;
                    case 12:
                        for (int i = 0; i < 6; i++)
                            c[i] = Enumerable.Range(0, 2).Select(x => cuts[x][i]).Count(x => x) == 1;
                        break;
                    case 13:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[2][i] != 1 && (colset[0][i] == 1 || colset[1][i] == 1);
                        break;
                    case 14:
                        for (int i = 0; i < 6; i++)
                            c[i] = i % 2 == 1 && Enumerable.Range(0, 3).Select(x => colset[x][i]).Any(x => x % 3 == 0);
                        break;
                    case 15:
                        for (int i = 0; i < 6; i++)
                            c[i] = !cuts[1][i] && colset[2][i] < 4;
                        break;
                    case 16:
                        for (int i = 0; i < 6; i++)
                            c[i] = Enumerable.Range(0, 3).Select(x => colset[x][i]).Distinct().Count() > 2;
                        break;
                    case 17:
                        for (int i = 0; i < 6; i++)
                            c[i] = Enumerable.Range(0, 3).Select(x => colset[x][i]).Count(x => x == 2) == 1;
                        break;
                    case 18:
                        for (int i = 0; i < 6; i++)
                            c[i] = cuts[2][i] && colset[3][i] < 2;
                        break;
                    case 19:
                        for (int i = 0; i < 6; i++)
                            c[i] = cuts[1][i] && (colset[3][i] == 2 || colset[3][i] == 4);
                        break;
                    case 20:
                        for (int i = 0; i < 6; i++)
                            c[i] = Enumerable.Range(0, 4).Select(x => colset[x][i]).Count(x => x == 3) == 1;
                        break;
                    case 21:
                        for (int i = 0; i < 6; i++)
                            c[i] = Enumerable.Range(0, 4).Select(x => colset[x][i]).Count(x => x == 4) % 2 == 0;
                        break;
                    case 22:
                        for (int i = 0; i < 6; i++)
                            c[i] = i < 5 && cuts[0][i + 1];
                        break;
                    case 23:
                        for (int i = 0; i < 6; i++)
                            c[i] = Enumerable.Range(0, 3).Select(x => cuts[x][i]).Count(x => x) == 2;
                        break;
                    case 24:
                        for (int i = 0; i < 6; i++)
                            c[i] = !Enumerable.Range(0, 4).Select(x => colset[x][i]).Contains(colset[4][i]);
                        break;
                    case 25:
                        for (int i = 0; i < 6; i++)
                            c[i] = (i > 0 && colset[1][i - 1] == colset[4][i]) || (i < 5 && colset[1][i + 1] == colset[4][i]);
                        break;
                    case 26:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[4][i] != 2 && (i == 0 || colset[3][i - 1] != 2) && (i == 5 || colset[3][i + 1] != 2);
                        break;
                    case 27:
                        for (int i = 0; i < 6; i++)
                            c[i] = !cuts[0][i] && ((i > 0 && colset[4][i - 1] % 3 == 0) || (i < 5 && colset[4][i + 1] % 3 == 0));
                        break;
                    case 28:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[4][i] > 3 ^ Enumerable.Range(0, 4).Select(x => cuts[x][i]).Count(x => x) % 2 == 0;
                        break;
                    default:
                        for (int i = 0; i < 6; i++)
                            c[i] = (i == 0 || (colset[4][i - 1] != 1 && colset[2][i - 1] != 1)) && (i == 5 || (colset[4][i + 1] != 1 && colset[2][i + 1] != 1));
                        break;
                }
                break;
            default:
                switch ((s * 6) + n)
                {
                    case 0:
                        for (int i = 0; i < 6; i++)
                            c[i] = i > 0 && i < 5 && colset[0][i - 1] != colset[0][i] && colset[0][i + 1] != colset[0][i];
                        break;
                    case 1:
                        for (int i = 0; i < 6; i++)
                            c[i] = (i == 0 || colset[0][i - 1] < 4) && (i == 5 || colset[0][i + 1] < 4);
                        break;
                    case 2:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[0].Contains(1) && i < colset[0].TakeWhile(x => x != 1).Count();
                        break;
                    case 3:
                        for (int i = 0; i < 6; i++)
                            c[i] = (i > 0 && colset[0][i - 1] == 2) || (i < 5 && colset[0][i + 1] == 0);
                        break;
                    case 4:
                        for (int i = 0; i < 6; i++)
                            c[i] = i % 2 == 1 && colset[0][i] != 3;
                        break;
                    case 5:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[0].Count(x => x == colset[0][i]) == 2;
                        break;
                    case 6:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[1].Count(x => x == colset[1][i]) < colset[0].Count(x => x == colset[1][i]);
                        break;
                    case 7:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[0][i] < 1 || colset[1][i] < 1;
                        break;
                    case 8:
                        for (int i = 0; i < 6; i++)
                            c[i] = !cuts[0][i] && colset[0][i] > 2;
                        break;
                    case 9:
                        for (int i = 0; i < 6; i++)
                            c[i] = !cuts[0][i] && colset[1][i] != 2;
                        break;
                    case 10:
                        for (int i = 0; i < 6; i++)
                            c[i] = i > 0 && colset[0][i - 1] == 1;
                        break;
                    case 11:
                        for (int i = 0; i < 6; i++)
                            c[i] = (i > 0 && !cuts[0][i - 1]) || (i < 5 && !cuts[0][i + 1]);
                        break;
                    case 12:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[2][i] != 1 && ((i > 0 && cuts[1][i - 1]) || (i < 5 && cuts[1][i + 1]));
                        break;
                    case 13:
                        for (int i = 0; i < 6; i++)
                            c[i] = ((i > 0 && colset[0][i - 1] < 1) || (i < 5 && colset[0][i + 1] < 1));
                        break;
                    case 14:
                        for (int i = 0; i < 6; i++)
                            c[i] = !cuts[0][i] && !cuts[1][i];
                        break;
                    case 15:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[0][i] != 2 && colset[1][i] != 2;
                        break;
                    case 16:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[2][i] > 3 || ((i > 0 && colset[0][i - 1] > 3) || (i < 5 && colset[0][i + 1] > 3));
                        break;
                    case 17:
                        for (int i = 0; i < 6; i++)
                            c[i] = (!cuts[0][i] && colset[0][i] == 3) || (!cuts[1][i] && colset[1][i] == 3);
                        break;
                    case 18:
                        for (int i = 0; i < 6; i++)
                            c[i] = !cuts[0][i] && (colset[3][i] == 0 || colset[3][i] == 2);
                        break;
                    case 19:
                        for (int i = 0; i < 6; i++)
                            c[i] = (Enumerable.Range(0, 4).Count(x => i > 0 && colset[x][i - 1] == 1) + Enumerable.Range(0, 4).Count(x => i < 5 && colset[x][i + 1] == 1)) % 2 == 0;
                        break;
                    case 20:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[3].Count(x => x == colset[3][i]) < colset[2].Count(x => x == colset[3][i]);
                        break;
                    case 21:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[3][i] != 3 && colset[3][i] != colset[2][i];
                        break;
                    case 22:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[3][i] < 4 && Enumerable.Range(0, 3).Count(x => cuts[x][i]) % 2 == 1;
                        break;
                    case 23:
                        for (int i = 0; i < 6; i++)
                            c[i] = colset[3][i] == colset[1][i];
                        break;
                    case 24:
                        for (int i = 0; i < 6; i++)
                            c[i] = (i > 0 && colset[0][i - 1] == colset[4][i]) || (i < 5 && colset[0][i + 1] == colset[4][i]);
                        break;
                    case 25:
                        for (int i = 0; i < 6; i++)
                            c[i] = (colset[4][i] == 1 && ((i > 0 && cuts[2][i - 1]) || (i < 5 && cuts[2][i + 1]))) || (colset[4][i] != 1 && ((i > 0 && !cuts[2][i - 1]) || (i < 5 && !cuts[2][i + 1])));
                        break;
                    case 26:
                        for (int i = 0; i < 6; i++)
                            c[i] = !cuts[3][i] && Enumerable.Range(0, 3).Count(x => cuts[x][i]) > 1;
                        break;
                    case 27:
                        for (int i = 0; i < 6; i++)
                            c[i] = (i > 0 && colset[1][i - 1] == 2) || (i < 5 && colset[1][i + 1] == 3);
                        break;
                    case 28:
                        for (int i = 0; i < 6; i++)
                            c[i] = Enumerable.Range(0, 5).Select(x => colset[x][i]).Distinct().Count() > 3;
                        break;
                    default:
                        for (int i = 0; i < 6; i++)
                            c[i] = ((i > 0 && colset[4][i - 1] % 4 == 0) || (i < 5 && colset[4][i + 1] % 4 == 0)) && Enumerable.Range(0, 4).Select(x => colset[x][i]).Where((x, k) => !cuts[x][k]).GroupBy(x => x).Any(x => x.Count() > 1);
                        break;
                }
                break;
        }
        if (c.All(x => !x))
            Generate(s, (n + Random.Range(1, 6)) % 6);
        else
        {
            for (int i = 0; i < 6; i++)
            {
                cuts[s][i] = c[i];
                docut[i] = c[i];
            }
            dispnum = n + 1;
            Debug.LogFormat("[Memory Wires #{0}] The colours of the wires are: {1}.", moduleID, string.Join(", ", colset[s].Select(x => logcol[x]).ToArray()));
            Debug.LogFormat("[Memory Wires #{0}] The digit displayed on the screen is {1}.", moduleID, dispnum);
            Debug.LogFormat("[Memory Wires #{0}] Cut the wire{1} connected to{2}port{3} {4}.", moduleID, docut.Count(x => x) > 1 ? "s" : "", docut.Count(x => x) > 1 ? " the " : " ", docut.Count(x => x) > 1 ? "s:" : "", string.Join(", ", Enumerable.Range(0, 6).Where(x => docut[x]).Select(x => ((6 * s) + x + 1).ToString()).ToArray()));
        }
    }

    private IEnumerator Stageset()
    {
        cuttable = false;
        labels[6].text = "";
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 6; i++)
        {
            StartCoroutine(Hubmove(i, false));
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1);
        labels[6].text = dispnum.ToString();
        cuttable = true;
    }

    private IEnumerator Hubmove(int h, bool up)
    {
        Vector3 ht = hubs[h].localPosition;
        float e = 0;
        if (up)
        {
            yield return new WaitForSeconds(0.2f);
            labels[h].text = ((6 * stage) + h + 1).ToString();
            wrends[h].material = wcols[colset[stage][h]];
            wrends[h + 6].material = wcols[colset[stage][h]];
            wireobjs[h].SetActive(true);
            cutwires[h].SetActive(false);
            while(e < 0.4f)
            {
                e += Time.deltaTime;
                hubs[h].localPosition = new Vector3(ht.x, (e / 2) - 0.2f, ht.z);
                yield return null;
            }
            hubs[h].localPosition = new Vector3(ht.x, 0, ht.z);
        }
        else
        {
            while (e < 0.4f)
            {
                e += Time.deltaTime;
                hubs[h].localPosition = new Vector3(ht.x, - e / 2, ht.z);
                yield return null;
            }
            hubs[h].localPosition = new Vector3(ht.x, -0.2f, ht.z);
            StartCoroutine(Hubmove(h, true));
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} cut <1-30> [Cuts wires connected to the specified ports. Separate with spaces.]";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if (commands[0] == "cut")
        {
            int w = 0;
            List<int> cutpending = new List<int> { };
            for (int i = 1; i < commands.Length; i++)
            {
                if(int.TryParse(commands[i], out w))
                {
                    if ((w - 1) / 6 == stage)
                    {
                        cutpending.Add((w - 1) % 6);
                        if (wireobjs[cutpending[i - 1]].activeSelf == false)
                        {
                            yield return "sendtochaterror!f The wire connected to port " + commands[i] + " has already been cut.";
                            yield break;
                        }
                    }
                    else
                    {
                        yield return "sendtochaterror!f Port " + commands[i] + " is not present.";
                        yield break;
                    }
                }
                else
                {
                    yield return "sendtochaterror!f Invalid port number: " + commands[i];
                    yield break;
                }
            }
            cutpending = cutpending.Distinct().OrderBy(x => x).ToList();
            for(int i = 0; i < cutpending.Count; i++)
            {
                yield return new WaitForSeconds(0.1f);
                wires[cutpending[i]].OnInteract();
            }
        }
        else
            yield return "sendtochaterror!f Cut wires with the \"cut\" command.";
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            while (!cuttable)
                yield return null;
            yield return new WaitForSeconds(0.2f);
            for(int i = 0; i < 6; i++)
            {
                if (docut[i])
                {
                    yield return new WaitForSeconds(0.1f);
                    wires[i].OnInteract();
                }
            }
        }
    }
}
