using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using System.Collections;

public class HexUnit : MonoBehaviour
{ 

    const float travelSpeed = 3f;
    const float rotationSpeed = 200f;
    public bool isTravel;
    HexCell location;
    float orientation;
    public static HexUnit unitPrefab;
    List<HexCell> pathToTravel;

    public HexGrid Grid { get; set; }

    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            if (location)
            {
                Grid.DecreaseVisibility(location, VisionRange);
                location.Unit = null;
            }
            location = value;
            value.Unit = this;
            Grid.IncreaseVisibility(value, VisionRange);
            transform.localPosition = value.Position;
        }
    }

    public int VisionRange
    {
        get
        {
            return 3;
        }
    }

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    public int Speed
    {
        get
        {
            return 24;
        }
    }

    public void Travel(List<HexCell> path)
    {
        // Location = path[path.Count - 1];

        location.Unit = null;
        location = path[path.Count - 1];
        location.Unit = this;
        pathToTravel = path;
        StopAllCoroutines();
        isTravel = true;
        StartCoroutine(TravelPath());
    }

    IEnumerator TravelPath()
    {
        transform.localPosition = pathToTravel[0].Position;
        yield return LookAt(pathToTravel[1].Position);
        Grid.DecreaseVisibility(pathToTravel[0], VisionRange);
        for (int i = 1; i < pathToTravel.Count; i++)
        {
            Vector3 a = pathToTravel[i - 1].Position;
            Vector3 b = pathToTravel[i].Position;
            Grid.IncreaseVisibility(pathToTravel[i], VisionRange);
            for (float t = 0f; t < 1f; t += Time.deltaTime* travelSpeed)
            {
                transform.localPosition = Vector3.Lerp(a, b, t);
                Vector3 d = GetDerivative(a, b, t);
                d.y = 0f;
                transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }
            Grid.DecreaseVisibility(pathToTravel[i], VisionRange);
        }
        Grid.IncreaseVisibility(location, VisionRange);
        transform.localPosition = location.Position;
        orientation = transform.localRotation.eulerAngles.y;

        isTravel = false;
        //ListPool<HexCell>.Free(pathToTravel);
       // pathToTravel = null;
    }

    IEnumerator LookAt(Vector3 point)
    {
        point.y = transform.localPosition.y;
        Quaternion fromRotation = transform.localRotation;
        Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
        float angle = Quaternion.Angle(fromRotation, toRotation);
        if (angle > 0f)
        {
            float speed = rotationSpeed / angle;

            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
            {
                transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }
        }
        transform.LookAt(point);
        orientation = transform.localRotation.eulerAngles.y;
    }

    public int GetMoveCost(HexCell fromCell, HexCell toCell, HexDirection direction)
    {
        HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
        if (edgeType == HexEdgeType.Cliff)
        {
            return -1;
        }
        int moveCost;
        if (fromCell.HasRoadThroughEdge(direction))
        {
            moveCost = 1;
        }
        else if (fromCell.Walled != toCell.Walled)
        {
            return -1;
        }
        else
        {
            moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
            moveCost +=
                toCell.UrbanLevel + toCell.PlantLevel;
        }
        return moveCost;
    }

    public Vector3 GetDerivative(Vector3 a, Vector3 b, float t)
    {
        return 2f * ((1f - t) * (b - a));
    }

    /*void OnDrawGizmos()
    {
        if (pathToTravel == null || pathToTravel.Count == 0) return;

        for (int i = 1; i < pathToTravel.Count; i++)
        {
            Vector3 a = pathToTravel[i - 1].Position;
            Vector3 b = pathToTravel[i].Position;
            for (float t = 0f; t < 1f; t += 0.1f)
            {
                Gizmos.DrawSphere(Vector3.Lerp(a, b, t), 2f);
            }
        }
    }*/

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void Die()
    {
        if(location) Grid.DecreaseVisibility(location, VisionRange);

        location.Unit = null;
        Destroy(gameObject);
    }

    public void Save(BinaryWriter writer)
    {
        location.coordinates.Save(writer);
        writer.Write(orientation);
    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();
        grid.AddUnit(Instantiate(unitPrefab), grid.GetCell(coordinates), orientation);
    }


    public bool IsValidDestination(HexCell cell)
    {
        return cell.IsExplored && !cell.IsUnderWater && !cell.Unit;
    }

    void OnEnable()
    {
        if(location) transform.localPosition = location.Position;
    }
}
