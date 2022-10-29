using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MorseCompScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> wires;
    public GameObject[] wobjs;
    public Renderer[] wrends;
    public Renderer[] leds;
    public Light[] lights;
    public Material[] wmats;
    public Material[] io;
    public Renderer[] stars;

    private readonly int[] txkey = new int[12] { 5, 2, 0, 4, 3, 1, 3, 1, 5, 0, 4, 2};
    private readonly string[] morse = new string[26] { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };
    private readonly string[] words = new string[62] { "EXPAND", "FORMAT", "REASON", "SQUASH", "SKETCH", "HUNTER", "WINDOW", "MOVING", "RESULT", "SOURCE", "EXOTIC", "FIGURE", "JACKET", "FROZEN", "PEANUT", "SALMON", "QUICHE", "VISUAL", "DANGER", "CANVAS", "WINTER", "ASYLUM", "VALLEY", "COLUMN", "JUNIOR", "ANSWER", "BUCKET", "BISTRO", "FLOWER", "NATURE", "RHYTHM", "STROBE", "OBJECT", "EXCITE", "CARROT", "GROUND", "PILLOW", "STUDIO", "SWITCH", "ADVICE", "VOYAGE", "CREDIT", "EARWAX", "MEDIUM", "BISHOP", "ADJUST", "SPIDER", "TONGUE", "PLANET", "CAMERA", "CLIQUE", "AZTECS", "ARTIST", "GRAVEL", "SMOOTH", "SUBWAY", "CHOOSE", "MUSCLE", "PERISH", "CHORUS", "POWDER", "ACTION"};
    private int[] transformations = new int[6];
    private string[] txs = new string[6];
    private bool[] ans = new bool[6];
    private bool[] cut = new bool[6];

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        int pick = Random.Range(1, 63);
        char[] tword = words[pick - 1].ToCharArray().Shuffle();
        for (int i = 0; i < 6; i++)
        {
            int r = Random.Range(0, 12);
            transformations[i] = r;
            if (r < 6)
                stars[i].enabled = false;
            wrends[i].material = wmats[r % 6];
            wrends[i + 6].material = wmats[r % 6];
            wobjs[i + 6].SetActive(false);
            string m = morse[tword[i] - 'A'];
            switch (txkey[r])
            {
                case 0:
                    m = morse[(tword[i] - 'A' + 25) % 26];
                    txs[i] = TGen(m);
                    break;
                case 1:
                    m = morse[(tword[i] - 'A' + 1) % 26];
                    txs[i] = TGen(m);
                    break;
                case 2:
                    m = morse[25 - (tword[i] - 'A')];
                    txs[i] = TGen(m);
                    break;
                case 3:
                    txs[i] = new string(TGen(m).Reverse().ToArray());
                    break;
                case 4:
                    m = m.Replace('.', '#').Replace('-', '.').Replace('#', '-');
                    txs[i] = TGen(m);
                    break;
                default:
                    txs[i] = TGen(m);
                    txs[i] = txs[i].Replace('#', '.').Replace('-', '#').Replace('.', '-');
                    break;
            }
            Debug.LogFormat("[Morse Complication #{0}] The {1} LED transmits: {2}", moduleID, new string[] { "first", "second", "third", "fourth", "fifth", "sixth"}[i], txs[i]);
        }
        float scale = module.transform.lossyScale.x;
        int p = 0;
        p = pick;
        for (int i = 0; i < 6; i++)
        {
            lights[i].range *= scale;
            if (p % 2 == 1)
                ans[i] = true;
            p /= 2;
            Debug.LogFormat("[Morse Complication #{0}] The {1} wire is {2}{3}. Apply the transformation: {4}.", moduleID, new string[] { "first", "second", "third", "fourth", "fifth", "sixth" }[i], new string[] { "white", "red", "blue", "white and red", "white and blue", "red and blue" }[transformations[i] % 6], transformations[i] > 6 ? " with star" : ", no star", new string[] { "Ceasar shift +1", "Ceasar shift -1", "Atbash", "Reverse", "Swap dots and dashes", "Invert on/off states" }[txkey[transformations[i]]]);
            StartCoroutine(TX(i, txs[i]));
        }
        Debug.LogFormat("[Morse Complication #{0}] The transformed transmissions form Morse code for the letters: {1}", moduleID, string.Join("-", tword.Select(x => x.ToString()).ToArray()));
        Debug.LogFormat("[Morse Complication #{0}] The unscrambled word is {1}. Cut the wires: {2}", moduleID, words[pick - 1], string.Join(", ", ans.Select((x, i) => new { ind = i, t = x}).Where(x => x.t).Select(i => (i.ind + 1).ToString()).ToArray()));
        foreach(KMSelectable wire in wires)
        {
            int w = wires.IndexOf(wire);
            wire.OnInteract = delegate ()
            {
                wire.AddInteractionPunch(0.5f);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, wire.transform);
                wobjs[w].SetActive(false);
                wobjs[w + 6].SetActive(true);
                cut[w] = true;
                Debug.LogFormat("[Morse Complication #{0}] Wire {1} cut.", moduleID, w + 1);
                if (ans[w])
                {
                    ans[w] = false;
                    if (ans.All(x => !x))
                    {
                        moduleSolved = true;
                        module.HandlePass();
                    }
                }
                else
                    module.HandleStrike();
                return false;
            };
        }
    }

    private string TGen(string m)
    {
        string t = "";
        for(int i = 0; i < m.Length; i++)
        {
            if (m[i] == '-')
                t += "###";
            else
                t += "#";
            t += "-";
        }
        t += "--";
        return t;
    }

    private IEnumerator TX(int i, string t)
    {
        int f = 0;
        while (!moduleSolved)
        {
            if(t[f] == '#')
            {
                lights[i].enabled = true;
                leds[i].material = io[0];
            }
            else
            {
                lights[i].enabled = false;
                leds[i].material = io[1];
            }
            f++;
            f %= t.Length;
            yield return new WaitForSeconds(0.2f);
        }
        lights[i].enabled = false;
        leds[i].material = io[1];
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} cut <1-6> [Cuts wires in the specified positions. Chain with spaces.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ').Distinct().ToArray();
        if(commands[0] != "cut")
        {
            yield return "sendtochaterror!f Use the \"cut\" command to cut wires.";
            yield break;
        }
        List<int> p = new List<int> { };
        for(int i = 1; i < commands.Length; i++)
        {
            if(commands[i].Length != 1)
            {
                yield return "sendtochaterror!f \"" + commands[i] + "\" is an invalid wire position.";
                yield break;
            }
            int w = "123456".IndexOf(commands[i]);
            if(w < 0)
            {
                yield return "sendtochaterror!f \"" + commands[i] + "\" is an invalid wire position.";
                yield break;
            }
            if (cut[w])
            {
                yield return "sendtochaterror!f Wire " + commands[i] + " has already been cut.";
                yield break;
            }
            p.Add(w);
        }
        yield return "strike";
        yield return "solve";
        for (int i = 0; i < p.Count(); i++)
        {
            yield return null;
            wires[p[i]].OnInteract();
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        for(int i = 0; i < 6; i++)
        {
            if(ans[i])
            {
                yield return null;
                wires[i].OnInteract();
            }
        }
    }
}
