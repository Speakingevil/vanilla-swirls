using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhosSimonScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> buttons;
    public Transform[] bpos;
    public Renderer[] brends;
    public Light[] lights;
    public Material[] bmats;
    public Renderer[] stageleds;
    public Material[] io;
    public TextMesh display;

    private readonly string[] words = new string[12] { "RED", "YELLOW", "GREEN", "BLUE", "ORANGE", "PURPLE", "TOP LEFT", "TOP RIGHT", "MIDDLE LEFT", "MIDDLE RIGHT", "BOTTOM LEFT", "BOTTOM RIGHT" };
    private readonly string[] pos = new string[5] { "first", "second", "third", "fourth", "fifth" };
    private readonly int[] eyes = new int[12] { 4, 2, 1, 5, 0, 3, 5, 2, 0, 4, 3, 1 };
    private readonly int[][] lists = new int[6][]{
        new int[6]{ 5, 2, 1, 0, 3, 4},
        new int[6]{ 2, 0, 5, 1, 4, 3},
        new int[6]{ 3, 5, 4, 2, 0, 1},
        new int[6]{ 1, 4, 3, 5, 2, 0},
        new int[6]{ 4, 1, 0, 3, 5, 2},
        new int[6]{ 0, 3, 2, 4, 1, 5}
    };
    private List<int> barrange = new List<int> { 0, 1, 2, 3, 4, 5};
    private List<int> seq = new List<int> { };
    private List<int> ans = new List<int> { };
    private int stage;
    private int index;
    private bool pressable;
    private bool play;
    private float e = 6;
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
                    presslog += words[barrange[b]];
                    if(barrange[b] == ans[index])
                    {
                        if (index == stage + 2)
                        {
                            stageleds[stage].material = io[0];
                            Debug.LogFormat("[Who's Simon? #{0}] Buttons pressed: {1}", moduleID, presslog);
                            if (stage > 1)
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
                        Debug.LogFormat("[Who's Simon? #{0}] Buttons pressed: {1}", moduleID, presslog);
                        module.HandleStrike();
                        StartCoroutine(Generate(stage));
                    }
                }
                return false;
            };
        }
    }

    private IEnumerator Generate(int s)
    {
        pressable = false;
        display.text = "";
        string w = "";
        if (!moduleSolved)
        {
            int p = Random.Range(0, 12);
            w = words[p];
            display.fontSize = p > 5 ? 180 : 300;
            Debug.LogFormat("[Who's Simon? #{0}] The display is \"{1}\".", moduleID, w);
            barrange = barrange.Shuffle();
            Debug.LogFormat("[Who's Simon? #{0}] The arrangement of buttons, in reading order, is: {1}.", moduleID, string.Join(", ", barrange.Select(x => words[x]).ToArray()));
            ans.Clear();
            seq.Clear();
            for (int i = 0; i < stage + 3; i++)
                seq.Add(Random.Range(0, 6));
            Debug.LogFormat("[Who's Simon? #{0}] The sequence of flashing buttons is: {1}.", moduleID, string.Join(", ", seq.Select(x => words[barrange[x]]).ToArray()));
            switch (stage)
            {
                default:
                    index = barrange[eyes[p]];
                    Debug.LogFormat("[Who's Simon? #{0}] The initial list is {1}.", moduleID, words[index]);
                    for (int i = 0; i < 3; i++)
                    {
                        if (lists[index].Take(i + 1).Contains(barrange[seq[i]]))
                        {
                            index = lists[index][i];
                            Debug.LogFormat("[Who's Simon? #{0}] Move to the {1} list before the {2} press.", moduleID, words[index], pos[i]);
                        }
                        ans.Add(lists[index][i]);
                    }
                    break;
                case 1:
                    index = barrange[eyes[p > 5 ? barrange[p - 6] : (barrange.IndexOf(p) + 6)]];
                    Debug.LogFormat("[Who's Simon? #{0}] The initial list is {1}.", moduleID, words[index]);
                    for (int i = 0; i < 4; i++)
                    {
                        if (seq.Take(i + 1).Select(x => barrange[x]).Contains(lists[index][i]))
                        {
                            index = barrange[eyes[seq[i] + 6]];
                            Debug.LogFormat("[Who's Simon? #{0}] Move to the {1} list before the {2} press.", moduleID, words[index], pos[i]);
                        }
                        ans.Add(lists[index][i]);
                    }
                    break;
                case 2:
                    index = barrange[eyes[p > 5 ? barrange[p - 6] : (barrange.IndexOf(p) + 6)]];
                    index = barrange[eyes[index]];
                    Debug.LogFormat("[Who's Simon? #{0}] The initial list is {1}.", moduleID, words[index]);
                    for (int i = 0; i < 5; i++)
                    {
                        if (seq.Select(x => barrange[x]).All(x => x != lists[index][i]))
                        {
                            index = barrange[eyes[p > 5 ? barrange.IndexOf(lists[index][i]) + 6 : lists[index][i]]];
                            Debug.LogFormat("[Who's Simon? #{0}] Move to the {1} list before the {2} press.", moduleID, words[index], pos[i]);
                        }
                        ans.Add(lists[index][i]);
                    }
                    break;
            }
            Debug.LogFormat("[Who's Simon? #{0}] Press the buttons in the order: {1}.", moduleID, string.Join(", ", ans.Select(x => words[x]).ToArray()));
        }
        index = 0;
        e = 6;
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
        if (!moduleSolved)
        {
            display.text = w;
            StartCoroutine("Sequence");
        }
    }

    private IEnumerator Sequence()
    {
        while (!moduleSolved)
        {
            yield return new WaitForSeconds(1f);
            for(int i = 0; i < stage + 3; i++)
            {
                lights[seq[i]].enabled = true;
                if (play)
                    Audio.PlaySoundAtTransform("Beep" + barrange[seq[i]], bpos[seq[i]]);
                yield return new WaitForSeconds(0.5f);
                lights[seq[i]].enabled = false;
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
            bpos[i].localPosition = new Vector3(p.x, p.y, Mathf.Lerp(0.5f, -2, e));
            yield return null;
        }
        if (!moduleSolved)
        {
            yield return new WaitForSeconds(0.5f);
            brends[i].material = bmats[barrange[i]];
            while (e > 0)
            {
                e -= Time.deltaTime * 2;
                bpos[i].localPosition = new Vector3(p.x, p.y, Mathf.Lerp(0.5f, -2, e));
                yield return null;
            }
            bpos[i].localPosition = new Vector3(p.x, p.y, 0.5f);
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
            StartCoroutine("Sequence");
        }
        else
            e = 6;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} <t/m/b><l/r> [Position of button. Chain with spaces.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        List<int> p = new List<int> { };
        for (int i = 0; i < commands.Length; i++)
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
            for(int i = index; i < ans.Count(); i++)
            {
                yield return new WaitForSeconds(0.5f);
                for (int j = 0; j < 6; j++)
                    if (barrange[j] == ans[i])
                    {
                        buttons[j].OnInteract();
                        break;
                    }
            }
        }
    }
}
