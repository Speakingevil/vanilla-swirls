using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class SimonSwivelsScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> simons;
    public KMSelectable knob;
    public Transform krot;
    public Renderer[] srends;
    public Renderer krend;
    public Renderer[] lrends;
    public Material[] scols;
    public Material[] kcols;
    public Material[] lcols;
    public Light[] slights;
    public Light[] llights;
    public GameObject matstore;

    private readonly int[,] coltable = new int[16, 4]
    {
        { 3, 2, 0, 1}, { 2, 3, 1, 0}, { 1, 2, 3, 0}, { 2, 3, 0, 1},
        { 0, 3, 1, 2}, { 3, 0, 2, 1}, { 1, 3, 0, 2}, { 2, 0, 1, 3},
        { 2, 1, 3, 0}, { 1, 2, 0, 3}, { 3, 0, 1, 2}, { 0, 2, 3, 1},
        { 1, 0, 3, 2}, { 3, 1, 0, 2}, { 3, 2, 1, 0}, { 1, 3, 2, 0}
    };
    private readonly bool[,,] ledtable = new bool[5, 4, 6]
    {
        { { true, false, true, false, false, true}, { false, true, false, false, true, false}, { false, true, true, true, false, true}, { true, false, true, true, false, false} },
        { { false, true, false, false, true, true}, { true, true, true, false, true, false}, { false, true, false, true, true, false}, { true, false, true, true, false, true} },
        { { false, false, true, false, true, false}, { true, true, true, true, false, true}, { false, true, false, true, true, true}, { false, false, true, false, false, false} },
        { { true, false, true, true, false, false}, { false, true, true, false, true, false}, { true, false, false, true, false, true}, { false, true, false, true, false, true} },
        { { true, false, true, true, true, true}, { true, false, false, true, false, false}, { false, false, true, true, false, true}, { true, true, false, true, true, false} }
    };
    private readonly string[] collog = new string[4] { "Blue", "Yellow", "Green", "Red" };
    private List<int> sarr = new List<int> { 0, 1, 2, 3 };
    private int[] varconds = new int[4];
    private int starttime = 1;
    private int modtotal = 1;
    private int kpick;
    private List<int> seq = new List<int> { };
    private bool[] infconds = new bool[2];
    private int anscol = -1;
    private List<int> ans = new List<int> { };
    private int kpos;
    private int seqpos;
    private int resets;
    private bool sound;
    private int safecol;
    private bool lowtime;
    private IEnumerator flash;
    private KMAudio.KMAudioRef warning;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved = true;

    private void Start()
    {
        flash = Flash(0);
        moduleID = ++moduleIDCounter;
        kpick = Random.Range(0, 4);
        krend.material = kcols[kpick];
        sarr = sarr.Shuffle();
        float scale = module.transform.lossyScale.x;
        for (int i = 0; i < 8; i++)
            llights[i].range *= scale;
        LEDO();
        for (int i = 0; i < 4; i++)
        {
            slights[i].range *= scale;
            slights[i].color = new Color[] { new Color(0.5f, 0.5f, 1), new Color(1, 1, 0), new Color(0, 1, 0), new Color(1, 0, 0) }[sarr[i]];
            slights[i].enabled = false;
            srends[i].material = scols[sarr[i]];
        }
        matstore.SetActive(false);
        starttime = (int)info.GetTime();
        module.OnActivate = Activate;
    }

    private void LEDO()
    {
        for (int i = 0; i < 8; i++)
        {
            llights[i].enabled = false;
            lrends[i].material = lcols[1];
        }
    }

    private void Activate()
    { 
        for (int i = 0; i < 2; i++)
            seq.Add(Random.Range(0, 4));
        modtotal = info.GetSolvableModuleNames().Count;
        Debug.LogFormat("[Simon Swivels #{0}] The colours of the buttons, in clockwise order, are: {1}.", moduleID, string.Join(", ", sarr.Select(x => collog[x]).ToArray()));
        Debug.LogFormat("[Simon Swivels #{0}] The knob is {1}.", moduleID, collog[kpick]);
        infconds[0] = info.GetSerialNumberLetters().Any(x => "AEIOU".Contains(x.ToString())) ^ info.GetBatteryCount() > 1;
        infconds[1] = info.IsIndicatorOn(Indicator.FRK) ^ info.IsPortPresent(Port.Parallel);
        Debug.LogFormat("[Simon Swivels #{0}] The {1} set of rules is used to assign the Up position.", moduleID, infconds[0] ? "left" : "right");
        Debug.LogFormat("[Simon Swivels #{0}] The {1} set of rules is used to set the knob's position.", moduleID, infconds[1] ? "left" : "right");
        Gen();
        moduleSolved = false;
        StartCoroutine("FlashSeq");
        knob.OnInteract = delegate ()
        {
            kpos += 1;
            kpos %= 4;
            krot.Rotate(0, 0, 90);
            return false;
        };
        foreach(KMSelectable simon in simons)
        {
            int b = simons.IndexOf(simon);
            simon.OnInteract = delegate ()
            {
                simon.AddInteractionPunch(0.7f);
                StopCoroutine(flash);
                foreach (Light l in slights)
                    l.enabled = false;
                flash = Flash(b);
                if (!moduleSolved)
                {
                    Debug.LogFormat("[Simon Swivels #{0}] {1} pressed.", moduleID, collog[sarr[b]]);
                    StopCoroutine("FlashSeq");
                    StopCoroutine("Pause");
                    if (!sound)
                    {
                        sound = true;
                        StartCoroutine("NeedyKnob");
                    }
                    if (kpos != sarr.IndexOf(anscol))
                    {
                        module.HandleStrike();
                        Debug.LogFormat("[Simon Swivels #{0}] Knob is in an incorrect position.", moduleID);
                        StartCoroutine("FlashSeq");
                    }
                    else if (ans[seqpos] != sarr[b])
                    {
                        module.HandleStrike();
                        StartCoroutine("FlashSeq");
                    }
                    else
                    {
                        if (seqpos < seq.Count() - 1)
                        {
                            StartCoroutine("Pause");
                            seqpos++;
                        }
                        else if (seq.Count() < 5)
                        {
                            seqpos = 0;
                            seq.Add(Random.Range(0, 4));
                            StartCoroutine("FlashSeq");
                            Gen();
                        }
                        else
                        {
                            moduleSolved = true;
                            module.HandlePass();
                            StopCoroutine("NeedyKnob");
                            LEDO();
                        }
                    }
                }
                StartCoroutine(flash);
                return false;
            };
        }
	}

    private void Gen()
    {
        Debug.LogFormat("[Simon Swivels #{0}] The sequence of flashes is: {1}.", moduleID, string.Join(", ", seq.Select(x => collog[x]).ToArray()));
        int up = 0;
        if (infconds[0])
        {
            if (seq.Last() == kpick)
                up = 3;
            else
            {
                List<bool> t = new List<bool> { };
                for (int i = 0; i < seq.Count() - 1; i++)
                    t.Add(seq[i] == seq[i + 1]);
                if (t.Any(x => x))
                    up = 1;
                else if (!seq.Contains(2))
                    up = 2;
            }
        }
        else 
        {
            if (sarr.IndexOf(seq.Last()) == 2)
                up = 2;
            else if (seq.Count(x => x == 0) != 1)
            {
                List<bool> t = new List<bool> { };
                for (int i = 0; i < seq.Count() - 1; i++)
                    t.Add((seq[i] == 1 && seq[i + 1] == 3) || (seq[i] == 3 && seq[i + 1] == 1));
                up = t.Any(x => x) ? 1 : 3;
            }
        }
        Debug.LogFormat("[Simon Swivels #{0}] The {1} button faces up.", moduleID, collog[up]);
        up = sarr.IndexOf(up);
        if (infconds[1])
        {
            int[] s = seq.Select(x => sarr.IndexOf(x)).ToArray();
            List<bool> t = new List<bool> { };
            for (int i = 0; i < s.Length - 1; i++)
                t.Add(Mathf.Abs(s[i] - s[i + 1]) == 2);
            if (t.Count(x => x) > 1)
                up += 3;
            else if (s.Count(x => x == 0) == 1)
                up += 2;
            else if (seq.Count(x => x == 3) > 1)
                up += 1;
        }
        else
        {
            if (seq.GroupBy(x => x).Any(x => x.Count() > 2))
                up += 2;
            else if (sarr[(up + 2) % 4] == seq.Last())
                up += 1;
            else if (!seq.Contains(kpick))
                up += 3;
        }
        up %= 4;
        anscol = sarr[up];
        Debug.LogFormat("[Simon Swivels #{0}] Set the knob to face the {1} button when inputting.", moduleID, collog[anscol]);
        ans = Ansgen(anscol, varconds);
        Debug.LogFormat("[Simon Swivels #{0}] Press the buttons in the order: {1}", moduleID, string.Join(", ", ans.Select(x => collog[x]).ToArray()));
    }

    private void Update()
    {
        if (!moduleSolved)
        {
            int[] v = new int[4];
            v[0] = Mathf.Clamp(resets, 0, 6) / 2;
            v[1] = starttime == 0 ? 0 : Mathf.Clamp(3 - (4 * ((int)info.GetTime()) / starttime), 0, 3);
            v[2] = (4 * info.GetSolvedModuleNames().Count) / modtotal;
            v[3] = Mathf.Clamp(info.GetStrikes(), 0, 3);
            if (v.Select((x, i) => x != varconds[i]).Any(x => x))
            {
                Debug.Log(v.Select(x => x.ToString()).Join());
                if (v[anscol] != varconds[anscol])
                {
                    ans = Ansgen(anscol, v);
                    Debug.LogFormat("[Simon Swivels #{0}] Press sequence variable change. New input sequence: {1}.", moduleID, string.Join(", ", ans.Select(x => collog[x]).ToArray()));
                }
                for (int i = 0; i < 4; i++)
                    varconds[i] = v[i];
            }
        }
    }

    private List<int> Ansgen(int a, int[] v)
    {
        List<int> n = new List<int> { };
        for (int i = 0; i < seq.Count(); i++)
            n.Add(coltable[(4 * a) + v[a], seq[i]]);
        return n;
    }

    private IEnumerator FlashSeq()
    {
        int[] s = seq.Select(x => sarr.IndexOf(x)).ToArray();
        yield return new WaitForSeconds(1);
        seqpos = 0;
        while (!moduleSolved)
        {
            for (int i = 0; i < s.Length; i++)
            {
                StartCoroutine(Flash(s[i]));
                yield return new WaitForSeconds(1);
            }
            yield return new WaitForSeconds(2);
        }
    }

    private IEnumerator Flash(int k)
    {
        if (sound)
            Audio.PlaySoundAtTransform("Beep" + sarr[k], simons[k].transform);
        slights[k].enabled = true;
        yield return new WaitForSeconds(0.5f);
        slights[k].enabled = false;
    }

    private IEnumerator Pause()
    {
        yield return new WaitForSeconds(6);
        StartCoroutine("FlashSeq");
    }

    private IEnumerator NeedyKnob()
    {
        while (!moduleSolved)
        {
            resets++;
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.NeedyActivated, transform);
            int k = Random.Range(0, 5);
            safecol = k == 4 ? kpick : k;
            int[] p = new int[2] { Random.Range(0, 3), Random.Range(0, 3)};
            bool[] l = new bool[8];
            for (int i = 0; i < 8; i++)
                l[i] = ledtable[k, p[1] + (i / 4), p[0] + (i % 4)];
            LEDI(l);
            Debug.LogFormat("[Simon Swivels #{0}] Timer active. The pattern of LEDs is:", moduleID);
            Debug.LogFormat("[Simon Swivels #{0}] {1}", moduleID, string.Join("", l.Take(4).Select(x => x ? "\u25a1" : "\u25a0").ToArray()));
            Debug.LogFormat("[Simon Swivels #{0}] {1}", moduleID, string.Join("", l.Skip(4).Select(x => x ? "\u25a1" : "\u25a0").ToArray()));
            Debug.LogFormat("[Simon Swivels #{0}] Set the knob to face the {1} button when the timer expires.", moduleID, k == 4 ? collog[kpick] : collog[k]);
            yield return new WaitForSeconds(35);
            warning = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.NeedyWarning, transform);
            lowtime = true;
            for (int i = 0; i < 25; i++)
            {
                if (i % 2 == 0)
                    LEDO();
                else
                    LEDI(l);
                yield return new WaitForSeconds(0.2f);
            }
            warning.StopSound();
            lowtime = false;
            if (kpos == sarr.IndexOf(safecol))
                Debug.LogFormat("[Simon Swivels #{0}] Timer reset.", moduleID);
            else
            {
                module.HandleStrike();
                Debug.LogFormat("[Simon Swivels #{0}] Timer reset. Knob is in an incorrect position.", moduleID);
            }
            yield return new WaitForSeconds(20);
        }
    }

    private void LEDI(bool[] b)
    {
        for(int i = 0; i < 8; i++)
            if (b[i])
            {
                llights[i].enabled = true;
                lrends[i].material = lcols[0];
            }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} set N/E/S/W [Sets knob position] | !{0} press <RYGB> [Inputs sequence]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToUpperInvariant().Split(' ');
        if(commands[0] == "SET")
        {
            if(commands.Length != 2 || commands[1].Length != 1)
            {
                yield return "sendtochaterror!f Invalid command length.";
                yield break;
            }
            int d = "NESW".IndexOf(commands[1]);
            if(d < 0)
            {
                yield return "sendtochaterror!f \"" + commands[1] + "\" is an invalid direction.";
                yield break;
            }
            yield return null;
            while(kpos != d)
            {
                knob.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if(commands[0] == "PRESS")
        {
            command = commands.Skip(1).Join();
            List<int> p = new List<int> { };
            for(int i = 0; i < command.Length; i++)
            {
                string b = command[i].ToString();
                if ("ACDEFHIJKLMNOPQSTUVWXZ".Contains(b))
                {
                    yield return "sendtochaterror!f \"" + b + "\" is not a valid colour.";
                    yield break;
                }
                int c = "BYGR".IndexOf(b);
                if (c >= 0)
                    p.Add(sarr.IndexOf(c));
            }
            if(p.Count() > seq.Count() - seqpos)
            {
                yield return "sendtochaterror!f Warning: Excessive input sequence length.";
                yield break;
            }
            for (int i = 0; i < p.Count(); i++)
            {
                yield return null;
                simons[p[i]].OnInteract();
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            yield return "sendtochaterror!f \"" + commands[0] + "\" is an invalid command.";
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            if(lowtime && sarr[kpos] != safecol)
            {
                yield return null;
                knob.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            if(!lowtime && sarr[kpos] != anscol)
            {
                yield return null;
                knob.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            if(sarr[kpos] == anscol)
            {
                yield return null;
                simons[sarr.IndexOf(ans[seqpos])].OnInteract();
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
        }
    }
}
