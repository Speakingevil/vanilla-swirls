using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class WireswordScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> scrolls;
    public GameObject[] panels;
    public List<KMSelectable> wires;
    public GameObject[] wobjs;
    public Renderer[] wrends;
    public Material[] wmats;
    public TextMesh[] labels;

    private KMAudio.KMAudioRef mechanism;
    private string alph = "ABCDEFGHIJKLMNOPRSTUVWXYZ";
    private readonly int[,] wenc = new int[4, 25] {{6, 17, 0, 22, 19, 20, 5, 4, 10, 15, 7, 24, 13, 18, 2, 21, 11, 1, 23, 8, 12, 16, 14, 9, 3}, { 20, 5, 23, 1, 13, 12, 19, 10, 4, 22, 0, 2, 24, 11, 8, 18, 17, 9, 14, 16, 3, 7, 15, 21, 6 }, { 12, 21, 14, 5, 7, 1, 13, 6, 16, 11, 19, 8, 0, 2, 10, 4, 24, 15, 17, 22, 9, 23, 3, 18, 20 }, { 3, 13, 17, 18, 21, 9, 22, 23, 8, 4, 11, 15, 6, 24, 16, 10, 5, 12, 1, 0, 20, 14, 7, 2, 19 } };
    private readonly string[] words = new string[125] { "AMITY", "MATZO", "YEARN", "BORAX", "DELTA", "BLIND", "UBYKH", "ELBOW", "TURBO", "SCRUB", "CLIMB", "ICHOR", "FOCUS", "DISCO", "MAGIC", "DUSTY", "IDEAS", "NUDGE", "KENDO", "GUILD", "EPOXY", "PERIL", "TWEAK", "INDEX", "SHADE", "FUNGI", "OFTEN", "AWFUL", "KNIFE", "THIEF", "GHAST", "OGHAM", "AEGIS", "SHOGI", "WRONG", "HOTEL", "CHEWY", "OTHER", "EIGHT", "ZILCH", "INGOT", "PIXEL", "SNICK", "DEVIL", "ZOMBI", "JOKER", "FJORD", "CAJUN", "KANJI", "BASIJ", "KLUTZ", "SKEIN", "TOKEN", "PORKY", "DRUNK", "LITRE", "SLUMP", "HELIX", "WORLD", "ETHYL", "MOVIE", "SMART", "HUMAN", "GNOME", "PRISM", "NEXUS", "ENJOY", "PONZI", "BURNT", "DOZEN", "OUIJA", "SOLID", "CHOUX", "DECOY", "BANJO", "PIVOT", "SPARK", "HYPER", "ALEPH", "UNZIP", "RHYME", "TRICK", "DIRGE", "KAURI", "LOWER", "SUAVE", "PSALM", "TESLA", "WHISK", "VIRUS", "TULIP", "ITCHY", "SATYR", "WIDTH", "CRYPT", "USHER", "JUICY", "STUMP", "FRAUD", "BAYOU", "VENOM", "IVORY", "HAVOC", "ABOVE", "SCHAV", "WIRES", "SWORD", "HOWDY", "PRAWN", "STRAW", "XENIA", "EXACT", "TOXIC", "PROXY", "REDUX", "YACHT", "CYBER", "MAYOR", "BERYL", "GRAVY", "ZEBRA", "AZTEC", "HAZEL", "GYOZA", "WALTZ" };
    private int[][][] warr = new int[5][][];
    private int[] panelnum = new int[5];
    private List<int> targcut = new List<int> { };
    private bool[] wcut = new bool[625];
    private bool scroll;

    private static int moduleIDCounter;
    private int moduleID;

    private void OnDestroy()
    {
        if (mechanism != null)
        {
            mechanism.StopSound();
            mechanism = null;
        }
    }

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        int[] targcom = new int[5];
        for (int i = 0; i < 5; i++)
            targcom[i] = Random.Range(0, 5);
        int[] colord = Enumerable.Range(0, 4).ToArray().Shuffle().ToArray();
        string targword = words.PickRandom();
        for (int i = 0; i < 5; i++)
        {
            int[][] x = new int[5][];
            for (int j = 0; j < 5; j++)
            {
                int[] y = new int[5];
                for (int k = 0; k < 5; k++)
                    y[k] = -1;
                x[j] = y;
            }
            warr[i] = x;
        }
        int[] targplace = new int[5];
        List<string>[][] letters = new List<string>[4][] { new List<string>[5], new List<string>[5], new List<string>[5], new List<string>[5] };
        List<string>[] av = new List<string>[5];
        for (int c = 0; c < 4; c++)
        {
            for (int i = 0; i < 5; i++)
            {
                av[i] = Enumerable.Range(0, 25).Select(x => alph[x].ToString()).ToList();
                letters[c][i] = new List<string> { c == 0 ? targword[i].ToString() : alph.PickRandom().ToString() };
            }
            if (c > 0)
                while (words.Contains(string.Join("", Enumerable.Range(0, 5).Select(x => letters[c][x][0]).ToArray())))
                    for (int i = 0; i < 5; i++)
                        letters[c][i] = new List<string> { alph.PickRandom().ToString() };
            for (int i = 0; i < 5; i++)
                av[i].Remove(letters[c][i][0]);
            while (letters[c].Any(x => x.Count() < 6))
            {
                List<int> a = new List<int> { };
                for (int i = 0; i < 5; i++)
                    for (int j = 0; j < 6 - letters[c][i].Count(); j++)
                        a.Add(i);
                int p = a.PickRandom();
                string q = av[p].PickRandom();
                letters[c][p].Add(q);
                av[p].Remove(q);
                switch (p)
                {
                    case 0:
                        for (int i = 0; i < letters[c][2].Count(); i++)
                            for (int j = 0; j < letters[c][3].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[1].Count(); x++)
                                    {
                                        string w = q + av[1][x] + letters[c][2][i] + letters[c][3][j] + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[1].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][1].Count(); i++)
                            for (int j = 0; j < letters[c][3].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[2].Count(); x++)
                                    {
                                        string w = q + letters[c][1][i] + av[2][x] + letters[c][3][j] + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[2].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][1].Count(); i++)
                            for (int j = 0; j < letters[c][2].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[3].Count(); x++)
                                    {
                                        string w = q + letters[c][1][i] + letters[c][2][j] + av[3][x] + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[3].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][1].Count(); i++)
                            for (int j = 0; j < letters[c][2].Count(); j++)
                                for (int k = 0; k < letters[c][3].Count(); k++)
                                    for (int x = 0; x < av[4].Count(); x++)
                                    {
                                        string w = q + letters[c][1][i] + letters[c][2][j] + letters[c][3][k] + av[4][x];
                                        if (words.Contains(w))
                                        {
                                            av[4].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        break;
                    case 1:
                        for (int i = 0; i < letters[c][2].Count(); i++)
                            for (int j = 0; j < letters[c][3].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[0].Count(); x++)
                                    {
                                        string w = av[0][x] + q + letters[c][2][i] + letters[c][3][j] + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[0].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][3].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[2].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + q + av[2][x] + letters[c][3][j] + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[2].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][2].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[3].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + q + letters[c][2][j] + av[3][x] + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[3].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][2].Count(); j++)
                                for (int k = 0; k < letters[c][3].Count(); k++)
                                    for (int x = 0; x < av[4].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + q + letters[c][2][j] + letters[c][3][k] + av[4][x];
                                        if (words.Contains(w))
                                        {
                                            av[4].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        break;
                    case 2:
                        for (int i = 0; i < letters[c][1].Count(); i++)
                            for (int j = 0; j < letters[c][3].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[0].Count(); x++)
                                    {
                                        string w = av[0][x] + letters[c][1][i] + q + letters[c][3][j] + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[0].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][3].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[1].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + av[1][x] + q + letters[c][3][j] + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[1].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][1].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[3].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + letters[c][1][j] + q + av[3][x] + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[3].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][1].Count(); j++)
                                for (int k = 0; k < letters[c][3].Count(); k++)
                                    for (int x = 0; x < av[4].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + letters[c][1][j] + q + letters[c][3][k] + av[4][x];
                                        if (words.Contains(w))
                                        {
                                            av[4].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        break;
                    case 3:
                        for (int i = 0; i < letters[c][1].Count(); i++)
                            for (int j = 0; j < letters[c][2].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[0].Count(); x++)
                                    {
                                        string w = av[0][x] + letters[c][1][i] + letters[c][2][j] + q + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[0].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][2].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[1].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + av[1][x] + letters[c][2][j] + q + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[1].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][1].Count(); j++)
                                for (int k = 0; k < letters[c][4].Count(); k++)
                                    for (int x = 0; x < av[2].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + letters[c][1][j] + av[2][x] + q + letters[c][4][k];
                                        if (words.Contains(w))
                                        {
                                            av[2].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][1].Count(); j++)
                                for (int k = 0; k < letters[c][2].Count(); k++)
                                    for (int x = 0; x < av[4].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + letters[c][1][j] + letters[c][2][k] + q + av[4][x];
                                        if (words.Contains(w))
                                        {
                                            av[4].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        break;
                    default:
                        for (int i = 0; i < letters[c][1].Count(); i++)
                            for (int j = 0; j < letters[c][2].Count(); j++)
                                for (int k = 0; k < letters[c][3].Count(); k++)
                                    for (int x = 0; x < av[0].Count(); x++)
                                    {
                                        string w = av[0][x] + letters[c][1][i] + letters[c][2][j] + letters[c][3][k] + q;
                                        if (words.Contains(w))
                                        {
                                            av[0].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][2].Count(); j++)
                                for (int k = 0; k < letters[c][3].Count(); k++)
                                    for (int x = 0; x < av[1].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + av[1][x] + letters[c][2][j] + letters[c][3][k] + q;
                                        if (words.Contains(w))
                                        {
                                            av[1].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][1].Count(); j++)
                                for (int k = 0; k < letters[c][3].Count(); k++)
                                    for (int x = 0; x < av[2].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + letters[c][1][j] + av[2][x] + letters[c][3][k] + q;
                                        if (words.Contains(w))
                                        {
                                            av[2].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        for (int i = 0; i < letters[c][0].Count(); i++)
                            for (int j = 0; j < letters[c][1].Count(); j++)
                                for (int k = 0; k < letters[c][2].Count(); k++)
                                    for (int x = 0; x < av[3].Count(); x++)
                                    {
                                        string w = letters[c][0][i] + letters[c][1][j] + letters[c][2][k] + av[3][x] + q;
                                        if (words.Contains(w))
                                        {
                                            av[3].RemoveAt(x);
                                            x--;
                                        }
                                    }
                        break;
                }
            }
            Debug.Log("[" + string.Join("], [", letters[c].Select(x => string.Join("", x.ToArray())).ToArray()) + "]");
        }
        for (int i = 0; i < 5; i++)
        {
            int w = wenc[colord[0], alph.IndexOf(letters[0][i][0])];
            warr[i][targcom[i]][w / 5] = (colord[0] * 5) + w % 5;
            targplace[i] = w / 5;
            letters[0][i].RemoveAt(0);
        }
        Debug.Log("RYBK"[colord[0]] + ": " + string.Join(", ", targcom.Select(x => (x + 1).ToString()).ToArray()));
        for (int c = 1; c < 4; c++)
        {
            for (int i = 0; i < 5; i++)
            {
                int w = wenc[colord[c], alph.IndexOf(letters[colord[c]][i][0])];
                int[] p = Enumerable.Range(0, 5).Where(x => warr[i][x].Max() < 0).ToArray();
                warr[i][p.PickRandom()][w / 5] = (colord[c] * 5) + (w % 5);
                letters[colord[c]][i].RemoveAt(0);
            }
        }
        for (int r = 0; r < 100; r++)
        {
            List<int> a = new List<int> { };
            for (int i = 0; i < 20; i++)
                for (int j = 0; j < letters[i / 5][i % 5].Count(); j++)
                    a.Add(i);
            int pc = a.PickRandom();
            a.Clear();
            int w = wenc[pc / 5, alph.IndexOf(letters[pc / 5][pc % 5][0])];
            for (int i = 0; i < 5; i++)
                if (warr[pc % 5][i][w / 5] < 0 && Enumerable.Range(0, 5).All(x => warr[pc % 5][i][x] % 5 != w % 5))
                    a.Add(i);
            if (a.Any())
                warr[pc % 5][a.PickRandom()][w / 5] = ((pc / 5) * 5) + (w % 5);
            letters[pc / 5][pc % 5].RemoveAt(0);
        }
        for (int i = 0; i < 5; i++)
        {
            string[] panelog = new string[5];
            for (int j = 0; j < 5; j++)
            {
                List<string> portlog = new List<string> { };
                for (int k = 0; k < 5; k++)
                {
                    int w = warr[i][j][k];
                    if (w > 0)
                    {
                        string q = "RYBK"[w / 5].ToString();
                        q += ":";
                        q += (k + 1).ToString();
                        q += "-";
                        q += ((w % 5) + 1).ToString();
                        q += "=";
                        q += alph[Enumerable.Range(0, 25).First(x => wenc[w / 5, x] == (k * 5) + (w % 5))];
                        portlog.Add(q);
                    }
                }
                panelog[j] = string.Join(", ", portlog.ToArray());
            }
            Debug.LogFormat("[Wiresword #{0}] The {1} position has the panels: [{2}]", moduleID, new string[] { "first", "second", "third", "fourth", "fifth" }[i], string.Join("], [", panelog));
        }
        Debug.LogFormat("[Wiresword #{0}] The password is \"{1}\"- encoded in {2} wires on panels: {3}", moduleID, targword, new string[] { "red", "yellow", "blue", "black" }[colord[0]], string.Join(", ", targcom.Select(x => (x + 1).ToString()).ToArray()));
        for (int i = 0; i < 5; i++)
        {
            int cut = -1;
            int[] panel = warr[i][targcom[i]];
            char[] pletters = new char[5];
            for (int j = 0; j < 5; j++)
                pletters[j] = panel[j] < 0 ? '#' : alph[Enumerable.Range(0, 25).First(x => wenc[panel[j] / 5, x] == (j * 5) + (panel[j] % 5))];
            switch (panel.Count(x => x >= 0))
            {
                default:
                    cut = Placement(panel, 1);
                    break;
                case 2:
                    if (pletters.Where(x => x != '#').All(x => info.GetSerialNumberLetters().Any(y => y == x)))
                        cut = Enumerable.Range(0, panel.Length).First(x => panel[x] >= 0 && (panel[x] / 5 != colord[0] || pletters[x] != targword[i]));
                    else if (panel.Where(x => x >= 0).Select(x => x / 5).Distinct().Count() < 2)
                        cut = targplace[i];
                    else
                        cut = Placement(panel, 1 - (targcom[i] % 2));
                    break;
                case 3:
                    if (pletters.Where(x => x != '#').Distinct().Count() > 2 && panel.Where(x => x >= 0).Select(x => x / 5).Distinct().Count() > 2)
                        cut = Placement(panel, 2);
                    else if (panel.Count(x => x > 15) > 1)
                        cut = Placement(panel, 1);
                    else if (pletters.Where(x => x != '#').All(x => !"AEIOU".Contains(x.ToString())))
                        cut = Enumerable.Range(0, panel.Length).Last(x => panel[x] >= 0 && (panel[x] / 5 != colord[0] || pletters[x] != targword[i]));
                    else
                        cut = targplace[i];
                    break;
                case 4:
                    if (panel.Count(x => x / 5 == colord[0]) == 1)
                        cut = Placement(panel, 3);
                    else if (panel.Select(x => x / 5).Distinct().Count() < 3)
                        cut = Placement(panel, 4);
                    else if (panel.Count(x => x / 5 == 1) == 1)
                        cut = Enumerable.Range(0, panel.Length).First(x => panel[x] / 5 == 1);
                    else
                        cut = Enumerable.Range(0, panel.Length).First(x => panel[x] >= 0 && (panel[x] / 5 != colord[0] || pletters[x] != targword[i]));
                    break;
                case 5:
                    if (targplace[i] == targcom[i])
                        cut = 4;
                    else if (pletters.Where(x => x != targword[i]).GroupBy(x => x).Any(x => x.Count() > 1))
                        cut = 2;
                    else if (panel.All(x => x / 5 > 0))
                        cut = 1;
                    else
                        cut = 3;
                    break;
            }
            targcut.Add((((i * 5) + cut) * 5) + (panel[cut] % 5));
            Debug.LogFormat("[Wiresword #{0}] Position {1} - Panel {2} = [{3}]. Cut the wire in the {4} port.", moduleID, i + 1, targcom[i] + 1, string.Join(", ", panel.Select((x, q) => x < 0 ? "-" : new string[] { "Red", "Yellow", "Blue", "Black" }[x / 5] + " " + pletters[q]).ToArray()), new string[] { "first", "second", "third", "fourth", "fifth" }[cut]);

        }
        module.OnActivate += delegate ()
        {
            mechanism = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
            for (int i = 0; i < 5; i++)
                StartCoroutine(Hatchmove(i, false));
        };
        foreach (KMSelectable button in scrolls)
        {
            int b = scrolls.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!scroll)
                {
                    button.AddInteractionPunch(0.3f);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    panelnum[b % 5] += b < 5 ? 4 : 1;
                    panelnum[b % 5] %= 5;
                    StartCoroutine(Hatchmove(b % 5, true));
                }
                return false;
            };
        }
        foreach (KMSelectable wire in wires)
        {
            int w = wires.IndexOf(wire);
            wire.OnInteract += delegate ()
            {
                int c = ((w / 25) * 125) + (panelnum[w / 25] * 25) + (w % 25);
                if (!wcut[c] && !scroll)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, wire.transform);
                    wire.AddInteractionPunch(0.2f);
                    wcut[c] = true;
                    wrends[w + 125].material = wrends[w].material;
                    wobjs[w].SetActive(false);
                    wobjs[w + 125].SetActive(true);
                    Debug.LogFormat("[Wiresword #{0}] Wire cut at Position {1} - Panel {2} - Port {3}", moduleID, (w / 25) + 1, panelnum[w / 25] + 1, ((w % 25) / 5) + 1);
                    if (targcut.Contains(w))
                    {
                        targcut.Remove(w);
                        if (!targcut.Any())
                            module.HandlePass();
                    }
                    else
                        module.HandleStrike();
                }
                return false;
            };
        }
    }

    private void Panelset(int x, int y)
    {
        int[] panel = warr[x][y];
        labels[x].text = (y + 1).ToString();
        for(int i = 0; i < 5; i++)
            for(int j = 0; j < 5; j++)
                if(panel[i] % 5 == j)
                {
                    if (wcut[(((((x * 5) + y) * 5) + i) * 5) + j])
                    {
                        wobjs[(((x * 5) + i) * 5) + j + 125].SetActive(true);
                        wrends[(((x * 5) + i) * 5) + j + 125].material = wmats[panel[i] / 5];
                    }
                    else
                    {
                        wobjs[(((x * 5) + i) * 5) + j].SetActive(true);
                        wrends[(((x * 5) + i) * 5) + j].material = wmats[panel[i] / 5];
                    }
                }
                else
                {
                    wobjs[(((x * 5) + i) * 5) + j].SetActive(false);
                    wobjs[(((x * 5) + i) * 5) + j + 125].SetActive(false);
                }
    }

    private int Placement(int[] p, int x)
    {
        for (int i = 0; i < p.Length; i++)
        {
            if (p[i] >= 0)
                x--;
            if (x < 1)
                return i;
        }
        return -1;
    }

    private IEnumerator Hatchmove(int p, bool down)
    {
        scroll = true;
        Transform[] hatch = new Transform[4];
        for (int i = 0; i < 4; i++)
            hatch[i] = panels[(p * 4) + i].transform;
        float x = hatch[0].localPosition.x;
        float e = 1;
        if (down)
        {
            mechanism = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
            while (e > 0)
            {
                float d = Time.deltaTime * 2;
                e -= d;
                hatch[0].localPosition = new Vector3(x, (e / 50) - 0.005f, 0);
                if (e > 0.5f)
                    hatch[1].localPosition = new Vector3(x, -(e / 100) - 0.0254f, -0.0199f);
                else
                {
                    hatch[2].localEulerAngles = new Vector3(0, -e * 180, 0);
                    hatch[3].localEulerAngles = new Vector3(0, e * 180, 0);
                }
                yield return null;
            }
        }
        yield return null;
        Panelset(p, panelnum[p]);
        e = 0;
        while(e < 1)
        {
            float d = Time.deltaTime * 2;
            e += d;
            hatch[0].localPosition = new Vector3(x, (e / 50) - 0.005f, 0);
            if (e > 0.5f)
                hatch[1].localPosition = new Vector3(x, - (e / 100) - 0.0254f, -0.0199f);
            else
            {
                hatch[2].localEulerAngles = new Vector3(0, - e * 180, 0);
                hatch[3].localEulerAngles = new Vector3(0, e * 180, 0);
            }
            yield return null;
        }
        hatch[0].localPosition = new Vector3(x, 0.01f, 0);
        hatch[1].localPosition = new Vector3(x, -0.0304f, -0.0199f);
        hatch[2].localEulerAngles = new Vector3(0, -90, 0);
        hatch[3].localEulerAngles = new Vector3(0, 90, 0);
        if(mechanism != null)
        {
            mechanism.StopSound();
            mechanism = null;
        }
        scroll = false;
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} scroll 1-5 +/- [Scrolls given position forwards or backwards.] | !{0} cut 1-5 # [Cuts #th wire on the current panel at the given position.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if(commands.Length != 3)
        {
            yield return "sendtochaterror Invalid command length.";
            yield break;
        }
        int d = 0;
        if (int.TryParse(commands[1], out d))
        {
            if(d < 1 || d > 5)
            {
                yield return "sendtochaterror Position must be a number from 1 to 5.";
                yield break;
            }
            d--;
            switch (commands[0])
            {
                case "scroll":
                    switch (commands[2])
                    {
                        case "+":
                            d += 5;
                            break;
                        case "-":
                            break;
                        default:
                            yield return "sendtochaterror Use + or - to indicate scroll direction.";
                            yield break;
                    }
                    yield return null;
                    scrolls[d].OnInteract();
                    yield break;
                case "cut":
                    int w = 0;
                    int[] panel = warr[d][panelnum[d]];
                    if(int.TryParse(commands[2], out w))
                    {
                        w = Placement(panel, w);
                        if(w >= 0)
                        {
                            yield return null;
                            d *= 5;
                            d += w;
                            d *= 5;
                            d += panel[w] % 5;
                            wires[d].OnInteract();
                        }
                        else
                            yield return "sendtochaterror There are only " + panel.Count(x => x >= 0).ToString() + " wires on the panel in position " + (d + 1).ToString() + ".";
                    }
                    else
                        yield return "sendtochaterror Wire position is NaN.";
                    yield break;
                default:
                    yield return "sendtochaterror \"" + commands[0] + "\" is an invalid command.";
                    yield break;
            }
        }
        else
            yield return "sendtochaterror \"" + commands[1] + "\" is NaN.";
        yield break;
    }
}
