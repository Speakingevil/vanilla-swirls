using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class PasswordButtonsScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Transform[] bpush;
    public Renderer[] brends;
    public Renderer[] lrends;
    public Renderer screen;
    public Light[] leds;
    public Light scrlight;
    public Material[] bmats;
    public Material[] scrmats;
    public Material[] io;
    public TextMesh[] disps;
    public GameObject matstore;

    private readonly List<string> words = new List<string> {"ABYSS", "AFTER", "ALBUM", "AVOID", "AZTEC", "BANJO", "BLOOM", "BRICK", "BYTES", "CHEWY", "CLOTH", "CRAWL", "CRISP", "CURSE", "DELTA", "DJINN", "DOZEN", "DWARF", "EARLY", "EIGHT", "EJECT", "EQUAL", "EXIST", "FEWER", "FIRST", "FORUM", "FUNGI", "GHOST", "GLYPH", "GUMBO", "GYROS", "HAVOC", "HERTZ", "HONEY", "HYDRA", "IAMBS", "IDIOM", "IOTAS", "ITCHY", "IVORY", "JAZZY", "JIVED", "JOCKS", "JUICE", "KANJI", "KARMA", "KINGS", "KLUTZ", "KUGEL", "LATEX", "LENTO", "LIMIT", "LUNAR", "MAJOR", "MERCY", "MONAD", "MUNTZ", "NEWLY", "NIXIE", "NORTH", "NUDGE", "OCTAL", "OLIVE", "ORBIT", "OXBOW", "OZONE", "PANDA", "PIANO", "PRISM", "PUNCH", "QOPHS", "QUACK", "QUBIT", "QUOTA", "RADIX", "RIVET", "ROUND", "RUGBY", "SHOGI", "SKULL", "SQUID", "SPICY", "STYLE", "SWIFT", "SYRUP", "TAROK", "THORN", "TEMPO", "TOQUE", "TWIXT", "ULTRA", "UMAMI", "URBAN", "UVULA", "VENOM", "VIBEX", "VINYL", "VOWEL", "WALTZ", "WHISK", "WITCH", "WORLD", "WRONG", "WUSHU", "XENIA", "XYLEM", "YACHT", "YEILD", "YOLKS", "YUCCA", "ZEBRA", "ZILCH", "ZLOTY", "ZOWIE"};
    private readonly int[,] subdigs = new int[114, 5] { { 1, 8, 6, 5, 4 }, { -1, 8, 7, 0, 1 }, { 0, 7, 2, -1, 1 }, { 4, 8, 3, 5, 0 }, { 7, 9, 2, 3, 8 }, { 3, 2, 4, 5, 0 }, { 1, 4, 0, 3, 9 }, { 2, 0, 9, 3, -1 }, { 4, 1, 2, 0, 3 }, { 4, 2, 7, 8, -1 }, { 9, 7, -1, 3, 8 }, { 5, 7, 8, 2, 4 }, { 9, 7, 3, 4, 6 }, { 5, 8, 0, -1, 6 }, { 8, 6, 4, 7, 0 }, { 8, 7, 5, 2, 6 }, { 4, 5, 7, 1, 0 }, { 0, 7, 8, 9, 3 }, { 8, 1, 6, 9, 2 }, { 2, 4, -1, 3, 7 }, { 0, 5, 3, 4, 8 }, { 4, 7, 9, 5, 1 }, { 8, 5, 1, 6, 3 }, { 7, 1, 3, 4, 2 }, { 0, 1, 9, 2, 6 }, { 9, 5, 1, 7, 8 }, { 0, 7, 3, 8, 4 }, { 2, 7, 4, -1, 6 }, { 7, 6, 2, 1, 0 }, { 8, 1, 7, 6, 4 }, { 1, 7, 9, -1, 3 }, { 9, 3, 2, -1, 5 }, { 2, 5, -1, 7, 1 }, { 7, 1, 2, 9, 4 }, { 5, 8, 9, 7, 3 }, { 9, 0, 8, 7, -1 }, { -1, 6, 3, 2, 1 }, { 3, 5, 9, 4, 0 }, { 0, -1, 9, 5, 4 }, { 0, 9, 1, 3, 6 }, { 8, 4, 0, 1, 3 }, { 8, 1, 0, 5, 6 }, { 2, 9, 6, 7, 3 }, { 3, 9, 5, 2, 4 }, { 0, 3, 6, -1, 7 }, { 0, 6, 2, 5, -1 }, { 5, 1, 0, 9, 8 }, { 6, 2, 1, 3, 5 }, { 5, 1, 6, 4, 3 }, { 3, 0, 1, 8, 2 }, { 1, 5, 4, 0, 7 }, { 2, 6, 4, 3, 9 }, { 1, 4, 7, 6, 8 }, { 6, 3, 0, 2, 4 },
        { 5, 3, 0, 4, -1 }, { 7, 3, 4, 0, -1 }, { 1, 4, 6, 9, 3 }, { 2, 1, 0, 8, 7 }, { 8, 0, -1, 6, 9 }, { 5, 2, 4, 3, 6 }, { 9, 6, 5, 8, 0 }, { 5, 9, 1, 6, 8 }, { 8, 9, 3, 0, 6 }, { 9, -1, 5, 3, 7 }, { 2, 8, 1, 9, 4 }, { 3, 4, 6, 2, -1 }, { 0, 4, 9, 1, 2 }, { 5, 3, 2, 7, 6 }, { 1, -1, 2, 6, 3 }, { 0, 9, -1, 8, 1 }, { 6, 5, 8, 0, 1 }, { 2, 4, 3, 8, 0 }, { 1, 0, 2, 4, 7 }, { 2, 7, 0, 6, 1 }, { 8, 5, 9, 1, 7 }, { 8, 6, -1, 5, 1 }, { 5, 2, 3, -1, 9 }, { 0, 6, 5, 7, 8 }, { 4, 0, 1, 2, 9 }, { -1, 5, 6, 3, 7 }, { 6, 3, 9, 5, 0 }, { 8, 6, 1, 3, 7 }, { 9, 1, 6, 8, 4 }, { 2, -1, 3, 5, 8 }, { 9, 1, 8, 4, 7 }, { 2, 0, 7, 5, 1 }, { 7, 2, -1, 0, 6 }, { 3, 4, 8, -1, 7 }, { 5, 4, 1, 7, 2 }, { 8, 3, 5, 7, 1 }, { -1, 7, 5, 6, 9 }, { 0, 4, 5, 9, 1 }, { 5, 2, -1, 8, 7 }, { 6, 1, 2, 7, 9 }, { 7, 8, 0, 6, 9 }, { 0, 5, 4, 1, 9 }, { 8, 5, 0, 9, 4 }, { 4, 2, 8, 7, 6 }, { 0, 1, 2, 8, 5 }, { 9, 8, 0, 5, -1 }, { 3, -1, 8, 2, 9 }, { 1, 5, -1, 8, 0 }, { 6, 3, 5, 0, 7 }, { 2, -1, 7, 3, 4 }, { 2, 6, 7, 8, 5 }, { 8, 4, 3, 5, 9 }, { 4, 3, 6, 8, 0 }, { 3, 9, 2, 0, 5 }, { 6, 9, 3, -1, 4 }, { 2, 7, 9, 1, 0 }, { 4, 6, 9, 2, 0 }, { 6, 9, 7, 8, 0 }, { 6, 2, -1, 5, 3 }, { 4, 5, 8, 2, 3 } };
    private List<string>[] letterlists = new List<string>[5];
    private string[,] lselect = new string[5, 5];
    private string word;
    private int[] displets = new int[5];
    private int[] bcols = new int[5];
    private int[][] pressigns = new int[2][] { new int[5] { 0, 1, 2, 3, 4}, new int[5] { -1, -1, -1, -1, -1} };
    private int[] holdtime = new int[2];
    private bool hold;
    private bool struckhold;
    private int holdton;
    private bool lockrel;
    private bool[] unlock = new bool[5];
    private int[,] skip = new int[5, 5];
    private int subton;
    private int scrcol;

    private static int moduleIDCounter;
    private int moduleID;

	private void Start()
    {        
        moduleID = ++moduleIDCounter;
        matstore.SetActive(false);
	float scalar = transform.lossyScale.x;
        foreach (Light l in leds)
            l.range *= scalar;
	scrlight.range *= scalar;
        word = words.PickRandom();
        for(int i = 0; i < 5; i++)
        {
            letterlists[i] = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
            lselect[i, 0] = word[i].ToString();
            letterlists[i].Remove(word[i].ToString());
            displets[i] = Random.Range(0, 5);
            bcols[i] = Random.Range(0, 5);
            brends[i].material = bmats[bcols[i]];
        }
        Debug.LogFormat("[Password Buttons #{0}] The buttons have the colours: {1}.", moduleID, string.Join(", ", bcols.Select(x => new string[] { "Red", "Yellow", "Blue", "White", "Black"}[x]).ToArray()));
        while(displets.All(x => x == 0))
            for(int i = 0; i < 5; i++)
                displets[i] = Random.Range(0, 5);
        for (int i = 1; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
            repeat:;
                string let = letterlists[j].PickRandom();
                letterlists[j].Remove(let);
                lselect[j, i] = let;
                string[] test = new string[5];
                test[j] = let;
                for(int k = 0; k < i + 1; k++)
                {
                    if (j > 0)
                        test[0] = lselect[0, k];
                    for(int m = 0; m < (j >= i ? i : i + 1); m++)
                    {
                        if (j != 1)
                            test[1] = lselect[1, m];
                        for(int n = 0; n < (j >= i ? i : i + 1); n++)
                        {
                            if (j != 2)
                                test[2] = lselect[2, n];
                            for(int p = 0; p < (j >= i ? i : i + 1); p++)
                            {
                                if (j != 3)
                                    test[3] = lselect[3, p];
                                for(int q = 0; q < (j >= i ? i : i + 1); q++)
                                {
                                    if (j < 4)
                                        test[4] = lselect[4, q];
                                    if (words.Contains(string.Join("", test)))
                                        goto repeat;
                                }
                            }
                        }
                    }
                }
            }
        }
        for (int i = 0; i < 5; i++)
        {
            disps[i].text = lselect[i, displets[i]];
            Debug.LogFormat("[Password Buttons #{0}] The {1} display shows the letters: {2}.", moduleID, new string[] { "first", "second", "third", "fourth", "fifth"}[i], string.Join(", ", Enumerable.Range(0, 5).Select(x => lselect[i, (x + displets[i]) % 5]).ToArray()));
        }
        Debug.LogFormat("[Password Buttons #{0}] The password is {1}.", moduleID, word);
        pressigns[0] = pressigns[0].Shuffle();
        subton = Random.Range(0, 5);
        pressigns[1][subton] = pressigns[0][subton];
        int[] set = new int[2] { Random.Range(0, 5), pressigns[1][subton] };
        while (set[0] == subton)
            set[0] = Random.Range(0, 5);
        pressigns[1][set[0]] = set[1];
        for(int i = 0; i < 3; i++)
        {
            set[1] = pressigns[0][set[0]];
            while (pressigns[1][set[0]] >= 0)
                set[0] = Random.Range(0, 5);
            pressigns[1][set[0]] = set[1];
        }
        for (int i = 0; i < 5; i++)
            if(i == subton)
                Debug.LogFormat("[Password Buttons #{0}] The {1} button toggles display {2}.", moduleID, new string[] { "first", "second", "third", "fourth", "fifth" }[i], pressigns[0][i] + 1);
            else
                Debug.LogFormat("[Password Buttons #{0}] The {1} button toggles displays {2} and {3}.", moduleID, new string[] { "first", "second", "third", "fourth", "fifth" }[i], pressigns[0][i] + 1, pressigns[1][i] + 1);
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate ()
            {
                bpush[b].localPosition -= new Vector3(0, 0.5f, 0);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, bpush[b]);
                button.AddInteractionPunch(0.1f);
                holdton = b + 1;
                if (!unlock[b])
                {
                    int r = 0;
                    holdtime[0] = (int)info.GetTime() % 10;
                    string display = string.Join("", displets.Select((x, i) => lselect[i, x]).ToArray());
                    Debug.LogFormat("[Password Buttons #{0}] Button {1} pressed at {2} with display: {3}", moduleID, b + 1, info.GetFormattedTime(), display);
                    switch (bcols[b])
                    {
                        case 0:
                            if (!info.GetSerialNumberLetters().Any(x => display.Contains(x.ToString())))
                            {
                                if (display.Skip(1).All(x => x > display[0]))
                                    r = 1;
                                else if (b == 0 || b == 4)
                                    r = 2;
                                else
                                    r = 3;
                            }
                            break;
                        case 1:
                            if (display.Distinct().Count() > 4)
                            {
                                if (display[2] == display.Take(4).Skip(1).OrderBy(x => x).ToArray()[1])
                                    r = 1;
                                else if ((b == 0 || (bcols[b - 1] == 2 || bcols[b - 1] == 4)) && (b == 4 || (bcols[b + 1] == 2 || bcols[b + 1] == 4)))
                                    r = 2;
                                else
                                    r = 3;
                            }
                            break;
                        case 2:
                            if (display.Any(x => "AEIOU".Contains(x.ToString())))
                            {
                                if (display.Take(4).All(x => x < display[4]))
                                    r = 1;
                                else if (bcols.Where((x, i) => Mathf.Abs(b - i) > 1).Contains(3))
                                    r = 2;
                                else
                                    r = 3;
                            }
                            break;
                        case 3:
                            int[] pairs = new int[25];
                            for (int i = 0; i < 25; i++)
                                pairs[i] = Mathf.Abs(display[i / 5] - display[i % 5]);
                            if (!pairs.Contains(1))
                            {
                                if (display[0] < 'N' ^ display[4] < 'N')
                                    r = 1;
                                else if (!bcols.Contains(1))
                                    r = 2;
                                else
                                    r = 3;
                            }
                            break;
                        default:
                            if (display.All(x => x != 'X'))
                            {
                                if (info.GetSerialNumberLetters().Any(x => Mathf.Abs(x - display[2]) < 2))
                                    r = 1;
                                else if (bcols.Distinct().Count() % 2 == 1)
                                    r = 2;
                                else
                                    r = 3;
                            }
                            break;
                    }
                    holdtime[1] = new int[,] { { 5, 9, 2, 3}, { 7, 1, 6, 5}, { 4, 3, 8, 0}, { 2, 0, 7, 9}, { 8, 6, 1, 4} }[bcols[b], r];
                    Debug.LogFormat("[Password Buttons #{0}] {1} {2} rule {3}.", moduleID, new string[] { "First", "Second", "Third", "Fourth"}[r], new string[] { "Red", "Yellow", "Blue", "White", "Black"}[bcols[b]], holdtime[0] == holdtime[1] ? "satisfied" : "failed");
                    StartCoroutine("ScreenFlash");
                }
                else
                    hold = true;
                return false;
            };
            button.OnInteractEnded = delegate ()
            {
                bpush[b].localPosition += new Vector3(0, 0.5f, 0);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
                button.AddInteractionPunch(0.6f);
                holdton = 0;
                if (!unlock[b])
                {
                    if (lockrel)
                    {
                        lockrel = false;
                        Debug.LogFormat("[Password Buttons #{0}] Locked button released. Display unchanged.", moduleID);
                        return;
                    }
                    string display = string.Join("", displets.Select((x, i) => lselect[i, x]).ToArray());
                    if (hold)
                    {
                        hold = false;
                        if (struckhold)
                        {
                            struckhold = false;
                            return;
                        }
                        bool accept = false;
                        int r = 0;
                        if (display.Contains("RYBW"[scrcol].ToString()))
                            r = 1;
                        else if (scrcol == bcols[b])
                            r = 2;
                        else if ((b > 0 && bcols[b - 1] == scrcol) || (b < 4 && bcols[b + 1] == scrcol))
                            r = 3;
                        else if (display.Any(x => bcols.Select(y => "RYBWK"[y]).Contains(x)))
                            r = 4;
                        else if (bcols[b] == 4)
                            r = 5;
                        accept = info.GetFormattedTime().Contains(r.ToString());
                        Debug.LogFormat("[Password Buttons #{0}] Button {1} released at {2}. {3} condition {4}.", moduleID, b + 1, info.GetFormattedTime(), new string[] { "First", "Second", "Third", "Fourth", "Fifth", "Sixth" }[(r + 5) % 6], accept ? "satisfied" : "failed");
                        if (!accept)
                        {
                            module.HandleStrike();
                            return;
                        }
                        StartCoroutine(Unlock(b));
                        Debug.LogFormat("[Password Buttons #{0}] Button {1} unlocked.", moduleID, b + 1);
                        for (int i = 0; i < 2; i++)
                            skip[b, pressigns[i][b]] = displets[pressigns[i][b]] + 1;

                    }
                    else
                    {
                        lockrel = false;
                        StopCoroutine("ScreenFlash");
                        Debug.LogFormat("[Password Buttons #{0}] Button {1} immediately released. {2}.", moduleID, b + 1, b == subton ? "Submitted " + display : "Incorrect submit button");
                        if (b != subton)
                            module.HandleStrike();
                        else
                        {
                            if (display == word)
                            {
                                Debug.LogFormat("[Password Buttons #{0}] Password submitted at {1}.", moduleID, info.GetFormattedTime());
                                int k = subdigs[words.IndexOf(word), bcols[b]];
                                if ((int)info.GetTime() % 10 == (k < 0 ? info.GetSerialNumberNumbers().Last() : k))
                                {
                                    module.HandlePass();
                                    StopAllCoroutines();
                                    for (int i = 0; i < 5; i++)
                                    {
                                        leds[i].enabled = false;
                                        lrends[i].material = io[1];
                                        unlock[i] = true;
                                        for (int j = 0; j < 5; j++)
                                            skip[i, j] = 0;
                                    }
                                }
                                else
                                {
                                    Debug.LogFormat("[Password Buttons #{0}] Incorrect submission time.", moduleID);
                                    module.HandleStrike();
                                }
                            }
                            else
                            {
                                Debug.LogFormat("[Password Buttons #{0}] {1} is not the password.", moduleID, display);
                                module.HandleStrike();
                            }
                        }
                        return;
                    }
                }
                else
                    hold = false;
                for(int i = 0; i < 2; i++)
                {
                    if (i == 0 && b == subton)
                        continue;
                    int c = pressigns[i][b];
                    displets[c] = (displets[c] + 1) % 5;
                    if(skip[b, c] == displets[c] + 1)
                        displets[c] = (displets[c] + 1) % 5;
                    disps[c].text = lselect[c, displets[c]];
                }
            };
        }
    }

    private IEnumerator ScreenFlash()
    {
        yield return new WaitForSeconds(0.5f);
        hold = true;
        if (holdtime[0] == holdtime[1])
        {
            scrcol = Random.Range(0, 4);
            Debug.LogFormat("[Password Buttons #{0}] The display has turned {1}.", moduleID, new string[] { "Red", "Yellow", "Blue", "White"}[scrcol]);
            screen.material = scrmats[scrcol + 1];
            scrlight.color = new Color[] { new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 1), new Color(1, 1, 1) }[scrcol];
            scrlight.enabled = true;
            foreach (TextMesh d in disps)
                d.color = new Color[] { new Color(0.3f, 0, 0), new Color(0.3f, 0.3f, 0), new Color(0, 0, 0.3f), new Color(0.3f, 0.3f, 0.3f) }[scrcol];
            float a = 0;
            while (hold)
            {
                a += Time.deltaTime;
                float c = Mathf.Cos(a * 1.5f - (Mathf.PI / 2));
                c = c * c * c * c;
                scrlight.intensity = 30 * c;
                c = 1 - c;
                foreach (TextMesh d in disps)
                {
                    Color t = d.color;
                    t.a = c;
                    d.color = t;
                }
                yield return null;
            }
            scrlight.intensity = 0;
            scrlight.enabled = false;
            screen.material = scrmats[0];
            foreach (TextMesh d in disps)
                d.color = new Color(0, 0.3f, 0);
        }
        else
        {
            struckhold = true;
            module.HandleStrike();
        }
    }

    private IEnumerator Unlock(int x)
    {
        unlock[x] = true;
        leds[x].enabled = false;
        lrends[x].material = io[1];
        yield return new WaitForSeconds(9);
        for(int i = 0; i < 9; i++)
        {
            leds[x].enabled = i % 2 == 0;
            lrends[x].material = io[i % 2];
            yield return new WaitForSeconds(0.11f);
        }
        Debug.LogFormat("[Password Buttons #{0}] Button {1} locked. The display is now: {2}", moduleID, x + 1, string.Join("", displets.Select((z, i) => lselect[i, z]).ToArray()));
        if (hold && holdton == x + 1)
            lockrel = true;
        leds[x].enabled = true;
        lrends[x].material = io[0];
        unlock[x] = false;
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "!{0} hold/release # at # [Interacts with the button at the specified position at the specified timer digit.] | !{0} cycle # [Cycles the letters corresponding to the specified unlocked button.] | !{0} toggle # [Presses each unlocked button the specified number of times.] | !{0} submit # at # [Taps the locked button at the specified position at the specified timer digit.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if (commands[0] == "hold")
        {
            if (commands.Length < 2 || commands.Length > 4)
            {
                yield return "sendtochaterror!f Invalid hold command length.";
                yield break;
            }
            int b = "12345".IndexOf(commands[1].ToString());
            if (commands[1].Length > 1 || b < 0)
            {
                yield return "sendtochaterror!f Invalid button position. Valid positions are numbers in the range 1-5.";
                yield break;
            }
            if (unlock[b])
            {
                yield return "sendtochaterror!f Button " + (b + 1) + "is unlocked. Use the toggle command to press unlocked buttons.";
                yield break;
            }
            if (hold)
            {
                yield return "sendtochaterror!f The held button must be released before another button can be held.";
                yield break;
            }
            int t = 0;
            if (int.TryParse(commands.Last(), out t))
            {
                if (t > 9)
                {
                    yield return "sendtochaterror!f Invalid hold time. Valid times are single digits.";
                    yield break;
                }
		yield return null;
                while ((int)info.GetTime() % 10 == t)
                    yield return null;
                while ((int)info.GetTime() % 10 != t)
                {
                    yield return "trycancel";
                    yield return null;
                }
                buttons[b].OnInteract();
                yield return null;
                if(holdtime[0] != holdtime[1])
                {
                    yield return "strike";
                    while (!struckhold)
                        yield return null;
                    buttons[b].OnInteractEnded();
                }
            }
            else
            {
                yield return "sendtochaterror!f Invalid hold time. Valid times are whole numbers.";
                yield break;
            }
        }
        else if (commands[0] == "release")
        {
            if (commands.Length < 2 || commands.Length > 4)
            {
                yield return "sendtochaterror!f Invalid release command length.";
                yield break;
            }
            int b = "12345".IndexOf(commands[1].ToString());
            if (commands[1].Length > 1 || b < 0)
            {
                yield return "sendtochaterror!f Invalid button position. Valid positions are numbers in the range 1-5.";
                yield break;
            }
            if(b + 1 != holdton)
            {
                yield return "sendtochaterror!f Button " + (b + 1) + " is not held down.";
                yield break;
            }
            int t = 0;
            if (int.TryParse(commands.Last(), out t))
            {
                if (t > 9)
                {
                    yield return "sendtochaterror!f Invalid release time. Valid times are single digits.";
                    yield break;
                }
		yield return "strike";
                while (info.GetFormattedTime().Contains(t.ToString()) && info.GetTime() % 10 <= 1)
                    yield return null;
                while (!info.GetFormattedTime().Contains(t.ToString()))
                {
                    yield return "trycancel";
                    yield return null;
                }
                buttons[b].OnInteractEnded();
            }
            else
                yield return "sendtochaterror!f Invalid release time. Valid times are whole numbers.";
        }
        else if (commands[0] == "cycle")
        {
            if(commands.Length != 2)
            {
                yield return "sendtochaterror!f Invalid cycle command length.";
                yield break;
            }
            int b = "12345".IndexOf(commands[1].ToString());
            if(commands[1].Length > 1 || b < 0)
            {
                yield return "sendtochaterror!f Invalid button position. Valid positions are numbers in the range 1-5.";
                yield break;
            }
            if (!unlock[b])
            {
                yield return "sendtochaterror!f Button " + (b + 1) + " must be unlocked before it can be cycled.";
                yield break;
            }
            for(int i = 0; i < 4; i++)
            {
                yield return null;
                if (!unlock[b])
                {
                    yield return "sendtochat Button " + (b + 1) + " locked before completing the cycle.";
                    yield break;
                }
                buttons[b].OnInteract();
                buttons[b].OnInteractEnded();
                yield return new WaitForSeconds(1);
            }
        }
        else if (commands[0] == "toggle")
        {
            if(commands.Length > 2)
            {
                yield return "sendtochaterror!f Invalid toggle command length.";
                yield break;
            }
            if(unlock.All(x => !x))
            {
                yield return "sendtochaterror!f There are no unlocked buttons to toggle.";
                yield break;
            }
            List<int> p = Enumerable.Range(0, 5).Where(x => unlock[x]).ToList();
            int n = 0;
            if (commands.Length < 2)
                n = 1;
            else if (!int.TryParse(commands[1], out n))
            {
                yield return "sendtochaterror!f Invalid number entered.";
                yield break;
            }
            n %= 4;
            for(int i = 0; i < n; i++)
            {
                for(int j = 0; j < p.Count(); j++)
                {
                    yield return null;
                    if (!unlock[p[j]])
                    {
                        yield return "sendtochat Button " + (p[j] + 1) + " locked before completing toggling.";
                        p.Remove(p[j]);
                        continue;
                    }
                    buttons[p[j]].OnInteract();
                    buttons[p[j]].OnInteractEnded();
                }
            }
        }
        else if (commands[0] == "submit")
        {
            if (commands.Length < 2 || commands.Length > 4)
            {
                yield return "sendtochaterror!f Invalid release command length.";
                yield break;
            }
            int b = "12345".IndexOf(commands[1].ToString());
            if (commands[1].Length > 1 || b < 0)
            {
                yield return "sendtochaterror!f Invalid button position. Valid positions are numbers in the range 1-5.";
                yield break;
            }
            if (b + 1 == holdton)
            {
                yield return "sendtochaterror!f Button " + (b + 1) + " must be released before it can be used for submission.";
                yield break;
            }
            if (unlock[b])
            {
                yield return "sendtochaterror!f Button " + (b + 1) + " must be locked before it can be used for submission.";
                yield break;
            }
            int t = 0;
            if (int.TryParse(commands.Last(), out t))
            {
                if (t > 9)
                {
                    yield return "sendtochaterror!f Invalid submission time. Valid times are single digits.";
                    yield break;
                }
		yield return "strike";
                yield return "solve";
                while ((int)info.GetTime() % 10 == t)
                    yield return null;
                while ((int)info.GetTime() % 10 != t)
                {
                    yield return "trycancel";
                    yield return null;
                }
                buttons[b].OnInteract();
                buttons[b].OnInteractEnded();
            }
            else
                yield return "sendtochaterror!f Invalid submission time. Valid times are whole numbers.";
        }
        else
            yield return "sendtochaterror!f \"" + commands[0] + "\" is an invalid command.";
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (unlock.Any(x => x))
            yield return null;
        yield return null;
        int[] seq = new int[5];
        seq[4] = subton;
        for(int i = 3; i >= 0; i--)
        {
            for(int j = 0; j < 5; j++)
            {
                if(pressigns[1][j] == pressigns[0][seq[i + 1]] && pressigns[0][j] != pressigns[0][seq[i + 1]])
                {
                    seq[i] = j;
                    break;
                }
            }
        }
        for (int i = 0; i < 5; i++)
        {
            string display = string.Join("", displets.Select((x, q) => lselect[q, x]).ToArray());
            int b = seq[i];
            int c = pressigns[0][b];
            if (displets[c] == 0)
                continue;
            int r = 0;
            switch (bcols[b])
            {
                case 0:
                    if (!info.GetSerialNumberLetters().Any(x => display.Contains(x.ToString())))
                    {
                        if (display.Skip(1).All(x => x > display[0]))
                            r = 1;
                        else if (b == 0 || b == 4)
                            r = 2;
                        else
                            r = 3;
                    }
                    break;
                case 1:
                    if (display.Distinct().Count() > 4)
                    {
                        if (display[2] == display.Take(4).Skip(1).OrderBy(x => x).ToArray()[1])
                            r = 1;
                        else if ((b == 0 || (bcols[b - 1] == 2 || bcols[b - 1] == 4)) && (b == 4 || (bcols[b + 1] == 2 || bcols[b + 1] == 4)))
                            r = 2;
                        else
                            r = 3;
                    }
                    break;
                case 2:
                    if (display.Any(x => "AEIOU".Contains(x.ToString())))
                    {
                        if (display.Take(4).All(x => x < display[4]))
                            r = 1;
                        else if (bcols.Where((x, q) => Mathf.Abs(b - q) > 1).Contains(3))
                            r = 2;
                        else
                            r = 3;
                    }
                    break;
                case 3:
                    int[] pairs = new int[25];
                    for (int q = 0; q < 25; q++)
                        pairs[q] = Mathf.Abs(display[q / 5] - display[q % 5]);
                    if (!pairs.Contains(1))
                    {
                        if (display[0] < 'N' ^ display[4] < 'N')
                            r = 1;
                        else if (!bcols.Contains(1))
                            r = 2;
                        else
                            r = 3;
                    }
                    break;
                default:
                    if (display.All(x => x != 'X'))
                    {
                        if (info.GetSerialNumberLetters().Any(x => Mathf.Abs(x - display[2]) < 2))
                            r = 1;
                        else if (bcols.Distinct().Count() % 2 == 1)
                            r = 2;
                        else
                            r = 3;
                    }
                    break;
            }
            int h = new int[,] { { 5, 9, 2, 3 }, { 7, 1, 6, 5 }, { 4, 3, 8, 0 }, { 2, 0, 7, 9 }, { 8, 6, 1, 4 } }[bcols[b], r];
            yield return null;
            while ((int)info.GetTime() % 10 == h)
                yield return null;
            while ((int)info.GetTime() % 10 != h)
                yield return null;
            buttons[b].OnInteract();
            while(!hold)
                yield return null;
            yield return null;
            r = 0;
            if (display.Contains("RYBW"[scrcol].ToString()))
                r = 1;
            else if (scrcol == bcols[b])
                r = 2;
            else if ((b > 0 && bcols[b - 1] == scrcol) || (b < 4 && bcols[b + 1] == scrcol))
                r = 3;
            else if (display.Any(x => bcols.Select(y => "RYBWK"[y]).Contains(x)))
                r = 4;
            else if (bcols[b] == 4)
                r = 5;
            while ((int)info.GetTime() % 10 == r)
                yield return null;
            while (!info.GetFormattedTime().Contains(r.ToString()))
                yield return null;
            buttons[b].OnInteractEnded();
            while(displets[c] != 0)
            {
                yield return null;
                buttons[b].OnInteract();
                buttons[b].OnInteractEnded();
            }
            yield return null;
            yield return true;
        }
        int k = subdigs[words.IndexOf(word), bcols[subton]];
        if (k < 0)
            k = info.GetSerialNumberNumbers().Last();
        while (unlock[subton])
            yield return null;
        yield return null;
        while ((int)info.GetTime() % 10 == k)
            yield return null;
        while ((int)info.GetTime() % 10 != k)
            yield return null;
        buttons[subton].OnInteract();
        buttons[subton].OnInteractEnded();
    }
}
