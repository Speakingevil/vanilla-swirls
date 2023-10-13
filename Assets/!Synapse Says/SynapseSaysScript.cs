using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SynapseSaysScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> buttons;
    public Transform[] bpos;
    public Renderer[] brends;
    public Material[] bmats;
    public Light[] lights;
    public Renderer[] stageleds;
    public Material[] io;
    public TextMesh display;

    private readonly string[] collog = new string[4] { "Red", "Yellow", "Green", "Blue" };
    private readonly Color[] cols = new Color[4] { new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 0), new Color(0, 0, 1)};
    private int[] barrange = new int[4] { 0, 1, 2, 3 };
    private int[,,] seq = new int[5, 4, 2];
    private List<int> ans = new List<int> { };
    private int stage;
    private int index;
    private bool play;
    private float e = 6;
    private bool pressable;
    private string presslog;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        stageleds[0].material = io[1];
        StartCoroutine(Generate(0));
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved && pressable)
                {
                    play = true;
                    Audio.PlaySoundAtTransform("Beep" + barrange[b], bpos[b]);
                    StartCoroutine(Press(b));
                    if (index == 0)
                        presslog = "";
                    else
                        presslog += ", ";
                    presslog += collog[barrange[b]];
                    if (barrange[b] == ans[index])
                    {
                        if (index == stage)
                        {
                            stageleds[stage].material = io[0];
                            Debug.LogFormat("[Synapse Says #{0}] Buttons pressed: {1}", moduleID, presslog);
                            if (stage > 3)
                            {
                                moduleSolved = true;
                                module.HandlePass();
                            }
                            else
                                stage++;
                            StartCoroutine(Generate(stage));
                        }
                        else
                            index++;
                    }
                    else
                    {
                        Debug.LogFormat("[Synapse Says #{0}] Buttons pressed: {1}", moduleID, presslog);
                        module.HandleStrike();
                        stage = 0;
                        foreach (Renderer l in stageleds)
                            l.material = io[1];
                        ans.Clear();
                        StartCoroutine(Generate(0));
                    }
                }
                return false;
            };
        }
    }

    private IEnumerator Generate(int s)
    {
        display.text = "";
        int p = Random.Range(0, 4);
        if (!moduleSolved)
        {
            Debug.LogFormat("[Synapse Says #{0}] Stage {1}:", moduleID, stage + 1);
            index = 0;
            presslog = "";
            pressable = false;
            barrange = barrange.Shuffle().ToArray();
            Debug.LogFormat("[Synapse Says #{0}] The arrangement of buttons is: {1}", moduleID, string.Join(", ", barrange.Select(x => collog[x]).ToArray()));
            for (int i = 0; i < 4; i++)
            {
                int r = Random.Range(0, 4);
                seq[s, i, 0] = r;
                seq[s, i, 1] = barrange[r];
            }
            Debug.LogFormat("[Synapse Says #{0}] The sequence of flashes is: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 4).Select(x => collog[seq[stage, x, 1]]).ToArray()));
            Debug.LogFormat("[Synapse Says #{0}] The display is {1}.", moduleID, p + 1);
            int a = 0;
            switch (4 * s + p)
            {
                default:
                    if (seq[0, 3, 0] == 3)
                        a = seq[0, 0, 0];
                    else if (seq[0, 0, 1] == 3)
                        a = 2;
                    else if (Enumerable.Range(0, 4).All(x => seq[0, x, 1] != 1))
                        a = seq[0, 1, 0];
                    else
                        a = 0;
                    break;
                case 1:
                    if (Enumerable.Range(0, 4).Count(x => seq[0, x, 1] == 0) > 1)
                        a = 3;
                    else if (Enumerable.Range(0, 4).All(x => seq[0, x, 0] != 1))
                        a = 1;
                    else if (seq[0, 0, 1] == 2)
                        a = seq[0, 2, 0];
                    else
                        a = seq[0, 1, 0];
                    break;
                case 2:
                    if (Enumerable.Range(0, 4).Select(x => seq[0, x, 0]).Distinct().Count() > 3)
                        a = 0;
                    else if (seq[0, 0, 0] == seq[0, 3, 0])
                        a = seq[0, 2, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[0, x, 1] == 2) == 1)
                        a = 3;
                    else
                        a = seq[0, 3, 0];
                    break;
                case 3:
                    if (seq[0, 0, 1] == 0)
                        a = 1;
                    else if (Enumerable.Range(0, 3).Any(x => seq[0, x, 0] == seq[0, x + 1, 0]))
                        a = seq[0, 3, 0];
                    else if (seq[0, 2, 0] == 1)
                        a = seq[0, 0, 0];
                    else
                        a = 2;
                    break;
                case 4:
                    if (seq[1, 1, 0] < seq[1, 2, 0])
                        a = seq[0, 0, 0];
                    else if (Enumerable.Range(0, 4).All(x => seq[1, x, 0] != 3))
                        a = seq[0, 3, 0];
                    else if (seq[1, 0, 0] == seq[1, 2, 0])
                        a = 0;
                    else
                        a = seq[1, 3, 0];
                    break;
                case 5:
                    if (seq[1, 2, 0] == seq[1, 3, 0])
                        a = 3;
                    else if (Enumerable.Range(0, 4).All(x => seq[1, x, 1] != 0))
                        a = seq[0, 1, 0];
                    else if (seq[1, 0, 0] == 1)
                        a = seq[1, 1, 0];
                    else
                        a = seq[0, 2, 0];
                    break;
                case 6:
                    if (Enumerable.Range(0, 3).Any(x => (seq[1, x, 1] == 1 && seq[1, x + 1, 1] == 3) || (seq[1, x, 1] == 3 && seq[1, x + 1, 1] == 1)))
                        a = seq[1, 0, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[1, x, 0] == 0) == 1)
                        a = seq[0, 3, 0];
                    else if (Enumerable.Range(0, 4).All(x => seq[1, x, 1] != 2))
                        a = seq[0, 2, 0];
                    else
                        a = 2;
                    break;
                case 7:
                    if (Enumerable.Range(0, 4).Count(x => seq[1, x, 1] == 1) > 1)
                        a = seq[1, 0, 0];
                    else if (seq[1, 3, 0] == 2)
                        a = seq[0, 0, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[1, x, 0] == 1) == 1)
                        a = seq[0, 2, 0];
                    else
                        a = 1;
                    break;
                case 8:
                    if (seq[2, 1, 0] == 0)
                        a = seq[0, 1, 0];
                    else if (barrange[0] > seq[2, 0, 0])
                        a = seq[1, 3, 0];
                    else if (Enumerable.Range(0, 3).Any(x => (seq[2, x, 1] == 2 && seq[2, x + 1, 1] == 3) || (seq[2, x, 1] == 3 && seq[2, x + 1, 1] == 2)))
                        a = 0;
                    else
                        a = seq[2, 2, 0];
                    break;
                case 9:
                    if (seq[2, 1, 0] == 3)
                        a = seq[2, 0, 0];
                    else if (Enumerable.Range(0, 4).All(x => seq[2, x, 0] != 0))
                        a = seq[0, 2, 0];
                    else if (Enumerable.Range(0, 3).Any(x => Mathf.Abs(seq[2, x, 0] - seq[2, x + 1, 0]) == 1))
                        a = seq[1, 1, 0];
                    else
                        a = 3;
                    break;
                case 10:
                    if (seq[2, 3, 0] == seq[2, 1, 0])
                        a = seq[1, 2, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[2, x, 1] == 1) > 1)
                        a = 1;
                    else if (seq[2, 3, 0] == 0)
                        a = seq[2, 1, 0];
                    else
                        a = seq[0, 0, 0];
                    break;
                case 11:
                    if (seq[2, 0, 0] == 3)
                        a = 2;
                    else if (Enumerable.Range(0, 4).Count(x => seq[2, x, 1] < 2) == 2)
                        a = seq[2, 3, 0];
                    else if (seq[2, 3, 0] > seq[2, 0, 0])
                        a = seq[0, 3, 0];
                    else
                        a = seq[1, 1, 0];
                    break;
                case 12:
                    if (Enumerable.Range(0, 4).Count(x => seq[3, x, 1] == 1) > 1)
                        a = seq[1, 2, 0];
                    else if (Enumerable.Range(0, 3).Any(x => (seq[3, x, 1] == 0 && seq[3, x + 1, 1] == 3) || (seq[3, x, 1] == 3 && seq[3, x + 1, 1] == 3)))
                        a = seq[2, 0, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[3, x, 0] == 0) == 1)
                        a = seq[0, 3, 0];
                    else
                        a = seq[3, 0, 0];
                    break;
                case 13:
                    if (Enumerable.Range(0, 4).Select(x => seq[3, x, 0]).Distinct().Count() == 2)
                        a = seq[0, 3, 0];
                    else if (seq[3, 0, 1] == 0)
                        a = seq[3, 1, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[3, x, 1] == 1 || seq[3, x, 1] == 2) == 2)
                        a = seq[1, 0, 0];
                    else
                        a = seq[2, 2, 0];
                    break;
                case 14:
                    if (Enumerable.Range(0, 4).All(x => seq[3, x, 1] != ans[0]))
                        a = seq[2, 2, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[3, x, 0] == 1) == 1)
                        a = seq[1, 1, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[3, x, 1] == 3) > Enumerable.Range(0, 4).Count(x => seq[3, x, 1] == 2))
                        a = seq[3, 3, 0];
                    else
                        a = seq[0, 0, 0];
                    break;
                case 15:
                    if (Enumerable.Range(0, 4).Count(x => seq[3, x, 0] == 1) > Enumerable.Range(0, 4).Count(x => seq[3, x, 0] == 0))
                        a = seq[3, 0, 0];
                    else if (seq[3, 3, 1] == 3)
                        a = seq[1, 2, 0];
                    else if (Enumerable.Range(0, 3).Any(x => (seq[3, x, 1] == 0 && seq[3, x + 1, 1] == 2) || (seq[3, x, 1] == 2 && seq[3, x + 1, 1] == 3)))
                        a = seq[2, 1, 0];
                    else
                        a = seq[0, 3, 0];
                    break;
                case 16:
                    if (Enumerable.Range(0, 4).Count(x => seq[4, x, 1] % 2 == 1) == 2)
                        a = seq[0, 1, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[4, x, 0] == 3) > Enumerable.Range(0, 4).Count(x => seq[4, x, 0] == 2))
                        a = seq[2, 3, 0];
                    else if (Enumerable.Range(0, 4).All(x => seq[4, x, 0] != ans[2]))
                        a = seq[3, 0, 0];
                    else
                        a = seq[1, 2, 0];
                    break;
                case 17:
                    if (Enumerable.Range(0, 4).Count(x => seq[4, x, 1] == 0) > Enumerable.Range(0, 4).Count(x => seq[4, x, 1] == 3))
                        a = seq[2, 0, 0];
                    else if (Enumerable.Range(0, 4).All(x => seq[4, x, 0] != ans[3]))
                        a = seq[0, 1, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[4, x, 0] == 3) > 1)
                        a = seq[1, 2, 0];
                    else
                        a = seq[3, 3, 0];
                    break;
                case 18:
                    if (Enumerable.Range(0, 4).Count(x => seq[4, x, 0] == 1) > Enumerable.Range(0, 4).Count(x => seq[4, x, 0] == 4))
                        a = seq[1, 3, 0];
                    else if (Enumerable.Range(0, 4).All(x => seq[4, x, 1] != 3))
                        a = seq[3, 0, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[4, x, 1] % 2 == 0) == 2)
                        a = seq[2, 1, 0];
                    else
                        a = seq[0, 3, 0];
                    break;
                case 19:
                    if (Enumerable.Range(0, 4).All(x => seq[4, x, 0] != ans[1]))
                        a = seq[3, 2, 0];
                    else if (Enumerable.Range(0, 3).Any(x => (seq[4, x, 0] == 0 && seq[4, x + 1, 1] == 3) || (seq[4, x, 1] == 3 && seq[4, x + 1, 1] == 0)))
                        a = seq[1, 2, 0];
                    else if (Enumerable.Range(0, 4).Count(x => seq[4, x, 0] == 2) > 1)
                        a = seq[0, 1, 0];
                    else
                        a = seq[2, 0, 0];
                    break;
            }
            ans.Add(barrange[a]);
            if (stage > 0)
                Debug.LogFormat("[Synapse Says #{0}] Press the buttons: {1}", moduleID, string.Join(", ", ans.Select(x => collog[x]).ToArray()));
            else
                Debug.LogFormat("[Synapse Says #{0}] Press the {1} button.", moduleID, collog[ans[0]]);
        }
        StartCoroutine(Push(0));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Push(1));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Push(2));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Push(3));
        yield return new WaitForSeconds(1);
        if (!moduleSolved)
        {
            pressable = true;
            display.text = (p + 1).ToString();
            StartCoroutine("Sequence");
        }
    }

    private IEnumerator Sequence()
    {
        while (!moduleSolved)
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < 4; i++)
            {
                lights[seq[stage, i, 0]].enabled = true;
                if (play)
                    Audio.PlaySoundAtTransform("Beep" + seq[stage, i, 1], bpos[seq[stage, i, 0]]);
                yield return new WaitForSeconds(0.5f);
                lights[seq[stage, i, 0]].enabled = false;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator Push(int i)
    {
        Vector3 p = bpos[i].localPosition;
        float e = 0;
        while (e < 1)
        {
            e += Time.deltaTime * 2;
            bpos[i].localPosition = new Vector3(p.x, -0.51f * e, p.z);
            yield return null;
        }
        if (!moduleSolved)
        {
            yield return new WaitForSeconds(0.5f);
            brends[i].material = bmats[barrange[i]];
            lights[i].color = cols[barrange[i]];
            while (e > 0)
            {
                e -= Time.deltaTime * 2;
                bpos[i].localPosition = new Vector3(p.x, -0.51f * e, p.z);
                yield return null;
            }
            bpos[i].localPosition = new Vector3(p.x, 0, p.z);
        }
    }

    private IEnumerator Press(int i)
    {
        StopCoroutine("Sequence");
        foreach (Light l in lights)
            l.enabled = false;
        lights[i].enabled = true;
        yield return new WaitForSeconds(0.5f);
        lights[i].enabled = false;
        if (e == 6)
        {
            while (e > 0)
            {
                e -= Time.deltaTime;
                yield return null;
                if (!pressable)
                    yield break;
            }
            index = 0;
            StartCoroutine("Sequence");
        }
        else
            e = 6;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} <1-4> [Position of button. Chain with spaces.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        List<int> p = new List<int> { };
        for (int i = 0; i < commands.Length; i++)
        {
            if (commands[i].Length < 1)
                continue;
            if (commands[i].Length > 1)
            {
                yield return "sendtochaterror!f " + commands[i] + " is an invalid command.";
                yield break;
            }
            int s = 0;
            if(int.TryParse(commands[i], out s))
            {
                if (s > 0 && s < 5)
                    p.Add(s - 1);
                else
                {
                    yield return "sendtochaterror!f Position " + s + "is not present.";
                    yield break;
                }
            }
            else
            {
                yield return "sendtochaterror!f NaN command (\"" + commands[i] + "\") entered.";
                yield break;
            }
        }
        while (!pressable)
            yield return true;
        for (int i = 0; i < p.Count(); i++)
        {
            yield return null;
            buttons[p[i]].OnInteract();
            yield return new WaitForSeconds(0.5f);
            if (!pressable)
                yield break;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            while (!pressable)
                yield return true;
            for (int i = index; i < ans.Count(); i++)
            {
                yield return new WaitForSeconds(0.5f);
                for(int j = 0; j < 4; j++)
                    if(barrange[j] == ans[i])
                    {
                        buttons[j].OnInteract();
                        break;
                    }
            }
        }
    }
}
