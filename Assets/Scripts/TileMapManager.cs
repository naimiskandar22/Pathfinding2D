using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileMapManager : MonoBehaviour {

	public static TileMapManager instance;

//	public Transform player;
//	public bool canMove = false;
//	int moveCount = 0;
//	public int moveLimit = 2;
//
//	public GameObject enemyPrefab;
//	public List<GameObject> enemyList = new List<GameObject>();
//	List<Vector2> enemyPos = new List<Vector2>();
//	public int enemyMoveLimit = 5;
//	int enemyTurn = 0;
//	bool enemyPathFound = false;

	public Transform tilePrefab;
	public Vector2 mapSize;
	public TileScript[,] tileMap = new TileScript[10, 10];
	public List<TileScript> tilePath;

	public TileScript selectedTile;
	public TileScript currTile;
	public TileScript node;

	int lowestStep;

	public bool playerTurn;

	public bool djikstra = true;
	public int weight;
	public bool allowDiagonal = false;

	//UI Buttons
	public bool selectingStart;
	public bool selectingEnd;

	public GameObject startBorder;
	public GameObject endBorder;
	public GameObject diagonalBorder;
	public Text diagonalText;
	public Slider weightSlider;
	public Text weightText;

	public bool finding = false;

	//Codes for Djikstra
	//lines 132 - 679

	//Codes for A*
	//lines 682 - 1365

	void Awake()
	{
		if(instance == null) instance = this;
	}

	// Use this for initialization
	void Start () 
	{
		tileMap = new TileScript[(int)mapSize.y, (int)mapSize.x];

		tilePath = new List<TileScript>();
		GenerateMap();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!finding)
		{
			weight = (int)weightSlider.value * 10;
			weightText.text = (weightSlider.value * 10).ToString();
		}

		if(Input.GetKeyDown(KeyCode.Return))
		{
			if(selectedTile != null && !selectedTile.tileNeighbourCheck.isOccupied)
			{
				if(djikstra)
				{
					FindPath_Djikstra();
				}
				else
				{
					FindPath_Astar();
				}
			}
		}

	}

	public void GenerateMap()
	{
		for(int x = 0; x < mapSize.x; x++)
		{
			for(int y = 0; y < mapSize.y; y++)
			{
				Vector3 tilePosition = new Vector3(-mapSize.x/2 + 0.5f + x, -mapSize.y/2 + 0.5f + y, 0);
				Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as Transform;
				tileMap[y,x] = newTile.GetComponent<TileScript>();
				newTile.GetComponent<TileScript>().tilePos[0] = y;
				newTile.GetComponent<TileScript>().tilePos[1] = x;
				//tileMap[y,x].rend = tileMap[y,x].GetComponent<SpriteRenderer>();
				newTile.parent = transform;
			}
		}

		currTile = tileMap[0,0];

	}

	public void RecolourTile()
	{
		for(int x = 0; x < mapSize.x; x++)
		{
			for(int y = 0; y < mapSize.y; y++)
			{
				if(!tileMap[y,x].tileNeighbourCheck.isOccupied)
				{
					tileMap[y,x].tileNeighbourCheck.moveStep = 0;
					tileMap[y,x].rend.color = Color.white;
					tileMap[y,x].stepText.text = "";
				}
			}
		}
	}

	public void FindPath_Djikstra()
	{
		for(int a = 0; a < mapSize.x; a++)
		{
			for(int b = 0; b < mapSize.y; b++)
			{
				tileMap[b,a].OnCanvas();
			}
		}

		finding = true;

		RecolourTile();

		node = currTile;

		node.rend.color = Color.green;
		selectedTile.rend.color = Color.red;

		for(int a = 0; a < mapSize.x; a++)
		{
			for(int b = 0; b < mapSize.y; b++)
			{
				tileMap[b,a].ResetTile();
			}
		}

		tilePath = new List<TileScript>();

		int x = node.tilePos[1];
		int y = node.tilePos[0];

		tilePath.Add(tileMap[y,x]);

		StartCoroutine(FindTarget_Djikstra());

//				if(tile.GetComponent<TileScript>().tileNeighbourCheck.moveStep > moveLimit)
//				{
//					tile.GetComponent<MeshRenderer>().material.color = Color.yellow;
//				}
//				else
//				{
//					tile.GetComponent<MeshRenderer>().material.color = Color.cyan;
//				}
				
	}

	IEnumerator FindTarget_Djikstra()
	{
		int x = node.tilePos[1];
		int y = node.tilePos[0];

		bool targetReached = false;

		List<TileScript> tempList = new List<TileScript>();

		while(!targetReached)
		{
			yield return new WaitForSeconds(0.1f);
			tempList = new List<TileScript>();

			int moveNum = 0;

			for(int i = 0; i < tilePath.Count; i++)
			{
				if(moveNum == 0)
				{
					if(!tilePath[i].tileNeighbourCheck.allSidesChecked)
					{
						moveNum = tilePath[i].tileNeighbourCheck.moveStep;
					}
				}
				else
				{
					if(!tilePath[i].tileNeighbourCheck.allSidesChecked)
					{
						if(moveNum > tilePath[i].tileNeighbourCheck.moveStep)
						{
							moveNum = tilePath[i].tileNeighbourCheck.moveStep;
						}
					}
				}
			}

			for(int i = 0; i < tilePath.Count; i++)
			{
				x = tilePath[i].tilePos[1];
				y = tilePath[i].tilePos[0];
				
				if(!tileMap[y,x].tileNeighbourCheck.allSidesChecked)
				{
					if(tileMap[y,x].tileNeighbourCheck.moveStep == moveNum)
					{
						if(y < mapSize.y - 1)
						{
							//Check north side
							if(tileMap[y+1,x] != null)
							{
								if(!tileMap[y+1,x].tileNeighbourCheck.isOccupied)
								{
									if(tileMap[y,x].tileNeighbourCheck.isConnectable_N && tileMap[y+1,x].tileNeighbourCheck.isConnectable_S 
										&& tileMap[y+1,x] != node)
									{
										if(!tileMap[y+1,x].tileNeighbourCheck.isChecked)
										{
											tileMap[y+1,x].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y+1,x].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y+1,x].tileNeighbourCheck.isChecked = true;
											tempList.Add(tileMap[y+1,x]);

											tileMap[y+1,x].rend.color = Color.yellow;
											tileMap[y+1,x].stepText.text = tileMap[y+1,x].tileNeighbourCheck.moveStep.ToString();

											if(tileMap[y+1,x] == selectedTile)
											{
												tileMap[y+1,x].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
										else
										{
											if(tileMap[y+1,x].tileNeighbourCheck.moveStep > tileMap[y,x].tileNeighbourCheck.moveStep + 10)
											{
												tileMap[y+1,x].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y+1,x].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
												tileMap[y+1,x].tileNeighbourCheck.isChecked = true;
												tempList.Add(tileMap[y+1,x]);

												tileMap[y+1,x].rend.color = Color.yellow;
												tileMap[y+1,x].stepText.text = tileMap[y+1,x].tileNeighbourCheck.moveStep.ToString();

												if(tileMap[y+1,x] == selectedTile)
												{
													tileMap[y+1,x].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
										}
									}
								}
							}
						}

						if(y > 0)
						{
							//Check south side
							if(tileMap[y-1,x] != null)
							{
								if(!tileMap[y-1,x].tileNeighbourCheck.isOccupied)
								{
									if(tileMap[y,x].tileNeighbourCheck.isConnectable_S && tileMap[y-1,x].tileNeighbourCheck.isConnectable_N 
										&& tileMap[y-1,x] != node)
									{
										if(!tileMap[y-1,x].tileNeighbourCheck.isChecked)
										{
											tileMap[y-1,x].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y-1,x].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y-1,x].tileNeighbourCheck.isChecked = true;
											tempList.Add(tileMap[y-1,x]);

											tileMap[y-1,x].rend.color = Color.yellow;
											tileMap[y-1,x].stepText.text = tileMap[y-1,x].tileNeighbourCheck.moveStep.ToString();

											if(tileMap[y-1,x] == selectedTile)
											{
												tileMap[y-1,x].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
										else
										{
											if(tileMap[y-1,x].tileNeighbourCheck.moveStep > tileMap[y,x].tileNeighbourCheck.moveStep + 10)
											{
												tileMap[y-1,x].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y-1,x].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
												tileMap[y-1,x].tileNeighbourCheck.isChecked = true;
												tempList.Add(tileMap[y-1,x]);

												tileMap[y-1,x].rend.color = Color.yellow;
												tileMap[y-1,x].stepText.text = tileMap[y-1,x].tileNeighbourCheck.moveStep.ToString();

												if(tileMap[y-1,x] == selectedTile)
												{
													tileMap[y-1,x].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
										}
									}
								}
							}
						}

						if(x < mapSize.x - 1)
						{
							//Check east side
							if(tileMap[y,x+1] != null)
							{
								if(!tileMap[y,x+1].tileNeighbourCheck.isOccupied)
								{
									if(tileMap[y,x].tileNeighbourCheck.isConnectable_E && tileMap[y,x+1].tileNeighbourCheck.isConnectable_W 
										&& tileMap[y,x+1] != node)
									{
										if(!tileMap[y,x+1].tileNeighbourCheck.isChecked)
										{
											tileMap[y,x+1].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y,x+1].tileNeighbourCheck.isChecked = true;
											tempList.Add(tileMap[y,x+1]);

											tileMap[y,x+1].rend.color = Color.yellow;
											tileMap[y,x+1].stepText.text = tileMap[y,x+1].tileNeighbourCheck.moveStep.ToString();

											if(tileMap[y,x+1] == selectedTile)
											{
												tileMap[y,x+1].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
										else
										{
											if(tileMap[y,x+1].tileNeighbourCheck.moveStep > tileMap[y,x].tileNeighbourCheck.moveStep + 10)
											{
												tileMap[y,x+1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
												tileMap[y,x+1].tileNeighbourCheck.isChecked = true;
												tempList.Add(tileMap[y,x+1]);

												tileMap[y,x+1].rend.color = Color.yellow;
												tileMap[y,x+1].stepText.text = tileMap[y,x+1].tileNeighbourCheck.moveStep.ToString();

												if(tileMap[y,x+1] == selectedTile)
												{
													tileMap[y,x+1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
										}
									}
								}
							}
						}

						if(x > 0)
						{
							//Check west side
							if(tileMap[y,x-1] != null)
							{
								if(!tileMap[y,x-1].tileNeighbourCheck.isOccupied)
								{
									if(tileMap[y,x].tileNeighbourCheck.isConnectable_W && tileMap[y,x-1].tileNeighbourCheck.isConnectable_E 
										&& tileMap[y,x-1] != node)
									{
										if(!tileMap[y,x-1].tileNeighbourCheck.isChecked)
										{
											tileMap[y,x-1].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y,x-1].tileNeighbourCheck.isChecked = true;
											tempList.Add(tileMap[y,x-1]);

											tileMap[y,x-1].rend.color = Color.yellow;
											tileMap[y,x-1].stepText.text = tileMap[y,x-1].tileNeighbourCheck.moveStep.ToString();

											if(tileMap[y,x-1] == selectedTile)
											{
												tileMap[y,x-1].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
										else
										{
											if(tileMap[y,x-1].tileNeighbourCheck.moveStep > tileMap[y,x].tileNeighbourCheck.moveStep + 10)
											{
												tileMap[y,x-1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
												tileMap[y,x-1].tileNeighbourCheck.isChecked = true;
												tempList.Add(tileMap[y,x-1]);

												tileMap[y,x-1].rend.color = Color.yellow;
												tileMap[y,x-1].stepText.text = tileMap[y,x-1].tileNeighbourCheck.moveStep.ToString();

												if(tileMap[y,x-1] == selectedTile)
												{
													tileMap[y,x-1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
										}
									}
								}
							}
						}

						if(allowDiagonal)
						{
							if(x > 0 && y < mapSize.y - 1)
							{
								//Check northwest side
								if(tileMap[y+1,x-1] != null)
								{
									if(!tileMap[y+1,x-1].tileNeighbourCheck.isOccupied)
									{
										if(tileMap[y,x].tileNeighbourCheck.isConnectable_NW && tileMap[y+1,x-1].tileNeighbourCheck.isConnectable_SE 
											&& tileMap[y+1,x-1] != node)
										{
											if(!tileMap[y+1,x-1].tileNeighbourCheck.isChecked)
											{
												tileMap[y+1,x-1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y+1,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y+1,x-1].tileNeighbourCheck.isChecked = true;
												tempList.Add(tileMap[y+1,x-1]);

												tileMap[y+1,x-1].rend.color = Color.yellow;
												tileMap[y+1,x-1].stepText.text = moveNum.ToString();

												if(tileMap[y+1,x-1] == selectedTile)
												{
													tileMap[y+1,x-1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
											else
											{
												if(tileMap[y+1,x-1].tileNeighbourCheck.moveStep > tileMap[y,x].tileNeighbourCheck.moveStep + 14)
												{
													tileMap[y+1,x-1].tileNeighbourCheck.prev = tileMap[y,x];
													tileMap[y+1,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
													tileMap[y+1,x-1].tileNeighbourCheck.isChecked = true;
													tempList.Add(tileMap[y+1,x-1]);

													tileMap[y+1,x-1].rend.color = Color.yellow;
													tileMap[y+1,x-1].stepText.text = tileMap[y+1,x-1].tileNeighbourCheck.moveStep.ToString();

													if(tileMap[y+1,x-1] == selectedTile)
													{
														tileMap[y+1,x-1].rend.color = Color.red;
														targetReached = true;
														break;
													}
												}
											}
										}
									}
								}
							}

							if(x < mapSize.x - 1 && y < mapSize.y - 1)
							{
								//Check northeast side
								if(tileMap[y+1,x+1] != null)
								{
									if(!tileMap[y+1,x+1].tileNeighbourCheck.isOccupied)
									{
										if(tileMap[y,x].tileNeighbourCheck.isConnectable_NE && tileMap[y+1,x+1].tileNeighbourCheck.isConnectable_SW 
											&& tileMap[y+1,x+1] != node)
										{
											if(!tileMap[y+1,x+1].tileNeighbourCheck.isChecked)
											{
												tileMap[y+1,x+1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y+1,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y+1,x+1].tileNeighbourCheck.isChecked = true;
												tempList.Add(tileMap[y+1,x+1]);

												tileMap[y+1,x+1].rend.color = Color.yellow;
												tileMap[y+1,x+1].stepText.text = tileMap[y+1,x+1].tileNeighbourCheck.moveStep.ToString();

												if(tileMap[y+1,x+1] == selectedTile)
												{
													tileMap[y+1,x+1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
											else
											{
												if(tileMap[y+1,x+1].tileNeighbourCheck.moveStep > tileMap[y,x].tileNeighbourCheck.moveStep + 14)
												{
													tileMap[y+1,x+1].tileNeighbourCheck.prev = tileMap[y,x];
													tileMap[y+1,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
													tileMap[y+1,x+1].tileNeighbourCheck.isChecked = true;
													tempList.Add(tileMap[y+1,x+1]);

													tileMap[y+1,x+1].rend.color = Color.yellow;
													tileMap[y+1,x+1].stepText.text = tileMap[y+1,x+1].tileNeighbourCheck.moveStep.ToString();

													if(tileMap[y+1,x+1] == selectedTile)
													{
														tileMap[y+1,x+1].rend.color = Color.red;
														targetReached = true;
														break;
													}
												}
											}
										}
									}
								}
							}

							if(x < mapSize.x - 1 && y > 0)
							{
								//Check southeast side
								if(tileMap[y-1,x+1] != null)
								{
									if(!tileMap[y-1,x+1].tileNeighbourCheck.isOccupied)
									{
										if(tileMap[y,x].tileNeighbourCheck.isConnectable_SE && tileMap[y-1,x+1].tileNeighbourCheck.isConnectable_NW 
											&& tileMap[y-1,x+1] != node)
										{
											if(!tileMap[y-1,x+1].tileNeighbourCheck.isChecked)
											{
												tileMap[y-1,x+1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y-1,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y-1,x+1].tileNeighbourCheck.isChecked = true;
												tempList.Add(tileMap[y-1,x+1]);

												tileMap[y-1,x+1].rend.color = Color.yellow;
												tileMap[y-1,x+1].stepText.text = tileMap[y-1,x+1].tileNeighbourCheck.moveStep.ToString();

												if(tileMap[y-1,x+1] == selectedTile)
												{
													tileMap[y-1,x+1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
											else
											{
												if(tileMap[y-1,x+1].tileNeighbourCheck.moveStep > tileMap[y,x].tileNeighbourCheck.moveStep + 14)
												{
													tileMap[y-1,x+1].tileNeighbourCheck.prev = tileMap[y,x];
													tileMap[y-1,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
													tileMap[y-1,x+1].tileNeighbourCheck.isChecked = true;
													tempList.Add(tileMap[y-1,x+1]);

													tileMap[y-1,x+1].rend.color = Color.yellow;
													tileMap[y-1,x+1].stepText.text = tileMap[y-1,x+1].tileNeighbourCheck.moveStep.ToString();

													if(tileMap[y-1,x+1] == selectedTile)
													{
														tileMap[y-1,x+1].rend.color = Color.red;
														targetReached = true;
														break;
													}
												}
											}
										}
									}
								}
							}

							if(x > 0 && y > 0)
							{
								//Check southwest side
								if(tileMap[y-1,x-1] != null)
								{
									if(!tileMap[y-1,x-1].tileNeighbourCheck.isOccupied)
									{
										if(tileMap[y,x].tileNeighbourCheck.isConnectable_SW && tileMap[y-1,x-1].tileNeighbourCheck.isConnectable_NE 
											&& tileMap[y-1,x-1] != node)
										{
											if(!tileMap[y-1,x-1].tileNeighbourCheck.isChecked)
											{
												tileMap[y-1,x-1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y-1,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y-1,x-1].tileNeighbourCheck.isChecked = true;
												tempList.Add(tileMap[y-1,x-1]);

												tileMap[y-1,x-1].rend.color = Color.yellow;
												tileMap[y-1,x-1].stepText.text = tileMap[y-1,x-1].tileNeighbourCheck.moveStep.ToString();

												if(tileMap[y-1,x-1] == selectedTile)
												{
													tileMap[y-1,x-1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
											else
											{
												if(tileMap[y-1,x-1].tileNeighbourCheck.moveStep > tileMap[y,x].tileNeighbourCheck.moveStep + 14)
												{
													tileMap[y-1,x-1].tileNeighbourCheck.prev = tileMap[y,x];
													tileMap[y-1,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
													tileMap[y-1,x-1].tileNeighbourCheck.isChecked = true;
													tempList.Add(tileMap[y-1,x-1]);

													tileMap[y-1,x-1].rend.color = Color.yellow;
													tileMap[y-1,x-1].stepText.text = tileMap[y-1,x-1].tileNeighbourCheck.moveStep.ToString();

													if(tileMap[y-1,x-1] == selectedTile)
													{
														tileMap[y-1,x-1].rend.color = Color.red;
														targetReached = true;
														break;
													}
												}
											}
										}
									}
								}
							}
						}

						tileMap[y,x].tileNeighbourCheck.allSidesChecked = true;
					}

				}
			}

			for(int i = 0; i < tempList.Count; i++)
			{
				tilePath.Add(tempList[i]);
			}

			yield return null;
		}

		tilePath = new List<TileScript>();

		bool exitLoop = false;
		tilePath.Add(selectedTile);
		TileScript tile = selectedTile.tileNeighbourCheck.prev;

		while(!exitLoop)
		{
			if(tile != node)
			{
				tile.rend.color = Color.blue;
				tilePath.Add(tile);
				tile = tile.tileNeighbourCheck.prev;
			}
			else
			{
				//canMove = true;
				exitLoop = true;
			}
		}

		finding = false;
	}

	public void FindPath_Astar()
	{
		for(int a = 0; a < mapSize.x; a++)
		{
			for(int b = 0; b < mapSize.y; b++)
			{
				tileMap[b,a].OnCanvas();
			}
		}

		finding = true;

		RecolourTile();

		node = currTile;

		node.rend.color = Color.green;
		selectedTile.rend.color = Color.red;

		for(int a = 0; a < mapSize.x; a++)
		{
			for(int b = 0; b < mapSize.y; b++)
			{
				tileMap[b,a].ResetTile();
			}
		}

		tilePath = new List<TileScript>();

		int x = node.tilePos[1];
		int y = node.tilePos[0];

		tilePath.Add(tileMap[y,x]);

		StartCoroutine(FindTarget_Astar());

		//				if(tile.GetComponent<TileScript>().tileNeighbourCheck.moveStep > moveLimit)
		//				{
		//					tile.GetComponent<MeshRenderer>().material.color = Color.yellow;
		//				}
		//				else
		//				{
		//					tile.GetComponent<MeshRenderer>().material.color = Color.cyan;
		//				}

	}

	IEnumerator FindTarget_Astar()
	{

		int x = node.tilePos[1];
		int y = node.tilePos[0];
		int moveNum = 1;
		bool targetReached = false;

		List<TileScript> tempList = new List<TileScript>();

		while(!targetReached)
		{
			int lowestStep = 0;

			yield return new WaitForSeconds(0.1f);
			tempList = new List<TileScript>();

			Debug.Log("A* loop start");
			Debug.Log("Tilelist count : " + tilePath.Count);

			for(int i = 0; i < tilePath.Count; i++)
			{
				if(lowestStep == 0)
				{
					if(!tilePath[i].tileNeighbourCheck.allSidesChecked)
					{
						Debug.Log("LowestStep");
						lowestStep = tilePath[i].tileNeighbourCheck.heurStep;
					}
				}
				else
				{
					if(lowestStep >= tilePath[i].tileNeighbourCheck.heurStep)
					{
						if(!tilePath[i].tileNeighbourCheck.allSidesChecked)
						{
							Debug.Log("LowestStep");
							lowestStep = tilePath[i].tileNeighbourCheck.heurStep;
						}
					}
				}

			}

			for(int i = 0; i < tilePath.Count; i++)
			{
				x = tilePath[i].tilePos[1];
				y = tilePath[i].tilePos[0];


				if(!tilePath[i].tileNeighbourCheck.allSidesChecked)
				{
					if(tilePath[i].tileNeighbourCheck.heurStep <= lowestStep)
					{
						Debug.Log("1");

						//					if(!tileMap[y,x].tileNeighbourCheck.allSidesChecked)
						//					{
						Debug.Log("2");
						if(y < mapSize.y - 1)
						{

							//Check north side
							if(tileMap[y+1,x] != null)
							{
								if(!tileMap[y+1,x].tileNeighbourCheck.isOccupied)
								{
									if(tileMap[y,x].tileNeighbourCheck.isConnectable_N && tileMap[y+1,x].tileNeighbourCheck.isConnectable_S 
										&& tileMap[y+1,x] != node)
									{
										Vector2 vect = new Vector2(tileMap[y+1,x].tilePos[1] - selectedTile.tilePos[1], tileMap[y+1,x].tilePos[0] - selectedTile.tilePos[0]);

										int check = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;

										if(tileMap[y+1,x].tileNeighbourCheck.heurStep == 0)
										{
											Debug.Log("CheckN");

											tileMap[y+1,x].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y+1,x].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y+1,x].tileNeighbourCheck.isChecked = true;

											tileMap[y+1,x].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
											tileMap[y+1,x].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + tileMap[y+1,x].tileNeighbourCheck.h;

											Debug.Log("Templist Add");
											tempList.Add(tileMap[y+1,x]);

											tileMap[y+1,x].rend.color = Color.yellow;
											tileMap[y+1,x].stepText.text = tileMap[y+1,x].tileNeighbourCheck.heurStep.ToString();

											if(tileMap[y+1,x] == selectedTile)
											{
												tileMap[y+1,x].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
										else if(tileMap[y+1,x].tileNeighbourCheck.heurStep >= check)
										{
											Debug.Log("CheckN");

											tileMap[y+1,x].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y+1,x].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y+1,x].tileNeighbourCheck.isChecked = true;

											tileMap[y+1,x].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
											tileMap[y+1,x].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + tileMap[y+1,x].tileNeighbourCheck.h;

											Debug.Log("Templist Add");
											tempList.Add(tileMap[y+1,x]);

											tileMap[y+1,x].rend.color = Color.yellow;
											tileMap[y+1,x].stepText.text = tileMap[y+1,x].tileNeighbourCheck.heurStep.ToString();

											if(tileMap[y+1,x] == selectedTile)
											{
												tileMap[y+1,x].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
									}
								}
							}
						}

						if(y > 0)
						{
							//Check south side
							if(tileMap[y-1,x] != null)
							{
								if(!tileMap[y-1,x].tileNeighbourCheck.isOccupied)
								{
									if(tileMap[y,x].tileNeighbourCheck.isConnectable_S && tileMap[y-1,x].tileNeighbourCheck.isConnectable_N 
										&& tileMap[y-1,x] != node)
									{

										Vector2 vect = new Vector2(tileMap[y-1,x].tilePos[1] - selectedTile.tilePos[1], tileMap[y-1,x].tilePos[0] - selectedTile.tilePos[0]);

										int check = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;

										if(tileMap[y-1,x].tileNeighbourCheck.heurStep == 0)
										{
											Debug.Log("CheckS");
											tileMap[y-1,x].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y-1,x].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y-1,x].tileNeighbourCheck.isChecked = true;

											tileMap[y-1,x].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
											tileMap[y-1,x].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + tileMap[y-1,x].tileNeighbourCheck.h;

											Debug.Log("Templist Add");

											tempList.Add(tileMap[y-1,x]);

											tileMap[y-1,x].rend.color = Color.yellow;
											tileMap[y-1,x].stepText.text = tileMap[y-1,x].tileNeighbourCheck.heurStep.ToString();

											if(tileMap[y-1,x] == selectedTile)
											{
												tileMap[y-1,x].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
										else if(tileMap[y-1,x].tileNeighbourCheck.heurStep >= check)
										{
											Debug.Log("CheckS");
											tileMap[y-1,x].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y-1,x].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y-1,x].tileNeighbourCheck.isChecked = true;

											tileMap[y-1,x].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
											tileMap[y-1,x].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + tileMap[y-1,x].tileNeighbourCheck.h;

											Debug.Log("Templist Add");

											tempList.Add(tileMap[y-1,x]);

											tileMap[y-1,x].rend.color = Color.yellow;
											tileMap[y-1,x].stepText.text = tileMap[y-1,x].tileNeighbourCheck.heurStep.ToString();

											if(tileMap[y-1,x] == selectedTile)
											{
												tileMap[y-1,x].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
									}
								}
							}
						}

						if(x < mapSize.x - 1)
						{
							//Check east side
							if(tileMap[y,x+1] != null)
							{
								if(!tileMap[y,x+1].tileNeighbourCheck.isOccupied)
								{
									if(tileMap[y,x].tileNeighbourCheck.isConnectable_E && tileMap[y,x+1].tileNeighbourCheck.isConnectable_W 
										&& tileMap[y,x+1] != node)
									{
										Vector2 vect = new Vector2(tileMap[y,x+1].tilePos[1] - selectedTile.tilePos[1], tileMap[y,x+1].tilePos[0] - selectedTile.tilePos[0]);

										int check = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;

										if(tileMap[y,x+1].tileNeighbourCheck.heurStep == 0)
										{
											Debug.Log("CheckE");
											tileMap[y,x+1].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y,x+1].tileNeighbourCheck.isChecked = true;

											tileMap[y,x+1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
											tileMap[y,x+1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + tileMap[y,x+1].tileNeighbourCheck.h;

											Debug.Log("Templist Add");

											tempList.Add(tileMap[y,x+1]);

											tileMap[y,x+1].rend.color = Color.yellow;
											tileMap[y,x+1].stepText.text = tileMap[y,x+1].tileNeighbourCheck.heurStep.ToString();

											if(tileMap[y,x+1] == selectedTile)
											{
												tileMap[y,x+1].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
										else if(tileMap[y,x+1].tileNeighbourCheck.heurStep >= check)
										{
											Debug.Log("CheckE");
											tileMap[y,x+1].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y,x+1].tileNeighbourCheck.isChecked = true;

											tileMap[y,x+1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
											tileMap[y,x+1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + tileMap[y,x+1].tileNeighbourCheck.h;

											Debug.Log("Templist Add");

											tempList.Add(tileMap[y,x+1]);

											tileMap[y,x+1].rend.color = Color.yellow;
											tileMap[y,x+1].stepText.text = tileMap[y,x+1].tileNeighbourCheck.heurStep.ToString();

											if(tileMap[y,x+1] == selectedTile)
											{
												tileMap[y,x+1].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
									}
								}
							}
						}

						if(x > 0)
						{
							//Check west side
							if(tileMap[y,x-1] != null)
							{
								if(!tileMap[y,x-1].tileNeighbourCheck.isOccupied)
								{
									if(tileMap[y,x].tileNeighbourCheck.isConnectable_W && tileMap[y,x-1].tileNeighbourCheck.isConnectable_E 
										&& tileMap[y,x-1] != node)
									{
										Vector2 vect = new Vector2(tileMap[y,x-1].tilePos[1] - selectedTile.tilePos[1], tileMap[y,x-1].tilePos[0] - selectedTile.tilePos[0]);

										int check = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;

										if(tileMap[y,x-1].tileNeighbourCheck.heurStep == 0)
										{
											Debug.Log("CheckW");
											tileMap[y,x-1].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y,x-1].tileNeighbourCheck.isChecked = true;

											tileMap[y,x-1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
											tileMap[y,x-1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + tileMap[y,x-1].tileNeighbourCheck.h;

											Debug.Log("Templist Add");

											tempList.Add(tileMap[y,x-1]);

											tileMap[y,x-1].rend.color = Color.yellow;
											tileMap[y,x-1].stepText.text = tileMap[y,x-1].tileNeighbourCheck.heurStep.ToString();

											if(tileMap[y,x-1] == selectedTile)
											{
												tileMap[y,x-1].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
										else if(tileMap[y,x-1].tileNeighbourCheck.heurStep >= check)
										{
											Debug.Log("CheckW");
											tileMap[y,x-1].tileNeighbourCheck.prev = tileMap[y,x];
											tileMap[y,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10;
											tileMap[y,x-1].tileNeighbourCheck.isChecked = true;

											tileMap[y,x-1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
											tileMap[y,x-1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 10 + tileMap[y,x-1].tileNeighbourCheck.h;

											Debug.Log("Templist Add");

											tempList.Add(tileMap[y,x-1]);

											tileMap[y,x-1].rend.color = Color.yellow;
											tileMap[y,x-1].stepText.text = tileMap[y,x-1].tileNeighbourCheck.heurStep.ToString();

											if(tileMap[y,x-1] == selectedTile)
											{
												tileMap[y,x-1].rend.color = Color.red;
												targetReached = true;
												break;
											}
										}
									}
								}
							}
						}

						if(allowDiagonal)
						{
							if(x > 0 && y < mapSize.y - 1)
							{
								//Check northwest side
								if(tileMap[y+1,x-1] != null)
								{
									if(!tileMap[y+1,x-1].tileNeighbourCheck.isOccupied)
									{
										if(tileMap[y,x].tileNeighbourCheck.isConnectable_NW && tileMap[y+1,x-1].tileNeighbourCheck.isConnectable_SE 
											&& tileMap[y+1,x-1] != node)
										{
											Vector2 vect = new Vector2(tileMap[y+1,x-1].tilePos[1] - selectedTile.tilePos[1], tileMap[y+1,x-1].tilePos[0] - selectedTile.tilePos[0]);

											int check = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;

											if(tileMap[y+1,x-1].tileNeighbourCheck.heurStep == 0)
											{
												Debug.Log("CheckW");
												tileMap[y+1,x-1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y+1,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y+1,x-1].tileNeighbourCheck.isChecked = true;

												tileMap[y+1,x-1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
												tileMap[y+1,x-1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + tileMap[y+1,x-1].tileNeighbourCheck.h;

												Debug.Log("Templist Add");

												tempList.Add(tileMap[y+1,x-1]);

												tileMap[y+1,x-1].rend.color = Color.yellow;
												tileMap[y+1,x-1].stepText.text = tileMap[y+1,x-1].tileNeighbourCheck.heurStep.ToString();

												if(tileMap[y+1,x-1] == selectedTile)
												{
													tileMap[y+1,x-1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
											else if(tileMap[y+1,x-1].tileNeighbourCheck.heurStep >= check)
											{
												Debug.Log("CheckW");
												tileMap[y+1,x-1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y+1,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y+1,x-1].tileNeighbourCheck.isChecked = true;

												tileMap[y+1,x-1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
												tileMap[y+1,x-1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + tileMap[y+1,x-1].tileNeighbourCheck.h;

												Debug.Log("Templist Add");

												tempList.Add(tileMap[y+1,x-1]);

												tileMap[y+1,x-1].rend.color = Color.yellow;
												tileMap[y+1,x-1].stepText.text = tileMap[y+1,x-1].tileNeighbourCheck.heurStep.ToString();

												if(tileMap[y+1,x-1] == selectedTile)
												{
													tileMap[y+1,x-1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
										}
									}
								}
							}

							if(x < mapSize.x - 1 && y < mapSize.y - 1)
							{
								//Check northeast side
								if(tileMap[y+1,x+1] != null)
								{
									if(!tileMap[y+1,x+1].tileNeighbourCheck.isOccupied)
									{
										if(tileMap[y,x].tileNeighbourCheck.isConnectable_NE && tileMap[y+1,x+1].tileNeighbourCheck.isConnectable_SW 
											&& tileMap[y+1,x+1] != node)
										{
											Vector2 vect = new Vector2(tileMap[y+1,x+1].tilePos[1] - selectedTile.tilePos[1], tileMap[y+1,x+1].tilePos[0] - selectedTile.tilePos[0]);

											int check = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;

											if(tileMap[y+1,x+1].tileNeighbourCheck.heurStep == 0)
											{
												Debug.Log("CheckW");
												tileMap[y+1,x+1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y+1,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y+1,x+1].tileNeighbourCheck.isChecked = true;

												tileMap[y+1,x+1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
												tileMap[y+1,x+1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + tileMap[y+1,x+1].tileNeighbourCheck.h;

												Debug.Log("Templist Add");

												tempList.Add(tileMap[y+1,x+1]);

												tileMap[y+1,x+1].rend.color = Color.yellow;
												tileMap[y+1,x+1].stepText.text = tileMap[y+1,x+1].tileNeighbourCheck.heurStep.ToString();

												if(tileMap[y+1,x+1] == selectedTile)
												{
													tileMap[y+1,x+1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
											else if(tileMap[y+1,x+1].tileNeighbourCheck.heurStep >= check)
											{
												Debug.Log("CheckW");
												tileMap[y+1,x+1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y+1,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y+1,x+1].tileNeighbourCheck.isChecked = true;

												tileMap[y+1,x+1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
												tileMap[y+1,x+1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + tileMap[y+1,x+1].tileNeighbourCheck.h;

												Debug.Log("Templist Add");

												tempList.Add(tileMap[y+1,x+1]);

												tileMap[y+1,x+1].rend.color = Color.yellow;
												tileMap[y+1,x+1].stepText.text = tileMap[y+1,x+1].tileNeighbourCheck.heurStep.ToString();

												if(tileMap[y+1,x+1] == selectedTile)
												{
													tileMap[y+1,x+1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
										}
									}
								}
							}

							if(x < mapSize.x - 1 && y > 0)
							{
								//Check southeast side
								if(tileMap[y-1,x+1] != null)
								{
									if(!tileMap[y-1,x+1].tileNeighbourCheck.isOccupied)
									{
										if(tileMap[y,x].tileNeighbourCheck.isConnectable_SE && tileMap[y-1,x+1].tileNeighbourCheck.isConnectable_NW 
											&& tileMap[y-1,x+1] != node)
										{
											Vector2 vect = new Vector2(tileMap[y-1,x+1].tilePos[1] - selectedTile.tilePos[1], tileMap[y-1,x+1].tilePos[0] - selectedTile.tilePos[0]);

											int check = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;

											if(tileMap[y-1,x+1].tileNeighbourCheck.heurStep == 0)
											{
												Debug.Log("CheckW");
												tileMap[y-1,x+1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y-1,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y-1,x+1].tileNeighbourCheck.isChecked = true;

												tileMap[y-1,x+1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
												tileMap[y-1,x+1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + tileMap[y-1,x+1].tileNeighbourCheck.h;

												Debug.Log("Templist Add");

												tempList.Add(tileMap[y-1,x+1]);

												tileMap[y-1,x+1].rend.color = Color.yellow;
												tileMap[y-1,x+1].stepText.text = tileMap[y-1,x+1].tileNeighbourCheck.heurStep.ToString();

												if(tileMap[y-1,x+1] == selectedTile)
												{
													tileMap[y-1,x+1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
											else if(tileMap[y-1,x+1].tileNeighbourCheck.heurStep >= check)
											{
												Debug.Log("CheckW");
												tileMap[y-1,x+1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y-1,x+1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y-1,x+1].tileNeighbourCheck.isChecked = true;

												tileMap[y-1,x+1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
												tileMap[y-1,x+1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + tileMap[y-1,x+1].tileNeighbourCheck.h;

												Debug.Log("Templist Add");

												tempList.Add(tileMap[y-1,x+1]);

												tileMap[y-1,x+1].rend.color = Color.yellow;
												tileMap[y-1,x+1].stepText.text = tileMap[y-1,x+1].tileNeighbourCheck.heurStep.ToString();

												if(tileMap[y-1,x+1] == selectedTile)
												{
													tileMap[y-1,x+1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
										}
									}
								}
							}

							if(x > 0 && y > 0)
							{
								//Check southwest side
								if(tileMap[y-1,x-1] != null)
								{
									if(!tileMap[y-1,x-1].tileNeighbourCheck.isOccupied)
									{
										if(tileMap[y,x].tileNeighbourCheck.isConnectable_SW && tileMap[y-1,x-1].tileNeighbourCheck.isConnectable_NE 
											&& tileMap[y-1,x-1] != node)
										{
											Vector2 vect = new Vector2(tileMap[y-1,x-1].tilePos[1] - selectedTile.tilePos[1], tileMap[y-1,x-1].tilePos[0] - selectedTile.tilePos[0]);

											int check = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;

											if(tileMap[y-1,x-1].tileNeighbourCheck.heurStep == 0)
											{
												Debug.Log("CheckW");
												tileMap[y-1,x-1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y-1,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y-1,x-1].tileNeighbourCheck.isChecked = true;

												tileMap[y-1,x-1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
												tileMap[y-1,x-1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + tileMap[y-1,x-1].tileNeighbourCheck.h;

												Debug.Log("Templist Add");

												tempList.Add(tileMap[y-1,x-1]);

												tileMap[y-1,x-1].rend.color = Color.yellow;
												tileMap[y-1,x-1].stepText.text = tileMap[y-1,x-1].tileNeighbourCheck.heurStep.ToString();

												if(tileMap[y-1,x-1] == selectedTile)
												{
													tileMap[y-1,x-1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
											else if(tileMap[y-1,x-1].tileNeighbourCheck.heurStep >= check)
											{
												Debug.Log("CheckW");
												tileMap[y-1,x-1].tileNeighbourCheck.prev = tileMap[y,x];
												tileMap[y-1,x-1].tileNeighbourCheck.moveStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14;
												tileMap[y-1,x-1].tileNeighbourCheck.isChecked = true;

												tileMap[y-1,x-1].tileNeighbourCheck.h = ((int)Mathf.Abs(vect.x) + (int)Mathf.Abs(vect.y)) * weight;
												tileMap[y-1,x-1].tileNeighbourCheck.heurStep = tileMap[y,x].tileNeighbourCheck.moveStep + 14 + tileMap[y-1,x-1].tileNeighbourCheck.h;

												Debug.Log("Templist Add");

												tempList.Add(tileMap[y-1,x-1]);

												tileMap[y-1,x-1].rend.color = Color.yellow;
												tileMap[y-1,x-1].stepText.text = tileMap[y-1,x-1].tileNeighbourCheck.heurStep.ToString();

												if(tileMap[y-1,x-1] == selectedTile)
												{
													tileMap[y-1,x-1].rend.color = Color.red;
													targetReached = true;
													break;
												}
											}
										}
									}
								}
							}
						}

						tileMap[y,x].tileNeighbourCheck.allSidesChecked = true;
						//					}
					}
				}
			}

			for(int i = 0; i < tempList.Count; i++)
			{
				tilePath.Add(tempList[i]);

				Debug.Log("Templist count : " + tempList.Count);
			}

			moveNum++;

			yield return null;
		}

		tilePath = new List<TileScript>();

		bool exitLoop = false;
		tilePath.Add(selectedTile);
		TileScript tile = selectedTile.tileNeighbourCheck.prev;

		while(!exitLoop)
		{
			if(tile != node)
			{
				tile.rend.color = Color.blue;
				tilePath.Add(tile);
				tile = tile.tileNeighbourCheck.prev;
			}
			else
			{
				//canMove = true;
				exitLoop = true;
			}
		}

		finding = false;
	}

	public void SetStart()
	{
		if(!finding)
		{
			for(int x = 0; x < mapSize.x; x++)
			{
				for(int y = 0; y < mapSize.y; y++)
				{
					tileMap[y,x].OffCanvas();
				}
			}

			RecolourTile();

			if(!selectingStart)
			{
				selectingStart = true;

				startBorder.SetActive(true);
			}
			else
			{
				selectingStart = false;

				startBorder.SetActive(false);
			}

			if(currTile != null)
			{
				currTile.rend.color = Color.green;
			}

			if(selectedTile != null)
			{
				selectedTile.rend.color = Color.red;
			}
		}

	}

	public void SetEnd()
	{
		if(!finding)
		{
			for(int x = 0; x < mapSize.x; x++)
			{
				for(int y = 0; y < mapSize.y; y++)
				{
					tileMap[y,x].OffCanvas();
				}
			}

			RecolourTile();

			if(!selectingEnd)
			{
				selectingEnd = true;

				endBorder.SetActive(true);
			}
			else
			{
				selectingEnd = false;

				endBorder.SetActive(false);
			}

			if(currTile != null)
			{
				currTile.rend.color = Color.green;
			}

			if(selectedTile != null)
			{
				selectedTile.rend.color = Color.red;
			}
		}

	}

	public void ClearBox()
	{
		if(!finding)
		{
			for(int x = 0; x < mapSize.x; x++)
			{
				for(int y = 0; y < mapSize.y; y++)
				{
					if(tileMap[y,x].tileNeighbourCheck.isOccupied)
					{
						tileMap[y,x].tileNeighbourCheck.isOccupied = false;
						tileMap[y,x].rend.color = Color.white;
					}
				}
			}
		}

	}

	public void ToggleDiagonal()
	{
		allowDiagonal = !allowDiagonal;

		if(allowDiagonal)
		{
			diagonalBorder.SetActive(true);

			diagonalText.text = "Set Diagonal: On";
		}
		else
		{
			diagonalBorder.SetActive(false);

			diagonalText.text = "Set Diagonal: Off";
		}


	}
}
