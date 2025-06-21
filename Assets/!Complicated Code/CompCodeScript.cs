using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class CompCodeScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Renderer bulb;
    public Material[] bmats;
    public Light flash;
    public Transform pointer;
    public TextMesh disp;

    private readonly Color[] cols = new Color[3] { new Color(1, 0.55f, 0), new Color(1, 0, 0), new Color(0, 0, 1) };
    private readonly string[] morse = new string[26] { "#-###", "###-#-#-#", "###-#-###-#", "###-#-#", "#", "#-#-###-#", "###-###-#", "#-#-#-#", "#-#", "#-###-###-###", "###-#-###", "#-###-#-#", "###-###", "###-#", "###-###-###", "#-###-###-#", "###-###-#-###", "#-###-#", "#-#-#", "###", "#-#-###", "#-#-#-###", "#-###-###", "###-#-#-###", "###-#-###-###", "###-###-#-#" };
    private readonly string[,] words = new string[2, 30] { { "RUSTIC", "APPLY", "LOVERS", "ZENITH", "STORMY", "QUINCE", "BISTRO", "INJECT", "KNIGHT", "WIZARD", "SWORD", "TUXEDO", "NOUGHT", "SIXTY", "FORGET", "FUZZY", "WHOSE", "PLAYER", "VECTOR", "ACQUIT", "CORKY", "INCHES", "DRAUGR", "AZTEC", "KIOSK", "TENNIS", "SHROUD", "YARD", "JOUST", "SQUID" }, { "STICKY", "CUBIC", "NAUSEA", "ZEALOT", "HONEST", "EQUALS", "STROBE", "FJORD", "GHOST", "DWARF", "WORDS", "KILOS", "PICOS", "HEFTY", "BYWAY", "CUBOID", "ALOHA", "PROXY", "STACKS", "HYPNO", "QUOKKA", "CRISS", "GENUINE", "GNAWS", "CAMERA", "HECTO", "REALISE", "JERK", "COMBO", "HYRAX" } };
    private readonly int[] venn = new int[128] { 7, 3, 4, 2, 5, 3, 1, 6, 0, 7, 5, 3, 4, 0, 6, 4, 1, 7, 6, 6, 4, 1, 7, 0, 5, 2, 6, 1, 3, 1, 7, 4, 6, 1, 5, 7, 1, 5, 7, 2, 0, 6, 3, 2, 4, 6, 0, 5, 3, 5, 0, 2, 0, 4, 5, 3, 2, 0, 1, 5, 6, 3, 2, 6, 2, 6, 0, 5, 6, 1, 1, 3, 4, 7, 3, 2, 3, 7, 5, 3, 5, 0, 2, 4, 3, 4, 2, 2, 1, 6, 7, 4, 0, 5, 3, 1, 7, 0, 6, 6, 2, 4, 5, 1, 7, 6, 2, 1, 1, 0, 6, 4, 4, 1, 5, 0, 3, 7, 1, 7, 6, 3, 6, 5, 5, 2, 3, 0};
    private bool[,] tx = new bool[6, 2];
    private int[] freq;
    private int seq;
    private bool f;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Start()
    {
        moduleID = ++moduleIDCounter;
        float scale = module.transform.lossyScale.x;
        flash.range *= scale;
        freq = Enumerable.Range(0, 1000).ToArray().Shuffle().Take(6).OrderBy(x => x).ToArray();
        int[] wsel = Enumerable.Range(0, 30).ToArray().Shuffle().ToArray();
        for(int i = 0; i < 6; i++)
        {
            int index = 0;
            string c = "";
            string log = string.Format("[Complicated Code #{0}] Transmission {1}:\n", moduleID, i + 1);
            bool[] fc = new bool[2];
            if(Random.Range(0, 2) == 0)
            {
                index = 1;
                c = "A";
            }
            string word = words[1 - index, wsel[i]];
            log += string.Format("[Complicated Code #{0}] The transmitted word is \"{1}\" at a frequency of 3.{2} MHz.\n", moduleID, word, (freq[i] < 10 ? "00" : (freq[i] < 100 ? "0" : "")) + freq[i].ToString());
            if (Random.Range(0, 2) == 0)
            {
                index += 2;
                c += "B";
                fc[0] = true;
            }
            if (Random.Range(0, 2) == 0)
            {
                index += 4;
                c += "C";
                fc[1] = true;
            }
            StartCoroutine(FlashSeq(string.Join("---", word.Select(x => morse[x - 'A']).ToArray()) + "-------", fc[0], fc[1], i));
            if (info.GetSerialNumberLetters().Any(x => word.Contains(x.ToString())))
            {
                index += 8;
                c += "D";
            }
            string d = info.GetSerialNumberNumbers().Last().ToString();
            if ((freq[i] < 100 && d == "0") || freq[i].ToString().Contains(d))
            {
                index += 16;
                c += "E";
            }
            if (freq[i] % 3 == 0)
            {
                index += 32;
                c += "F";
            }
            if (word.Length == 5)
            {
                index += 64;
                c += "G";
            }
            if (c == "")
                c = "O";
            switch (venn[index])
            {
                case 0:
                    tx[i, 1] = true;
                    break;
                case 1:
                    if (info.GetSerialNumberNumbers().First() % 2 == 1)
                        tx[i, 1] = true;
                    break;
                case 2:
                    if (info.GetBatteryCount() % 2 == 1)
                        tx[i, 1] = true;
                    break;
                case 3:
                    if (info.GetIndicators().Any(x => x.Contains(word.Last().ToString())))
                        tx[i, 1] = true;
                    break;
                case 4:
                    if (info.GetOnIndicators().Any())
                        tx[i, 1] = true;
                    break;
                case 5:
                    if (info.GetPorts().Contains("PS2"))
                        tx[i, 1] = true;
                    break;
                case 6:
                    if (freq[i] % 5 == 0)
                        tx[i, 1] = true;
                    break;
            }
            log += string.Format("[Complicated Code #{0}] {1} \u2192 {2}\n", moduleID, c, "TFBIOPZX"[venn[index]]);
            log += string.Format("[Complicated Code #{0}] {1}ransmit this frequency.", moduleID, tx[i, 1] ? "T" : "Do not t" );
            Debug.Log(log);
        }
        if(!Enumerable.Range(0, 6).Select(x => tx[x, 1]).Any())
        {
            int o = info.GetSerialNumberNumbers().Sum() % 6;
            tx[o, 1] = true;
            Debug.LogFormat("[Complicated Code #{0}] No transmissable frequencies.\n[Complicated Code #{0}] Override: Transmit 3.{1} MHz", moduleID, (freq[o] < 10 ? "00" : (freq[o] < 100 ? "0" : "")) + freq[o].ToString());
        }
        disp.text = "3." + (freq[0] < 10 ? "00" : (freq[0] < 100 ? "0" : "")) + freq[0].ToString();
        StartCoroutine("MoveTick");
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved)
                {
                    Off();
                    if (b == 2)
                    {
                        if (!tx[seq, 0])
                        {
                            tx[seq, 0] = true;
                            button.AddInteractionPunch(0.6f);
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                            Debug.LogFormat("[Complicated Code #{0}] 3.{1} MHz frequency transmitted.", moduleID, (freq[seq] < 10 ? "00" : (freq[seq] < 100 ? "0" : "")) + freq[seq].ToString());
                            if (tx[seq, 1])
                            {
                                tx[seq, 1] = false;
                                Debug.Log(string.Join(" ", Enumerable.Range(0, 6).Select(x => tx[x, 1] ? "1" : "0").ToArray()));
                                if (Enumerable.Range(0, 6).Select(x => tx[x, 1]).All(x => !x))
                                {
                                    moduleSolved = true;
                                    module.HandlePass();
                                }
                            }
                            else
                                module.HandleStrike();

                        }
                    }
                    else
                    {
                        button.AddInteractionPunch(0.2f);
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                        if (seq > 0 && b == 0)
                            seq--;
                        else if (seq < 5 && b == 1)
                            seq++;
                        disp.text = "3." + (freq[seq] < 10 ? "00" : (freq[seq] < 100 ? "0" : "")) + freq[seq].ToString();
                        StartCoroutine("MoveTick");
                    }
                }
                return false;
            };
        }
    }

    private IEnumerator MoveTick()
    {
        float t = 4.547f - (9.094f * freq[seq] / 1000);
        while (Mathf.Abs(pointer.localPosition.x - t) > 0.03f)
        {
            t = 4.547f - (9.094f * freq[seq] / 1000);
            float d = Time.deltaTime;
            if (pointer.localPosition.x < t)
                pointer.localPosition += new Vector3(d * 3, 0, 0);
            else
                pointer.localPosition -= new Vector3(d * 3, 0, 0);
            yield return null;
        }
        pointer.localPosition = new Vector3(t, 0.183f, 0);
    }

    private IEnumerator FlashSeq(string t, bool a, bool b, int i)
    {
        int l = t.Length;
        int k = Random.Range(0, l);
        while (!tx[i, 0])
        {
            if (seq == i)
            {
                if (t[k] == '#' && (k == 0 || t[k - 1] == '-'))
                {
                    if (f && a)
                    {
                        bulb.material = bmats[2];
                        flash.color = cols[2];
                    }
                    else if(!f && b)
                    {
                        bulb.material = bmats[3];
                        flash.color = cols[1];
                    }
                    else
                    {
                        bulb.material = bmats[1];
                        flash.color = cols[0];
                    }
                    flash.enabled = true;
                }
                else if (t[k - 1] == '#' && t[k] == '-')
                { 
                    f ^= true;
                    Off();
                }
            }
            yield return new WaitForSeconds(0.2f);
            k++;
            k %= l;
        }
    }

    private void Off()
    {
        bulb.material = bmats[0];
        flash.enabled = false;
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} signal <1-6> [Selects transmission] | !{0} tx [Presses transmit button]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        command = command.ToLowerInvariant();
        if(command == "tx")
        {
            if(tx[seq, 0])
            {
                yield return "sendtochaterror!f Signal already transmitted.";
                yield break;
            }
            buttons[2].OnInteract();
        }
        if(command.Substring(0, 6) == "signal")
        {
            int d = 0;
            if (int.TryParse(command.Last().ToString(), out d))
            {
                if(d > 0 && d < 7)
                {
                    d--;
                    while(seq < d)
                    {
                        buttons[1].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    while (seq > d)
                    {
                        buttons[0].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                    yield return "sendtohaterror!f Invalid signal.";
            }
            else
                yield return "sendtohaterror!f Invalid signal.";
        }
        else
            yield return "sendtohaterror!f Invalid command.";
    }
}