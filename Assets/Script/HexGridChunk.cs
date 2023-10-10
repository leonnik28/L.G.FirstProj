
using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour
{

    HexCell[] cells;

    public HexMesh terrain, rivers, roads, water, estuaries, line;

    public HexFeatureManager features;
    // HexMesh hexMesh;
    Canvas gridCanvas;

    static Color weights1 = new Color(1f, 0f, 0f);
    static Color weights2 = new Color(0f, 1f, 0f);
    static Color weights3 = new Color(0f, 0f, 1f);

    void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        //  hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        //ShowUI(false);
    }

    public void ShowUI(bool visible)
    {
        gridCanvas.gameObject.SetActive(visible);
    }

    public void AddCell(int index, HexCell cell)
    {
        cells[index] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
    }


    public void Refresh()
    {
        enabled = true;
    }

    void LateUpdate()
    {
        Triangulate();
        //		hexMesh.Triangulate(cells);
        enabled = false;
    }

    public void Triangulate()
    {

        terrain.Clear();
        rivers.Clear();
        roads.Clear();
        water.Clear();
        estuaries.Clear();
        features.Clear();
        line.Clear();
        //		hexMesh.Clear();
        //		vertices.Clear();
        //		colors.Clear();
        //		triangles.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        terrain.Apply();
        rivers.Apply();
        roads.Apply();
        water.Apply();
        estuaries.Apply();
        features.Apply();
        line.Apply();
        //		hexMesh.vertices = vertices.ToArray();
        //		hexMesh.colors = colors.ToArray();
        //		hexMesh.triangles = triangles.ToArray();
        //		hexMesh.RecalculateNormals();
        //		meshCollider.sharedMesh = hexMesh;
    }

    void TriangulateWater(HexDirection direction, HexCell cell, Vector3 center)
    {
        center.y = cell.WaterSurfaceY;

        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null || !neighbor.IsUnderWater)
        {
            TriangulateShoreWater(direction, cell, neighbor, center);
        }
        else
        {
            TriangulateOpenWater(direction, cell, neighbor, center);
        }
    }

    void TriangulateOpenWater(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center)
    {
        Vector3 c1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 c2 = center + HexMetrics.GetSecondSolidCorner(direction);

        water.AddTriangle(center, c1, c2);
        Vector3 indices;
        indices.x = indices.y = indices.z = cell.Index;
        water.AddTriangleCellData(indices, weights1);

        if (direction <= HexDirection.SE)
        {
           // HexCell neighbor = cell.GetNeighbor(direction);
           // if (neighbor == null || !neighbor.IsUnderWater) return;

            Vector3 bridge = HexMetrics.GetBridge(direction);
            Vector3 e1 = c1 + bridge;
            Vector3 e2 = c2 + bridge;

            water.AddQuad(c1, c2, e1, e2);
            indices.y = neighbor.Index;
            water.AddQuadCellData(indices, weights1, weights2);
            if (direction <= HexDirection.E)
            {
                HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
                if (nextNeighbor == null || !nextNeighbor.IsUnderWater) return;

                water.AddTriangle(c2, e2, c2 + HexMetrics.GetBridge(direction.Next()));
                indices.z = nextNeighbor.Index;
                water.AddTriangleCellData(indices, weights1, weights2, weights3);
            }
        }
    }

    void TriangulateShoreWater(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center)
    {
        EdgeVertices e1 = new EdgeVertices(center + HexMetrics.GetFirstSolidCorner(direction), center + HexMetrics.GetSecondSolidCorner(direction));

        water.AddTriangle(center, e1.v1, e1.v2);
        water.AddTriangle(center, e1.v2, e1.v3);
        water.AddTriangle(center, e1.v3, e1.v4);
        water.AddTriangle(center, e1.v4, e1.v5);
        Vector3 indices;
        indices.x = indices.y = indices.z = cell.Index;
        //indices.y = neighbor.Index;
        water.AddTriangleCellData(indices, weights1);
        water.AddTriangleCellData(indices, weights1);
        water.AddTriangleCellData(indices, weights1);
        water.AddTriangleCellData(indices, weights1);

        Vector3 bridge = HexMetrics.GetBridge(direction);
        EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

        if (cell.HasRiverThroughEdge(direction))
        {
            TriangulateEstuary(e1, e2, cell.IncomingRiver == direction, indices);
        }

        water.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);          
        water.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);  
        water.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);         
        water.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
        water.AddQuadCellData(indices, weights1, weights2);
        water.AddQuadCellData(indices, weights1, weights2);
        water.AddQuadCellData(indices, weights1, weights2);
        water.AddQuadCellData(indices, weights1, weights2);

        HexCell newNeighbor = cell.GetNeighbor(direction.Next());
        if(newNeighbor != null)
        {
            water.AddTriangle(e1.v5, e2.v5, e1.v5 + HexMetrics.GetBridge(direction.Next()));
            water.AddTriangleCellData(indices, weights1, weights2, weights3);
        }
    }

    void TriangulateEstuary(EdgeVertices e1, EdgeVertices e2, bool incomingRiver, Vector3 indices)
    {
        water.AddQuad(e2.v1, e1.v2, e2.v2, e1.v3);
        water.AddTriangle(e1.v3, e2.v2, e2.v4);
        water.AddQuad(e1.v3, e1.v4, e2.v4, e2.v5);

        water.AddQuadCellData(indices, weights2, weights1, weights2, weights1);
        water.AddTriangleCellData(indices, weights1, weights2, weights2);
        water.AddQuadCellData(indices, weights1, weights2);
    }

    void TriangulateWaterfallInWater(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float waterY, Vector3 indices)
    {
        v1.y = v2.y = y1;
        v3.y = v4.y = y2;
        v1 = HexMetrics.Perturb(v1);
        v2 = HexMetrics.Perturb(v2);
        v3 = HexMetrics.Perturb(v3);
        v4 = HexMetrics.Perturb(v4);
        float t = (waterY - y2) / (y1 - y2);
        v3 = Vector3.Lerp(v3, v1, t);
        v4 = Vector3.Lerp(v4, v2, t);
        rivers.AddQuadUnperturbed(v1, v2, v3, v4);
        rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
        rivers.AddQuadCellData(indices, weights1, weights2);
    }

    void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y, float v, bool reversed, Vector3 indices)
    {
        TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, reversed, indices);
    }

    void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float v, bool reverse, Vector3 indices)
    {
        v1.y = v2.y = y1;
        v3.y = v4.y = y2;
        rivers.AddQuad(v1, v2, v3, v4);

        if (reverse)
        {
            rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
        }
        else
        {
            rivers.AddQuadUV(0f, 1f, v, v + 0.2f);
        }
        rivers.AddQuadCellData(indices, weights1, weights2);
    }

    void TriangulateRoadSegment(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Color w1, Color w2, Vector3 indices)
    {
        roads.AddQuad(v1, v2, v4, v5);
        roads.AddQuad(v2, v3, v5, v6);
        roads.AddQuadUV(0f, 1f, 0f, 0f);
        roads.AddQuadUV(1f, 0f, 0f, 0f);
        roads.AddQuadCellData(indices, w1, w2);
        roads.AddQuadCellData(indices, w1, w2);
    }

    void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float index)
    {
        terrain.AddTriangle(center, edge.v1, edge.v2);
        //		terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v2, edge.v3);
        //		terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v3, edge.v4);
        //		terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v4, edge.v5);
        //		terrain.AddTriangleColor(color);

        Vector3 indices;
        indices.x = indices.y = indices.z = index;

        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);
    }

    void TriangulateEdgeStrip(EdgeVertices e1, Color w1, EdgeVertices e2, Color w2, float index1, float index2, bool hasRoad = false)
    {
        terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

        Vector3 indices;
        indices.x = indices.z = index1;
        indices.y = index2;

        terrain.AddQuadCellData(indices, w1, w2);
        terrain.AddQuadCellData(indices, w1, w2);
        terrain.AddQuadCellData(indices, w1, w2);
        terrain.AddQuadCellData(indices, w1, w2);

        if (hasRoad)
        {
            TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4, w1, w2, indices);
        }
    }

    void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell, bool hasRoad)
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexMetrics.TerraceLerp(weights1, weights2, 1);
        float i1 = beginCell.TerrainTypeIndex;
        float i2 = endCell.TerrainTypeIndex;

        TriangulateEdgeStrip(begin, weights1, e2, c2, i1, i2, hasRoad);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(weights1, weights2, i);
            TriangulateEdgeStrip(e1, c1, e2, c2, i1, i2, hasRoad);
        }

        TriangulateEdgeStrip(e2, c2, end, weights2, i1, i2, hasRoad);
    }

    void AddLineBetweenCell(Vector3 nearLeft, Vector3 farLeft, Vector3 nearRight, Vector3 farRight)
    {
        Vector3 left = Vector3.Lerp(nearLeft, farLeft, 0.5f);
        Vector3 left1 = Vector3.Lerp(nearLeft, farLeft, 0.4f);
        Vector3 right1 = Vector3.Lerp(nearRight, farRight, 0.4f);
        Vector3 left2 = Vector3.Lerp(nearLeft, farLeft, 0.6f);
        Vector3 right2 = Vector3.Lerp(nearRight, farRight, 0.6f);

        left1.y = left2.y = right1.y = right2.y = left.y + 0.3f;

        line.AddQuadDefaultVersion(left1, right1, left2, right2);

    }

    void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
        {
            return;
        }

        Vector3 bridge = HexMetrics.GetBridge(direction);
        bridge.y = neighbor.Position.y - cell.Position.y;
        EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

        //AddLineBetweenCell(e1.v1, e2.v1, e1.v5, e2.v5);

        if (cell.HasRiverThroughEdge(direction))
        {
            e2.v3.y = neighbor.StreamBedY;
        }

        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(e1, cell, e2, neighbor, cell.HasRoadThroughEdge(direction));
        }

        else
        {
            TriangulateEdgeStrip(e1, weights1, e2, weights2, cell.Index, neighbor.Index, cell.HasRoadThroughEdge(direction));
        }
        bool hasRiver = cell.HasRiverThroughEdge(direction);
        bool hasRoad = cell.HasRoadThroughEdge(direction);
        features.AddWall(e1, cell, e2, neighbor, hasRiver, hasRoad);

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null)
        {
            Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Position.y;

            if (cell.Elevation <= neighbor.Elevation)
            {
                if (cell.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
                TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
            }
        }

        if (cell.HasRiverThroughEdge(direction))
        {
            e2.v3.y = neighbor.StreamBedY;
            Vector3 indices;
            indices.x = indices.z = cell.Index;
            indices.y = neighbor.Index;
            if (!cell.IsUnderWater)
            {
                if (!neighbor.IsUnderWater)
                {
                    TriangulateRiverQuad(
                        e1.v2, e1.v4, e2.v2, e2.v4,
                        cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                        cell.HasIncomingRiver && cell.IncomingRiver == direction, indices
                    );
                }
                else if (cell.Elevation > neighbor.WaterLevel)
                {
                    TriangulateWaterfallInWater(
                        e1.v2, e1.v4, e2.v2, e2.v4,
                        cell.RiverSurfaceY, neighbor.RiverSurfaceY,
                        neighbor.WaterSurfaceY, indices
                    );
                }
            }
            else if (!neighbor.IsUnderWater && neighbor.Elevation > cell.WaterLevel)
            {
                TriangulateWaterfallInWater(
                    e2.v4, e2.v2, e1.v4, e1.v2,
                    neighbor.RiverSurfaceY, cell.RiverSurfaceY,
                    cell.WaterSurfaceY, indices
                );
            }
        }
    }

    void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }

        if (!cell.IsUnderWater && !cell.HasRiver && !cell.HasRoads)
        {
            features.AddFeature(cell, cell.Position);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.Position;
        EdgeVertices e = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
        );

        if (cell.HasRiver)
        {
            if (cell.HasRiverThroughEdge(direction))
            {
                e.v3.y = cell.StreamBedY;
                TriangulateWithRiver(direction, cell, center, e);
            }
            else { TriangulateAdjacentToRiver(direction, cell, center, e); }
        }
        else 
        { 
            TriangulateWithoutRiver(direction, cell, center, e);

            if (!cell.IsUnderWater)
            {
                if (!cell.HasRiver && !cell.HasRoads)
                {
                    features.AddFeature(cell, (center + e.v1 + e.v5) * (1f / 3f));
                }
                if (cell.IsSpecial)
                {
                    features.AddSpecialFeature(cell, cell.Position);
                }
            }
        }

        if (direction <= HexDirection.SE) TriangulateConnection(direction, cell, e);

        if (cell.IsUnderWater) TriangulateWater(direction, cell, center);
    }

    void TriangulateWithoutRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        TriangulateEdgeFan(center, e, cell.Index);

        if (cell.HasRoads)
        {
            Vector2 interpolators = GetRoadInterpolators(direction, cell);
            TriangulateRoad(center, Vector3.Lerp(center, e.v1, interpolators.x), Vector3.Lerp(center, e.v5, interpolators.y), e, cell.HasRoadThroughEdge(direction), cell.Index);
        }
    }

    void TriangulateRoad (Vector3 center, Vector3 mL, Vector3 mR, EdgeVertices e, bool hasRoadThroughCellEdge, float index)
    {
        if (hasRoadThroughCellEdge)
        {
            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            Vector3 mC = Vector3.Lerp(mL, mR, 0.5f);
            TriangulateRoadSegment(mL, mC, mR, e.v2, e.v3, e.v4, weights1, weights1, indices);

            roads.AddTriangle(center, mL, mC);
            roads.AddTriangle(center, mC, mR);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f));
            roads.AddTriangleCellData(indices, weights1);
            roads.AddTriangleCellData(indices, weights1);
        }
        else
        {
            TriangulateRoadEdge(center, mL, mR, index);
        }
    }

    void TriangulateRoadEdge(Vector3 center, Vector3 mL, Vector3 mR, float index)
    {
        roads.AddTriangle(center, mL, mR);
        roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        Vector3 indices;
        indices.x = indices.y = indices.z = index;
        roads.AddTriangleCellData(indices, weights1);
    }

    Vector2 GetRoadInterpolators(HexDirection direction, HexCell cell)
    {
        Vector2 interpolators;
        if(cell.HasRoadThroughEdge(direction))
        {
            interpolators.x = interpolators.y = 0.5f;
        }
        else
        {
            interpolators.x = cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
            interpolators.y = cell.HasRoadThroughEdge(direction.Next()) ? 0.5f : 0.25f;
        }
        return interpolators;
    }

    void TriangulateAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        if (cell.HasRoads)
        {
            TriangulateRoadAdjacentToRiver(direction, cell, center, e);
        }

        if (cell.HasRiverThroughEdge(direction.Next()))
        {
            if (cell.HasRiverThroughEdge(direction.Previous()))
            {
                center += HexMetrics.GetSolidEdgeMiddle(direction) *
                    (HexMetrics.innerToOuter * 0.5f);
            }
            else if (
                cell.HasRiverThroughEdge(direction.Previous2())
            )
            {
                center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
            }
        }
        else if (
            cell.HasRiverThroughEdge(direction.Previous()) &&
            cell.HasRiverThroughEdge(direction.Next2())
        )
        {
            center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
        }
        
        EdgeVertices m = new EdgeVertices(Vector3.Lerp(center, e.v1, 0.5f), Vector3.Lerp(center, e.v5, 0.5f));

        TriangulateEdgeStrip(m, weights1, e, weights1, cell.Index, cell.Index);
        TriangulateEdgeFan(center, m, cell.Index);

        if (!cell.IsUnderWater && !cell.HasRoadThroughEdge(direction))
        {
            features.AddFeature(cell, (center + e.v1 + e.v5) * (1f / 3f));
        }
    }

    void TriangulateRoadAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        bool hasRoadThroughEdge = cell.HasRoadThroughEdge(direction);
        bool previousHasRiver = cell.HasRiverThroughEdge(direction.Previous());
        bool nextHasRiver = cell.HasRiverThroughEdge(direction.Next());
        Vector2 interpolators = GetRoadInterpolators(direction, cell);
        Vector3 roadCenter = center;

        if (cell.HasRiverBeginOrEnd)
        {
            roadCenter += HexMetrics.GetSolidEdgeMiddle(cell.RiverBeginOrEndDirection.Opposite()) * (1f / 3f);
        }
        else if (cell.IncomingRiver == cell.OutgoingRiver.Opposite())
        {
            Vector3 corner;
            if (previousHasRiver)
            {
                if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Next())) return;

                corner = HexMetrics.GetSecondSolidCorner(direction);
            }
            else
            {
                if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Previous())) return;

                corner = HexMetrics.GetFirstSolidCorner(direction);
            }
            roadCenter += corner * 0.25f;
            if(cell.IncomingRiver == direction.Next() && cell.HasRoadThroughEdge(direction.Next2()) || cell.HasRoadThroughEdge(direction.Opposite()))
            {
            features.AddBridge(roadCenter, center - corner * 0.5f);
            }

            center += corner * 0.25f;
        }
        else if(cell.IncomingRiver == cell.OutgoingRiver.Previous())
        {
            roadCenter -= HexMetrics.GetSecondCorner(cell.IncomingRiver) * 0.2f;
        }
        else if(cell.IncomingRiver == cell.OutgoingRiver.Next())
        {
            roadCenter -= HexMetrics.GetFirstCorner(cell.IncomingRiver) * 0.2f;
        }
        else if(previousHasRiver && nextHasRiver)
        {
            if (!hasRoadThroughEdge) return;

            Vector3 offset = HexMetrics.GetSolidEdgeMiddle(direction) * HexMetrics.innerToOuter;
            roadCenter += 0.7f * offset;
            center += 0.5f * offset;
        }
        else
        {
            HexDirection middle;
            if (previousHasRiver) middle = direction.Next();
            else if (nextHasRiver) middle = direction.Previous();
            else middle = direction;

            if(!cell.HasRoadThroughEdge(direction) && !cell.HasRoadThroughEdge(direction.Next()) && !cell.HasRoadThroughEdge(direction.Previous())) return;

            Vector3 offset = HexMetrics.GetSolidEdgeMiddle(middle);
            roadCenter += offset * 0.25f;
            if (direction == middle && cell.HasRoadThroughEdge(direction.Opposite()))
            {
                features.AddBridge(roadCenter, center - offset * (HexMetrics.innerToOuter * 0.7f));
            }
        }

        Vector3 mL = Vector3.Lerp(roadCenter, e.v1, interpolators.x);
        Vector3 mR = Vector3.Lerp(roadCenter, e.v5, interpolators.y);
        TriangulateRoad(roadCenter, mL, mR, e, hasRoadThroughEdge, cell.Index);
        if (previousHasRiver)
        {
            TriangulateRoadEdge(roadCenter, center, mL, cell.Index);
        }
        if (nextHasRiver)
        {
            TriangulateRoadEdge(roadCenter, mR, center, cell.Index);
        }
    }

    void TriangulateWithRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {

        Vector3 centerL, centerR, centerY;
        centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
        centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
        centerY = center;
        /*if (cell.HasRiverThroughEdge(direction.Opposite()))
        {
            centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
            centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
        }   */
        if (cell.HasRiverThroughEdge(direction.Next()))
        {
            centerL = center;
            centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
            AddTriangleRiver(direction, cell, center, centerL, centerR, e);
        }
        else if (cell.HasRiverThroughEdge(direction.Previous()))
        {
            centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
            centerR = center;
            AddTriangleRiver(direction, cell, center, centerL, centerR, e);
        }
        else if (cell.HasRiverThroughEdge(direction.Next2()))
        {
            centerL = center;
            centerR = center +
                HexMetrics.GetSolidEdgeMiddle(direction.Next()) *
                (0.5f * HexMetrics.innerToOuter);

            AddTriangleRiver(direction, cell, center, centerL, centerR, e);
        }
        else if (cell.HasRiverThroughEdge(direction.Previous2()))
        {
            centerL = center +
                HexMetrics.GetSolidEdgeMiddle(direction.Previous()) *
                (0.5f * HexMetrics.innerToOuter);
            centerR = center;

            AddTriangleRiver(direction, cell, center, centerL, centerR, e);
        }
        else if (!cell.HasRiverThroughEdge(direction.Opposite()))
        {
            center.y = e.v3.y;
           // terrain.AddTriangle(centerR, centerL, center);
            //terrain.AddTriangleColor(cell.Color);
            AddTriangleRiverNotOpposite(direction, cell, center, centerL, centerR, centerY, e);
        }
        else
        {
            AddTriangleRiver(direction, cell, center, centerL, centerR, e);
        }
    }

    void AddTriangleRiver(HexDirection direction, HexCell cell, Vector3 center, Vector3 centerL, Vector3 centerR, EdgeVertices e)
    {
        center = Vector3.Lerp(centerL, centerR, 0.5f);
        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(centerL, e.v1, 0.5f),
            Vector3.Lerp(centerR, e.v5, 0.5f),
            1f / 6f
        );
        m.v3.y = center.y = e.v3.y;

        AddTriangleRiverLast(direction, cell, center, centerL, centerR, e, m);

    }

    void AddTriangleRiverNotOpposite(HexDirection direction, HexCell cell, Vector3 center, Vector3 centerL, Vector3 centerR, Vector3 centerY, EdgeVertices e)
    {
        center = Vector3.Lerp(centerL, centerR, 0.5f);
        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(centerL, e.v1, 0.5f),
            Vector3.Lerp(centerR, e.v5, 0.5f),
            1f / 6f
        );
        m.v3.y = center.y = e.v3.y;

        TriangulateEdgeStrip(m, weights1, e, weights1, cell.Index, cell.Index);

        //terrain.AddTriangle(centerL, m.v1, m.v2);
        //terrain.AddTriangleColor(cell.Color);
        //terrain.AddQuad(centerL, center, m.v2, m.v3);
        //terrain.AddQuadColor(cell.Color);
        //terrain.AddQuad(center, centerR, m.v3, m.v4);
        //terrain.AddQuadColor(cell.Color);
        terrain.AddTriangle(m.v2, m.v3, centerY);
        terrain.AddTriangle(m.v3, m.v4, centerY);
        terrain.AddTriangle(centerR, m.v4, m.v5);

        Vector3 indices;
        indices.x = indices.y = indices.z = cell.Index;

        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);

        if (!cell.IsUnderWater)
        {
            bool reversed = cell.HasIncomingRiver;
            TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed, indices);
            TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed, indices);
        }
    }

    void AddTriangleRiverLast(HexDirection direction, HexCell cell, Vector3 center, Vector3 centerL, Vector3 centerR, EdgeVertices e, EdgeVertices m)
    {

        TriangulateEdgeStrip(m, weights1, e, weights2, cell.Index, cell.Index);

        terrain.AddTriangle(centerL, m.v1, m.v2);
        terrain.AddQuad(centerL, center, m.v2, m.v3);
        terrain.AddQuad(center, centerR, m.v3, m.v4);
        terrain.AddTriangle(centerR, m.v4, m.v5);

        Vector3 indices;
        indices.x = indices.y = indices.z = cell.Index;

        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddQuadCellData(indices, weights1);
        terrain.AddQuadCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);

        if (!cell.IsUnderWater)
        {
            bool reversed = cell.IncomingRiver == direction;

            TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed, indices);
            TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed, indices);
        }
        //TriangulateEdgeFan(centerR, m, cell.Color);
        /* center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;
         rivers.AddTriangle(center, m.v2, m.v4);
         if (reversed)
         {
             rivers.AddTriangleUV(
                 new Vector2(0.5f, 0.4f),
                 new Vector2(1f, 0.2f), new Vector2(0f, 0.2f)
             );
         }
         else
         {
             rivers.AddTriangleUV(
                 new Vector2(0.5f, 0.4f),
                 new Vector2(0f, 0.6f), new Vector2(1f, 0.6f)
             );
         }*/
    }

    void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else
            {
                TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            }
        }
        else
        {
            terrain.AddTriangle(bottom, left, right);
            Vector3 indices;
            indices.x = bottomCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;
            terrain.AddTriangleCellData(indices, weights1, weights2, weights3);
        }

        features.AddWall(bottom, bottomCell, left, leftCell, right, rightCell);
    }

    void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color w3 = HexMetrics.TerraceLerp(weights1, weights2, 1);
        Color w4 = HexMetrics.TerraceLerp(weights1, weights3, 1);
        Vector3 indices;
        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        terrain.AddTriangle(begin, v3, v4);
        terrain.AddTriangleCellData(indices, weights1, w3, w4);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color w1 = w3;
            Color w2 = w4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            w3 = HexMetrics.TerraceLerp(weights1, weights2, i);
            w4 = HexMetrics.TerraceLerp(weights1, weights3, i);
            terrain.AddQuad(v1, v2, v3, v4);
            terrain.AddQuadCellData(indices, w1, w2, w3, w4);
        }

        terrain.AddQuad(v3, v4, left, right);
        terrain.AddQuadCellData(indices, w3, w4, weights2, weights3);
    }

    void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
        Color boundaryWeight = Color.Lerp(weights1, weights2, b);
        Vector3 indices;
        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        TriangulateBoundaryTriangle(begin, weights1, left, weights2, boundary, boundaryWeight, indices);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeight, indices);
        }
        else
        {
            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeight);
        }
    }

    void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
        Color boundaryWeight = Color.Lerp(weights1, weights2, b);
        Vector3 indices;
        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        TriangulateBoundaryTriangle(right, weights3, begin, weights1, boundary, boundaryWeight, indices);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeight, indices);
        }
        else
        {
            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeight);
        }
    }

    void TriangulateBoundaryTriangle(Vector3 begin, Color beginWeight, Vector3 left, Color leftWeight, Vector3 boundary, Color boundaryWeight, Vector3 indices)
    {
        Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color w2 = HexMetrics.TerraceLerp(beginWeight, leftWeight, 1);

        terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
        terrain.AddTriangleCellData(indices, beginWeight, w2, boundaryWeight);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color w1 = w2;
            v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
            w2 = HexMetrics.TerraceLerp(beginWeight, leftWeight, i);
            terrain.AddTriangleUnperturbed(v1, v2, boundary);
            terrain.AddTriangleCellData(indices, w1, w2, boundaryWeight);
        }

        terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
        terrain.AddTriangleCellData(indices, w2, leftWeight, boundaryWeight);
    }
}