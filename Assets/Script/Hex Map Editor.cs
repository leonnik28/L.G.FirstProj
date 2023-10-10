using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    OptionalToggle riverMode, roadMode, walledMode;

    //public Color[] colors;

    public HexGrid hexGrid;
    public Material terrainMaterial;
    //private Color activeColor;

    //bool applyColor;

    bool applyElevation = true;
    bool applyWaterLevel = true;
    bool applyUrbanLevel;
    bool applyPlantLevel;
    bool applySpecialLevel;

    bool isDrag;
    //bool editMode;
    bool searchMode;

    HexDirection dragDirection;
    HexCell previousCell;
    int activeElevation;
    int activeWaterLevel;
    int activeUrbanLevel;
    int activePlantLevel;
    int activeSpecialLevel;
    int activeTerrainTypeIndex;

    int brushSize;

    HexCell currentCell;

    void Awake()
    {
        terrainMaterial.DisableKeyword("GRID_ON");
        Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        SetEditMode(true);
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
                /*if (!searchMode)
                {
                    if (currentCell == null)
                    {
                        HexCell cell = GetCellUnderCursor();
                        if (cell)
                        {
                            currentCell = cell;
                            currentCell.EnableHighlight(Color.white, false);
                        }
                    }
                    else
                    {
                        currentCell.EnableHighlight(Color.white, true);
                        currentCell = null;
                        HexCell cell = GetCellUnderCursor();
                        if (cell)
                        {
                            currentCell = cell;
                            currentCell.EnableHighlight(Color.white, false);
                        }
                    }
                }*/
                
            if (Input.GetMouseButton(0))              
            {              
                HandleInput();              
                return;               
            }               
            if (Input.GetKeyDown(KeyCode.U))              
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DestroyUnit();
                }
                else
                {
                    CreateUnit();
                }
                return;               
            }
            

        }
        previousCell = null;
    }

    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }

    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void SetSpecialMode(float mode)
    {
        activeSpecialLevel = (int)mode;
    }

    public void SetApplySpecialLevel(bool toogle)
    {
        applySpecialLevel = toogle;
    }

    public void SetWalledMode(int mode)
    {
        walledMode = (OptionalToggle)mode;
    }

    public void SetApplyUrbanLevel(bool toggle)
    {
        applyUrbanLevel = toggle;
    }

    public void SetApplyPlantLevel(bool toggle)
    {
        applyPlantLevel = toggle;
    }

    public void SetPlantLevel(float level)
    {
        activePlantLevel = (int)level;
    }

    public void SetUrbanLevel(float level)
    {
        activeUrbanLevel = (int)level;
    }


    public void SetApllyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel(float level)
    {
        activeWaterLevel = (int)level;
    }

    public void SetEditMode(bool toggle)
    {
        //editMode = toggle;
        //hexGrid.ShowUI(!toggle);
        enabled = toggle;
    }

    public void SetUnit()
    {
       // unitMode = toggle;
        CreateUnit();
    }

    public void SetSearchWay()
    {
        searchMode = true;
    }

    public void ShowGrid(bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
        }
    }

    void CreateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit)
        {

            hexGrid.AddUnit(Instantiate(HexUnit.unitPrefab), cell, Random.Range(0f, 360f));
        }
    }

    void DestroyUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Unit)
        {
            hexGrid.RemoveUnit(cell.Unit);
        }
    }

    HexCell GetCellUnderCursor()
    {
        return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }

    void HandleInput()
    {
        HexCell currentCell = GetCellUnderCursor();
        if (currentCell)
        {
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            //if (editMode)
            //{
                EditCells(currentCell);
            //}
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
        searchMode = false;
    }

    void ValidateDrag(HexCell currentCell)
    {
        for (
            dragDirection = HexDirection.NE;
            dragDirection <= HexDirection.NW;
            dragDirection++
        )
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    void EditCell(HexCell cell)
    {
        if (cell)
        {
            //if (applyColor) cell.Color = activeColor;
            if (activeTerrainTypeIndex >= 0) { cell.TerrainTypeIndex = activeTerrainTypeIndex; } 

            if (applyElevation) cell.Elevation = activeElevation;

            if (applyWaterLevel) cell.WaterLevel = activeWaterLevel;

            if(applyUrbanLevel) cell.UrbanLevel = activeUrbanLevel;

            if (applyPlantLevel) cell.PlantLevel = activePlantLevel;

            if (applySpecialLevel) cell.SpecialIndex = activeSpecialLevel;

            if (riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            if (roadMode == OptionalToggle.No)
            {
                cell.RemoveRoads();
            }
            if (walledMode != OptionalToggle.Ignore)
            {
                cell.Walled = walledMode == OptionalToggle.Yes;
            }

            if (isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    if (riverMode == OptionalToggle.Yes)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if (roadMode == OptionalToggle.Yes)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    /*public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }*/

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    /*public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
    }*/

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

}
