using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public struct neighboursCheck
{
	public bool isConnectable_N;
	public bool isConnectable_S;
	public bool isConnectable_E;
	public bool isConnectable_W;
	public bool isConnectable_NE;
	public bool isConnectable_NW;
	public bool isConnectable_SE;
	public bool isConnectable_SW;

	public bool allSidesChecked;
	public bool isOccupied;
	public bool isChecked;
	public int moveStep;

	public int h;
	public int heurStep;

	public TileScript next;
	public TileScript prev;
}

public class TileScript : MonoBehaviour {

	bool heldMouse = false;

	public neighboursCheck tileNeighbourCheck;
	public int[] tilePos = new int[2];
	public SpriteRenderer rend;

	public SpriteRenderer[] borderRend;

	public Text stepText;
	public Canvas tileCanvas;

	// Use this for initialization
	void Start () 
	{
		ResetTile();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetMouseButtonUp(0))
		{
			if(heldMouse) heldMouse = false;
		}
	}

	void OnMouseOver()
	{
		if(TileMapManager.instance.selectingEnd)
		{
			for(int i = 0; i < borderRend.Length; i++)
			{
				borderRend[i].color = Color.red;
			}

		}
		if(TileMapManager.instance.selectingStart)
		{
			for(int i = 0; i < borderRend.Length; i++)
			{
				borderRend[i].color = Color.green;
			}
		}

		if(Input.GetMouseButtonDown(0))
		{
				if(TileMapManager.instance.selectingEnd)
				{
					if(!tileNeighbourCheck.isOccupied)
					{
						if(TileMapManager.instance.selectedTile != null) TileMapManager.instance.selectedTile.rend.color = Color.white;

						if(TileMapManager.instance.tilePath.Count > 0)
						{
							for(int i = 0; i < TileMapManager.instance.tilePath.Count; i++)
							{
								TileMapManager.instance.tilePath[i].ResetTile();
							}

							TileMapManager.instance.RecolourTile();
							TileMapManager.instance.tilePath.Clear();
						}

						if(TileMapManager.instance.currTile != null) TileMapManager.instance.currTile.rend.color = Color.green;

						rend.color = Color.red;
						TileMapManager.instance.selectedTile = this;
						TileMapManager.instance.endBorder.SetActive(false);

						TileMapManager.instance.selectingEnd = false;

						for(int i = 0; i < borderRend.Length; i++)
						{
							borderRend[i].color = Color.grey;
						}
					}
				}
				else if(TileMapManager.instance.selectingStart)
				{
					if(!tileNeighbourCheck.isOccupied)
					{
						if(TileMapManager.instance.currTile != null) TileMapManager.instance.currTile.rend.color = Color.white;

						if(TileMapManager.instance.tilePath.Count > 0)
						{
							for(int i = 0; i < TileMapManager.instance.tilePath.Count; i++)
							{
								TileMapManager.instance.tilePath[i].ResetTile();
							}

							TileMapManager.instance.RecolourTile();
							TileMapManager.instance.tilePath.Clear();
						}

						if(TileMapManager.instance.selectedTile != null)
						{
							TileMapManager.instance.selectedTile.rend.color = Color.red;
						}

						rend.color = Color.green;
						TileMapManager.instance.currTile = this;
						TileMapManager.instance.startBorder.SetActive(false);

						TileMapManager.instance.selectingStart = false;

						for(int i = 0; i < borderRend.Length; i++)
						{
							borderRend[i].color = Color.grey;
						}
					}
				}
				else
				{
					if(!TileMapManager.instance.finding)
					{
						if(TileMapManager.instance.currTile != this && TileMapManager.instance.selectedTile != this)
						{
							TileMapManager.instance.RecolourTile();

							OffCanvas();

							if(TileMapManager.instance.currTile != null)
							{
								TileMapManager.instance.currTile.rend.color = Color.green;
							}

							if(TileMapManager.instance.selectedTile != null)
							{
								TileMapManager.instance.selectedTile.rend.color = Color.red;
							}

							if(!tileNeighbourCheck.isOccupied)
							{
								tileNeighbourCheck.isOccupied = true;
								rend.color = Color.black;
							}
							else
							{
								tileNeighbourCheck.isOccupied = false;
								rend.color = Color.white;
							}
						}
					}

				}
		}
	}

	void OnMouseExit()
	{
		if(TileMapManager.instance.selectingEnd || TileMapManager.instance.selectingStart)
		{
			for(int i = 0; i < borderRend.Length; i++)
			{
				borderRend[i].color = Color.grey;
			}
		}
	}

	public void OnCanvas()
	{
		tileCanvas.enabled = true;
	}

	public void OffCanvas()
	{
		tileCanvas.enabled = false;
	}

	public void ResetTile()
	{
		tileNeighbourCheck.isConnectable_E = true;
		tileNeighbourCheck.isConnectable_W = true;
		tileNeighbourCheck.isConnectable_N = true;
		tileNeighbourCheck.isConnectable_S = true;
		tileNeighbourCheck.isConnectable_NE = true;
		tileNeighbourCheck.isConnectable_NW = true;
		tileNeighbourCheck.isConnectable_SE = true;
		tileNeighbourCheck.isConnectable_SW = true;

		//tileNeighbourCheck.isOccupied = false;
		tileNeighbourCheck.isChecked = false;
		tileNeighbourCheck.allSidesChecked = false;
		tileNeighbourCheck.moveStep = 0;
		tileNeighbourCheck.prev = null;

		tileNeighbourCheck.h = 0;
		tileNeighbourCheck.heurStep = 0;

		stepText.text = "";
	}
}
