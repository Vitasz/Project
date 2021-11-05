using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : MonoBehaviour
{
    public GridFunc grid;
    public Clock clock;
    List<HumanFunctionality> Humans = new List<HumanFunctionality>();
    List<HumanFunctionality> queue = new List<HumanFunctionality>();
    public float speed=3;
    public void Start()
    {
        StartCoroutine("Go");
    }
    public IEnumerator Go()
    {
        while (true)
        {
            foreach (HumanFunctionality a in queue) Humans.Add(a);
            queue.Clear();
            Dictionary<HumanFunctionality, bool> tryedMoved = new Dictionary<HumanFunctionality, bool>();
            foreach (HumanFunctionality a in Humans) tryedMoved.Add(a, false);
            List<HumanFunctionality> todel = new List<HumanFunctionality>();
            List<(HumanFunctionality, Vector3, Vector3)> toMove = new List<(HumanFunctionality, Vector3, Vector3)>();
            foreach (HumanFunctionality i in Humans)
            {
                if (!tryedMoved[i])
                {
                    tryedMoved[i] = true;
                    List<HumanFunctionality> tryedMovedHuman = new List<HumanFunctionality>() { i };
                    while (true)
                    {
                        HumanFunctionality nowHuman = tryedMovedHuman[tryedMovedHuman.Count - 1];

                        if (nowHuman.CanMove() == null)
                        {
                            for (int j = tryedMovedHuman.Count - 1; j >= 0; j--)
                            {
                                Vector3 from, to;
                                bool notok = tryedMovedHuman[j].MoveToNext(out from, out to);
                                if (notok) todel.Add(tryedMovedHuman[j]);
                                else toMove.Add((tryedMovedHuman[j], from, to));
                            }
                            break;
                        }
                        else
                        {
                            if (tryedMovedHuman.Contains(nowHuman.CanMove()))
                            {
                                int lastind = tryedMovedHuman.IndexOf(nowHuman.CanMove());
                                for (int j = tryedMovedHuman.Count - 1; j >= lastind; j--)
                                {
                                    Vector3 from, to;
                                    bool notok = tryedMovedHuman[j].MoveToNext(out from, out to);
                                    if (notok) todel.Add(tryedMovedHuman[j]);
                                    else toMove.Add((tryedMovedHuman[j], from, to));
                                }
                                break;
                            }
                            else
                            {
                                tryedMovedHuman.Add(nowHuman.CanMove());
                                if (tryedMoved[tryedMovedHuman[tryedMovedHuman.Count - 1]]) break;
                                tryedMoved[nowHuman.CanMove()] = true;
                            }
                        }
                    }
                }
            }
            foreach (HumanFunctionality a in todel)
            {
                a.DeleteHuman();
                Humans.Remove(a);
            }
            float progress = 0f;
            clock.UpdateWaitTime();
            // Debug.Log(toMove.Count);
            if (toMove.Count!=0)
            {
                while (progress < 1f)
                {
                    progress += Time.deltaTime*speed;
                    foreach ((HumanFunctionality, Vector3, Vector3) a in toMove)
                    {
                        a.Item1.transform.localPosition = Vector3.Lerp(a.Item2, a.Item3, progress);
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
            else 
                yield return new WaitForEndOfFrame(); ;
        }
        
    }
    public void AddHuman(HumanFunctionality human) => queue.Add(human);
    public void DeleteAllHumans()
    {
        foreach (HumanFunctionality a in queue) Humans.Add(a);
        queue.Clear();
        for (int i = 0; i < Humans.Count; i++)
        {
            Humans[i].DeleteHuman();
        }
        Humans.Clear();
    }
}
