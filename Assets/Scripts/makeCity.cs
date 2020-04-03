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

class State
{
    public Vector3 pos;
    public Quaternion angle;
}

public class makeCity : MonoBehaviour
{
    //public int size = 100;
    //public int population = 100;
    //public GameObject[] buildings;
    public GameObject road;
    public GameObject roadConnector;
    public GameObject secRoad;
    public int mapWidth = 20;
    public int mapHeight = 20;
    int gap = 100;
    List<Coords> nodes = new List<Coords>();
    List<float> distances = new List<float>();

    void generateSecondaryRoads(GameObject road, string pattern, int iterations)
    {
        string axiom = "F";
        string oldSequence;
        Dictionary<char, string> rules = new Dictionary<char, string>();
        rules.Add('F', pattern);
        //'+' - 90 kraadi paremale
        //'-' - 90 kraadi vasakule
        //'[' - salvesta asukoht ja nurk
        //']' - mine tagasi salvestatud kohta
        oldSequence = axiom;
        Stack<State> stateStack = new Stack<State>();

        for(int x = 0; x < iterations; x++)
        {
            string newSequence = "";
            char[] sequence = oldSequence.ToCharArray();
            for (int i = 0; i < sequence.Length; i++)
            {
                char variable = sequence[i];
                if (rules.ContainsKey(variable))
                {
                    newSequence += rules[variable];
                }
                else
                {
                    newSequence += variable.ToString();
                }
            }
            oldSequence = newSequence;
            //Debug.Log(oldSequence);

            sequence = oldSequence.ToCharArray();

            List<Vector3> locations = new List<Vector3>();

            for (int i = 0; i < sequence.Length; i++)
            {
                char variable = sequence[i];

                if (variable == 'F')
                {
                    Vector3 startPos = transform.position;
                    Quaternion changeAxis = Quaternion.Euler(0f, 90f, 0f);
                    transform.Translate(Vector3.forward * 50);
                    Vector3 centerPos = new Vector3(startPos.x + transform.position.x, 0, startPos.z + transform.position.z) / 2;
                    if (!(locations.Contains(centerPos)))
                    {
                        locations.Add(centerPos);
                        GameObject newRoad = Instantiate(road, centerPos, transform.rotation * changeAxis);
                        newRoad.transform.localScale = new Vector3(50, road.transform.localScale.y, road.transform.localScale.z);
                    }
                    //Debug.DrawLine(startPos,transform.position, Color.white, 10000f, false);
                }
                else if (variable == '+')
                {
                    transform.Rotate(0, 90, 0, Space.Self);
                }
                else if (variable == '-')
                {
                    transform.Rotate(0, -90, 0, Space.Self);
                }
                else if (variable == '[')
                {
                    State last = new State();
                    last.pos = transform.position;
                    last.angle = transform.rotation;
                    stateStack.Push(last);
                }
                else if (variable == ']')
                {
                    State last = stateStack.Pop();
                    transform.position = last.pos;
                    transform.rotation = last.angle;
                }
            }
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
        transform.position = new Vector3(mapWidth * gap / (-2), 0, mapHeight * gap / 2);
        transform.Rotate(0, 180, 0, Space.Self);
        generateSecondaryRoads(secRoad, "F[-F][+F]", 3);
    }

}
