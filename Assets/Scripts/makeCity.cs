using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Coords
{
    public int X;
    public int Y;
    public Coords(int x, int y)
    {
        X = x;
        Y = y;
    }

}
public class makeCity : MonoBehaviour
{
    //public GameObject[] buildings;
    public GameObject road;
    public GameObject roadConnector;
    //public GameObject sideRoad;
    public int mapWidth = 20;
    public int mapHeight = 20;
    int gap = 100;
    List<Coords> nodes = new List<Coords>();
    List<float> distances = new List<float>();

    void generateSecondaryRoads()
    {
        string axiom = "A";
        //string axiom = "F";
        string oldSequence;
        Dictionary<char, string> rules = new Dictionary<char, string>();
        rules.Add('A',"AB");
        rules.Add('B',"A");
        //rules.Add('F',"F+F+F++");
        //'+' - 90 kraadi paremale
        oldSequence = axiom;

        for(int x = 0; x < 3; x++)
        {
            string newSequence = "";
            char[] sequence = oldSequence.ToCharArray();
            for (int i = 0; i < sequence.Length; i++)
            {
                char variable = sequence[i];
                newSequence += rules[variable];
            }
            oldSequence = newSequence;
            Debug.Log(oldSequence);
        }
    }

    void Start()
    {
        // loob lõimed juhusliku seediga Perlini müra heledates kohtades
        float seed = Random.Range(0,100);
        for(int h = mapHeight/(-2); h < mapHeight/2; h++)
        {
            for(int w = mapWidth/(-2); w < mapWidth/2; w++)
            {
                int n = (int)(Mathf.PerlinNoise(w/2.5f + seed, h/2.5f + seed) * 10);
                Vector3 pos = new Vector3(w * gap, 0, h * gap);

                if (n > 7)
                {
                    nodes.Add(new Coords(w,h));
                    Instantiate(roadConnector, pos, Quaternion.identity);
                }
            }
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            distances.Add(0);
            Vector3 posRoad = new Vector3(0,0,0);
            Vector3 pos = new Vector3(nodes[i].X * gap, 0, nodes[i].Y * gap);
            for (int j = 0; j < nodes.Count; j++)
            {
                float dist = Mathf.Sqrt(Mathf.Pow((nodes[j].X * gap - nodes[i].X * gap),2) + Mathf.Pow((nodes[j].Y * gap - nodes[i].Y * gap),2));
                if ((dist < distances[i]) || (distances[i] == 0))
                {
                    distances[i] = dist;
                    posRoad.x = (nodes[i].X + nodes[j].X) * gap / 2;
                    posRoad.z = (nodes[i].Y + nodes[j].Y) * gap / 2;
                }
            }
            Vector3 direction = pos - posRoad;
            if (distances[i] != 0)
            {
                Quaternion changeAxis = Quaternion.Euler(0f, 90f, 0f);
                Quaternion faceTarget = Quaternion.LookRotation(direction);

                GameObject newRoad = Instantiate(road, posRoad, faceTarget * changeAxis);
                newRoad.transform.localScale = new Vector3(distances[i], road.transform.localScale.y, road.transform.localScale.z);
            }
        }
        generateSecondaryRoads();
    }

}
