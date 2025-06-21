using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VentingFirstScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMNeedyModule module;
    public KMBombInfo info;
    public Transform rot;
    public List<KMSelectable> buttons;
    public TextMesh[] disps;

    private readonly string[] plog = new string[8] { "top-left", "top-middle-left", "bottom-middle-left", "bottom-left", "top-right", "top-middle-right", "bottom-middle-right", "bottom-right"};
    private readonly string[] prompts = new string[32] { "RUN", "PLAY", "EXECUTE", "GO", "START", "ALLOW", "REPEAT", "ACTIVATE", "INVOKE", "WRITE", "PERMIT", "ACCEPT", "AUTHORISE", "APPROVE", "INITIATE", "VENT GAS", "HALT", "SKIP", "CLOSE", "STOP", "END", "DENY", "ABORT", "CEASE", "WAIVE", "CLEAR", "BLOCK", "REJECT", "PROHIBIT", "FORBID", "TERMINATE", "DETONATE" };
    private readonly string[] tables = new string[32] { "XXXYXNXX", "YXNXXXXX", "XNXXXYXX", "XXNXYXXX", "NXXXXYXX", "XXXXXXYN", "NXXXYXXX", "XNXXXXYX", "XXXNXYXX", "XXXYXXXN", "XXXYXNXX", "XYNXXXXX", "XXXNXXYX", "YXXXXNXX", "XXNYXXXX", "XXXXXNYX", "NXXXXXYX", "XYXXXNXX", "XXYNXXXX", "XXNXXXXY", "XNXXYXXX", "NXXXXXXY", "XXNXXXXY", "XXXNXXXY", "NXYXXXXX", "XXYXXXNX", "XYXXNXXX", "XYXXXXNX", "XXXYNXXX", "XXXXNXXY", "XYXNXXXX", "YXXXNXXX" };
    private readonly string[] labels = new string[18] { "YES", "YEA", "YUP", "YEAH", "OK", "OKAY", "AYE", "YEP", "AHUH", "NO", "NAY", "NOPE", "NAH", "NOT", "NIX", "NAW", "NA", "UHUH"} ;
    private readonly List<int>[] seq = new List<int>[18] { new List<int> { 5, 9, 7, 14, 8, 11, 2, 12, 4, 15, 0 }, new List<int> { 4, 14, 6, 16, 0, 17, 2, 12, 3, 13, 1 }, new List<int> { 0, 11, 6, 14, 5, 10, 8, 13, 4, 12, 2 }, new List<int> { 6, 13, 7, 9, 2, 15, 8, 14, 5, 11, 3 }, new List<int> { 5, 10, 2, 15, 7, 9, 3, 16, 6, 17, 4 }, new List<int> { 0, 12, 3, 14, 4, 10, 6, 13, 7, 17, 5 }, new List<int> { 7, 10, 1, 17, 4, 13, 2, 16, 5, 14, 6 }, new List<int> { 1, 13, 5, 17, 3, 12, 2, 11, 0, 16, 7 }, new List<int> { 2, 10, 0, 15, 7, 16, 4, 11, 3, 17, 8 }, new List<int> { 17, 6, 12, 0, 11, 7, 15, 5, 10, 1, 9 }, new List<int> { 12, 2, 17, 1, 16, 6, 15, 5, 11, 0, 10 }, new List<int> { 15, 8, 14, 4, 13, 5, 17, 0, 16, 3, 11 }, new List<int> { 16, 1, 11, 4, 17, 6, 14, 0, 10, 2, 12 }, new List<int> { 17, 6, 15, 2, 11, 4, 9, 7, 10, 5, 13 }, new List<int> { 13, 3, 17, 5, 10, 4, 16, 0, 11, 2, 14 }, new List<int> { 13, 7, 16, 0, 17, 1, 11, 8, 10, 2, 15 }, new List<int> { 14, 5, 10, 2, 11, 6, 13, 4, 9, 1, 16 }, new List<int> { 16, 4, 10, 7, 15, 6, 9, 3, 11, 1, 17 } };
    private int[,] callback = new int[4, 2] { { -1, -1}, { -1, -1}, { -1, -1 }, { -1, -1} };
    private bool active;
    private int starttime;
    private bool initiated;


    private static int moduleIDCounter;
    private int moduleID;

    private void Awake()
    {
        List<int> order = Enumerable.Range(1, 8).ToArray().Shuffle().ToList();
        order.Insert(0, 0);
        string[] t = new string[18];
        for (int i = 0; i < 18; i += 2)
        {
            int j = order[i / 2];
            string d = "<tr><td class = \"k\"><span class = \"g\">\"";
            d += labels[j];
            d += "\":</td><td>";
            for(int k = 0; k < 11; k++)
            {
                d += "<span class = \"";
                d += k % 2 == 0 ? "g" : "r";
                d += "\">";
                d += labels[seq[j][k]];
                d += "</span>, ";
            }
            d += "</td>\n";
            t[i] = d;
        }
        for (int i = 1; i < 18; i += 2)
        {
            int j = order[i / 2] + 9;
            string d = "<tr><td class = \"k\"><span class = \"r\">\"";
            d += labels[j];
            d += "\":</td><td>";
            for (int k = 0; k < 11; k++)
            {
                d += "<span class = \"";
                d += k % 2 == 1 ? "g" : "r";
                d += "\">";
                d += labels[seq[j][k]];
                d += "</span>, ";
            }
            d += "</td>\n";
            t[i] = d;
        }
        Debug.Log(string.Join("", t));
        moduleID = ++moduleIDCounter;
        StartCoroutine("Rot");
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (active)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    button.AddInteractionPunch(0.7f);
                    foreach (TextMesh disp in disps)
                        disp.text = "";
                    active = false;
                    module.HandlePass();
                    StartCoroutine(Press(b == callback[3, 0]));
                }              
                return false;
            };
        }
        module.OnNeedyActivation += delegate () { StartCoroutine(Startup()); };
        module.OnTimerExpired += delegate ()
        {
            foreach (TextMesh disp in disps)
                disp.text = "";
            active = false;
            callback = new int[4, 2] { { -1, -1 }, { -1, -1 }, { -1, -1 }, { -1, -1 } };
            module.HandleStrike();
        };
    }

    private void Start()
    {
        starttime = (int)info.GetTime();
        StartCoroutine("Initiate");
    }

    private IEnumerator Rot()
    {
        yield return null;
        rot.localEulerAngles = new Vector3(0, 90, 0);
    }

    private IEnumerator Initiate()
    {
        yield return new WaitForSeconds(600);
        initiated = true;
    }

    private IEnumerator Press(bool correct)
    {
        for(int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(0.6f);
            if(i % 2 == 0)
                Audio.PlaySoundAtTransform("Bip", transform);
            disps[0].text += ".";
        }
        if (correct)
        {
            disps[0].text = "GAS VENTED";
            Audio.PlaySoundAtTransform("Vent" + Random.Range(0, 100) / 99, transform);
        }
        else
        {
            disps[0].text = "STRUCK";
            callback = new int[4, 2] { { -1, -1 }, { -1, -1 }, { -1, -1 }, { -1, -1 } };
            module.HandleStrike();
        }
        yield return new WaitForSeconds(1);
        disps[0].text = "";
    }

    private void Roll(int x, int y)
    {
        callback[x, 1] = callback[x, 0];
        callback[x, 0] = y;
    }

    private bool Read(int d)
    {
        switch (d)
        {
            case 0: return info.GetTime() < 600;                                                    //RUN       | HALT
            case 1: return info.GetModuleNames().Count() / info.GetSolvedModuleNames().Count() < 2; //PLAY      | SKIP
            case 2: return info.GetStrikes() > 1;                                                   //EXECUTE   | CLOSE
            case 3: return callback[0, 0] == 0;                                                     //GO        | STOP
            case 4: return info.GetSolvedModuleNames().Count() > 4;                                 //START     | END
            case 5: return callback[2, 0] == 0;                                                     //ALLOW     | DENY
            case 6: return callback[3, 0] == 0;                                                     //REPEAT    | ABORT
            case 7: return info.GetStrikes() < 1;                                                   //ACTIVATE  | CEASE
            case 8: return callback[3, 1] >= 0 && callback[3, 0] == callback[3, 1];                 //INVOKE    | WAIVE
            case 9: return callback[1, 0] == 0;                                                     //WRITE     | CLEAR
            case 10: return info.GetSolvedModuleNames().Count() % 2 == 0;                           //PERMIT    | BLOCK
            case 11: return callback[2, 1] >= 0 && callback[2, 0] == callback[2, 1];                //ACCEPT    | REJECT
            case 12: return starttime / info.GetTime() > 1;                                         //AUTHORISE | PROHIBIT
            case 13: return callback[0, 1] >= 0 && callback[0, 0] == callback[0, 1];                //APPROVE   | FORBID
            case 14: return !initiated;                                                             //INITIATE  | TERMINATE
            default: return true;                                                                   //VENT GAS  | DETONATE
        }
    }

    private IEnumerator Startup()
    {
        int[] scr = new int[9];
        scr[0] = Random.Range(0, 32);
        int[] order = Enumerable.Range(1, 8).ToArray();
        for(int i = 0; i < 2; i++)
        {
            int[] q = Enumerable.Range(i * 9, 9).ToArray().Shuffle().ToArray();
            for (int j = 0; j < 4; j++)
                scr[order[(i * 4) + j]] = q[j];
        }
        bool eye = (callback[1, 1] >= 0 && callback[1, 0] == callback[1, 1]) ? callback[1, 0] == 1 : (scr[0] > 15 ? !Read(scr[0] - 16) : Read(scr[0]));
        Roll(0, scr[0] / 16);
        int read = tables[scr[0]].IndexOf(eye ? "Y" : "N");
        Roll(1, eye ? 0 : 1);
        int p = seq[scr[read + 1]].First(x => scr.Skip(1).Contains(x));
        Roll(2, scr[read + 1] / 9);
        Roll(3, p / 9);
        for (int i = 5; i < 10; i++)
        {
            yield return new WaitForSeconds(0.15f);
            if (i < 9)
                disps[i].text = labels[scr[i]];
            if (i > 5)
                disps[i - 5].text = labels[scr[i - 5]];
        }
        yield return new WaitForSeconds(0.25f);
        disps[0].text = prompts[scr[0]] + "?";
        active = true;
        Debug.LogFormat("[Venting First? #{0}] The prompt is \"{1}\".", moduleID, disps[0].text);
        Debug.LogFormat("[Venting First? #{0}] The {1} eye is in the {2} position.", moduleID, eye ? "open" : "closed", plog[read]);
        Debug.LogFormat("[Venting First? #{0}] The label in the {1} position is \"{2}\".", moduleID, plog[read], labels[scr[read + 1]]);
        Debug.LogFormat("[Venting First? #{0}] The first matching label of the {1} sequence is \"{2}\".", moduleID, labels[scr[read + 1]], labels[p]);
        Debug.Log(string.Join(", ", seq[scr[read + 1]].Select(x => labels[x]).ToArray()));
        Debug.LogFormat("[Venting First? #{0}] Press {1}.", moduleID, p > 8 ? "N" : "Y");
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} press y/n";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if(commands[0] == "press" && commands.Length != 2 && commands[1].Length == 1 && "yn".Contains(commands[1]))
        {
            yield return null;
            buttons[commands[1] == "y" ? 0 : 1].OnInteract();
            yield break;
        }
        yield return "sendtochaterror Command \"" + command + "\" is invalid.";
    }
}
