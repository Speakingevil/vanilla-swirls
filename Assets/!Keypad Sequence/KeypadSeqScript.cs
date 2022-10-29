using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class KeypadSeqScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> keys;
    public KMSelectable[] scrolls;
    public Transform[] keyhatch;
    public Transform[] keypress;
    public Renderer[] keylights;
    public Renderer[] keysymbols;
    public Renderer[] stagelights;
    public Material[] lightstates;
    public Material[] symbols;
    public TextMesh panelnum;
    public GameObject matstore;

    private readonly string keyind = "0123456789ABCDEF";
    private List<char> keyorder = new List<char> { };
    private bool[] pressed = new bool[16];
    private int[] symbselect = new int[16];
    private int panelind;
    private int stage;
    private bool pressable;
    private KMAudio.KMAudioRef mechanism;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        matstore.SetActive(false);
        stagelights[0].material = lightstates[0];
        stagelights[1].material = lightstates[0];
        int[] randarr = Enumerable.Range(0, 36).ToArray();
        int[] panelarr;
        bool[] p = new bool[16];
        List<int> ports = new List<int> { info.GetPorts().Count(x => x == "StereoRCA"), info.GetPorts().Count(x => x == "RJ45"), info.GetPorts().Count(x => x == "PS2"), info.GetPorts().Count(x => x == "DVI") };
        int[][] quadrants = new int[4][] { new int[9] { 0, 1, 2, 6, 7, 8, 12, 13, 14 }, new int[9] { 3, 4, 5, 9, 10, 11, 15, 16, 17 }, new int[9] { 18, 19, 20, 24, 25, 26, 30, 31, 32 }, new int[9] { 21, 22, 23, 27, 28, 29, 33, 34, 35 } };
        randarr = randarr.Shuffle().ToArray();
        for (int i = 0; i < 4; i++)
        {
            randarr = randarr.Shuffle().ToArray();
            for (int j = 0; j < 4; j++)
                symbselect[(4 * i) + j] = randarr[j];
        }
        Panelset(0);
        panelarr = symbselect.Take(4).ToArray();
        int[] rccount = new int[12];
        for(int i = 0; i < 6; i++)
        {
            rccount[i] = panelarr.Where(x => (x / 6) == i).Count();
            rccount[i + 6] = panelarr.Where(x => (x % 6) == i).Count();
        }
        int m = rccount.Max();
        if(rccount.Count(x => x == m) < 2)
        {
            for(int i = 0; i < 12; i++)
            {
                if(rccount[i] == m)
                {
                    if (i > 5)
                    {
                        for (int j = 0; j < 4; j++)
                            if (panelarr[j] % 6 == i - 6)
                            {
                                p[j] = true;
                                keyorder.Add(keyind[j]);
                            }
                    }
                    else
                        for (int j = 0; j < 4; j++)
                            if (panelarr[j] / 6 == i)
                            {
                                p[j] = true;
                                keyorder.Add(keyind[j]);
                            }
                    break;
                }
            }
        }
        keyorder.Add('|');
        m = ports.Max();
        if(ports.Count(x => x == m) < 2)
        {
            switch (ports.IndexOf(m))
            {
                case 0:
                    m = panelarr.Select(x => x / 6).Min();
                    for(int i = 0; i < 4; i++)
                        if(!p[i] && panelarr[i] / 6 == m)
                        {
                            p[i] = true;
                            keyorder.Add(keyind[i]);
                        }
                    break;
                case 1:
                    m = panelarr.Select(x => x % 6).Max();
                    for (int i = 0; i < 4; i++)
                        if (!p[i] && panelarr[i] % 6 == m)
                        {
                            p[i] = true;
                            keyorder.Add(keyind[i]);
                        }
                    break;
                case 2:
                    m = panelarr.Select(x => x / 6).Max();
                    for (int i = 0; i < 4; i++)
                        if (!p[i] && panelarr[i] / 6 == m)
                        {
                            p[i] = true;
                            keyorder.Add(keyind[i]);
                        }
                    break;
                default:
                    m = panelarr.Select(x => x % 6).Min();
                    for (int i = 0; i < 4; i++)
                        if (!p[i] && panelarr[i] % 6 == m)
                        {
                            p[i] = true;
                            keyorder.Add(keyind[i]);
                        }
                    break;
            }
        }
        keyorder.Add('|');
        List<int> q = Enumerable.Range(0, 4).Select(x => quadrants[x].Count(y => panelarr.Contains(y))).ToList();
        for(int i = 0; i < 4; i++)
            if(q[i] == 1)
            {
                int c = quadrants[i].Intersect(panelarr).ToArray()[0];
                for(int j = 0; j < 4; j++)
                    if(panelarr[j] == c)
                    {
                        if (!p[j])
                        {
                            p[j] = true;
                            keyorder.Add(keyind[j]);
                        }
                        break;
                    }
            }
        keyorder.Add('|');
        if(p.Take(4).All(x => !x))
        {
            if(info.GetOnIndicators().Any())
            {
                if (info.GetOffIndicators().Any())
                {
                    p[0] = true;
                    keyorder.Add('0');
                }
                else
                {
                    p[1] = true;
                    keyorder.Add('1');
                }
            }
            else
            {
                if (info.GetOffIndicators().Any())
                {
                    p[3] = true;
                    keyorder.Add('3');
                }
                else
                {
                    p[2] = true;
                    keyorder.Add('2');
                }              
            }
        }
        keyorder.Add(']');
        panelarr = symbselect.Take(8).ToArray();
        q = quadrants.Select(x => x.Count(y => panelarr.Contains(y))).ToList();
        m = q.Max();
        if(q.Count(x => x == m) < 2)
        {
            int c = q.IndexOf(m);
            for(int j = 0; j < 8; j++)
            {
                if(!p[j] && quadrants[c].Contains(panelarr[j]))
                {
                    p[j] = true;
                    keyorder.Add(keyind[j]);
                }
            }
        }
        keyorder.Add('|');
        int[] n = Enumerable.Range(0, 8).Where(x => !p[x]).ToArray();
        int[] d = n.Select(x => panelarr[x]).ToArray();
        for (int i = 0; i < n.Length; i++)
            if (d.Count(x => x == d[i]) > 1)
            {
                p[n[i]] = true;
                keyorder.Add(keyind[n[i]]);
            }
        keyorder.Add('|');
        for(int i = 0; i < 8; i++)
            if(!p[i] && quadrants[i % 4].Contains(panelarr[i]))
            {
                p[i] = true;
                keyorder.Add(keyind[i]);
            }
        keyorder.Add('|');
        int[] e = new int[4][] { new int[6] { 0, 1, 2, 3, 4, 5 }, new int[6] { 5, 11, 17, 23, 29, 35 }, new int[6] { 30, 31, 32, 33, 34, 35 }, new int[6] { 0, 6, 12, 18, 24, 30 } }.Where((x, i) => ports[i] > 0).SelectMany(i => i).Distinct().ToArray();
        for(int i = 0; i < 8; i++)
        {
            if(!p[i] && e.Contains(panelarr[i]))
            {
                p[i] = true;
                keyorder.Add(keyind[i]);
            }
        }
        keyorder.Add('|');
        if(p.Skip(4).Take(4).All(x => !x))
        {
            for(int i = 0; i < 4; i++)
                if(p[3 - i])
                {
                    p[i + 4] = true;
                    keyorder.Add(keyind[i + 4]);
                }
        }
        keyorder.Add(']');
        panelarr = symbselect.Take(12).ToArray();
        for (int i = 0; i < 12; i++)
        {
            int s = panelarr[i];
            if(!p[i] && (s / 6 == 0 || !panelarr.Contains(s - 6)) && (s / 6 == 5 || !panelarr.Contains(s + 6)) && (s % 6 == 0 || !panelarr.Contains(s - 1)) && (s % 6 == 5 || !panelarr.Contains(s + 1)))
            {
                p[i] = true;
                keyorder.Add(keyind[i]);
            }
        }
        keyorder.Add('|');
        n = Enumerable.Range(0, 12).Where(x => !p[x]).ToArray();
        d = n.Select(x => panelarr[x]).ToArray();
        for (int i = 0; i < n.Length; i++)
            if (d.Any(x => x == 35 - d[i]))
            {
                p[n[i]] = true;
                keyorder.Add(keyind[n[i]]);
            }
        keyorder.Add('|');
        n = Enumerable.Range(0, 12).Where(x => !p[x]).ToArray();
        d = n.Select(x => panelarr[x]).ToArray();
        for (int i = 0; i < n.Length; i++)
            if (d.Count(x => x == d[i]) > 1)
            {
                p[n[i]] = true;
                keyorder.Add(keyind[n[i]]);
            }
        keyorder.Add('|');
        for (int i = 0; i < 6; i++)
        {
            rccount[i] = panelarr.Where(x => (x / 6) == i).Count();
            rccount[i + 6] = panelarr.Where(x => (x % 6) == i).Count();
        }
        m = rccount.Where(x => x > 0).Min();
        for(int i = 0; i < 12; i++)
            if(rccount[i] == m)
            {
                if(i > 5)
                {
                    int[] mincol = panelarr.Where(x => (x % 6) == i - 6).ToArray();
                    for (int j = 0; j < 12; j++)
                        if(!p[j] && mincol.Contains(panelarr[j]))
                        {
                            p[j] = true;
                            keyorder.Add(keyind[j]);
                        }
                }
                else
                {
                    int[] mincol = panelarr.Where(x => (x / 6) == i).ToArray();
                    for (int j = 0; j < 12; j++)
                        if (!p[j] && mincol.Contains(panelarr[j]))
                        {
                            p[j] = true;
                            keyorder.Add(keyind[j]);
                        }
                }
            }
        keyorder.Add('|');
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 5; j++)
            {
                int c = (6 * i) + j;
                int[] s = new int[4] { c, c + 1, c + 6, c + 7};
                if (s.All(x => panelarr.Contains(x)))
                    for(int k = 0; k < 12; k++)
                        if(!p[k] && s.Contains(panelarr[k]))
                        {
                            p[k] = true;
                            keyorder.Add(keyind[k]);
                        }
            }
        keyorder.Add('|');
        if(p.Skip(8).Take(4).All(x => !x))
            for(int i = 0; i < 4; i++)
                if(p[i] ^ p[i + 4])
                {
                    p[i + 8] = true;
                    keyorder.Add(keyind[i + 8]);
                }
        keyorder.Add(']');
        if(symbselect.Distinct().Count() > 15)
            for(int i = 12; i < 16; i++)
            {
                p[i] = true;
                keyorder.Add(keyind[i]);
            }
        keyorder.Add('|');
        bool[] paneldup = new bool[4];
        for(int i = 0; i < 4; i++)
        {
            int[] panel = symbselect.Skip(4 * i).Take(4).ToArray();
            int[] rpanels = symbselect.Take(4 * i).Concat(symbselect.Skip(4 * (i + 1)).Take(12 - (4 * i))).ToArray();
            paneldup[i] = panel.Any(x => rpanels.Contains(x));
        }
        if(paneldup.All(x => x))
            for(int i = 1; i < 16; i += 4)
                if (!p[i])
                {
                    p[i] = true;
                    keyorder.Add(keyind[i]);
                }
        keyorder.Add('|');
        q = quadrants.Select(x => x.Count(y => symbselect.Contains(y))).ToList();
        if(q.Any(x => x < 3))
            for (int i = 2; i < 16; i += 4)
                if (!p[i])
                {
                    p[i] = true;
                    keyorder.Add(keyind[i]);
                }
        keyorder.Add('|');
        for (int i = 0; i < 6; i++)
        {
            rccount[i] = symbselect.Where(x => (x / 6) == i).Count();
            rccount[i + 6] = symbselect.Where(x => (x % 6) == i).Count();
        }
        if(rccount.Any(x => x < 1))
            for (int i = 0; i < 16; i += 4)
                if (!p[i])
                {
                    p[i] = true;
                    keyorder.Add(keyind[i]);
                }
        keyorder.Add('|');
        if (rccount.Any(x => x > 5))
            for (int i = 3; i < 16; i += 4)
                if (!p[i])
                {
                    p[i] = true;
                    keyorder.Add(keyind[i]);
                }
        keyorder.Add('|');
        n = Enumerable.Range(0, 12).Where(x => !p[x]).ToArray();
        d = n.Select(x => panelarr[x]).ToArray();
        for (int i = 0; i < n.Length; i++)
            if (d.Count(x => x == d[i]) > 1)
            {
                p[n[i]] = true;
                keyorder.Add(keyind[n[i]]);
            }
        keyorder.Add('|');
        if(p.Skip(12).All(x => !x))
        {
            int[] pos = new int[4];
            for (int i = 0; i < 4; i++)
                for (int j = i; j < 16; j += 4)
                    if (p[j])
                        pos[i]++;
            m = pos.Min();
            for (int i = 0; i < 4; i++)
                if (pos[i] == m)
                    keyorder.Add(keyind[i + 12]);
        }
        keyorder.Add(']');
        Debug.Log(string.Join("", keyorder.Select(x => x.ToString()).ToArray()));
        while (keyorder[0] == '|')
            keyorder.RemoveAt(0);
        Debug.LogFormat("[Keypad Sequence #{0}] Panel 1 has the symbols: Top-left = {1}, Top-right = {2}, Bottom-left = {3}, Bottom-right = {4}", moduleID, Logsymb(0), Logsymb(1), Logsymb(2), Logsymb(3));
        Logkeys();
        module.OnActivate += delegate () { mechanism = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform); StartCoroutine(Hatchmove(true, true)); };
        foreach(KMSelectable scr in scrolls)
        {
            bool next = scr == scrolls[1];
            scr.OnInteract += delegate ()
            {
                if (pressable)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, scr.transform);
                    if (next)
                    {
                        if (panelind == stage)
                        {
                            if (keyorder[0] == ']')
                            {
                                keyorder.RemoveAt(0);
                                stagelights[stage].material = lightstates[1];
                                if (stage == 3)
                                    moduleSolved = true;
                                else
                                {
                                    stage++;
                                    while (keyorder[0] == '|')
                                        keyorder.RemoveAt(0);
                                    Debug.LogFormat("[Keypad Sequence #{0}] Panel {1} has the symbols: Top-left = {2}, Top-right = {3}, Bottom-left = {4}, Bottom-right = {5}", moduleID, stage + 1, Logsymb(stage * 4), Logsymb((stage * 4) + 1), Logsymb((stage * 4) + 2), Logsymb((stage * 4) + 3));
                                    Logkeys();
                                }
                            }
                            else
                            {
                                module.HandleStrike();
                                return false;
                            }
                        }
                        mechanism = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
                        StartCoroutine(Hatchmove(false, true));
                    }
                    else if(panelind > 0)
                    {
                        mechanism = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
                        StartCoroutine(Hatchmove(false, false));
                    }
                }
                return false;
            };
        }
        foreach(KMSelectable key in keys)
        {
            int k = keys.IndexOf(key);
            key.OnInteract = delegate ()
            {
                if (pressable)
                {
                    int pk = (4 * panelind) + k;
                    if (!pressed[pk])
                    {
                        Debug.LogFormat("[Keypad Sequence #{0}] {1}{2} pressed.", moduleID, new string[] { "Top-left key of Panel ", "Top-right key of Panel ", "Bottom-left key of Panel ", "Bottom-right key of Panel " }[k], panelind + 1);
                        if (keyorder.Contains(keyind[pk]) && keyorder.IndexOf(keyind[pk]) < keyorder.IndexOf(']') && (!keyorder.Contains('|') || keyorder.IndexOf(keyind[pk]) < keyorder.IndexOf('|')))
                        {
                            pressed[pk] = true;
                            keyorder.Remove(keyind[pk]);
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, key.transform);
                            keypress[k].localPosition = new Vector3(-0.021f, -0.021f, -0.21f);
                            keylights[k].material = lightstates[1];
                            if(keyorder[0] == '|' || keyorder[0] == ']')
                            {
                                Debug.LogFormat("[Keypad Sequence #{0}] All required keys pressed.", moduleID);
                                while (keyorder[0] == '|')
                                    keyorder.RemoveAt(0);
                                if (keyorder[0] == ']')
                                    Debug.LogFormat("[Keypad Sequence #{0}] No remaining keys need to be pressed.{1}", moduleID, stage == 3 ? "" : " Move to the next panel.");
                                else
                                    Logkeys();
                            }
                        }
                        else
                            StartCoroutine(KeyStrike(k));
                    }
                }
                return false;
            };
        }
    }

    private IEnumerator Hatchmove(bool open, bool up)
    {
        if (open)
        {
            float e = 0;
            while (e < 0.25f)
            {
                e += Time.deltaTime;
                keyhatch[0].localEulerAngles = new Vector3(360 * e, 0, 0);
                keyhatch[1].localEulerAngles = new Vector3(-360 * e, 0, 0);
                keyhatch[2].localPosition = new Vector3(-0.0186f, Mathf.Lerp(-0.044f, 0.014f, e * 2), 0);
                yield return null;
            }
            keyhatch[0].localEulerAngles = new Vector3(90, 0, 0);
            keyhatch[1].localEulerAngles = new Vector3(-90, 0, 0);
            while(e < 0.5f)
            {
                e += Time.deltaTime;
                keyhatch[0].localPosition = new Vector3(0, -0.242f * (e - 0.25f), 0.0608f);
                keyhatch[1].localPosition = new Vector3(0, -0.242f * (e - 0.25f), -0.0608f);
                keyhatch[2].localPosition = new Vector3(-0.0186f, Mathf.Lerp(-0.044f, 0.014f, e * 2), 0);
                yield return null;
            }
            keyhatch[0].localPosition = new Vector3(0, -0.0605f, 0.0608f);
            keyhatch[1].localPosition = new Vector3(0, -0.0605f, -0.0608f);
            keyhatch[2].localPosition = new Vector3(-0.0186f, 0.014f, 0);
            pressable = true;
            mechanism.StopSound();
        }
        else
        {
            pressable = false;
            float e = 0.5f;
            while (e > 0.25f)
            {
                e -= Time.deltaTime;
                keyhatch[0].localPosition = new Vector3(0, -0.242f * (e - 0.25f), 0.0608f);
                keyhatch[1].localPosition = new Vector3(0, -0.242f * (e - 0.25f), -0.0608f);
                keyhatch[2].localPosition = new Vector3(-0.0186f, Mathf.Lerp(-0.044f, 0.014f, e * 2), 0);
                yield return null;
            }
            keyhatch[0].localPosition = new Vector3(0, 0, 0.0608f);
            keyhatch[1].localPosition = new Vector3(0, 0, -0.0608f);
            while(e > 0)
            {
                e -= Time.deltaTime;
                keyhatch[0].localEulerAngles = new Vector3(360 * e, 0, 0);
                keyhatch[1].localEulerAngles = new Vector3(-360 * e, 0, 0);
                keyhatch[2].localPosition = new Vector3(-0.0186f, Mathf.Lerp(-0.044f, 0.014f, e * 2), 0);
                yield return null;
            }
            keyhatch[0].localEulerAngles = new Vector3(0, 0, 0);
            keyhatch[1].localEulerAngles = new Vector3(0, 0, 0);
            keyhatch[2].localPosition = new Vector3(-0.0186f, -0.044f, 0);
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

    private string Logsymb(int k)
    {
        int s = symbselect[k];
        return "ABCDEF"[s % 6].ToString() + ((s / 6) + 1).ToString();
    }

    private void Logkeys()
    {
        int[] k = keyorder.TakeWhile(x => x != '|' && x != ']').Select(x => keyind.IndexOf(x)).ToArray();
        if (k.Length < 1)
            Debug.LogFormat("[Keypad Sequence #{0}] Do not press any keys.", moduleID);
        else
            Debug.LogFormat("[Keypad Sequence #{0}] Press the keys: {1}", moduleID, string.Join(", ", k.Select(x => new string[] { "Top-left of Panel ", "Top-right of Panel ", "Bottom-left of Panel ", "Bottom-right of Panel "}[x % 4] + ((x / 4) + 1).ToString()).ToArray()));
    }

    private void Panelset(int k)
    {
        panelnum.text = (k + 1).ToString();
        panelnum.transform.localPosition = new Vector3(new float[4] { 0.02f, 0.08f, 0.065f, 0.05f}[k], 0.003f, 0);
        for(int i = 0; i < 4; i++)
        {
            int b = symbselect[(4 * k) + i];
            keysymbols[i].material = symbols[b];
            if(pressed[(4 * k) + i])
            {
                keypress[i].localPosition = new Vector3(-0.021f, -0.021f, -0.21f);
                keylights[i].material = lightstates[1];
            }
            else
            {
                keypress[i].localPosition = new Vector3(0, 0, 0.0415f);
                keylights[i].material = lightstates[0];
            }
        }
    }

    private IEnumerator KeyStrike(int k)
    {
        pressable = false;
        keypress[k].localPosition = new Vector3(-0.021f, -0.021f, -0.21f);
        keylights[k].material = lightstates[2];
        module.HandleStrike();
        yield return new WaitForSeconds(0.5f);
        pressable = true;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, keys[k].transform);
        keypress[k].localPosition = new Vector3(0, 0, 0.0415f);
        keylights[k].material = lightstates[0];
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} press TL/TR/BL/BR [Presses key] | !{0} previous/next [Scrolls between panels]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if(command == "previous")
        {
            yield return null;
            scrolls[0].OnInteract();
            yield break;
        }
        else if(command == "next")
        {
            yield return "strike";
            yield return "solve";
            yield return null;
            scrolls[1].OnInteract();
            yield break;
        }
        else
        {
            string[] commands = command.Split(' ');
            if(commands.Length != 2)
            {
                yield return "sendtochaterror!f Invalid command length.";
                yield break;
            }
            if(commands[0] != "press")
            {
                yield return "sendtochaterror!f Invalid command type.";
                yield break;
            }
            int k = new List<string> { "tl", "tr", "bl", "br" }.IndexOf(commands[1]);
            if(k < 0)
            {
                yield return "sendtochaterror!f Invalid key position.";
                yield break;
            }
            yield return "strike";
            yield return null;
            keys[k].OnInteract();
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        while (!moduleSolved)
        {
            while (!pressable)
                yield return true;
            if(keyorder[0] == ']')
            {
                while(panelind < stage)
                {
                    yield return null;
                    scrolls[1].OnInteract();
                    while (!pressable)
                        yield return null;
                }
                yield return null;
                scrolls[1].OnInteract();
            }
            else
            {
                int[] k = keyorder.TakeWhile(x => x != '|' && x != ']').Select(x => keyind.IndexOf(x)).OrderBy(x => x).ToArray();
                yield return null;
                while (panelind < k.Select(x => x / 4).Min())
                {
                    yield return null;
                    scrolls[1].OnInteract();
                    while (!pressable)
                        yield return null;
                }
                while (panelind > k.Select(x => x / 4).Min())
                {
                    yield return null;
                    scrolls[0].OnInteract();
                    while (!pressable)
                        yield return null;
                }
                int[] p = k.Where(x => x / 4 == panelind).ToArray();
                for (int i = 0; i < p.Count(); i++)
                {
                    yield return null;
                    keys[p[i] % 4].OnInteract();
                }
            }
        }
    }
}