using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhosOnGasScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Transform[] bpos;
    public Renderer[] lcds;
    public Material[] lmats;
    public TextMesh[] displays;

    private Vector3[] pos = new Vector3[6];
    private int stage;
    private int line;
    private int validbutton;
    private int prog;
    private List<int> ans = new List<int> { };
    private string[] labels = new string[6];
    private List<int> symbols = new List<int> { };
    private bool[] validsymbols = new bool[36];
    private bool pressable = false;
    private KMAudio.KMAudioRef warning;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        foreach (TextMesh d in displays)
            d.text = "";
        for(int i = 0; i < 6; i++)
        {
            pos[i] = bpos[i].localPosition;
            bpos[i].localPosition = new Vector3(pos[i].x, -0.143f, pos[i].z);
        }
        foreach (Renderer r in lcds)
            r.material = lmats[0];
        module.OnActivate += delegate () { StartCoroutine(Stage(1, false)); };
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved && pressable)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    if (stage % 2 == 0)
                    {
                        if (prog > 0)
                            bpos[ans[prog - 1]].localPosition = new Vector3(pos[b].x, 0, pos[b].z);
                        if (b == ans[prog])
                        {
                            if (prog >= ans.Count() - 1)
                            {
                                stage++;
                                prog = 3;
                                validbutton = b;
                                pressable = false;
                                ans.Clear();
                                StartCoroutine(Stage(2, true));
                            }
                            else
                            {
                                prog++;
                                bpos[ans[prog - 1]].localPosition = new Vector3(pos[b].x, -0.08f, pos[b].z);
                            }
                        }
                        else
                        {
                            module.HandleStrike();
                            pressable = false;
                            StartCoroutine(Stage(1, true));
                        }
                    }
                    else
                    {
                        StopCoroutine("Timer");
                        if (warning != null)
                            warning.StopSound();
                        if (b == validbutton)
                        {
                            prog--;
                            if(prog <= 0)
                            {
                                stage++;
                                for (int i = 0; i < 3; i++)
                                    lcds[i].material = lmats[stage >> 1 <= i ? 0 : 1];
                                if (stage >= 6)
                                {
                                    module.HandlePass();
                                    moduleSolved = true;
                                    displays[0].text = "";
                                    MoveButtons(b, true);
                                }
                                else
                                    StartCoroutine(Stage(1, true));
                            }
                            else
                            {
                                for (int i = 0; i < 3; i++)
                                    lcds[i].material = lmats[prog <= i ? (stage >> 1 < i ? 0 : 1) : 2];
                                ans.Add(symbols[validbutton] % 4);
                                StartCoroutine(Stage(2, true));
                            }
                        }
                        else
                        {
                            stage--;
                            module.HandleStrike();
                            for (int i = 0; i < 3; i++)
                                lcds[i].material = lmats[stage >> 1 <= i ? 0 : 1];
                            StartCoroutine(Stage(1, true));
                        }
                    }
                }
                return false;
            };
        }
    }

    private void OnDestroy()
    {
        if (warning != null)
        {
            warning.StopSound();
            warning = null;
        }
    }

    private IEnumerator Stage(int phase, bool up)
    {
        pressable = false;
        displays[0].text = "";
        if (up)
        {
            MoveButtons(validbutton, true);
            yield return new WaitForSeconds(0.9f);
        }
        if (phase == 1)
        {
            prog = 0;
            ans.Clear();
            line = Random.Range(0, 13);
            validbutton = Random.Range(0, 6);
            string[] validline = WhoOGTranscript.words[line];
            Debug.LogFormat("[Who's on Gas #{0}] The displayed word is \"{1}\".", moduleID, WhoOGTranscript.displays[line]);
            Debug.LogFormat("[Who's on Gas #{0}] The line of text this corresponds to is: \"{1}\".", moduleID, validline.Join());
            labels[validbutton] = validline.PickRandom();
            for(int i = 0; i < 6; i++)
            {
                if (i != validbutton)
                {
                    do
                    {
                        labels[i] = WhoOGTranscript.words.PickRandom().PickRandom();
                    } while (labels.Count(x => x == labels[i]) > 1);
                }
                displays[i + 1].text = labels[i];
                displays[i + 1].fontSize = (11 - labels[i].Length) * 50;
            }
            Debug.LogFormat("[Who's on Gas #{0}] The labels of the buttons are: {1}", moduleID, string.Join(", ", labels));
            for(int i = 0; i < validline.Length; i++)
                if(labels.Contains(validline[i]) && (ans.Count() < 1 || labels[ans.Last()] != validline[i]))
                    for(int j = 0; j < 6; j++)
                        if(labels[j] == validline[i])
                        {
                            ans.Add(j);
                            break;
                        }
            Debug.LogFormat("[Who's on Gas #{0}] Press the buttons in the order: {1}", moduleID, string.Join(", ", ans.Select(x => labels[x]).ToArray()));
            MoveButtons(0, false);
            yield return new WaitForSeconds(0.9f);
            pressable = true;
            displays[0].text = WhoOGTranscript.displays[line];
        }
        else
        {
            validsymbols = new bool[36];
            int strk = Mathf.Min(info.GetStrikes(), 2);
            bool deto = Random.Range(0, 2) == 0;
            Debug.Log(validbutton >> 1);
            Debug.LogFormat("[Who's on Gas #{0}] The display is \"{1}\".", moduleID, deto ? "DETONATE" : "VENT GAS");
            Debug.LogFormat("[Who's on Gas #{0}] The valid symbol does {1}belong to the {2} of the {3} strike grid{4}.", moduleID, deto ? "not " : "", new string[6] { "top row", "left column", "middle row", "middle column", "bottom row", "right column"}[validbutton], new string[3] { "zero", "one", "two plus"}[strk], prog < 3 ? " and does not belong to the " + string.Join(" or ", ans.Select(x => (x >> 1 > 0 ? "bottom" : "top") + (x % 2 > 0 ? "-right" : "-left")).ToArray()) + " of its cell" : "");
            if (validbutton % 2 == 0)
                for (int i = 0; i < 12; i++)
                    validsymbols[((validbutton >> 1) * 12) + i] = true;
            else
                for (int i = 0; i < 12; i++)
                    validsymbols[((validbutton >> 1) * 4) + ((i >> 2) * 12) + (i % 4)] = true;
            if (deto)
                for (int i = 0; i < 36; i++)
                    validsymbols[i] ^= true;
            if (prog < 3)
                for (int i = 0; i < 36; i++)
                    if (ans.Contains(i % 4))
                        validsymbols[i] = false;
            MoveButtons(validbutton, false);
            validbutton = Random.Range(0, 6);
            symbols = Enumerable.Range(0, 36).Where(x => !validsymbols[x]).ToArray().Shuffle().Take(5).ToList();
            Debug.Log(string.Join(", ", symbols.Select(x => x.ToString()).ToArray()));
            symbols.Insert(validbutton, Enumerable.Range(0, 36).Where(x => validsymbols[x]).PickRandom());
            Debug.Log(symbols[validbutton]);
            labels = symbols.Select(x => WhoOGTranscript.grids[strk][x]).ToArray();
            for (int i = 1; i < 7; i++)
            {
                displays[i].text = labels[i - 1];
                displays[i].fontSize = 450;
            }
            Debug.LogFormat("[Who's on Gas #{0}] The labels are: {1}", moduleID, string.Join(", ", labels));
            Debug.LogFormat("[Who's on Gas #{0}] Press the button with the label \"{1}\".", moduleID, labels[validbutton]);
            yield return new WaitForSeconds(0.9f);
            pressable = true;
            StartCoroutine("Timer");
            for (int i = 0; i < prog; i++)
                lcds[i].material = lmats[2];
            displays[0].text = deto ? "DETONATE" : "VENT GAS";
        }
    }

    private void MoveButtons(int x, bool up)
    {
        int[] adj = new int[6] { -1, -1, -1, -1, -1, -1 };
        bool[] adjto = new bool[6];
        int iter = 1;
        adj[x] = 0;
        while(adj.Any(k => k < 0))
        {
            for (int i = 0; i < 6; i++)
                if (adj[i] < 0 && ((i > 1 && adj[i - 2] >= 0) || (i < 4 && adj[i + 2] >= 0) || adj[i ^ 1] >= 0))
                    adjto[i] = true;
            for(int i = 0; i < 6; i++)
                if (adjto[i])
                {
                    adjto[i] = false;
                    adj[i] = iter;
                }
            iter++;
        }
        for (int i = 0; i < 6; i++)
            StartCoroutine(MoveButton(i, adj[i] * 0.1f, up));
    }

    private IEnumerator MoveButton(int i, float delay, bool up)
    {
        Vector2 p = new Vector2(pos[i].x, pos[i].z);
        float e = up ? 0 : 1;
        int v = up ? 2 : -2;
        yield return new WaitForSeconds(delay);
        while (up ? (e < 1) : (e > 0))
        {
            e += Time.deltaTime * v;
            bpos[i].localPosition = new Vector3(p.x, -0.143f * e, p.y);
            yield return null;
        }
        bpos[i].localPosition = new Vector3(p.x, -0.143f * e, p.y);
    }

    private IEnumerator Timer()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.NeedyActivated, transform);
        yield return new WaitForSeconds(55);
        warning = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.NeedyWarning, transform);
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < prog; j++)
                lcds[j].material = lmats[i % 2 == 1 ? (j < stage >> 1 ? 1 : 0) : 2];
            yield return new WaitForSeconds(0.25f);
        }
        warning.StopSound();
        pressable = false;
        Debug.LogFormat("[Who's on Gas #{0}] Times up!", moduleID);
        module.HandleStrike();
        stage--;
        StartCoroutine(Stage(1, true));
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <t/m/b><l/r> [Position of button to press. Commands can be chained with spaces in the first phase of each stage.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        List<int> p = new List<int> { };
        if(stage % 2 == 1 && commands.Length > 1)
        {
            yield return "sendtochaterror!f Only one button may be pressed at a time during the second phase.";
            yield break;
        }
        for(int i = 0; i < commands.Length; i++)
        {
            if (commands[i].Length < 1)
                continue;
            if(commands[i].Length != 2)
            {
                yield return "sendtochaterror!f " + commands[i] + " is an invalid command.";
                yield break;
            }
            int x = "lr".IndexOf(commands[i][1].ToString());
            int y = "tmb".IndexOf(commands[i][0].ToString()) * 2;
            if(x < 0 || y < 0)
            {
                yield return "sendtochaterror!f " + commands[i] + " is an invalid command.";
                yield break;
            }
            p.Add(x + y);
        }
        for(int i = 0; i < p.Count(); i++)
        {
            while (!pressable)
                yield return true;
            yield return null;
            buttons[p[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
            if (!pressable)
                yield break;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            while (!pressable)
                yield return stage % 2 < 1;
            if(stage % 2 < 1)
            {
                while (pressable)
                {
                    yield return new WaitForSeconds(0.1f);
                    buttons[ans[prog]].OnInteract();
                }
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                buttons[validbutton].OnInteract();
            }
        }
    }
}
