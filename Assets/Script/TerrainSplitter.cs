using UnityEngine;
public class TerrainSplitter : MonoBehaviour
{
    public ComputeShader splitTerrainShader;
    public Terrain terrain;
    public float tileSize = 10.0f;
    public int startCorner = 0; // 0 = bottom-left, 1 = bottom-right, 2 = top-left, 3 = top-right

    private ComputeBuffer tilePositionsBuffer;
    private Vector3[] tilePositions;
    private int tilesX, tilesZ;

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain is not assigned.");
            return;
        }

        // Get terrain dimensions
        float terrainWidth = terrain.terrainData.size.x;
        float terrainHeight = terrain.terrainData.size.z;

        // Calculate the number of tiles
        tilesX = Mathf.CeilToInt(terrainWidth / tileSize);
        tilesZ = Mathf.CeilToInt(terrainHeight / tileSize);
        int totalTiles = tilesX * tilesZ;

        // Initialize the compute buffer
        tilePositionsBuffer = new ComputeBuffer(totalTiles, sizeof(float) * 3);
        tilePositions = new Vector3[totalTiles];

        // Set compute shader parameters
        int kernelHandle = splitTerrainShader.FindKernel("SplitTerrain");
        splitTerrainShader.SetFloat("TerrainWidth", terrainWidth);
        splitTerrainShader.SetFloat("TerrainHeight", terrainHeight);
        splitTerrainShader.SetFloat("TileSize", tileSize);
        splitTerrainShader.SetInt("StartCorner", startCorner);
        splitTerrainShader.SetBuffer(kernelHandle, "TilePositions", tilePositionsBuffer);

        // Dispatch the compute shader
        splitTerrainShader.Dispatch(kernelHandle, tilesX, tilesZ, 1);

        // Read back the results
        tilePositionsBuffer.GetData(tilePositions);

        // Release the buffer
        tilePositionsBuffer.Release();
    }

    void OnDrawGizmos()
    {
        if (tilePositions == null || tilePositions.Length == 0)
            return;

        // Draw the grid using Gizmos
        Gizmos.color = Color.green;

        // Draw horizontal lines
        for (int z = 0; z <= tilesZ; z++)
        {
            Vector3 start = new Vector3(0, 0, z * tileSize);
            Vector3 end = new Vector3(tilesX * tileSize, 0, z * tileSize);
            Gizmos.DrawLine(start, end);
        }

        // Draw vertical lines
        for (int x = 0; x <= tilesX; x++)
        {
            Vector3 start = new Vector3(x * tileSize, 0, 0);
            Vector3 end = new Vector3(x * tileSize, 0, tilesZ * tileSize);
            Gizmos.DrawLine(start, end);
        }

        // Draw tile centers (optional)
        Gizmos.color = Color.red;
        foreach (var position in tilePositions)
        {
            Gizmos.DrawSphere(position, 0.5f);
        }
    }
}