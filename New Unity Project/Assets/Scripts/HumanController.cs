using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : MonoBehaviour
{
    public GridFunc grid;
    public Clock clock;
    List<Dictionary<(int, int), Dictionary<(float, float), HumanFunctionality>>> HumansInProcess = new List<Dictionary<(int, int), Dictionary<(float, float), HumanFunctionality>>>();
    Dictionary<HumanFunctionality, ((int,int), (float,float))> whenDelete = new Dictionary<HumanFunctionality, ((int, int), (float, float))>();
    public float speed=3;
    public void Start()
    {
        StartCoroutine("Go");
    }
    public IEnumerator Go()
    {
        while (true)
        {
            if (HumansInProcess.Count != 0)
            {
                List<(HumanFunctionality, Vector3)> tomove = new List<(HumanFunctionality, Vector3)>();
                List<HumanFunctionality> todel = new List<HumanFunctionality>();
                foreach ((int, int) a in HumansInProcess[0].Keys)
                {
                    foreach ((float, float) b in HumansInProcess[0][a].Keys)
                    {
                        tomove.Add((HumansInProcess[0][a][b], new Vector3(a.Item1 + b.Item1, a.Item2 + b.Item2, 0)));
                        if (whenDelete[HumansInProcess[0][a][b]] == (a, b)) todel.Add(HumansInProcess[0][a][b]);
                    }
                }

                foreach ((HumanFunctionality, Vector3) c in tomove)
                {
                    c.Item1.transform.localPosition = c.Item2;
                }
                
                HumansInProcess.RemoveAt(0);
                yield return new WaitForFixedUpdate();
                foreach (HumanFunctionality a in todel)
                {
                    a.DeleteHuman();
                    whenDelete.Remove(a);
                }
            }
            else yield return new WaitForFixedUpdate();
            /*foreach (HumanFunctionality a in todel)
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
            else */

        }
        
    }
    public void AddHuman(HumanFunctionality human, List<(int,int)>way)
    {
        int nowtime = 0;
        List<(float, float)> newway=new List<(float, float)>();
        for (int i = 1; i < way.Count-1; i++)
        {
            List<HashSet<(float, float)>> tmp = new List<HashSet<(float, float)>>();

            for (int j = 0; j < HumansInProcess.Count; j++)
            {
                HashSet<(float, float)> now = new HashSet<(float, float)>();
                tmp.Add(now);
                if (HumansInProcess[j].ContainsKey(way[i]))
                {
                    foreach ((float, float) a in HumansInProcess[j][way[i]].Keys) now.Add(a);
                }
            }
            newway = grid.Roads[way[i]].GetWayInTheCell(way[i - 1], way[i + 1], tmp, nowtime);
            //Debug.Log("HERE");
            //nowtime += newway.Count;
            for (int j = 0; j < newway.Count; j++)
            {
                if (HumansInProcess.Count>j+nowtime&&!HumansInProcess[j+nowtime].ContainsKey(way[i]))
                {
                    Dictionary<(float, float), HumanFunctionality> dict = new Dictionary<(float, float), HumanFunctionality>();
                    HumansInProcess[j + nowtime].Add(way[i], dict);
                    dict.Add(newway[j], human);
                }
                else
                {
                    while (HumansInProcess.Count!=j+nowtime&&HumansInProcess[j+nowtime].ContainsKey(way[i])&&HumansInProcess[j + nowtime][way[i]].ContainsKey(newway[j])) nowtime++;
                    if (HumansInProcess.Count == j + nowtime)
                    {
                        HumansInProcess.Add(new Dictionary<(int, int), Dictionary<(float, float), HumanFunctionality>>());
                        
                    }
                    if (!HumansInProcess[j + nowtime].ContainsKey(way[i]))
                    {
                        HumansInProcess[HumansInProcess.Count - 1].Add(way[i], new Dictionary<(float, float), HumanFunctionality>());
                    }
                    Dictionary<(float, float), HumanFunctionality> dict = HumansInProcess[j + nowtime][way[i]];
                    dict.Add(newway[j], human);
                }

            }
            nowtime += newway.Count;
        }
        whenDelete.Add(human, (way[way.Count - 2], newway[newway.Count - 1]));
    }
    
}
