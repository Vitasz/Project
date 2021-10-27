using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : MonoBehaviour
{
    public GridFunc grid;
    public Clock clock;
    List<HumanFunctionality> Humans = new List<HumanFunctionality>();
    List<HumanFunctionality> queue = new List<HumanFunctionality>();
    List<HFforOA> HumansForOA = new List<HFforOA>();
    
    public void Start()
    {
      //  StartCoroutine("Go");
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
                                bool notok = tryedMovedHuman[j].MoveToNext();
                                if (notok) todel.Add(tryedMovedHuman[j]);
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
                                    bool notok = tryedMovedHuman[j].MoveToNext();
                                    if (notok) todel.Add(tryedMovedHuman[j]);
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
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void GoNotInf()
    {
        int time = 1;
        while (HumansForOA.Count!=0)
        {
            Dictionary<HFforOA, bool> tryedMoved = new Dictionary<HFforOA, bool>();
            foreach (HFforOA a in HumansForOA) tryedMoved.Add(a, false);
            List<HFforOA> todel = new List<HFforOA>();
            foreach (HFforOA i in HumansForOA)
            {
                if (!tryedMoved[i])
                {
                    tryedMoved[i] = true;
                    List<HFforOA> tryedMovedHuman = new List<HFforOA>() { i };
                    while (true)
                    {
                        HFforOA nowHuman = tryedMovedHuman[tryedMovedHuman.Count - 1];

                        if (nowHuman.CanMove() == null)
                        {
                            for (int j = tryedMovedHuman.Count - 1; j >= 0; j--)
                            {
                                bool notok = tryedMovedHuman[j].MoveToNext();
                                if (notok) todel.Add(tryedMovedHuman[j]);
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
                                    bool notok = tryedMovedHuman[j].MoveToNext();
                                    if (notok) todel.Add(tryedMovedHuman[j]);
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
            clock.totalTimes += todel.Count * time;
            foreach (HFforOA a in todel)
            {
                
                a.DeleteHuman();
                HumansForOA.Remove(a);
            }
            time++;
        }
    }
    public void AddHuman(HumanFunctionality human) => queue.Add(human);
    public void AddHumanForOA(HFforOA human) => HumansForOA.Add(human);
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
