using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class SimonStrandsScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable modSelect;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Renderer bulb;
    public Material[] bulbmats;
    public Light[] flash;
    public Transform pointer;
    public TextMesh disp;

    private readonly int[,] ctable = new int[20, 4] { { 3, 2, 0, 1}, { 2, 3, 0, 1}, { 1, 2, 3, 0}, { 1, 3, 0, 2}, { 0, 2, 3, 1}, { 3, 0, 1, 2}, { 2, 0, 1, 3}, { 0, 3, 2, 1}, { 1, 2, 0, 3}, { 2, 3, 1, 0}, { 3, 1, 0, 2}, { 1, 0, 3, 2}, { 0, 3, 1, 2}, { 2, 0, 3, 1}, { 2, 1, 3, 0}, { 3, 2, 1, 0}, { 0, 1, 3, 2}, { 2, 0, 3, 1}, { 0, 2, 1, 3}, { 3, 0, 2, 1} };
    private readonly string[] collog = new string[4] { "Red", "Blue", "Green", "Yellow"};
    private readonly Color[] cols = new Color[4] { new Color(1, 0, 0), new Color(0, 0, 1), new Color(0, 1, 0), new Color(1, 1, 0)};
    private readonly string[] words = new string[20] { "REJECT", "REVERT", "DRIFT", "BISTRO", "DISKS", "PARITY", "STAPLE", "ENGULF", "CALLED", "RISKY", "SHARD", "ENGAGE", "TARIFF", "DESKS", "STROBE", "DISCS", "HARDY", "CALLER", "STREAK", "EXACT"};
    private readonly string[] morse = new string[26] { "#-###", "###-#-#-#", "###-#-###-#", "###-#-#", "#", "#-#-###-#", "###-###-#", "#-#-#-#", "#-#", "#-###-###-###", "###-#-###", "#-###-#-#", "###-###", "###-#", "###-###-###", "#-###-###-#", "###-###-#-###", "#-###-#", "#-#-#", "###", "#-#-###", "#-#-#-###", "#-###-###", "###-#-#-###", "###-#-###-###", "###-###-#-#"};
    private readonly int[] freqs = new int[20] { 5, 12, 15, 19, 22, 32, 35, 42, 45, 52, 62, 65, 71, 75, 78, 82, 85, 92, 95, 99};
    private int[] freqselect = new int[2];
    private string tx;
    private List<int> seq = new List<int> { };
    private List<int> ans = new List<int> { };
    private int stage = 1;
    private int index;
    private bool[] play = new bool[2];
    private bool movetick;
    private float e = 6;
    private bool pressable;
    private string presslog;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        float scale = module.transform.lossyScale.x;
        foreach (Light l in flash)
            l.range *= scale;
        modSelect.OnInteract = delegate () { if (!play[0]) { Activate(); play[0] = true; } return true; };
    }

    private void Activate()
    {
        Generate(1);
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved && pressable)
                {
                    switch (b)
                    {
                        case 4:
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                            button.AddInteractionPunch(0.2f);
                            if (freqselect[1] > 0)
                            {
                                freqselect[1]--;
                                disp.text = string.Format("3.5{0}{1}", freqs[freqselect[1]] < 10 ? "0" : "", freqs[freqselect[1]]);
                                if (!movetick)
                                    StartCoroutine("MoveTick");
                            }
                            break;
                        case 5:
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                            button.AddInteractionPunch(0.2f);
                            if (freqselect[1] < 19)
                            {
                                freqselect[1]++;
                                disp.text = string.Format("3.5{0}{1}", freqs[freqselect[1]] < 10 ? "0" : "", freqs[freqselect[1]]);
                                if (!movetick)
                                    StartCoroutine("MoveTick");
                            }
                            break;
                        default:
                            play[1] = true;
                            Audio.PlaySoundAtTransform("Beep" + b, button.transform);
                            StartCoroutine(Press(b + 1));
                            if(b == ans[index])
                            {
                                if (index == 0)
                                    presslog = "";
                                else
                                    presslog += ", ";
                                presslog += collog[b];
                                if (index + 1 < stage)
                                    index++;
                                else if(freqselect[0] == freqselect[1])
                                {
                                    Debug.LogFormat("[Simon Strands #{0}] Buttons pressed: {1}.", moduleID, presslog);
                                    Debug.LogFormat("[Simon Strands #{0}] Transmitting signal at {1} MHz. OK.", moduleID, disp.text);
                                    if(stage < 5)
                                    {
                                        stage++;
                                        Generate(stage);
                                    }
                                    else
                                    {
                                        pressable = false;
                                        bulb.material = bulbmats[0];
                                        flash[0].enabled = false;
                                        moduleSolved = true;
                                        module.HandlePass();
                                    }
                                }
                                else
                                {
                                    Debug.LogFormat("[Simon Strands #{0}] Buttons pressed: {1}.", moduleID, presslog);
                                    Debug.LogFormat("[Simon Strands #{0}] Transmitting signal at {1} MHz. Incorrect.", moduleID, disp.text);
                                    index = 0;
                                    module.HandleStrike();
                                }
                            }
                            else
                            {
                                Debug.LogFormat("[Simon Strands #{0}] Buttons pressed: {1}.", moduleID, presslog);
                                index = 0;
                                e = 6;
                                module.HandleStrike();
                            }
                            break;
                    }
                }
                return false;
            };
        }
    }

    private void Generate(int s)
    {
        index = 0;
        seq.Clear();
        ans.Clear();
        flash[0].enabled = false;
        bulb.material = bulbmats[0];
        int w = Random.Range(0, 20);
        int c = Random.Range(0, 4);
        flash[0].color = cols[c];
        for (int i = 0; i < s; i++)
            seq.Add(Random.Range(0, 4));
        Debug.LogFormat("[Simon Strands #{0}] The sequence of flashes is: {1}.", moduleID, string.Join(", ", seq.Select(x => collog[x]).ToArray()));
        ans = seq.Select(x => ctable[w, x]).ToList();
        Debug.LogFormat("[Simon Strands #{0}] The transmitted word is \"{1}\" in {2} flashes.", moduleID, words[w], collog[c]);
        tx = "-------" + string.Join("---", words[w].Select(x => morse[x - 'A'].ToString()).ToArray());
        int down = 0;
        switch (c)
        {
            case 0: down = Mathf.Max(info.GetStrikes(), info.GetSerialNumberNumbers().Min()); break;
            case 1: down = ((int)info.GetTime() - 1) / 60; break;
            case 2: down = info.GetModuleNames().Count() - info.GetSolvedModuleNames().Count(); break;
            default:
                int n = info.GetSerialNumberLetters().Count();
                down = info.GetSerialNumberLetters().ToArray()[Mathf.Min(n, s) - 1] - 'A' + 1;
                break;
        }
        freqselect[0] = (w + down) % 20;
        Debug.LogFormat("[Simon Strands #{0}] Set the frequency to 3.5{1}{2} MHz and press the buttons: {3}.", moduleID, freqs[freqselect[0]] < 10 ? "0" : "", freqs[freqselect[0]],string.Join(", ", ans.Select(x => collog[x]).ToArray()));
        pressable = false;
        StartCoroutine("Sequence");
        StartCoroutine("Transmit", c);
    }

    private IEnumerator Sequence()
    {
        yield return new WaitForSeconds(1);
        pressable = true;
        while (pressable)
        {
            yield return new WaitForSeconds(1);
            for (int i = 0; i < seq.Count(); i++)
            {
                flash[seq[i] + 1].enabled = true;
                if (play[1])
                    Audio.PlaySoundAtTransform("Beep" + seq[i], buttons[seq[i]].transform);
                yield return new WaitForSeconds(0.5f);
                flash[seq[i] + 1].enabled = false;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator Press(int k)
    {
        StopCoroutine("Sequence");
        for(int i = 1; i < 5; i++)
            flash[i].enabled = false;
        flash[k].enabled = true;
        yield return new WaitForSeconds(0.5f);
        flash[k].enabled = false;
        if (e >= 6)
        {
            while (e > 0)
            {
                e -= Time.deltaTime;
                yield return null;
                if (!pressable)
                {
                    e = 6;
                    yield break;
                }
            }
            index = 0;
            if(!moduleSolved)
                StartCoroutine("Sequence");
        }
        else
            e = 6;
    }

    private IEnumerator Transmit(int c)
    {
        yield return null;
        yield return new WaitForSeconds(1);
        int t = Random.Range(0, tx.Length);
        while (pressable)
        {
            Flicker(tx[t] == '#', c);
            yield return new WaitForSeconds(0.2f);
            t++;
            t %= tx.Length;
        }
    }

    private void Flicker(bool i, int c)
    {
        flash[0].enabled = i;
        bulb.material = bulbmats[i ? c + 1 : 0];
    }

    private IEnumerator MoveTick()
    {
        movetick = true;
        float t = 4.547f - (9.094f * freqs[freqselect[1]] / 100);
        while (Mathf.Abs(pointer.localPosition.x - t) > 0.03f)
        {
            t = 4.547f - (9.094f * freqs[freqselect[1]] / 100);
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
    private readonly string TwitchHelpMessage = "!{0} set # [Sets frequency] | !{0} press <r/b/g/y> [Presses buttons. Chain without spaces.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if(commands.Length != 2)
        {
            yield return "sendtochaterror!f Invalid command length.";
            yield break;
        }
        switch (commands[0])
        {
            case "set":
                if(commands[1].Length != 5)
                {
                    yield return "sendtochaterror!f Frequencies must have three decimal places.";
                    yield break;
                }
                if(new string(commands[1].Take(3).ToArray()) != "3.5")
                {
                    yield return "sendtochaterror!f Frequencies must be between 3.500 and 3.599 MHz.";
                    yield break;
                }
                int f = 0;
                if (int.TryParse(new string(commands[1].Skip(3).ToArray()), out f))
                {
                    if (freqs.Contains(f))
                    {
                        int b = f < freqs[freqselect[1]] ? 4 : 5;
                        while(f != freqs[freqselect[1]])
                        {
                            yield return null;
                            buttons[b].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                        yield return "sendtochaterror!f Given frequency is not an option for the module to display.";
                }
                else
                    yield return "sendtochaterror!f Frequencies must have only numerical digits.";
                yield break;
            case "press":
                List<int> p = new List<int> { };
                for(int i = 0; i < commands[1].Length; i++)
                {
                    int b = "rbgy".IndexOf(commands[1][i].ToString());
                    if(b < 0)
                    {
                        yield return "sendtochaterror!f " + commands[1][i].ToString() + "is not a valid colour.";
                        yield break;
                    }
                    p.Add(b);
                }
                for(int i = 0; i < p.Count(); i++)
                {
                    yield return null;
                    buttons[p[i]].OnInteract();
                    yield return new WaitForSeconds(0.5f);
                }
                yield break;
            default:
                yield return "sendtochaterror!f " + commands[0] + " is an invalid command.";
                yield break;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            while (!pressable)
                yield return true;
            int b = freqselect[0] < freqselect[1] ? 4 : 5;
            while(freqselect[0] != freqselect[1])
            {
                yield return null;
                buttons[b].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
            for (int i = index; i < ans.Count(); i++)
            {
                buttons[ans[i]].OnInteract();
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
