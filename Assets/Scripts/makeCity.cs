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
    //public int population = 100;
    public GameObject[] buildings;
    public GameObject road;
    public GameObject roadConnector;
    public GameObject secRoad;
    public int mapWidth = 20;
    public int mapHeight = 20;
    int gap = 100;
    int buildingGap = 10;
    List<Coords> nodes = new List<Coords>();
    List<float> distances = new List<float>();

    List<Vector3> generateSecondaryRoads(GameObject road, string pattern, int iterations, int minStreetLength, int maxStreetLength, int[] streetAngles, int limitWidth, int limitHeight)
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
        int limit = 0;
        int limitX = 0;

        //for (int x = 0; x < iterations; x++)
        while (limit == 0)
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
                    if (centerPos.z <= (limitHeight / (-2)))
                    {
                        limit = 1;
                        break;
                    }
                    if (centerPos.x <= (limitWidth / (-2)) || centerPos.x >= (limitWidth / 2))
                    {
                        limitX = 1;
                    }
                    else
                    {
                        GameObject newRoad = Instantiate(road, centerPos, transform.rotation * changeAxis);
                        newRoad.transform.localScale = new Vector3(randLength, road.transform.localScale.y, road.transform.localScale.z);
                    }
                }
                else if (variable == '+')
                {
                    int randNum = Random.Range(0, 3);
                    float randAng = streetAngles[randNum];
                    transform.Rotate(0, 90 + randAng, 0, Space.Self);
                }
                else if (variable == '-')
                {
                    int randNum = Random.Range(0, 3);
                    float randAng = streetAngles[randNum];
                    transform.Rotate(0, -(90+randAng), 0, Space.Self);
                }
                else if (variable == '.')
                {
                    if (limitX == 0)
                    {
                        int randNum = Random.Range(0, 3);
                        float randAng = streetAngles[randNum];
                        transform.Rotate(0, randAng, 0, Space.Self);
                    }
                    else
                    {
                        transform.Rotate(0, 0, 0, Space.Self);
                    }
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

    void connectStems(List<Vector3> stems, GameObject road, int iterations, int minStreetLength, int maxStreetLength, int[] streetAngles, int limitWidth, int limitHeight)
    {
        int limitX = 0;
        int limitY = 0;
        //for (int j = 0; j < iterations; j++)
        while (limitX != 2 && limitY == 0)
        {
            List<Vector3> newStems = new List<Vector3>();
            for (int i = 0; i < stems.Count; i++)
            {
                GameObject extension1;
                GameObject newRoad;
                // pikendab tänavaid ülespoole
                transform.position = stems[i];
                Vector3 startPos = transform.position;
                Quaternion changeAxis = Quaternion.Euler(0f, 90f, 0f);
                if (i == 0 || i == 1)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    int randNum1 = Random.Range(0, 3);
                    float randAng1 = streetAngles[randNum1];
                    transform.Rotate(0, randAng1, 0, Space.Self);
                    float randLength1 = Random.Range(minStreetLength, maxStreetLength);
                    transform.Translate(Vector3.forward * randLength1);
                    Vector3 centerPos1 = new Vector3(startPos.x + transform.position.x, 0, startPos.z + transform.position.z) / 2;
                    if (centerPos1.x <= (limitWidth / (-2)) || centerPos1.x >= (limitWidth / 2)) {}
                    else
                    {
                        extension1 = Instantiate(road, centerPos1, transform.rotation * changeAxis);
                        extension1.transform.localScale = new Vector3(randLength1, road.transform.localScale.y, road.transform.localScale.z);
                    }
                }
                // ühendab tänavaid allapoole
                Vector3 posRoad = new Vector3(0, 0, 0);
                if ((i + 3) <= stems.Count)
                {
                    posRoad.x = (stems[i].x + stems[i + 2].x) / 2;
                    posRoad.z = (stems[i].z + stems[i + 2].z) / 2;
                    if (posRoad.z <= (limitHeight / (-2)))
                    {
                        limitY = 1;
                    }
                    else
                    {
                        float dist = Mathf.Sqrt(Mathf.Pow((stems[i + 2].x - stems[i].x), 2) + Mathf.Pow((stems[i + 2].z - stems[i].z), 2));
                        Quaternion faceTarget = Quaternion.LookRotation(stems[i] - stems[i + 2]);
                        if ((dist <= 100) || (posRoad.x <= (limitWidth / (-2)) || posRoad.x >= (limitWidth / 2))) {}
                        else
                        {
                            newRoad = Instantiate(road, posRoad, faceTarget * changeAxis);
                            newRoad.transform.localScale = new Vector3(dist, road.transform.localScale.y, road.transform.localScale.z);
                        }
                    }
                }
                // pikendab tänavat kõrvale
                transform.position = stems[i];
                if (i % 2 == 0)
                {
                    transform.rotation = Quaternion.Euler(0, 90, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, -90, 0);
                }
                int randNum = Random.Range(0, 3);
                float randAng = streetAngles[randNum];
                transform.Rotate(0, randAng, 0, Space.Self);
                float randLength = Random.Range(minStreetLength, maxStreetLength);
                transform.Translate(Vector3.forward * randLength);
                Vector3 centerPos = new Vector3(startPos.x + transform.position.x, 0, startPos.z + transform.position.z) / 2;
                if (centerPos.x <= (limitWidth / (-2)) || centerPos.x >= (limitWidth / 2))
                {
                    limitX++;
                    newStems.Add(centerPos);
                }
                else
                {
                    GameObject extension = Instantiate(road, centerPos, transform.rotation * changeAxis);
                    extension.transform.localScale = new Vector3(randLength, road.transform.localScale.y, road.transform.localScale.z);
                    newStems.Add(transform.position);
                    limitX = 0;
                }
            }
            stems.Clear();
            stems = newStems;
        }
    }

    void makeBuildings(GameObject[] roadArray, GameObject[] buildings, int buildingGap, float seedNum, int gap)
    {
        for (int i = 0; i < roadArray.Length; i++)
        {
            // vaja luua hooned mööda tervet teed/tänavat
            int iterations = (int)((roadArray[i].transform.localScale.x - buildingGap) / (buildings[0].transform.localScale.x + buildingGap));
            transform.position = roadArray[i].transform.position;
            transform.rotation = roadArray[i].transform.rotation;
            transform.Translate(Vector3.back * (roadArray[i].transform.localScale.z + 15));
            transform.Translate(Vector3.left * (roadArray[i].transform.localScale.x / 2 - buildings[0].transform.localScale.x));
            for (int j = 0; j < iterations; j++)
            {
                // ühel pool teed
                float adjustmentY = buildings[0].transform.localPosition.y;
                Vector3 adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                // kontrolli vaja, kas satub tee peale
                if (!Physics.CheckBox(adjusted, buildings[0].transform.localScale / 2f, transform.rotation))
                {
                    int w = (int)adjusted.x / gap;
                    int h = (int)adjusted.z / gap;
                    if (w < 0)
                        w = w * (-1);
                    if (h < 0)
                        h = h * (-1);
                    int n = (int)(Mathf.PerlinNoise(w / 5f + seedNum, h / 5f + seedNum) * 10);
                    if (n < 2)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    else if (n < 4)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    else if (n < 6)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    else if (n < 8)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    else if (n < 10)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    Instantiate(buildings[0], adjusted, transform.rotation);
                }

                transform.Rotate(0, 180, 0, Space.Self);
                transform.Translate(Vector3.back * ((roadArray[i].transform.localScale.z + 15) * 2));

                // teisel pool teed
                adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                // kontrolli vaja, kas satub tee peale
                if (!Physics.CheckBox(adjusted, buildings[0].transform.localScale / 2f, transform.rotation))
                {
                    int w = (int)adjusted.x / gap;
                    int h = (int)adjusted.z / gap;
                    if (w < 0)
                        w = w * (-1);
                    if (h < 0)
                        h = h * (-1);
                    int n = (int)(Mathf.PerlinNoise(w / 5f + seedNum, h / 5f + seedNum) * 10);
                    if (n < 2)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    else if (n < 4)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    else if (n < 6)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    else if (n < 8)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    else if (n < 10)
                        adjusted = new Vector3(transform.position.x, adjustmentY, transform.position.z);
                    Instantiate(buildings[0], adjusted, transform.rotation);
                }

                transform.Rotate(0, 180, 0, Space.Self);
                transform.Translate(Vector3.back * ((roadArray[i].transform.localScale.z + 15) * 2));
                transform.Translate(Vector3.right * (buildings[0].transform.localScale.x + buildingGap));
            }
        }
    }

    void Start()
    {
        // loob lõimed juhusliku seediga Perlini müra heledates kohtades
        float seed = Random.Range(0, 100);
        for (int h = mapHeight / (-2); h < mapHeight / 2; h++)
        {
            for (int w = mapWidth / (-2); w < mapWidth / 2; w++)
            {
                int n = (int)(Mathf.PerlinNoise(w / 2.5f + seed, h / 2.5f + seed) * 10);
                Vector3 pos = new Vector3(w * gap, 0, h * gap);

                if (n > 7)
                {
                    nodes.Add(new Coords(w, h));
                    Instantiate(roadConnector, pos, Quaternion.identity);
                }
            }
        }

        // peateed
        for (int i = 0; i < nodes.Count; i++)
        {
            distances.Add(0);
            Vector3 posRoad = new Vector3(0, 0, 0);
            Vector3 pos = new Vector3(nodes[i].X * gap, 0, nodes[i].Y * gap);
            for (int j = 0; j < nodes.Count; j++)
            {
                float dist = Mathf.Sqrt(Mathf.Pow((nodes[j].X * gap - nodes[i].X * gap), 2) + Mathf.Pow((nodes[j].Y * gap - nodes[i].Y * gap), 2));
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

        // secondary roads
        transform.position = new Vector3(0, 0, mapHeight * gap / 2);
        transform.Rotate(0, 180, 0, Space.Self);
        int[] streetAngles = { -10, 0, 10 };
        List<Vector3> stems = generateSecondaryRoads(secRoad, ".F[-F][+F]X", 5, 140, 160, streetAngles, mapWidth * gap, mapHeight * gap);
        // iga tipu otsast pikendab linna
        connectStems(stems, secRoad, 10, 140, 160, streetAngles, mapWidth * gap, mapHeight * gap);

        // hooned
        GameObject[] createdRoads = GameObject.FindGameObjectsWithTag("Road");
        makeBuildings(createdRoads, buildings, buildingGap, seed, gap);
    }
}
