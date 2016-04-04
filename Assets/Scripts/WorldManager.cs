using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour {

	public GameObject knightPrefab;
	public GameObject skellyPrefab;

	public List<Transform> knights;
	public List<Transform> skellies;

	public GameObject[] path;
	public Vector3[] trees;
	public Vector3[,] flowField;

	private bool spawnSkellies;

	// Use this for initialization
	void Start () {
		spawnSkellies = false;
		GetTrees();
		CreateFlowField();
		SpawnKnights();
		StartCoroutine(SkeletonSpawnDelay());
	}
	
	// Update is called once per frame
	void Update () {
		if(spawnSkellies && skellies.Count < 10) {
			GameObject skelly = Instantiate(skellyPrefab, 
				new Vector3(Random.Range(10, 90), 1, Random.Range(175, 195)), Quaternion.identity)as GameObject;
			skellies.Add(skelly.transform);
			SkellyScript ss = skelly.GetComponent<SkellyScript>();
			ss.wm = this;
			ss.village = transform.GetChild(0);
		}
	}

	private void GetTrees() {
		float terWidth = 100;
		float terHeight = 200;
		TreeInstance[] treeInsts = Terrain.activeTerrain.terrainData.treeInstances;
		trees = new Vector3[treeInsts.Length];
		for(int i = 0; i < treeInsts.Length; i++) {
			trees[i] = new Vector3(terWidth * treeInsts[i].position.x, 0, terHeight * treeInsts[i].position.z);
		}
	}

	private void CreateFlowField() {
		flowField = new Vector3[10,10];
		Vector3 center = new Vector3(5, 0, 0);
		for(int i = 0; i < 10; i++) {
			for(int j = 0; j < 10; j++) {
				flowField[i, j] = center - new Vector3(i, 0, j);
				flowField[i, j].Normalize();
			}
		}
	}

	private void SpawnKnights() {
		knights = new List<Transform>();
		for(int i = 0; i < 8; i++) {
			GameObject knight = Instantiate(knightPrefab, new Vector3(50 + Random.Range(-1, 1), 1, 40 - i), Quaternion.identity)
			                                as GameObject;
			knights.Add(knight.transform);
			KnightScript ks = knight.GetComponent<KnightScript>();
			if(i > 0) {
				ks.leader = knights[i - 1];
				ks.isleader = false;
			}
			else ks.isleader = true;
			ks.wm = this;
		}
	}

	private IEnumerator SkeletonSpawnDelay() {
		yield return new WaitForSeconds(30);
		spawnSkellies = true;
	}

	public void CheckKnightCount() {
		if(knights.Count <= 0) {
			foreach(Transform skelly in skellies) {
				skelly.GetComponent<SkellyScript>().dance = true;
			}
		}
	}
}
