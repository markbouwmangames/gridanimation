using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Grid : MonoBehaviour {
    public Tile[,] tiles = new Tile[width, height];

    private const int width = 20;
    private const int height = 20;

    [SerializeField]
    private float rippleSpeed;

    [SerializeField]
    private GameObject OnGridTappedParticles;
    [SerializeField]
    private AudioSource OnGridEnableAudio;
    [SerializeField]
    private AudioSource OnGridTappedAudio;

    public void SetShape(Texture2D shape, Element element) {
        StopAllCoroutines();
        AudioManager.Play(OnGridEnableAudio);

        Color32[] pixels = shape.GetPixels32();
        if (shape.width != width) Debug.Log("Texture width is not the same as grid width!");
        if (shape.height != height) Debug.Log("Texture height is not the same as grid height!");

        StartCoroutine(Ripple((tile) => {
            Color32 pixel = pixels[tile.X * width + tile.Y];
            if (pixel == Color.black) {
                tile.Ping(element);
            } else {
                float brightness = ((pixel.r + pixel.g + pixel.b) / 3f) / 255f;
                tile.On(element, brightness);
            }
        }));
    }

    public void RippleElement(Element element) {
        StopAllCoroutines();
        StartCoroutine(Ripple((tile) => {
            tile.Ping(element);
        }));
    }

    public void TapTile(Tile tile) {
        if(tile == null) return;
        AudioManager.Play(OnGridTappedAudio);
        if(OnGridTappedParticles != null) {
            ParticleSystem[] systems = ParticleManager.InstantiateParticleSystems(OnGridTappedParticles, tile.transform.position);
            foreach(ParticleSystem ps in systems) {
                ps.startColor = ElementColorUtil.GetElementColor(tile.OrbElement).Color;
                ps.Play();
            }
        }
    }

    public Tile GetNearestTile(Vector3 tapPosition, bool includeDisabledTiles) {
        Tile nearestTile = null;
        float closestDistance = float.MaxValue;

        for(int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {
                Tile tile = tiles[x, y];
                if(tile.IsEnabled == false && includeDisabledTiles == false) {
                    continue;
                }

                float dist = Vector3.Distance(tile.GetScreenPos(), tapPosition);
                if(dist < closestDistance) {
                    nearestTile = tile;
                    closestDistance = dist;
                }
            }
        }
        return nearestTile;
    }

    public Tile GetRandomPositionFromShapeForAI(Texture2D texture) {
        Color32[] pixels = texture.GetPixels32();
        List<Tile> possibleTiles = new List<Tile>();
        for(int x = 0; x < texture.width; x++) {
            for(int y = 0; y < texture.height; y++) {
                if(pixels[x * width + y] != Color.black) {
                    int yFlipped = Mathf.CeilToInt(Mathf.Repeat((texture.height - 1) - y, texture.height));
                    int xFlipped = Mathf.CeilToInt(Mathf.Repeat((texture.width - 1) - x, texture.width));
                    possibleTiles.Add(tiles[yFlipped, xFlipped]);
                }
            }
        }
        if(possibleTiles.Count == 0) return null;
        return possibleTiles[UnityEngine.Random.Range(0, possibleTiles.Count)];
    }

    private void Awake() {
        int row = 0;
        int column = 0;

        foreach (Transform t in transform) {
            Tile tile = t.GetComponent<Tile>();

            tiles[row, column] = tile;
            tile.X = column;
            tile.Y = row;

            row++;
            if (row == width) {
                row = 0;
                column++;
            }
        }
    }

    private IEnumerator Ripple(Action<Tile> tileAction) {
        Vector3 startPosition = FindObjectOfType<PlayerShell>().transform.position;
        float distance = 0;
        List<Tile> allTiles = GetAllTiles();

        while (distance < 30f) {
            List<Tile> triggeredThisFrame = new List<Tile>();

            distance += Time.deltaTime * rippleSpeed;
            foreach (Tile tile in allTiles) {
                float dist = (tile.transform.position - startPosition).sqrMagnitude;
                if (dist < distance * distance) {
                    triggeredThisFrame.Add(tile);
                    tileAction(tile);
                }
            }

            foreach (Tile tile in triggeredThisFrame) {
                allTiles.Remove(tile);
            }
            yield return null;
        }
    }

    private List<Tile> GetAllTiles() {
        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);
        List<Tile> ret = new List<Tile>(width * height);
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                ret.Add(tiles[i, j]);
            }
        }
        return ret;
    }
}
