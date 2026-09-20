using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject manualLevel;
    [SerializeField] private GameObject[] tiles;
    [SerializeField] private Camera gameCamera;

    private int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0}
    };

    private int[,] fullMap;
    private bool[,,] rotations;
    private int[] rowStep = { -1, 0, 1, 0 };
    private int[] columnStep = { 0, 1, 0, -1 };

    private void Start()
    {
        manualLevel.SetActive(false);
        Destroy(manualLevel);
        CreateFullMap();
        FindRotations();

        Transform level = new GameObject("GeneratedLevel").transform;
        for (int quadrant = 0; quadrant < 4; quadrant++)
        {
            CreateQuadrant(level, quadrant);
        }

        FitCamera();
    }

    private void CreateFullMap()
    {
        int rows = levelMap.GetLength(0);
        int columns = levelMap.GetLength(1);
        fullMap = new int[rows * 2 - 1, columns * 2];

        for (int row = 0; row < fullMap.GetLength(0); row++)
        {
            for (int column = 0; column < fullMap.GetLength(1); column++)
            {
                int sourceRow = row < rows ? row : rows * 2 - 2 - row;
                int sourceColumn = column < columns ? column : columns * 2 - 1 - column;
                fullMap[row, column] = levelMap[sourceRow, sourceColumn];
            }
        }
    }

    private bool IsWall(int tile)
    {
        return tile == 1 || tile == 2 || tile == 3 || tile == 4 || tile == 7 || tile == 8;
    }

    private bool HasConnection(int tile, int rotation, int side)
    {
        side = (side - rotation + 4) % 4;
        if (tile == 1 || tile == 3)
        {
            return side == 1 || side == 2;
        }
        if (tile == 7)
        {
            return side != 0;
        }
        return side == 1 || side == 3;
    }

    private bool SameWallType(int first, int second)
    {
        if (first == 7 || second == 7)
        {
            return true;
        }
        bool firstOutside = first == 1 || first == 2;
        bool secondOutside = second == 1 || second == 2;
        return firstOutside == secondOutside;
    }

    private void FindRotations()
    {
        int rows = fullMap.GetLength(0);
        int columns = fullMap.GetLength(1);
        rotations = new bool[rows, columns, 4];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    rotations[row, column, rotation] = IsWall(fullMap[row, column]);
                }
            }
        }

        rotations[0, 0, 1] = false;
        rotations[0, 0, 2] = false;
        rotations[0, 0, 3] = false;

        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    for (int rotation = 0; rotation < 4; rotation++)
                    {
                        if (rotations[row, column, rotation] && !FitsNeighbours(row, column, rotation))
                        {
                            rotations[row, column, rotation] = false;
                            changed = true;
                        }
                    }
                }
            }
        }
    }

    private bool FitsNeighbours(int row, int column, int rotation)
    {
        int tile = fullMap[row, column];
        for (int side = 0; side < 4; side++)
        {
            int nextRow = row + rowStep[side];
            int nextColumn = column + columnStep[side];
            bool connection = HasConnection(tile, rotation, side);

            if (nextRow < 0 || nextRow >= fullMap.GetLength(0) || nextColumn < 0 || nextColumn >= fullMap.GetLength(1))
            {
                if (connection && !(tile == 2 && (side == 1 || side == 3)))
                {
                    return false;
                }
                continue;
            }

            int nextTile = fullMap[nextRow, nextColumn];
            if (!IsWall(nextTile))
            {
                if (connection)
                {
                    return false;
                }
                continue;
            }

            bool found = false;
            for (int nextRotation = 0; nextRotation < 4; nextRotation++)
            {
                bool nextConnection = HasConnection(nextTile, nextRotation, (side + 2) % 4);
                if (rotations[nextRow, nextColumn, nextRotation] && connection == nextConnection)
                {
                    if (!connection || SameWallType(tile, nextTile))
                    {
                        found = true;
                    }
                }
            }
            if (!found)
            {
                return false;
            }
        }
        return true;
    }

    private int GetRotation(int row, int column)
    {
        if (!IsWall(levelMap[row, column]))
        {
            return 0;
        }
        for (int rotation = 0; rotation < 4; rotation++)
        {
            if (rotations[row, column, rotation])
            {
                return rotation;
            }
        }
        Debug.LogError("Wall connections do not match at " + row + ", " + column);
        return 0;
    }

    private void CreateQuadrant(Transform level, int quadrant)
    {
        bool right = quadrant == 1 || quadrant == 3;
        bool bottom = quadrant >= 2;
        int rows = levelMap.GetLength(0);
        int columns = levelMap.GetLength(1);
        string[] names = { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
        Transform group = new GameObject(names[quadrant]).transform;
        group.SetParent(level, false);
        group.localPosition = new Vector3(right ? columns * 2 - 1 : 0, bottom ? -(rows * 2 - 2) : 0, 0);
        group.localScale = new Vector3(right ? -1 : 1, bottom ? -1 : 1, 1);

        int rowCount = bottom ? rows - 1 : rows;
        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int tile = levelMap[row, column];
                if (tile == 0)
                {
                    continue;
                }
                GameObject piece = Instantiate(tiles[tile - 1], group);
                piece.name = tiles[tile - 1].name + "_" + row + "_" + column;
                piece.transform.localPosition = new Vector3(column, -row, 0);
                piece.transform.localRotation = Quaternion.Euler(0, 0, -90 * GetRotation(row, column));
            }
        }
    }

    private void FitCamera()
    {
        float width = fullMap.GetLength(1);
        float height = fullMap.GetLength(0);
        gameCamera.transform.position = new Vector3((width - 1) / 2f, -(height - 1) / 2f - 1f, -10f);
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = Mathf.Max((height + 3f) / 2f, (width + 2f) / (2f * gameCamera.aspect));
    }
}
