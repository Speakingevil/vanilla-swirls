using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class ButtonwordScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public KMSelectable modselect;
    public List<KMSelectable> buttons;
    public Transform[] bpos;
    public Renderer[] brends;
    public Material[] bmats;
    public TextMesh[] blabels;
    public Transform lid;
    public Renderer strip;
    public Light[] striplights;
    public Light[] stagelights;

    private readonly string alph = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly string[] words = new string[130] { "ABORT", "ALERT", "ANGRY", "AVOWS", "AZURE", "BASIC", "BOXES", "BRAWL", "BRAVO", "BRUTE", "CARGO", "CHALK", "CLIMB", "CODEX", "CYBER", "DECOY", "DISCO", "DOZEN", "DRIVE", "DWARF", "EARTH", "EIGHT", "EMPTY", "ENJOY", "EPOXY", "FANCY", "FJORD", "FLASH", "FUDGE", "FUNGI", "GHAST", "GLAZE", "GLYPH", "GNAWS", "GUARD", "HABIT", "HAVOC", "HAZEL", "HELIX", "HINGE", "IDENT", "IMAGE", "INDEX", "INPUT", "IVORY", "JACKS", "JERKY", "JOUST", "JUICY", "JUMBO", "KANJI", "KAPUT", "KLUTZ", "KNIFE", "KUDOS", "LAPSE", "LEMON", "LIMBO", "LOTUS", "LYRIC", "MACHO", "MERIT", "METAL", "MIXUP", "MUSIC", "NEXUS", "NITRO", "NOMAD", "NOVAE", "NYMPH", "OFTEN", "OMEGA", "OSCAR", "OUIJA", "OXIDE", "PANIC", "PEACH", "PIQUE", "POINT", "POWER", "QIBLA", "QUBIT", "QUERY", "QUICK", "QUOTA", "RADIX", "RATIO", "RHUMB", "RHYME", "RIOJA", "SALTY", "SAUCE", "SHAFT", "SPICE", "SPRAY", "TAUPE", "THINK", "TOPAZ", "TOXIC", "TWIRL", "ULTRA", "UMBRA", "UNBOX", "UNZIP", "UPJET", "VENOM", "VIBEX", "VOICE", "VODKA", "VOXEL", "WAVER", "WAZIR", "WHOMP", "WIDTH", "WURST", "XENIA", "XERIC", "XYLAN", "XYLIC", "XYSTI", "YACHT", "YIELD", "YOKEL", "YOUNG", "YUPON", "ZEBRA", "ZEROS", "ZILCH", "ZYGON", "ZYMES"};
    private readonly int[,] wnums = new int[130, 5] { { 0, 1, 14, 17, 19 }, { 0, 4, 11, 17, 19 }, { 0, 6, 13, 17, 24 }, { 0, 14, 18, 21, 22 }, { 0, 4, 17, 20, 25 }, { 0, 1, 2, 8, 18 }, { 1, 4, 14, 18, 23 }, { 0, 1, 11, 17, 22 }, { 0, 1, 14, 17, 21 }, { 1, 4, 17, 19, 20 }, { 0, 2, 6, 14, 17 }, { 0, 2, 7, 10, 11 }, { 1, 2, 8, 11, 12 }, { 2, 3, 4, 14, 23 }, { 1, 2, 4, 17, 24 }, { 2, 3, 4, 14, 24 }, { 2, 3, 8, 14, 18 }, { 3, 4, 13, 14, 25 }, { 3, 4, 8, 17, 21 }, { 0, 3, 5, 17, 22 }, { 0, 4, 7, 17, 19 }, { 4, 6, 7, 8, 19 }, { 4, 12, 15, 19, 24 }, { 4, 9, 13, 14, 24 }, { 4, 14, 15, 23, 24 }, { 0, 2, 5, 13, 24 }, { 3, 5, 9, 14, 17 }, { 0, 5, 7, 11, 18 }, { 3, 4, 5, 6, 20 }, { 5, 6, 8, 13, 20 }, { 0, 6, 7, 18, 19 }, { 0, 4, 6, 11, 25 }, { 6, 7, 11, 15, 24 }, { 0, 6, 13, 18, 22}, { 0, 3, 6, 17, 20 }, { 0, 1, 7, 8, 19 }, { 0, 2, 7, 14, 21 }, { 0, 4, 7, 11, 25 }, { 4, 7, 8, 11, 23 }, { 4, 6, 7, 8, 13 }, { 3, 4, 8, 13, 19 }, { 0, 4, 6, 8, 12 }, { 3, 4, 8, 13, 23 }, { 8, 13, 15, 19, 20 }, { 8, 14, 17, 21, 24 }, { 0, 2, 9, 10, 18 }, { 4, 9, 10, 17, 24 }, { 9, 14, 18, 19, 20 }, { 2, 8, 9, 20, 24 }, { 1, 9, 12, 14, 20 }, { 0, 8, 9, 10, 13 }, { 0, 10, 15, 19, 20 }, { 10, 11, 19, 20, 25 }, { 4, 5, 8, 10, 13 }, { 3, 10, 14, 18, 20 }, { 0, 4, 11, 15, 18 }, { 4, 11, 12, 13, 14 }, { 1, 8, 11, 12, 14 }, { 11, 14, 18, 19, 20 }, { 2, 8, 11, 17, 24 }, { 0, 2, 7, 12, 14 }, { 4, 8, 12, 17, 19 }, { 0, 4, 11, 12, 19 }, { 8, 12, 15, 20, 23 }, { 2, 8, 12, 18, 20 }, { 4, 13, 18, 20, 23 }, { 8, 13, 14, 17, 19 }, { 0, 3, 12, 13, 14 }, { 0, 4, 13, 14, 21 }, { 7, 12, 13, 15, 24 }, { 4, 5, 13, 14, 19 }, { 0, 4, 6, 12, 14 }, { 0, 2, 14, 17, 18 }, { 0, 8, 9, 14, 20 }, { 3, 4, 8, 14, 23 }, { 0, 2, 8, 13, 15 }, { 0, 2, 4, 7, 15 }, { 4, 8, 15, 16, 20 }, { 8, 13, 14, 15, 19 }, { 4, 14, 15, 17, 22 }, { 0, 1, 8, 11, 16 }, { 1, 8, 16, 19, 20 }, { 4, 16, 17, 20, 24 }, { 2, 8, 10, 16, 20 }, { 0, 14, 16, 19, 20 }, { 0, 3, 8, 17, 23 }, { 0, 8, 14, 17, 19 }, { 1, 7, 12, 17, 20 }, { 4, 7, 12, 17, 24 }, { 0, 8, 9, 14, 17 }, { 0, 11, 18, 19, 24 }, { 0, 2, 4, 18, 20 }, { 0, 5, 7, 18, 19 }, { 2, 4, 8, 15, 18 }, { 0, 15, 17, 18, 24 }, { 0, 4, 15, 19, 20 }, { 7, 8, 10, 13, 19 }, { 0, 14, 15, 19, 25 }, { 2, 8, 14, 19, 23 }, { 8, 11, 17, 19, 22 }, { 0, 11, 17, 19, 20 }, { 0, 1, 12, 17, 20 }, { 1, 13, 14, 20, 23 }, { 8, 13, 15, 20, 25 }, { 4, 9, 15, 19, 20 }, { 4, 12, 13, 14, 21 }, { 1, 4, 8, 21, 23 }, { 2, 4, 8, 14, 21 }, { 0, 3, 10, 14, 21 }, { 4, 11, 14, 21, 23 }, { 0, 4, 17, 21, 22 }, { 0, 8, 17, 22, 25 }, { 7, 12, 14, 15, 22 }, { 3, 7, 8, 19, 22 }, { 17, 18, 19, 20, 22 }, { 0, 4, 8, 13, 23 }, { 2, 4, 8, 17, 23 }, { 0, 11, 13, 23, 24 }, { 2, 8, 11, 23, 24 }, { 8, 18, 19, 23, 24 }, { 0, 2, 7, 19, 24 }, { 3, 4, 8, 11, 24 }, { 4, 10, 11, 14, 24 }, { 6, 13, 14, 20, 24 }, { 13, 14, 15, 20, 24 }, { 0, 1, 4, 17, 25 }, { 4, 14, 17, 18, 25 }, { 2, 7, 8, 11, 25 }, { 6, 13, 14, 24, 25 }, { 4, 12, 18, 24, 25 } };
    private readonly int[][] adj = new int[9][] { new int[2] { 1, 3}, new int[3] { 0, 2, 4}, new int[2] { 1, 5}, new int[3] { 0, 4, 6}, new int[4] { 1, 3, 5, 7}, new int[3] { 2, 4, 8}, new int[2] { 3, 7}, new int[3] { 4, 6, 8}, new int[2] { 5, 7} };
    private readonly int[,] immsel = new int[10, 5] { {2, 1, 0, -1, 3}, {0, 1, 3, 2, -1}, {3, -1, 2, 0, 1}, {1, 0, -1, 3, 2}, {2, 0, 3, 1, -1}, {-1, 3, 1, 2, 0}, {0, -1, 1, 3, 2}, {1, 2, -1, 0, 3}, {-1, 3, 2, 1, 0}, {3, 2, 0, -1, 1} };
    private readonly string[] plog = new string[9] { "top-left", "top-middle", "top-right", "middle-left", "centre", "middle-right", "bottom-left", "bottom-middle", "bottom-right"};
    private readonly string[] bclog = new string[4] { "red", "blue", "yellow", "white"};
    private List<int> order = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
    private int[] bcols = new int[9];
    private int[] press = new int[9];
    private int[] letters = new int[9];
    private string word;
    private int currpress = -1;
    private int stripcol;
    private int reltime;
    private int stage;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        strip.material.color = new Color(0, 0, 0);
        int index = Random.Range(0, 130);
        word = words[index];
        for (int i = 0; i < 9; i++)
            letters[i] = i < 5 ? wnums[index, i] : -1;
        for (int i = 5; i < 9; i++)
        {
            List<int> opt = alph.Select(x => x - 'A').ToList();
            for (int j = 0; j < i; j++)
                opt.Remove(letters[j]);
            for (int j = 0; j < i - 3; j++)
                for (int k = j + 1; k < i - 2; k++)
                    for (int p = k + 1; p < i - 1; p++)
                        for (int q = p + 1; q < i; q++)
                            for (int n = 0; n < 26; n++)
                            {
                                if (!opt.Contains(n))
                                    continue;
                                int[] sel = new int[5] { letters[j], letters[k], letters[p], letters[q], n }.OrderBy(x => x).ToArray();
                                if (Enumerable.Range(0, 130).Any(x => Enumerable.Range(0, 5).All(y => wnums[x, y] == sel[y])))
                                    opt.Remove(n);
                            }
            letters[i] = opt.PickRandom();
        }
        order = order.Shuffle().ToList();
        for (int i = 0; i < 9; i++)
        {
            blabels[i].text = alph[letters[order[i]]].ToString();
            bcols[i] = Random.Range(0, 4);
            brends[i].material = bmats[bcols[i]];
            press[i] = order[i] > 4 ? 0 : -1;
            Debug.LogFormat("[Buttonword #{0}] The {1} button is {2}, labelled {3}.", moduleID, plog[i], bclog[bcols[i]], blabels[i].text);
        }
        Debug.LogFormat("[Buttonword #{0}] The password is \"{1}\".", moduleID, word);
    }

    private void Start()
    {
        for (int i = 0; i < 9; i++)
            if (press[i] >= 0)
            {
                char t = blabels[i].text[0];
                switch (bcols[i])
                {
                    case 0:
                        if ("AEIOU".Contains(t.ToString()) || adj[i].Any(x => "AEIOU".Contains(alph[letters[order[x]]].ToString())))
                            press[i]++;
                        if (adj[i].All(x => letters[order[x]] < t) || adj[i].All(x => letters[order[x]] > t))
                            press[i]++;
                        if (Mathf.Abs(t - info.GetSerialNumberLetters().First()) <= 6)
                            press[i]++;
                        if (info.GetSerialNumberNumbers().Any(x => (t - 'A' + 1) % 10 == x))
                            press[i]++;
                        break;
                    case 1:
                        if (Enumerable.Range(0, 5).Any(x => Mathf.Abs(word[x] - t) <= 1))
                            press[i]++;
                        if (info.GetIndicators().Count() > 1 && info.GetIndicators().All(x => !x.Contains(t)))
                            press[i]++;
                        if (word[0] % 2 == t % 2)
                            press[i]++;
                        if (i % 3 == order[4] % 3 || i / 3 == order[4] / 3)
                            press[i]++;
                        break;
                    case 2:
                        if ((t < word[0] && t > word[4]) || (t < word[4] && t > word[0]))
                            press[i]++;
                        if (adj[i].All(x => order[x] < 5))
                            press[i]++;
                        if ((info.GetSerialNumberLetters().Last() - 'A' < 13 && t - 'A' < 13) || (info.GetSerialNumberLetters().Last() - 'A' > 12 && t - 'A' > 12))
                            press[i]++;
                        if (info.GetSerialNumberNumbers().Contains((i % 3) * 3 + (i / 3) + 1))
                            press[i]++;
                        break;
                    default:
                        if (info.GetSerialNumberLetters().Contains(t) || adj[i].Select(x => alph[letters[order[x]]]).Any(x => info.GetSerialNumberLetters().Contains(x)))
                            press[i]++;
                        if (Mathf.Abs(t - word[4]) <= 6)
                            press[i]++;
                        if (info.GetSerialNumberNumbers().Contains(letters.Count(x => x > t - 'A')))
                            press[i]++;
                        if (adj[i].Select(x => bcols[x]).GroupBy(x => x).Any(x => x.Count() > 1))
                            press[i]++;
                        break;
                }
                switch (press[i])
                {
                    case 0:
                        Debug.LogFormat("[Buttonword #{0}] The {1} button fails all criteria.", moduleID, plog[i]);
                        break;
                    case 1:
                        Debug.LogFormat("[Buttonword #{0}] The {1} button meets 1 criterion.", moduleID, plog[i]);
                        break;
                    default:
                        Debug.LogFormat("[Buttonword #{0}] The {1} button meets {2} criteria.", moduleID, plog[i], press[i]);
                        break;
                }
            }
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate () { StartCoroutine(Push(b)); return false; };
            button.OnInteractEnded += delegate () { StopAllCoroutines(); StartCoroutine(Release(b)); };
        }
        modselect.OnFocus += delegate () { StartCoroutine(Lid(true)); };
        modselect.OnDefocus += delegate () { StartCoroutine(Lid(false)); };
    }

    private IEnumerator Lid(bool up)
    {
        float e = (lid.localEulerAngles.z - 90) / 120;
        if (up)
        {
            while (e < 1)
            {
                float d = Time.deltaTime;
                e += d * 2;
                lid.localEulerAngles = new Vector3(0, -90, Mathf.Lerp(90, 210, e));
                yield return null;
            }
            lid.localEulerAngles = new Vector3(0, -90, 210);
        }
        else
        {
            while (e > 0)
            {
                float d = Time.deltaTime;
                e -= d * 2;
                lid.localEulerAngles = new Vector3(0, -90, Mathf.Lerp(90, 210, e));
                yield return null;
            }
            lid.localEulerAngles = new Vector3(0, -90, 90);
        }
    }

    private IEnumerator Push(int b)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, bpos[b]);
        float e = (0.215f - bpos[b].localPosition.z) / 0.135f;
        while(e < 1)
        {
            float d = Time.deltaTime;
            e += d * 6;
            bpos[b].localPosition = new Vector3(0, 0, Mathf.Lerp(0.215f, 0.08f, e));
            yield return null;
        }
        bpos[b].localPosition = new Vector3(0, 0, 0.08f);
        stripcol = Random.Range(0, 4);
        StartCoroutine(Stripflash(stripcol));
        if (!moduleSolved)
        {
            Debug.LogFormat("[Buttonword #{0}] Pressing the {1} button:", moduleID, plog[b]);
            currpress = b;
            int c = -1;
            if(press[b] >= 0)
                c = immsel[info.GetSerialNumberNumbers().Last(), press[b]];
            Debug.LogFormat("[Buttonword #{0}] The strip is flashing {1}.", moduleID, bclog[stripcol]);
            if (c == stripcol)
            {
                reltime = -1;
                Debug.LogFormat("[Buttonword #{0}] Release immediately!", moduleID);
                yield return new WaitForSeconds(0.75f);
                reltime = -2;
                Debug.LogFormat("[Buttonword #{0}] Too late!", moduleID);
            }
            else
            {
                char t = blabels[b].text[0];
                switch ((stripcol * 5) + press[b])
                {
                    case 0: reltime = word.Count(x => x < t); break;
                    case 1: reltime = info.GetSerialNumberNumbers().Min(); break;
                    case 2: reltime = adj[b].Select(x => bcols[x]).Distinct().Count(); break;
                    case 3: reltime = Mathf.Abs(word[0] - word[4]); break; ;
                    case 4: reltime = info.GetStrikes(); break;
                    case 5: reltime = bcols.Count(x => x == bcols[b]); break;
                    case 6: reltime = letters.Min() + 1; break;
                    case 7: reltime = info.GetSerialNumberNumbers().Max() - info.GetSerialNumberNumbers().Min(); break;
                    case 8: reltime = word[0] - 'A' + 1; break;
                    case 9: reltime = stage; break;
                    case 10: reltime = adj[b].Select(x => letters[order[x]]).Max() - adj[b].Select(x => letters[order[x]]).Min(); break;
                    case 11: reltime = letters.Where((x, i) => bcols[i] == bcols[b]).Min(); break;
                    case 12: reltime = t - 'A' + 1; break;
                    case 13: reltime = word.Count(x => x > t); break;
                    case 14: reltime = info.GetSolvedModuleNames().Count(); break;
                    case 15: reltime = bcols.GroupBy(x => x).Select(x => x.Count()).Max(); break;
                    case 16: reltime = Mathf.Abs(word[0] - word[1]); break;
                    case 17: reltime = info.GetSolvableModuleNames().Count() - info.GetSolvedModuleNames().Count(); break;
                    case 18: reltime = adj[b].Select(x => letters[order[x]]).Min() + 1; break;
                    default: reltime = letters.Max() - letters.Min(); break;
                }
                reltime %= 6;
                Debug.LogFormat("[Buttonword #{0}] Release when the bomb timer has a {1} in any position.", moduleID, reltime);
            }
        }
    }

    private IEnumerator Stripflash(int b)
    {
        Color basecol = new Color[] { new Color(1, 0, 0), new Color(0, 0.25f, 1), new Color(1, 0.75f, 0), new Color(1, 1, 1)}[b];
        foreach (Light c in striplights)
            c.color = basecol;
        float e = 0;
        while (true)
        {
            e += Mathf.PI * Time.deltaTime;
            float a = Mathf.Cos(e);
            a *= a;
            strip.material.color = basecol * a;
            a *= 10;
            if (currpress >= 5 && press[b] < 0)
                for (int i = 0; i < stage; i++)
                    stagelights[i].intensity = a;
            a *= 5;
            foreach (Light l in striplights)
                l.intensity = a;
            yield return null;
        }
    }


    private IEnumerator Release(int b)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, bpos[b]);
        strip.material.color = new Color(0, 0, 0);
        foreach (Light s in striplights)
            s.intensity = 0;
        foreach (Light s in stagelights)
            s.intensity = 10;
        if (!moduleSolved)
        {
            if (currpress >= 0)
            {
                Debug.LogFormat("[Buttonword #{0}] Released button at {1}.", moduleID, info.GetFormattedTime());
                if (order[currpress] < 5)
                {
                    Debug.LogFormat("[Buttonword #{0}] Wrong button held.", moduleID);
                    module.HandleStrike();
                }
                else if (press[b] < 0)
                    Debug.LogFormat("[Buttonword #{0}] Button press already submitted.", moduleID);
                else if (reltime == -1 || info.GetFormattedTime().SkipWhile(x => x == '0' || x == ':').Any(x => x - '0' == reltime))
                {
                    stagelights[stage].enabled = true;
                    stage++;
                    press[b] = -1;
                    Debug.LogFormat("[Buttonword #{0}] Correct.", moduleID);
                    if (stage > 3)
                    {
                        moduleSolved = true;
                        module.HandlePass();
                    }
                }
                else
                {
                    Debug.LogFormat("[Buttonword #{0}] Incorrect release time.", moduleID);
                    module.HandleStrike();
                }
            }
        }
        currpress = -1;
        float e = (bpos[b].localPosition.z - 0.215f) / (0.08f - 0.215f);
        while (e > 0)
        {
            float d = Time.deltaTime;
            e -= d * 12;
            bpos[b].localPosition = new Vector3(0, 0, Mathf.Lerp(0.215f, 0.08f, e));
            yield return null;
        }
        bpos[b].localPosition = new Vector3(0, 0, 0.215f);
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} hold 1-9 <R/B/Y/W> [Holds button in column-ordered position. Releases immediately if strip flashes the specified colour.] | !{0} release ##:## [Releases held button at the specified minutes and seconds.]";
#pragma warning restore 414

    bool ZenModeActive;

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToUpperInvariant().Split(' ');
        int d = 0;
        switch (commands[0])
        {
            case "HOLD":
                if (currpress >= 0)
                {
                    yield return "sendtochaterror The held button must be released before another can be held.";
                    yield break;
                }
                if (int.TryParse(commands[1], out d))
                {
                    if (d > 0 && d < 10)
                    {
                        d -= 1;
                        d = (d % 3) * 3 + (d / 3);
                        int r = -1;
                        if (commands.Length > 2)
                        {
                            if (commands[2].Length == 1 && "RBYW".Contains(commands[2]))
                                r = "RBYW".IndexOf(commands[2]);
                            else
                            {
                                yield return "sendtochaterror Invalid release colour.";
                                yield break;
                            }
                        }
                        yield return null;
                        buttons[d].OnInteract();
                        if(r == stripcol)
                        {
                            yield return new WaitForSeconds(0.25f);
                            buttons[d].OnInteractEnded();
                        }
                    }
                    else
                        yield return "sendtochaterror Invalid button position.";
                }
                else
                    yield return "sendtochaterror Invalid button position.";
                yield break;
            case "RELEASE":
                if (currpress < 0)
                {
                    yield return "sendtochaterror No buttons are held.";
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
                        buttons[currpress].OnInteractEnded();
                        currpress = -1;
                    }
                }
                else
                    yield return "sendtochaterror NaN minutes or seconds entered.";
                yield break;
            default:
                yield return "sendtochaterror Invalid command.";
                yield break;
        }
    }
}
