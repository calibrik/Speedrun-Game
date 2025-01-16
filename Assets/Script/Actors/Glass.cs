using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Glass : MonoBehaviour
{
    struct TileInfo
    {
        public GameObject vfx;
        public TileBase tile;
        public Vector3Int cellPos;
    }
    private Gore _goreComponent;
    private TilemapCollider2D _collider;
    private Tilemap _tilemap;
    private List<TileInfo> _tilesToRespawn;
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<TilemapCollider2D>();
        _tilesToRespawn = new List<TileInfo>();
        _tilemap = GetComponentInChildren<Tilemap>();
        _goreComponent = GetComponent<Gore>();
        Main.inputManager.Game.OnRestartLevel.performed += OnRestartLevel;
    }
    void OnRestartLevel(InputAction.CallbackContext value)
    {
        while (_tilesToRespawn.Count!=0)
        {
            _tilemap.SetTile(_tilesToRespawn[0].cellPos, _tilesToRespawn[0].tile);
            if (_tilesToRespawn[0].vfx)
                Destroy(_tilesToRespawn[0].vfx);
            _tilesToRespawn.RemoveAt(0);
        }
    }
    void BreakTile(Vector3Int cellPos)
    {
        GameObject tile = _tilemap.GetInstantiatedObject(cellPos);
        if (!tile) return;
        TileInfo tileInfo = new TileInfo();
        tileInfo.vfx = tile.transform.Find("glassExplode").gameObject;
        tileInfo.tile = _tilemap.GetTile(cellPos);
        tileInfo.cellPos = cellPos;
        tileInfo.vfx.transform.parent = null;
        tileInfo.vfx.SetActive(true);
        _tilemap.SetTile(cellPos, null);
        _tilesToRespawn.Add(tileInfo);
        _collider.ProcessTilemapChanges();
    }
    public void BreakByRay(Vector2 startPos,float rayLength,Vector2 direction)
    {
        int layerMask = 1 << gameObject.layer;
        int safeCounter = 0;
        while (true)
        {
            if (++safeCounter>100)
            {
                Debug.LogError("Too much glass rays");
                return;
            }
            RaycastHit2D hit = Physics2D.Raycast(startPos, direction, rayLength, layerMask);
            if (!hit)
                return;
            //Debug.DrawLine(startPos, hit.point, Color.yellow, 10);
            Vector3Int cellPos = _tilemap.WorldToCell(hit.point - hit.normal * 0.1f);
            BreakTile(cellPos);
        }
    }
    public void BreakByExplosion(Vector2 explosionPoint,float radius)
    {
        Vector3Int initCellPos = _tilemap.WorldToCell(explosionPoint);
        int squareSize = 0;
        Vector3Int currCellPos = initCellPos;
        currCellPos.x++;
        while (true)
        {
            Vector2 worldPos= _tilemap.CellToWorld(currCellPos) + _tilemap.cellSize / 2;
            if ((worldPos-explosionPoint).magnitude>radius)
                break;
            squareSize++;
            currCellPos.x++;
        }
        squareSize++;
        initCellPos.x-= squareSize;
        initCellPos.y-= squareSize;
        squareSize *= 2;
        for (int i = 0; i < squareSize; i++)
        {
            for (int j = 0; j < squareSize; j++)
            {
                currCellPos = initCellPos;
                currCellPos.y+=i;
                currCellPos.x += j;
                Vector2 worldPos = _tilemap.CellToWorld(currCellPos) + _tilemap.cellSize / 2;
                //Debug.DrawLine(explosionPoint, worldPos, Color.red, 15);
                if ((worldPos - explosionPoint).magnitude <= radius)
                    BreakTile(currCellPos);
            }
        }
    }
}
