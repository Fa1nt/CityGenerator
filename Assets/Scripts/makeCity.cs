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
    public GameObject[] buildings;
    public GameObject road;
    public GameObject roadConnector;
    public GameObject secRoad;
    public int mapWidth = 20;
    public int mapHeight = 20;
    int gap = 100;
    List<Coords> nodes = new List<Coords>();
    List<float> distances = new List<float>();

    List<Vector3> generateSecondaryRoads(GameObject road, string pattern, int iterations, int minStreetLength, int maxStreetLength)
    {
        string axiom = "X";
        string oldSequence;
        Dictionary<char, string> rules = new Dictionary<char, string>();
        rules.Add('X', pattern);
        //'+' - 90 kraadi paremale
        //'-' - 90 kraadi vasakule
        //'[' - salvesta asukoht ja nurk
        //']' - mine tagasi salvestatud kohta
        //'.' - muudab nurka
        oldSequence = axiom;
        Stack<State> stateStack = new Stack<State>();
        List<Vector3> stems = new List<Vector3>();

        for (int x = 0; x < iterations; x++)
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

            for (int i = 0; i < sequence.Length; i++)
            {
                char variable = sequence[i];

                if (variable == 'F')
                {
                    float randLength = Random.Range(minStreetLength, maxStreetLength);
                    Vector3 startPos = transform.position;
                    Quaternion changeAxis = Quaternion.Euler(0f, 90f, 0f);
                    transform.Translate(Vector3.forward * randLength);
                    Vector3 centerPos = new Vector3(startPos.x + transform.position.x, 0, startPos.z + transform.position.z) / 2;
                    GameObject newRoad = Instantiate(road, centerPos, transform.rotation * changeAxis);
                    newRoad.transform.localScale = new Vector3(randLength, road.transform.localScale.y, road.transform.localScale.z);
                }
                else if (variable == '+')
                {
                    float randAng = Random.Range(80, 100);
                    transform.Rotate(0, randAng, 0, Space.Self);
                }
                else if (variable == '-')
                {
                    float randAng = Random.Range(80, 100);
                    transform.Rotate(0, -randAng, 0, Space.Self);
                }
                else if (variable == '.')
                {
                    float randAng = Random.Range(-10, 10);
                    transform.Rotate(0, randAng, 0, Space.Self);
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
                    stems.Add(transform.position);
                    State last = stateStack.Pop();
                    transform.position = last.pos;
                    transform.rotation = last.angle;
                }
            }
        }
        return stems;
    }

    void connectStems(List<Vector3> stems, GameObject road, int iterations, int minStreetLength, int maxStreetLength)
    {
        for (int j = 0; j < iterations; j++)
        {
            List<Vector3> newStems = new List<Vector3>();
            for (int i = 0; i < stems.Count; i++)
            {
                // pikendab tänavat
                transform.position = stems[i];
                if (i % 2 == 0)
                {
                    transform.rotation = Quaternion.Euler(0, 90, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, -90, 0);
                }
                float randAng = Random.Range(-10, 10);
                transform.Rotate(0, randAng, 0, Space.Self);
                float randLength = Random.Range(minStreetLength, maxStreetLength);
                Vector3 startPos = transform.position;
                Quaternion changeAxis = Quaternion.Euler(0f, 90f, 0f);
                transform.Translate(Vector3.forward * randLength);
                Vector3 centerPos = new Vector3(startPos.x + transform.position.x, 0, startPos.z + transform.position.z) / 2;
                GameObject extension = Instantiate(road, centerPos, transform.rotation * changeAxis);
                extension.transform.localScale = new Vector3(randLength, road.transform.localScale.y, road.transform.localScale.z);
                newStems.Add(transform.position);
                // ühendab tänavaid
                Vector3 posRoad = new Vector3(0, 0, 0);
                if ((i + 3) <= stems.Count)
                {
                    posRoad.x = (stems[i].x + stems[i + 2].x) / 2;
                    posRoad.z = (stems[i].z + stems[i + 2].z) / 2;
                    float dist = Mathf.Sqrt(Mathf.Pow((stems[i + 2].x - stems[i].x), 2) + Mathf.Pow((stems[i + 2].z - stems[i].z), 2));
                    Quaternion faceTarget = Quaternion.LookRotation(stems[i] - stems[i + 2]);
                    GameObject newRoad = Instantiate(road, posRoad, faceTarget * changeAxis);
                    newRoad.transform.localScale = new Vector3(dist, road.transform.localScale.y, road.transform.localScale.z);
                }
            }
            stems.Clear();
            stems = newStems;
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
        transform.position = new Vector3(0, 0, mapHeight * gap / 2);
        transform.Rotate(0, 180, 0, Space.Self);
        List<Vector3> stems = generateSecondaryRoads(secRoad, ".F[-F][+F]X", 5, 140, 160);
        // iga tipu otsast pikendada linna
        connectStems(stems, secRoad, 7, 140, 160);
    }
}
