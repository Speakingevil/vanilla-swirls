using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MWoFScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> buttons;
    public Transform[] bpos;
    public Renderer[] leds;
    public Material[] io;
    public TextMesh[] displays;

    private readonly string[,] words = new string[6, 5]
    {
        { "COULD", "SMALL", "BELOW", "LARGE", "STUDY"},
        { "FIRST", "RIGHT", "THINK", "PLANT", "SOUND"},
        { "SIXTY", "BROWN", "VIRUS", "BUSHY", "FUNGI"},
        { "OPTED", "YOUNG", "ICHOR", "QUILL", "WRONG"},
        { "ZILCH", "JERKY", "BANJO", "PUNCH", "IVORY"},
        { "COQUI", "TOPAZ", "JAUNT", "NUDGE", "MAJOR"}
    };
    private readonly string[] morse = new string[26] { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };
    private readonly int[] eyes = new int[30] { 3, 1, 5, 2, 3, 2, 0, 1, 0, 4, 5, 4, 0, 2, 4, 3, 4, 0, 2, 5, 3, 1, 0, 2, 1, 3, 5, 1, 5, 4 };
    private string[] fragments = new string[6];
    private bool[] pressed = new bool[6];
    private string[] ans = new string[2];
    private int stage;
    private bool pressable;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        leds[0].material = io[1];
        StartCoroutine(Generate(0));
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if(!pressed[b] && !moduleSolved)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, bpos[b]);
                    button.AddInteractionPunch(0.2f);
                    pressed[b] = true;
                    Vector3 p = bpos[b].localPosition;
                    bpos[b].localPosition = new Vector3(p.x, p.y, -1);
                    ans[1] += fragments[b];
                    if(ans[0] == ans[1])
                    {
                        leds[stage].material = io[0];
                        Debug.LogFormat("[.--/---/..-. #{0}] {1} == {2} | Advancing.", moduleID, ans[1], ans[0]);
                        if (stage > 2)
                        {
                            moduleSolved = true;
                            Debug.LogFormat("[.--/---/..-. #{0}] Module complete.", moduleID);
                            module.HandlePass();
                        }
                        else
                            stage++;
                        StartCoroutine(Generate(stage));
                    }
                    else if(ans[1].Length >= ans[0].Length)
                    {
                        module.HandleStrike();
                        Debug.LogFormat("[.--/---/..-. #{0}] {1} =/= {2} | Resetting stage.", moduleID, ans[1], ans[0]);
                        StartCoroutine(Generate(stage));
                    }
                    else if(ans[0].Substring(0, ans[1].Length) != ans[1])
                    {
                        module.HandleStrike();
                        Debug.LogFormat("[.--/---/..-. #{0}] {1} =/= {2} | Resetting stage.", moduleID, ans[1], ans[0].Substring(0, ans[1].Length));
                        StartCoroutine(Generate(stage));
                    }
                    else
                    {
                        Debug.LogFormat("[.--/---/..-. #{0}] {1} == {2}", moduleID, ans[1], ans[0].Substring(0, ans[1].Length));
                    }
                }
                return false;
            };
        }
    }

    private IEnumerator Generate(int s)
    {
        ans[1] = "";
        displays[6].text = "";
        string w = "";
        pressable = false;
        if (!moduleSolved)
        {
            int[] shift = new int[2];
            shift[0] = Random.Range(0, 6);
            shift[1] = Random.Range(0, 5 - shift[0]);
            if (shift[0] == 0 && shift[1] == 0)
                shift[Random.Range(0, 2)] = 1;
            int[] choose = new int[2] { Random.Range(0, 6), Random.Range(0, 5) };
            int[] stgsh = new int[2] { shift[0], shift[1] };
            if (stage > 1)
                stgsh[0] = 6 - stgsh[0];
            if ((stage % 2 ^ stage >> 1) == 1)
                stgsh[1] = 5 - stgsh[1];
            w = words[choose[0], choose[1]];
            Debug.LogFormat("[.--/---/..-. #{0}] The displayed word is {1}.", moduleID, w);
            string a = words[(choose[0] + stgsh[0]) % 6, (choose[1] + stgsh[1]) % 5];
            ans[0] = string.Join("", a.Select(x => morse[x - 'A']).ToArray());
            int p = ans[0].Length - shift[0] - shift[1];
            int[] order = Enumerable.Range(0, p).ToArray().Shuffle().ToArray();
            bool tick = false;
            int eye = eyes[(choose[0] * 5) + choose[1]];
            Debug.Log(ans[0]);
            for (int i = 0; i < p; i++)
            {
                string snippet = new string(ans[0].Skip(order[i]).Take(shift[0] + shift[1]).ToArray());
                if (snippet.Count(x => x == '-') == shift[0])
                {
                    tick = true;
                    if (order[i] < p)
                        ans[0] = ans[0].Insert(order[i] + shift[0] + shift[1], "|");
                    if (order[i] > 0)
                        ans[0] = ans[0].Insert(order[i], "|");
                    fragments[eye] = snippet;
                    break;
                }
            }
            if (!tick)
            {
                fragments[eye] = "";
                int[] sh = new int[2] { shift[0], shift[1] };
                for (int i = 0; i < shift[0] + shift[1]; i++)
                {
                    int r = Random.Range(0, 2);
                    if (sh[r] < 1)
                        r ^= 1;
                    fragments[eye] += "-."[r].ToString();
                    sh[r]--;
                }
            }
            Debug.Log("(" + shift[0] + ", " + shift[1] + ")-" + tick + ": " + fragments[eye]);
            int min = 1;
            while (ans[0].Split('|').Any(x => x.Length > 5))
            {
                string[] an = ans[0].Split('|');
                int[] ord = Enumerable.Range(0, an.Length).ToArray().Shuffle().ToArray();
                for (int i = 0; i < an.Length; i++)
                {
                    int j = ord[i];
                    if (an[j].Length <= 5)
                        continue;
                    int r = Random.Range(min, an[j].Length - 1);
                    if (r == min)
                        min++;
                    an[j] = an[j].Insert(r, "|");
                }
                ans[0] = string.Join("|", an);
            }
            Debug.Log(ans[0]);
            while (ans[0].Count(x => x == '|') > (tick ? 5 : 4))
            {
                List<string> an = ans[0].Split('|').ToList();
                int r = Random.Range(0, an.Count() - 1);
                while (an[r].Length + an[r + 1].Length > 5 || (tick && (fragments[eye] == an[r] || fragments[eye] == an[r + 1])))
                    r = Random.Range(0, an.Count() - 1);
                an[r] += an[r + 1];
                an.RemoveAt(r + 1);
                ans[0] = string.Join("|", an.ToArray());
            }
            Debug.Log(ans[0]);
            if (true)
            {
                List<string> an = ans[0].Split('|').ToList();
                List<int> ord = new List<int> { 0, 1, 2, 3, 4, 5 }.Shuffle();
                ord.Remove(eye);
                if (tick)
                    ord.Insert(an.IndexOf(fragments[eye]), eye);
                for (int i = 0; i < an.Count(); i++)
                    fragments[ord[i]] = an[i];
                for (int i = an.Count(); i < (tick ? 6 : 5); i++)
                {
                    fragments[ord[i]] = ".-"[Random.Range(0, 2)].ToString();
                    while (string.Join("", an.ToArray()).Contains(fragments[ord[i]]) || fragments.Where((x, j) => j != ord[i]).Any(x => x == fragments[ord[i]]))
                    {
                        fragments[ord[i]] += ".-"[Random.Range(0, 2)].ToString();
                        if (fragments[ord[i]].Length > 5)
                        {
                            string f = "";
                            for (int j = 1; j < 6; j++)
                                f += fragments[ord[i]][j].ToString();
                            fragments[ord[i]] = f;
                        }
                    }
                }
            }
            ans[0] = ans[0].Replace("|", "");
            Debug.LogFormat("[.--/---/..-. #{0}] The label of the {1}-{2} button is \"{3}\"", moduleID, new string[] { "top", "middle", "bottom"}[eye / 2], eye % 2 == 0 ? "left" : "right", fragments[eye]);
            Debug.LogFormat("[.--/---/..-. #{0}] The target word is {1}.", moduleID, a);
            Debug.LogFormat("[.--/---/..-. #{0}] Enter the string: \"{1}\"", moduleID, ans[0]);
        }
        StartCoroutine(Push(0));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Push(1));
        StartCoroutine(Push(2));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Push(3));
        StartCoroutine(Push(4));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Push(5));
        yield return new WaitForSeconds(1);
        pressable = true;
        if(!moduleSolved)
            displays[6].text = w;
    }

    private IEnumerator Push(int i)
    {
        Vector3 p = bpos[i].localPosition;
        float e = 0;
        while (e < 1)
        {
            e += Time.deltaTime * 2;
            bpos[i].localPosition = new Vector3(p.x, p.y, Mathf.Lerp(p.z, -2, e));
            yield return null;
        }
        if (!moduleSolved)
        {
            yield return new WaitForSeconds(0.5f);
            displays[i].text = fragments[i];
            while (e > 0)
            {
                e -= Time.deltaTime * 2;
                bpos[i].localPosition = new Vector3(p.x, p.y, Mathf.Lerp(0.5f, -2, e));
                yield return null;
            }
            pressed[i] = false;
            bpos[i].localPosition = new Vector3(p.x, p.y, 0.5f);
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} <t/m/b><l/r> [Position of button. Chain with spaces.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        List<int> p = new List<int> { };
        for(int i = 0; i < commands.Length; i++)
        {
            if (commands[i].Length < 1)
                continue;
            if (commands[i].Length != 2)
            {
                yield return "sendtochaterror!f " + commands[i] + " is an invalid command.";
                yield break;
            }
            int x = "lr".IndexOf(commands[i][1].ToString());
            int y = "tmb".IndexOf(commands[i][0].ToString()) * 2;
            if (x < 0 || y < 0)
            {
                yield return "sendtochaterror!f " + commands[i] + " is an invalid position.";
                yield break;
            }
            p.Add(x + y);
        }
        if(p.GroupBy(x => x).Any(x => x.Count() > 1) || p.Any(x => pressed[x]))
        {
            yield return "sendtochaterror!f Buttons cannot be pressed more than once.";
            yield break;
        }
        while (!pressable)
            yield return true;
        for (int i = 0; i < p.Count(); i++)
        {
            yield return null;
            buttons[p[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
            if (!pressable)
                yield break;
        }
    }
}
