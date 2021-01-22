﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap map;
    public Tilemap mapDetail;

    [Space]
    public List<TileData> tileDatas;

    Dictionary<TileBase, TileData> dataFromTiles;

    [Header("UI gameobjects")]
    public GameObject CellSelection;
    public GameObject UIBreakBlock;
    public GameObject UIPlaceUmbrella;
    public GameObject UIBuildRStairs;
    public GameObject UIBuildLStairs;
    public GameObject UIDemolishBlock;
    public GameObject UIVictory;
    public GameObject UIDefeat;

    [Space]
    public GameObject PrefabUmbrella;

    [Header("Tiles")]
    public TileBase TileHighlight;
    public TileBase TileRStairs;
    public TileBase TileLStairs;
    public TileBase TileTop;

    TileBase selectedTile;
    Vector3Int selectedTilePos = Vector3Int.one;

    Dictionary<Vector3Int, GameObject> placedUmbrellas;

    Coroutine fadeSpriteCoroutine;
    bool interactingWithCell = false;

    AudioSource audioSource;

    void Awake()
    {
        // Create Dictionary and fill it with each tile type and its data
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var data in tileDatas)
        {
            foreach (var tile in data.tiles)
            {
                dataFromTiles.Add(tile, data);
            }
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        placedUmbrellas = new Dictionary<Vector3Int, GameObject>();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) { ReloadScene(); }

        if (Input.GetMouseButton(0) && !interactingWithCell)
        {
            // Get newly selected tile
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPos = map.WorldToCell(mousePos);

            // Refresh selected tile
            if (gridPos != selectedTilePos)
            {
                selectedTile = map.GetTile(gridPos);
                selectedTilePos = gridPos;
                SelectCell();
            }
        }

        if (Input.GetMouseButtonUp(0) && !interactingWithCell)
        {
            // Perform actions on the selected tile depending on its type
            TileClicked();
        }
        else if (Input.GetMouseButtonUp(0) && interactingWithCell)
        {
            // Remove any active buttons from screen
            interactingWithCell = false;
            CellSelection.SetActive(false);
            UIBreakBlock.SetActive(false);
            UIPlaceUmbrella.SetActive(false);
            UIBuildRStairs.SetActive(false);
            UIBuildLStairs.SetActive(false);
            UIDemolishBlock.SetActive(false);
            selectedTilePos = Vector3Int.one; // Reset selected tile pos
        }
    }

    void SelectCell()
    {
        // Select tile
        CellSelection.SetActive(true);
        CellSelection.transform.position = selectedTilePos + map.cellSize / 2;
        CellSelection.GetComponent<SpriteRenderer>().material.color = Color.white; // Reset alpha to 1f

        // If selection sprite was fading, stop
        if (fadeSpriteCoroutine != null)
        {
            StopCoroutine(fadeSpriteCoroutine);
            fadeSpriteCoroutine = null;
        }
    }

    void TileClicked()
    {
        // Case empty tile
        if (selectedTile == null)
        {
            // Option to place umbrella if tile below is empty too
            if (true)
            {
                Vector3 worldPosition = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
                worldPosition.y += map.cellSize.y * 1.5f; // Set position on top of the tile
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition); // Get position in screen coords
                RectTransformUtility.ScreenPointToWorldPointInRectangle(FindObjectOfType<Canvas>().GetComponent<RectTransform>(), screenPosition,
                    null, out Vector3 buttonPos); // Transform screen coords to point in the canvas
                UIPlaceUmbrella.SetActive(true);
                UIPlaceUmbrella.GetComponent<RectTransform>().position = buttonPos;

                worldPosition.x -= map.cellSize.x * 1.1f;
                screenPosition = Camera.main.WorldToScreenPoint(worldPosition); // Get position in screen coords
                RectTransformUtility.ScreenPointToWorldPointInRectangle(FindObjectOfType<Canvas>().GetComponent<RectTransform>(), screenPosition,
                    null, out buttonPos); // Transform screen coords to point in the canvas
                UIBuildRStairs.SetActive(true);
                UIBuildRStairs.GetComponent<RectTransform>().position = buttonPos;

                worldPosition.x += map.cellSize.x * 2.2f;
                screenPosition = Camera.main.WorldToScreenPoint(worldPosition); // Get position in screen coords
                RectTransformUtility.ScreenPointToWorldPointInRectangle(FindObjectOfType<Canvas>().GetComponent<RectTransform>(), screenPosition,
                    null, out buttonPos); // Transform screen coords to point in the canvas
                UIBuildLStairs.SetActive(true);
                UIBuildLStairs.GetComponent<RectTransform>().position = buttonPos;

                interactingWithCell = true;
            }
            // Option to build stairs if tile below is ground or continuing other stairs
            // Option to place STOP if tile below is ground
            return;
        }

        bool result = dataFromTiles.TryGetValue(selectedTile, out TileData clickedTileData);
        if (result && clickedTileData.selectable)
        {
            if (clickedTileData.destructable)
            {
                Vector3 worldPosition = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
                worldPosition.y += map.cellSize.y * 1.5f; // Set position on top of the tile
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition); // Get position in screen coords
                RectTransformUtility.ScreenPointToWorldPointInRectangle(FindObjectOfType<Canvas>().GetComponent<RectTransform>(), screenPosition,
                    null, out Vector3 buttonPos); // Transform screen coords to point in the canvas
                UIBreakBlock.SetActive(true);
                UIBreakBlock.GetComponent<RectTransform>().position = buttonPos;

                interactingWithCell = true;
            }
            if (clickedTileData.demolishable)
            {
                Vector3 worldPosition = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
                worldPosition.y += map.cellSize.y * 1.5f; // Set position on top of the tile
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition); // Get position in screen coords
                RectTransformUtility.ScreenPointToWorldPointInRectangle(FindObjectOfType<Canvas>().GetComponent<RectTransform>(), screenPosition,
                    null, out Vector3 buttonPos); // Transform screen coords to point in the canvas
                UIDemolishBlock.SetActive(true);
                UIDemolishBlock.GetComponent<RectTransform>().position = buttonPos;

                interactingWithCell = true;
            }
            if (clickedTileData.highlight)
            {
                RemoveUmbrella();
            }
        }
        else
        {
            // Fade cell selection sprite if cell cannot be selected
            fadeSpriteCoroutine = StartCoroutine(FadeSprite(CellSelection.GetComponent<SpriteRenderer>(), 0f, 0.5f));
            selectedTilePos = Vector3Int.one; // Reset selected tile pos
        }
    }

    public bool CheckForDamagingTile(Vector3 position)
    {
        // Get tile to check
        Vector3Int gridPos = map.WorldToCell(position);
        TileBase tileToCheck = map.GetTile(gridPos);

        if (tileToCheck != null)
        {
            // If tile is damaging return true
            bool result = dataFromTiles.TryGetValue(tileToCheck, out TileData clickedTileData);
            if (result && clickedTileData.damaging)
                return true;
        }

        return false;
    }

    public void BreakBlock()
    {
        map.SetTile(selectedTilePos, null); // Remove this tile
        mapDetail.SetTile(selectedTilePos, null); // Romove detail tile on same pos

        // Check if detail tile above is a top tile. Remove it if so
        TileBase aboveTile = mapDetail.GetTile(selectedTilePos + Vector3Int.up);
        if (aboveTile)
        {
            bool result = dataFromTiles.TryGetValue(aboveTile, out TileData aboveTileData);

            if (result && aboveTileData.top)
                mapDetail.SetTile(selectedTilePos + Vector3Int.up, null);
        }

        // Check if there is a tile below. If so, add a top tile over it
        TileBase belowTile = map.GetTile(selectedTilePos + Vector3Int.down);
        if (belowTile)
        {
            bool result = dataFromTiles.TryGetValue(belowTile, out TileData belowTileData);

            if (result && !belowTileData.damaging && !belowTileData.destructable)
                mapDetail.SetTile(selectedTilePos, TileTop);
        }

        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void PlaceUmbrella()
    {
        map.SetTile(selectedTilePos, TileHighlight);
        Vector3 cellWorldPos = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
        GameObject umbrella = Instantiate(PrefabUmbrella, cellWorldPos, Quaternion.identity);
        placedUmbrellas.Add(selectedTilePos, umbrella);
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void RemoveUmbrella()
    {
        map.SetTile(selectedTilePos, null); // Remove tile highlight
        // Remove umbrella
        placedUmbrellas.TryGetValue(selectedTilePos, out GameObject umbrella);
        if (umbrella)
        {
            Destroy(umbrella);
            placedUmbrellas.Remove(selectedTilePos);
        }
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void BuildRStairs()
    {
        //Vector3 cellWorldPos = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
        //Instantiate(PrefabStairs, cellWorldPos, Quaternion.identity);
        map.SetTile(selectedTilePos, TileRStairs);
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void BuildLStairs()
    {
        //Vector3 cellWorldPos = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
        //GameObject stairs = Instantiate(PrefabStairs, cellWorldPos, Quaternion.identity);
        //stairs.transform.localScale = new Vector3(-1f, 1f, 1f);
        map.SetTile(selectedTilePos, TileLStairs);
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void PlaySound(AudioClip sound)
    {
        audioSource.clip = sound;
        audioSource.Play();
    }

    public void DemolishBlock()
    {
        map.SetTile(selectedTilePos, null);
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }


    public void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void Victory()
    {
        UIVictory.SetActive(true);
    }

    void Defeat()
    {
        UIDefeat.SetActive(true);
    }

    public void CreatureSaved()
    {
        // Just using 1 creature for now, so call Victory
        Invoke("Victory", 1f);
    }

    public void CreatureDefeated()
    {
        // Just using 1 creature for now, so call Defeat
        Invoke("Defeat", 1f);
    }


    IEnumerator FadeSprite(SpriteRenderer sprite, float newAlpha, float fadeTime)
    {
        float alpha = sprite.material.color.a; // Save initial alpha
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / fadeTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, newAlpha, t));
            sprite.material.color = newColor;
            yield return null;
        }
    }
}
