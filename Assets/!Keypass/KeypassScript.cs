using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeypassScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> keys;
    public Transform[] kpos;
    public Renderer[] lcds;
    public Material[] io;
    public TextMesh[] labels;

    private string[] fours = new string[50] { "NICE", "PLAY", "GRIT", "HUSK", "BODY", "WORK", "TOMB", "QOPH", "VERY", "COAX", "CHEW", "NEXT", "LOCK", "JUMP", "EAST", "FIVE", "SWAP", "AUTO", "GOJI", "TAXI", "HAZE", "COGS", "EQUI", "MILD", "MORE", "NEWS", "FAUX", "YOLK", "FLAW", "CRAB", "UNDO", "JAMB", "BUYS", "LAZY", "JACK", "WORD", "GOAL", "LOVE", "WHAT", "IDLE", "CITY", "ZONE", "EMIT", "ONYX", "TZAR", "QAFS", "DISH", "DYMS", "OVUM", "VERB"};
    private string[] fives = new string[130] { "ABHOR", "AFTER", "AVOID", "AXION", "AZURE", "BASIJ", "BHUNA", "BLACK", "BOXES", "BURQA", "CARGO", "CHAIN", "CRUMB", "COQUI", "CULEX", "DANCE", "DIRTY", "DOZEN", "DRUNK", "DYING", "EARTH", "EIGHT", "ENJOY", "EQUIP", "EVICT", "FIRST", "FJORD", "FOLKS", "FRUIT", "FUDGE", "GANEV", "GHOUL", "GNOME", "GRAVE", "GYOZA", "HAVOC", "HERTZ", "HINDU", "HOCUS", "HYDRA", "IAMBS", "IMAGE", "INDEX", "ITCHY", "IVORY",
    "JABOT", "JERKY", "JINGO", "JOULE", "JUMBO", "KANJI", "KENDO", "KLUTZ", "KNOWS", "KOMBU", "LATEX", "LEFTY", "LOTUS", "LUCID", "LYMPH", "MAJOR", "MEANT", "MIXED", "MOTIF", "MYTHS", "NAIVE", "NDUJA", "NEXUS", "NOWAY", "NURSE", "OGHAM", "OKAPI", "OMEGA", "OPERA", "OVALS", "PHONE", "PIOUS", "PLANK", "PSALM", "PROXY", "QAIDS", "QUASI", "QUBIT", "QUEYS", "QUOTH", "RADIX", "REHAB", "RHYME", "RIGHT", "ROUND", "SMOKE", "SNAFU", "SQUAB", "STRAW", "SWIFT",
    "TABLE", "THROW", "TOPAZ", "TRANQ", "TWICE", "UKASE", "ULTRA", "UMIAQ", "UNHIP", "UPSET", "VAGUS", "VENOM", "VIEWS", "VODKA", "VOZDH", "WALTZ", "WEIRD", "WHIRL", "WIDTH", "WRONG", "XENIA", "XERIC", "XRAYS", "XYLIC", "XYSTI", "YACHT", "YETIS", "YIELD", "YOGIS", "YUPON", "ZARFS", "ZEROS", "ZILCH", "ZLOTY", "ZYMES"};
    private string[][] keypad = new string[5][] { new string[6], new string[6], new string[6], new string[6], new string[6]};
    private string ans;
    private int[] sub = new int[5] { -1, -1, -1, -1, -1};

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        lcds[0].material = io[1];
        ans = fives.PickRandom();
        string[] p = new string[0];
        while (p.Length < 1)
        {
            for (int i = 0; i < 4; i++)
            {
                string f = fours.Where(x => x.Contains(ans[i].ToString())).PickRandom();
                for (int j = 0; j < 4; j++)
                    keypad[i][j] = f[j].ToString();
            }
            List<char> a = new List<char> { };
            for(int i = 1; i < 256; i++)
            {
                int[] it = new int[4] { i / 64, (i / 16) % 4, (i / 4) % 4, i % 4 };
                string c = "";
                for (int j = 0; j < 4; j++)
                    c += keypad[j][it[j]];
                for (int j = 0; j < 130; j++)
                    if (fives[j].Take(4).ToString() == c)                    
                        a.Add(fives[j][4]);
                if (a.Contains(ans[4]))
                    break;
            }
            p = fours.Where(x => x.Contains(ans[4].ToString()) && x.All(c => !a.Contains(c))).ToArray();
        }
        string last = p.PickRandom();
        for (int i = 0; i < 4; i++)
            keypad[4][i] = last[i].ToString();
        string declog = string.Format("[Keypass #{0}] The valid keys in each column spell out the words: {1}", moduleID, string.Join(", ", keypad.Select(x => string.Join("", x)).ToArray()));
        List<string> decselect = fives.Where(x => Enumerable.Range(0, 5).Select(k => !keypad[k].Contains(x[k].ToString())).All(b => b)).ToList();
        decselect = decselect.Shuffle();
        for(int i = 0; i < decselect.Count(); i++)
        {
            string d = decselect[i];
            for (int j = 0; j < 5; j++)
            {
                string[] fs = fours.Where(x => x.Contains(d[j].ToString())).ToArray();
                for (int a = 0; a < 2; a++)
                    for (int b = a + 1; b < 3; b++)
                        for (int c = b + 1; c < 4; c++)
                            if (fs.Any(x => x.Any(y => y.ToString() == keypad[j][a]) && x.Any(y => y.ToString() == keypad[j][b]) && x.Any(y => y.ToString() == keypad[j][c])))
                            {
                                decselect.Remove(d);
                                i--;
                                goto skip;
                            }
                        skip:;
            }
        }
        List<string[]> dpair = new List<string[]> { };
        for (int i = 0; i < decselect.Count() - 1; i++)
            for (int j = i + 1; j < decselect.Count(); j++)
            {
                if(Enumerable.Range(0, 5).All(x => decselect[i][x] != decselect[j][x]))
                     dpair.Add(new string[2] { decselect[i], decselect[j] });
            }
        string[] dp = new string[2];
        for(int i = 0; i < dpair.Count(); i++)
        {
            dp = dpair[i];
            for(int j = 0; j < 5; j++)
            {
                string[] fs = fours.Where(x => x.Contains(dp[0][j].ToString()) && x.Contains(dp[1][j].ToString())).ToArray();
                for (int a = 0; a < 3; a++)
                    for (int b = a + 1; b < 4; b++)
                        if (fs.Any(x => x.Any(y => y.ToString() == keypad[j][a]) && x.Any(y => y.ToString() == keypad[j][b])))
                            goto skip;
            }
            break;
        skip:;
            if (i >= dpair.Count() - 1)
                Awake();
        }
        for(int i = 0; i < 5; i++)
            for(int j = 0; j < 2; j++)
                 keypad[i][j + 4] = dp[j][i].ToString();
        for (int i = 0; i < 5; i++)
        {
            keypad[i] = keypad[i].Shuffle();
            for (int j = 0; j < 6; j++)
                labels[(6 * i) + j].text = keypad[i][j];
        }
        Debug.LogFormat("[Keypass #{0}] The keys have the configuration:\n[Keypass #{0}] {1}", moduleID, string.Join("\n[Keypass #" + moduleID + "] ", Enumerable.Range(0, 6).Select(x => string.Join("", Enumerable.Range(0, 5).Select(y => keypad[y][x]).ToArray())).ToArray()));
        Debug.Log(declog);
        Debug.LogFormat("[Keypass #{0}] Enter the word \"{1}\"", moduleID, ans);
        foreach (KMSelectable key in keys)
        {
            int k = keys.IndexOf(key);
            key.OnInteract += delegate ()
            {
                if (!moduleSolved && k != (k / 6) + sub[k / 6])
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, kpos[k]);
                    int c = k / 6;
                    if (sub[c] >= 0)
                    {
                        lcds[(c * 6) + sub[c]].material = io[1];
                        kpos[(c * 6) + sub[c]].localPosition += new Vector3(0, 0, 3);
                    }
                    sub[c] = k % 6;
                    lcds[k].material = io[0];
                    kpos[k].localPosition -= new Vector3(0, 0, 3);
                    if (sub.All(x => x >= 0))
                    {
                        string s = string.Join("", Enumerable.Range(0, 5).Select(x => keypad[x][sub[x]]).ToArray());
                        Debug.LogFormat("[Keypass #{0}] Submitted \"{1}\"", moduleID, s);
                        if (ans.Equals(s))
                        {
                            moduleSolved = true;
                            module.HandlePass();
                        }
                        else
                            module.HandleStrike();
                        StartCoroutine(Push(moduleSolved));
                    }
                }
                return false;
            };
        }
    }

    private IEnumerator Push(bool good)
    {
        yield return new WaitForSeconds(0.5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
        int[] s = Enumerable.Range(0, 5).Select(x => (6 * x) + sub[x]).ToArray();
        sub = new int[5] { -1, -1, -1, -1, -1 };
        if (good)
        {
            for (int i = 0; i < 30; i++)
                if (!s.Contains(i))
                {
                    lcds[i].material = io[0];
                    kpos[i].localPosition -= new Vector3(0, 0, 3);
                }
        }
        else
            for (int i = 0; i < 30; i++)
                if (s.Contains(i))
                {
                    lcds[i].material = io[1];
                    kpos[i].localPosition += new Vector3(0, 0, 3);
                }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <ABCDE> [Enters password]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        if (command.Length != 5)
            yield return "sendtochaterror!f Passwords must be five letters long.";
        else if (command.Any(x => x - 'A' < 0 || x - 'A' > 25))
            yield return "sendtochaterror!f Passwords consist of only the letters A-Z.";
        List<int> p = new List<int> { };
        for(int i = 0; i < 5; i++)
        {
            int k = -1;
            for (int j = 0; j < 6; j++)
                if (keypad[i][j] == command[i].ToString())
                {
                    k = j;
                    p.Add((6 * i) + k);
                    break;
                }
            if(k < 0)
            {
                yield return "unsubmittablepenalty";
                yield return string.Format("sendtochaterror!f There is no \"{0}\" in the {1} column!", command[i], new string[] { "first", "second", "third", "fourth", "fifth"}[i]);
                yield break;
            }
        }
        for(int i = 0; i < 5; i++)
        {
            yield return null;
            keys[p[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        for(int i = 0; i < 5; i++)
        {
            if(sub[i] >= 0 && keypad[i][sub[i]] != ans[i].ToString())
                for(int j = 0; j < 6; j++)
                    if(keypad[i][j] == ans[i].ToString())
                    {
                        yield return true;
                        keys[(6 * i) + j].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
        }
        for (int i = 0; i < 5; i++)
        {
            if (sub[i] < 0)
                for (int j = 0; j < 6; j++)
                    if (keypad[i][j] == ans[i].ToString())
                    {
                        yield return true;
                        keys[(6 * i) + j].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
        }
    }
}
