using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WiresCrossedScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public Transform[] bpos;
    public Renderer[] brends;
    public Material[] bmats;
    public TextMesh[] blabels;
    public Renderer[] srends;
    public Material[] smats;
    public Light[] slights;
    public Transform[] wpos;
    public Renderer[] wrends;
    public Material[] wmats;

    private readonly string[] ordlog = new string[] { "first", "second", "third", "fourth", "fifth", "sixth" };
    private readonly string[] collog = new string[] { "Red", "Blue", "Yellow", "White", "Black" };
    private int[,] binfo = new int[5, 2];
    private int[,] winfo = new int[6, 2];
    private int[,] prinfo = new int[5, 5]; //Button Position; Button Colour; Button Label; Read Wire Colour; Light Colour;
    private int digit;
    private int stage;


    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        foreach (Renderer s in srends)
            s.material = smats[0];
        StartCoroutine(Stage());
        foreach (KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                    button.transform.localPosition -= new Vector3(0, 0.06f, 0);
                    StartCoroutine("Flash");
                }
                return false;
            };
            button.OnInteractEnded += delegate ()
            {
                if (!moduleSolved)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    button.transform.localPosition += new Vector3(0, 0.06f, 0);
                    button.AddInteractionPunch(0.7f);
                    StopCoroutine("Flash");
                    slights[stage].enabled = false;
                    Debug.LogFormat("[Wires Crossed #{0}] Button {1} released at {2}.", moduleID, b + 1, info.GetFormattedTime());
                    if (digit == -1 || info.GetFormattedTime().Any(x => x - '0' == digit))
                    {
                        srends[stage].material = smats[5];
                        stage++;
                        if (stage > 4)
                            moduleSolved = true;
                    }
                    else
                    {
                        module.HandleStrike();
                        foreach (Renderer l in srends)
                            l.material = smats[0];
                        stage = 0;
                    }
                    StartCoroutine("Stage");
                }
            };
        }
    }

    private IEnumerator Stage()
    {
        foreach (Renderer w in wrends)
            w.enabled = false;
        int wnum = new int[] { 3, 4, 5, 6, 6, 6 }.PickRandom();
        if (!moduleSolved)
        {
            Debug.LogFormat("[Wires Crossed #{0}] Stage {1}:", moduleID, stage + 1);
            List<int> order = new List<int> { 0, 1, 2, 3, 4, 5 }.Shuffle().Take(wnum).ToList();
            for (int i = 0; i < 6 - wnum; i++)
                order.Add(-1);
            order = order.Shuffle().ToList();
            for (int i = 0; i < 6; i++)
            {
                winfo[i, 0] = order[i];
                winfo[i, 1] = order[i] < 0 ? -1 : Random.Range(0, 5);
            }
            Debug.LogFormat("[Wires Crossed #{0}] {1} wires: {2}", moduleID, wnum, string.Join(", ", Enumerable.Range(0, 6).Where(x => order[x] >= 0).Select(x => collog[winfo[x, 1]] + " to " + "ABCDEF"[winfo[x, 0]]).ToArray()));
            order = new List<int> { 0, 1, 2, 3, 4 };
            for (int i = 0; i < 2; i++)
            {
                order = order.Shuffle().ToList();
                for (int j = 0; j < 5; j++)
                    binfo[j, i] = order[j];
            }
            Debug.LogFormat("[Wires Crossed #{0}] Buttons: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 5).Select(x => collog[binfo[x, 0]] + " " + (binfo[x, 1] + 1).ToString()).ToArray()));
        }
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.0625f);
            StartCoroutine(Move(i, true));
            yield return new WaitForSeconds(0.0625f);
        }
        if (!moduleSolved)
        {
            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.0625f);
                StartCoroutine(Move(i, false));
                yield return new WaitForSeconds(0.0625f);
            }
            yield return new WaitForSeconds(0.5f);
            int[] height = Enumerable.Range(0, 6).ToArray().Shuffle().ToArray();
            for (int i = 0; i < 6; i++)
            {
                int c = winfo[i, 1];
                if (c < 0)
                    continue;
                int[] w = new int[3] { i + 36, winfo[i, 0] + 42, (i * 6) + winfo[i, 0] };
                for (int j = 0; j < 3; j++)
                {
                    int q = w[j];
                    wrends[q].enabled = true;
                    wrends[q].material = wmats[c];
                }
                float h = Mathf.Lerp(0.009f, 0.12f, height[i] / 6f);
                Vector3 pos = wpos[w[2]].localPosition;
                wpos[w[2]].localPosition = new Vector3(0, h, pos.z);
            }
            int read = WireRule((stage * 4) + wnum - 3);
            read = Placement(read);
            prinfo[stage, 3] = winfo[read, 1];
            read = winfo[read, 0];
            int p = ButtonRule((stage * 6) + read);
            prinfo[stage, 0] = p;
            prinfo[stage, 1] = binfo[p, 0];
            prinfo[stage, 2] = binfo[p, 1];
            bool hold = HoldRule(stage);
            prinfo[stage, 4] = hold ? Random.Range(0, 4) : 4;
            Debug.LogFormat("[Wires Crossed #{0}] Rule {1}: Press and {3} the {2} button.", moduleID, "ABCDEF"[read], ordlog[p], hold ? "hold" : "immediately release");
            digit = hold ? RelRule((stage * 4) + prinfo[stage, 4]) + 1 : -1;
            if (digit > 0)
                Debug.LogFormat("[Wires Crossed #{0}] The light flashes {1}; release the button when there is a {2} on the bomb timer.", moduleID, collog[prinfo[stage, 4]], digit);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            module.HandlePass();
        }
    }

    private IEnumerator Flash()
    {
        yield return new WaitForSeconds(0.5f);
        if (digit < 0)
            digit = 0;
        int col = prinfo[stage, 4];
        if (col > 3)
            col = Random.Range(0, 4);
        srends[stage].material = smats[col + 1];
        Light s = slights[stage];
        s.enabled = true;
        s.color = new Color[] { new Color(1, 0.25f, 0.25f), new Color(0.25f, 1, 0.5f), new Color(1, 1, 0), new Color(1, 1, 1) }[col];
        float e = 0;
        float a = 0;
        while (!moduleSolved)
        {
            e += Time.deltaTime * Mathf.PI * 3;
            a = 10 * Mathf.Cos(e);
            s.intensity = a;
            yield return null;
        }
    }

    private int Placement(int r)
    {
        int[] pos = Enumerable.Range(0, 6).Select(x => winfo[x, 0]).ToArray();
        for (int i = 0; i < 6; i++)
        {
            if (pos[i] >= 0)
                r--;
            if (r < 0)
                return i;
        }
        return -1;
    }

    private IEnumerator Move(int b, bool down)
    {
        Vector3 q = bpos[b].localPosition;
        Vector2 p = new Vector2(down ? 0.194f : 0.1f, down ? 0.1f : 0.194f);
        float e = 0;
        while (e < 1)
        {
            e += Time.deltaTime * 2;
            bpos[b].localPosition = new Vector3(q.x, Mathf.Lerp(p.x, p.y, e), q.z);
            yield return null;
        }
        if (down)
        {
            brends[b].material = bmats[binfo[b, 0]];
            blabels[b].text = (binfo[b, 1] + 1).ToString();
        }
    }

    private int WireRule(int r)
    {
        int[] w = new int[2] { -1, 0 };
        int[] wcols = Enumerable.Range(0, 6).Select(x => winfo[x, 1]).ToArray();
        int[] presw = wcols.Where(x => x >= 0).ToArray();
        switch (r)
        {
            default:
                if (presw[2] == 1)
                    w = new int[2] { 0, 0 };
                else if (presw.All(x => x < 4))
                    w = new int[2] { 2, 1 };
                else if (presw.Distinct().Count() > 2)
                    w = new int[2] { 1, 2 };
                else
                    w = new int[2] { 2, 3 };
                break;
            case 4:
                if (presw.Count(x => x == 0) == 2)
                    w = new int[2] { presw[2] == 0 ? presw[1] == 0 ? 1 : 0 : 2, 0 };
                else if (presw[2] > 2)
                    w = new int[2] { 0, 1 };
                else if (presw.Any(x => x == 2))
                    w = new int[2] { 2, 2 };
                else
                    w = new int[2] { 1, 3 };
                break;
            case 8:
                if (presw.Any(x => x == 2) && presw.All(x => x != 3))
                    w = new int[2] { 2, 0 };
                else if (presw[0] == 0)
                    w = new int[2] { 1, 1 };
                else if (presw[1] != presw[2] && presw[1] != presw[0])
                    w = new int[2] { 0, 2 };
                else
                    w = new int[2] { 2, 3 };
                break;
            case 12:
                if (presw.GroupBy(x => x).Any(x => x.Count() == 2))
                    w = new int[2] { presw[0] == presw[1] ? 2 : (presw[0] == presw[2] ? 1 : 0), 0 };
                else if (presw.Any(x => x == 3) && presw.Any(x => x != 3))
                    w = new int[2] { presw[2] != 3 ? 2 : (presw[1] != 3 ? 1 : 0), 1 };
                else if (presw.All(x => x != 1))
                    w = new int[2] { 1, 2 };
                else
                    w = new int[2] { 0, 3 };
                break;
            case 16:
                if (presw.All(x => x > 2))
                    w = new int[2] { 0, 0 };
                else if (presw.Count(x => x == 2) == 1)
                    w = new int[2] { 1, 1 };
                else if (presw.Distinct().Count() < 3)
                    w = new int[2] { 2, 2 };
                else
                    w = new int[2] { Enumerable.Range(0, 3).First(x => presw[x] == Enumerable.Range(0, 5).First(y => presw.Contains(binfo[y, 0]))), 3 };
                break;
            case 1:
                if (presw[1] == presw[3])
                    w = new int[2] { 2, 0 };
                else if (presw[2] == presw[3])
                    w = new int[2] { 0, 1 };
                else if (presw[0] == presw[3])
                    w = new int[2] { 1, 2 };
                else
                    w = new int[2] { 2, 3 };
                break;
            case 5:
                if (Enumerable.Range(0, 3).Any(x => presw[x] == presw[x + 1]))
                    w = new int[2] { 1, 0 };
                else if (presw.Take(3).Count(x => x == 4) == 1)
                    w = new int[2] { 0, 1 };
                else if (presw.Skip(1).Count(x => x == 3) == 1)
                    w = new int[2] { Enumerable.Range(0, 3).First(x => presw[x + 1] == 3), 2 };
                else
                    w = new int[2] { 3, 3 };
                break;
            case 9:
                if (presw.GroupBy(x => x).All(x => x.Count() > 1))
                    w = new int[2] { 0, 0 };
                else if (presw.Distinct().Count() > 3)
                    w = new int[2] { 3, 1 };
                else if (presw.All(x => x > 1))
                    w = new int[2] { Enumerable.Range(1, 3).First(x => presw.Take(x).Contains(presw[x])), 2 };
                else
                    w = new int[2] { 1, 3 };
                break;
            case 13:
                if (presw[0] == 4)
                    w = new int[2] { 3, 0 };
                else if (Enumerable.Range(0, 3).Any(x => (presw[x] == 0 && presw[x + 1] == 1) || (presw[x] == 1 && presw[x + 1] == 0)))
                    w = new int[2] { 1, 1 };
                else if (presw.GroupBy(x => x).Count(x => x.Count() > 1) == 1)
                    w = new int[2] { Enumerable.Range(1, 3).Last(x => presw.Take(x).Contains(presw[x])), 2 };
                else
                    w = new int[2] { 0, 3 };
                break;
            case 17:
                if (presw.Distinct().Count() == 2)
                    w = new int[2] { Enumerable.Range(1, 3).First(x => presw[x] != presw[0]), 0 };
                else if (presw.Count(x => x == presw[1]) == 1 && presw.Count(x => x == presw[2]) == 1)
                    w = new int[2] { 3, 1 };
                else if (presw.All(x => x != 3))
                    w = new int[2] { 2, 2 };
                else
                    w = new int[2] { Enumerable.Range(0, 4).Last(x => presw[x] == Enumerable.Range(0, 5).First(y => presw.Contains(binfo[y, 0]))), 3 };
                break;
            case 2:
                if (presw.Count(x => x == 4) == 1 && presw.Count(x => x == 1) > 1)
                    w = new int[2] { 2, 0 };
                else if (presw.All(x => x != 1 && x != 2))
                    w = new int[2] { 4, 1 };
                else if (Enumerable.Range(1, 4).All(x => presw[x] != presw[0]))
                    w = new int[2] { 1, 2 };
                else
                    w = new int[2] { 3, 3 };
                break;
            case 6:
                if (presw.GroupBy(x => x).Count(x => x.Count() == 1) == 1)
                    w = new int[2] { Enumerable.Range(0, 5).First(x => presw.Where((y, i) => x != i).All(y => y != presw[x])), 0 };
                else if (presw.GroupBy(x => x).Any(x => x.Count() == 3))
                    w = new int[2] { Enumerable.Range(0, 3).First(x => presw.Where((y, i) => x != i).Count(y => y != presw[x]) == 3), 1 };
                else if (presw.Skip(1).Take(3).Distinct().Count() == 3)
                    w = new int[2] { 1, 2 };
                else
                    w = new int[2] { 0, 3 };
                break;
            case 10:
                if (presw.GroupBy(x => x).All(x => x.Count() > 1))
                    w = new int[2] { 4, 0 };
                else if (presw.Count(x => x == 0) == presw.Count(x => x == 3))
                    w = new int[2] { Enumerable.Range(0, 5).First(x => binfo[x, 0] == 4), 1 };
                else if (presw.All(x => x != 2))
                    w = new int[2] { 3, 2 };
                else
                    w = new int[2] { binfo[Enumerable.Range(0, 5).First(x => binfo[x, 0] == 1), 1], 3 };
                break;
            case 14:
                if (presw.Distinct().Count() == 4)
                    w = new int[2] { binfo[Enumerable.Range(0, 5).First(x => presw.Count(y => y == binfo[x, 0]) > 1), 1], 0 };
                else if (presw[0] == presw[4] || presw.Skip(1).Take(3).Count(x => x < 1) == 1)
                    w = new int[2] { 2, 1 };
                else if (presw.All(x => x != binfo[Enumerable.Range(0, 5).First(y => binfo[y, 1] == 0), 0]))
                    w = new int[2] { Enumerable.Range(0, 5).First(y => binfo[y, 1] == 1), 2 };
                else
                    w = new int[2] { Enumerable.Range(0, 5).First(y => binfo[y, 0] == 1), 3 };
                break;
            case 18:
                if (presw[2] == 1 && presw.Count(x => x == 1) == 1)
                    w = new int[2] { 0, 0 };
                else if (presw.All(x => x != 3))
                    w = new int[2] { binfo[Enumerable.Range(0, 5).First(y => binfo[y, 0] == 1), 1], 1 };
                else if (Enumerable.Range(0, 5).All(x => presw[x] != binfo[x, 0]))
                    w = new int[2] { Enumerable.Range(0, 5).First(y => binfo[y, 1] == 3), 2 };
                else
                    w = new int[2] { binfo[Enumerable.Range(0, 5).First(x => presw[x] == binfo[x, 0]), 1], 3 };
                break;
            case 3:
                if (presw.GroupBy(x => x).Count(x => x.Count() == 2) > 1)
                    w = new int[2] { 2, 0 };
                else if (presw[5] == 3 && presw.Count(x => x == 3) > 1)
                    w = new int[2] { 1, 1 };
                else if (presw.All(x => x < 4))
                    w = new int[2] { 5, 2 };
                else
                    w = new int[2] { 4, 3 };
                break;
            case 7:
                if (presw.Count(x => x < 1 || x > 3) == 3)
                    w = new int[2] { Enumerable.Range(0, 6).Last(x => presw.Count(y => y == presw[x]) > 1), 0 };
                else if (presw.GroupBy(x => x).Any(x => x.Count() > 2))
                    w = new int[2] { 0, 1 };
                else if (presw[5] != 2 && presw.Count(x => x == 2) == 1)
                    w = new int[2] { Enumerable.Range(0, 5).First(x => x == 2) + 1, 2 };
                else
                    w = new int[2] { 3, 3 };
                break;
            case 11:
                if (Enumerable.Range(1, 4).Any(x => presw[x] == 1 && (presw[x - 1] == 3 || presw[x + 1] == 3)))
                    w = new int[2] { 4, 0 };
                else if (presw.Count(x => x < 1) > 1 && Enumerable.Range(0, 5).All(x => presw[x] > 0 || presw[x + 1] > 0))
                    w = new int[2] { 0, 1 };
                else if (presw.GroupBy(x => x).Count(x => x.Count() == 2) == 1)
                    w = new int[2] { Enumerable.Range(0, 5).First(x => presw.Count(y => y == presw[x]) == 2), 2 };
                else
                    w = new int[2] { binfo[Enumerable.Range(0, 5).First(x => binfo[x, 0] == presw[5]), 1], 3 };
                break;
            case 15:
                if (presw.GroupBy(x => x).Count(x => x.Count() == 1) == 1)
                    w = new int[2] { 1, 0 };
                else if (Enumerable.Range(1, 4).All(x => presw[x] > 2 || presw[x - 1] > 2 || presw[x + 1] > 2))
                    w = new int[2] { Enumerable.Range(0, 6).Last(x => presw.Count(y => y == presw[x]) > 1), 1 };
                else if (presw.All(x => x != binfo[4, 0]))
                    w = new int[2] { Enumerable.Range(0, 5).First(x => binfo[x, 0] == 4), 2 };
                else
                    w = new int[2] { 2, 3 };
                break;
            case 19:
                if (presw.Distinct().Count() == 4)
                    w = new int[2] { binfo[Enumerable.Range(0, 5).First(x => !presw.Contains(x)), 1], 0 };
                else if (presw.Take(5).All(x => x != presw[5]))
                    w = new int[2] { binfo[Enumerable.Range(0, 5).First(x => binfo[x, 0] == presw[5]), 1], 1 };
                else if (presw.All(x => presw.Count(y => y == x) > 1))
                    w = new int[2] { Enumerable.Range(0, 5).First(x => binfo[x, 1] == presw.Select(y => Enumerable.Range(0, 5).First(z => binfo[z, 0] == y)).Min()), 2 };
                else
                    w = new int[2] { Enumerable.Range(0, 5).First(x => binfo[x, 1] == presw.Select(y => Enumerable.Range(0, 5).First(z => binfo[z, 0] == y)).Max()), 3 };
                break;
        }
        Debug.LogFormat("[Wires Crossed #{0}] The {1} rule applies; the key wire is the {2} wire.", moduleID, ordlog[w[1]], ordlog[w[0]]);
        return w[0];
    }

    private int ButtonRule(int r)
    {
        List<int> bc = Enumerable.Range(0, 5).Select(x => binfo[x, 0]).ToList();
        List<int> bl = Enumerable.Range(0, 5).Select(x => binfo[x, 1]).ToList();
        switch (r)
        {
            default: return 1;
            case 1: return bc.IndexOf(0);
            case 2: return bl.IndexOf(3);
            case 3: return 3;
            case 4: return bc.IndexOf(2);
            case 5: return bl.IndexOf(0);
            case 6: return bc.IndexOf(prinfo[0, 3]);
            case 7: return bc.IndexOf(prinfo[0, 1]);
            case 8: return 4;
            case 9: return bl.IndexOf(prinfo[0, 2]);
            case 10: return bl.IndexOf(1);
            case 11: return bc.IndexOf(4);
            case 12: return bl.IndexOf(4);
            case 13: return prinfo[0, 0];
            case 14: return bl.IndexOf(prinfo[1, 3]);
            case 15: return bc.IndexOf(prinfo[0, 4]);
            case 16: return bc.IndexOf(prinfo[1, 4]);
            case 17: return bl.IndexOf(prinfo[1, 2]);
            case 18: return bc.IndexOf(3);
            case 19: return bc.IndexOf(prinfo[1, 1]);
            case 20: return bl.IndexOf(prinfo[2, 2]);
            case 21: return prinfo[1, 0];
            case 22: return bl.IndexOf(2);
            case 23: return bc.IndexOf(prinfo[2, 4]);
            case 24: return bl.IndexOf(prinfo[3, 2]);
            case 25: return bc.IndexOf(prinfo[2, 1]);
            case 26: return bc.IndexOf(prinfo[3, 4]);
            case 27: return bc.IndexOf(prinfo[3, 3]);
            case 28: return bc.IndexOf(prinfo[2, 3]);
            case 29: return prinfo[2, 0];
        }
    }

    private bool HoldRule(int s)
    {
        List<int> bp = Enumerable.Range(0, 4).Select(x => prinfo[stage, x]).ToList();
        List<int> bc = Enumerable.Range(0, 5).Select(x => binfo[x, 0]).ToList();
        List<int> bl = Enumerable.Range(0, 5).Select(x => binfo[x, 1]).ToList();
        List<int> wc = Enumerable.Range(0, 6).Select(x => winfo[x, 1]).ToList();
        switch (s)
        {
            default:
                if (wc.Count(x => x == bp[1]) > 2) return false;
                else if (bp[1] == bp[3]) return true;
                else if (bp[0] == bp[2]) return false;
                else return true;
            case 1:
                if (wc.All(x => x != bp[1] && x != prinfo[0, 1])) return false;
                else if (prinfo[0, 4] > 3) return true;
                else if (bp[2] == wc.Where(x => x >= 0).ToList().IndexOf(bp[3])) return false;
                else return true;
            case 2:
                if (Enumerable.Range(0, 3).All(x => Enumerable.Range(0, 3).All(y => prinfo[y, 1] != prinfo[x, 3]))) return false;
                else if (Enumerable.Range(0, 3).Select(x => prinfo[x, 1]).Distinct().Count() > 2 || Enumerable.Range(0, 3).Select(x => prinfo[x, 2]).Distinct().Count() > 2) return true;
                else if (Enumerable.Range(0, 3).Select(x => prinfo[x, 3]).Distinct().Count() > 2) return false;
                else return true;
            case 3:
                if (Enumerable.Range(0, 3).All(x => prinfo[x, 0] != bp[1] && prinfo[x, 1] != bp[2])) return false;
                else if (Enumerable.Range(0, 3).Any(x => prinfo[x, 4] < 4 && prinfo[x, 4] == bp[1])) return true;
                else if (Enumerable.Range(0, 4).Any(x => prinfo[x, 1] < 4)) return false;
                else return true;
            case 4: return true;
        }
    }

    private int RelRule(int s)
    {
        List<int> bc = Enumerable.Range(0, 5).Select(x => binfo[x, 0]).ToList();
        List<int> bl = Enumerable.Range(0, 5).Select(x => binfo[x, 1]).ToList();
        switch (s)
        {
            default: return 0;
            case 1: return 3;
            case 2: return 4;
            case 3: return 2;
            case 4: return prinfo[0, 2];
            case 5: return bl.IndexOf(1);
            case 6: return bl[4];
            case 7: return bc.IndexOf(4);
            case 8: return bc.IndexOf(prinfo[2, 3]);
            case 9: return bc.IndexOf(prinfo[1, 1]);
            case 10: return bl.IndexOf(prinfo[0, 2]);
            case 11: return bl[prinfo[1, 0]];
            case 12: return bl.IndexOf(prinfo[2, 2]);
            case 13: return bc.IndexOf(prinfo[1, 3]);
            case 14: return bl[bc.IndexOf(prinfo[2, 1])];
            case 15: return bl[bc.IndexOf(prinfo[0, 1])];
            case 16: return bl[bc.IndexOf(prinfo[3, 3])];
            case 17: return bl[bc.IndexOf(prinfo[1, 3])];
            case 18: return bl[bc.IndexOf(prinfo[0, 3])];
            case 19: return bl[bc.IndexOf(prinfo[2, 3])];
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} hold/tap 1-5 [Position of button to press] | !{0} release ##:## [Releases held button at the specified minutes and seconds.]";
#pragma warning restore 414

    private int currpress = -1;
    private bool ZenModeActive;

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToUpperInvariant().Split(' ');
        int d = 0;
        switch (commands[0])
        {
            case "HOLD":
            case "TAP":
                if (currpress >= 0)
                {
                    yield return "sendtochaterror The held button must be released before another can be held.";
                    yield break;
                }
                if (int.TryParse(commands[1], out d))
                {
                    if (d > 0 && d < 6)
                    {
                        d -= 1;
                        yield return null;
                        buttons[d].OnInteract();
                        currpress = d;
                        if (commands[0] == "TAP")
                        {
                            yield return new WaitForSeconds(0.25f);
                            currpress = -1;
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