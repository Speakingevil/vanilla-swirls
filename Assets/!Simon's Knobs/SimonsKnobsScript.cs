using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class SimonsKnobsScript : MonoBehaviour {

    public KMAudio Audio;
    public KMNeedyModule module;
    public KMBombInfo info;
    public List<KMSelectable> knobs;
    public Transform[] krot;
    public Transform[] drot;
    public Light[] kflash;

    private readonly string[,] logs = new string[2, 4] { { "up", "right", "down", "left" }, { "blue", "red", "yellow", "green"} };
    private readonly int[][,,] seq = new int[2][,,] {new int[16, 5, 4] { { { 0, 1, 3, 2 }, { 2, 1, 2, 3 }, { 1, 1, 1, 3 }, { 1, 2, 0, 2 }, { 0, 2, 2, 3 } }, { { 0, 2, 0, 1 }, { 2, 3, 0, 2 }, { 2, 0, 3, 3 }, { 1, 2, 3, 0 }, { 3, 0, 0, 3 } }, { { 2, 1, 0, 3 }, { 2, 0, 1, 2 }, { 0, 0, 1, 1 }, { 3, 2, 1, 0 }, { 2, 3, 1, 2 } }, { { 2, 3, 1, 3 }, { 0, 3, 0, 0 }, { 3, 0, 3, 3 }, { 1, 0, 1, 1 }, { 1, 3, 1, 0 } }, { { 3, 0, 3, 0 }, { 1, 0, 3, 0 }, { 0, 3, 2, 1 }, { 3, 1, 1, 3 }, { 1, 0, 0, 0 } }, { { 1, 1, 0, 1 }, { 0, 1, 3, 3 }, { 1, 3, 1, 1 }, { 3, 3, 0, 3 }, { 2, 0, 1, 0 } }, { { 2, 1, 2, 0 }, { 1, 2, 2, 1 }, { 0, 0, 2, 3 }, { 3, 1, 2, 2 }, { 2, 1, 3, 0 } }, { { 2, 2, 0, 3 }, { 1, 3, 1, 2 }, { 0, 1, 0, 1 }, { 2, 0, 2, 2 }, { 3, 2, 3, 3 } }, { { 3, 1, 3, 0 }, { 1, 2, 2, 3 }, { 3, 3, 2, 0 }, { 0, 3, 1, 2 }, { 1, 2, 1, 1 } }, { { 2, 0, 0, 0 }, { 2, 1, 0, 2 }, { 3, 3, 0, 0 }, { 2, 2, 0, 2 }, { 3, 3, 1, 2 } }, { { 0, 2, 3, 1 }, { 3, 2, 1, 2 }, { 0, 0, 2, 0 }, { 3, 0, 2, 2 }, { 2, 3, 0, 3 } }, { { 2, 3, 0, 1 }, { 3, 3, 1, 1 }, { 3, 0, 2, 3 }, { 0, 0, 3, 1 }, { 0, 1, 2, 1 } }, { { 1, 0, 0, 3 }, { 1, 0, 2, 3 }, { 2, 0, 1, 1 }, { 3, 1, 1, 2 }, { 0, 3, 3, 1 } }, { { 1, 2, 1, 2 }, { 2, 3, 2, 1 }, { 1, 0, 0, 2 }, { 0, 2, 0, 2 }, { 3, 0, 1, 2 } }, { { 0, 0, 2, 1 }, { 2, 1, 3, 2 }, { 1, 2, 1, 0 }, { 1, 1, 1, 2 }, { 0, 0, 3, 2 } }, { { 2, 0, 2, 1 }, { 2, 0, 3, 2 }, { 3, 0, 3, 1 }, { 0, 2, 2, 0 }, { 0, 0, 3, 3 } } },
                                                     new int[16, 5, 4] { { {2, 3, 0, 3}, { 0, 0, 2, 0}, { 3, 0, 2, 2}, { 3, 2, 1, 2}, { 0, 2, 3, 1} }, { { 3, 3, 0, 3}, { 2, 0, 1, 0}, { 0, 1, 3, 3}, { 1, 1, 0, 1}, { 1, 3, 1, 1} }, { { 1, 2, 0, 2}, { 0, 2, 2, 3}, { 0, 1, 3, 2}, { 1, 1, 1, 3}, { 2, 1, 2, 3} }, { { 3, 3, 0, 0}, { 3, 3, 1, 2}, { 2, 2, 0, 2}, { 2, 1, 0, 2}, { 2, 0, 0, 0} }, { { 0, 1, 0, 1}, { 2, 0, 2, 2}, { 1, 3, 1, 2}, { 3, 2, 3, 3}, { 2, 2, 0, 3} }, { { 2, 0, 3, 2}, { 2, 0, 2, 1}, { 0, 0, 3, 3}, { 3, 0, 3, 1}, { 0, 2, 2, 0} }, { { 3, 1, 1, 2}, { 2, 0, 1, 1}, { 0, 3, 3, 1}, { 1, 0, 0, 3}, { 1, 0, 2, 3} }, { { 3, 0, 2, 3}, { 0, 1, 2, 1}, { 0, 0, 3, 1}, { 3, 3, 1, 1}, { 2, 3, 0, 1} }, { { 2, 1, 3, 0}, { 3, 1, 2, 2}, { 2, 1, 2, 0}, { 0, 0, 2, 3}, { 1, 2, 2, 1} }, { { 3, 3, 2, 0}, { 0, 3, 1, 2}, { 1, 2, 1, 1}, { 3, 1, 3, 0}, { 1, 2, 2, 3} }, { { 1, 0, 0, 0}, { 0, 3, 2, 1}, { 3, 0, 3, 0}, { 1, 0, 3, 0}, { 3, 1, 1, 3} }, { { 1, 1, 1, 2}, { 1, 2, 1, 0}, { 0, 0, 3, 2}, { 2, 1, 3, 2}, { 0, 0, 2, 1} }, { { 1, 3, 1, 0}, { 1, 0, 1, 1}, { 2, 3, 1, 3}, { 3, 0, 3, 3}, { 0, 3, 0, 0} }, { { 3, 0, 0, 3}, { 2, 0, 3, 3}, { 1, 2, 3, 0}, { 2, 3, 0, 2}, { 0, 2, 0, 1} }, { { 1, 0, 0, 2}, { 1, 2, 1, 2}, { 2, 3, 2, 1}, { 3, 0, 1, 2}, { 0, 2, 0, 2} }, { { 3, 2, 1, 0}, { 0, 0, 1, 1}, { 2, 0, 1, 2}, { 2, 3, 1, 2}, { 2, 1, 0, 3} } } };
    private int[] offs = new int[4];
    private int[] rots = new int[4];
    private int[] req = new int[2];

    private static int moduleIDCounter;
    private int moduleID;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        module.OnNeedyActivation += delegate () { StartCoroutine("On"); };
        module.OnTimerExpired += Off;
        foreach(KMSelectable knob in knobs)
        {
            int k = knobs.IndexOf(knob);
            knob.OnInteract += delegate ()
            {
                rots[k]++;
                rots[k] %= 4;
                krot[k].localEulerAngles += new Vector3(0, 0, 90);
                return false;
            };
        }
        float scale = module.transform.lossyScale.x;
        foreach (Light l in kflash)
            l.range *= scale;
    }

    private IEnumerator Rotate(int p, int r)
    {
        float e = r;
        while(e > 0)
        {
            float d = Time.deltaTime;
            e -= d;
            drot[p].localEulerAngles -= new Vector3(0, 0, d * 90);
            yield return null;
        }
        drot[p].localEulerAngles = new Vector3(0, 0, -offs[p] * 90);
    }

    private IEnumerator On()
    {
        bool sound = true;
        for (int i = 0; i < 2; i++)
            req[i] = Random.Range(0, 4);
        Debug.LogFormat("[Simon's Knobs #{0}] Set the {1} dial to the {2} position.", moduleID, logs[1, req[0]], logs[0, req[1]]);
        for(int i = 0; i < 4; i++)
        {
            int k = Random.Range(0, 4);
            offs[i] += k;
            offs[i] %= 4;
            if (k > 0)
                StartCoroutine(Rotate(i, k));
        }
        yield return new WaitForSeconds(1);
        int lr = Random.Range(0, 2);
        while (true)
        {
            for(int i = 0; i < 4; i++)
            {
                int f = seq[lr][(req[0] * 4) + req[1],Mathf.Min(info.GetStrikes(), 4),i];
                kflash[f].enabled = true;
                if(sound)
                  Audio.PlaySoundAtTransform("Beep" + f, krot[f]);
                yield return new WaitForSeconds(0.5f);
                foreach (Light l in kflash)
                    l.enabled = false;
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(1);
            sound = false;
        }
    }

    private void Off()
    {
        StopCoroutine("On");
        foreach (Light l in kflash)
            l.enabled = false;
        int[] rel = new int[4];
        for (int i = 0; i < 4; i++)
        {
            rel[i] = (rots[i] + offs[i]) % 4;
            Debug.LogFormat("[Simon's Knobs #{0}] The {1} knob was set to the {2} position.", moduleID, logs[1, i], logs[0, rel[i]]);
        }
        if (rel.Distinct().Count() < 4 || rel[req[1]] != req[0])
            module.HandleStrike();
    }
}
