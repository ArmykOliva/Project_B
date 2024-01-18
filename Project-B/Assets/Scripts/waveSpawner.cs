using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
	public List<EnemySpawnInfo> enemies;
	public int nextWaveSeconds;
}

[System.Serializable]
public class EnemySpawnInfo
{
	public GameObject enemyPrefab;
	public float spawnTime; // Time after the wave starts when the enemy spawns
	
	[HideInInspector] public bool spawned = false;
}

public class waveSpawner : MonoBehaviour
{
	public List<EnemyWave> waves;
	public EnemyBorder spawnBorder;
	public EnemyBorder enemyBorder; // the border where the enemy will be moving most of the time
	public EnemyBorder enemyBorderSubmarine; // the border where the enemy will be attacking (shooting, biting, charging...)

	private int currentWaveIndex = 0;
	private float waveTimer;
	private List<GameObject> activeEnemies = new List<GameObject>();
	private bool waitingForNextWave = false;

	private void Update()
	{
		if (waitingForNextWave)
		{
			return;
		}

		waveTimer += Time.deltaTime;
		CheckForSpawns();
		CheckWaveCompletion();
	}

	private void StartWave(int waveIndex)
	{
		Debug.Log("wave " +  waveIndex);
		waveTimer = 0;
		waitingForNextWave = false;
		// Initialize things specific to the wave
	}

	private void CheckForSpawns()
	{
		foreach (var enemyInfo in waves[currentWaveIndex].enemies)
		{
			if (!enemyInfo.spawned && waveTimer >= enemyInfo.spawnTime)
			{
				SpawnEnemy(enemyInfo);
				enemyInfo.spawned = true;
			}
		}
	}

	private void SpawnEnemy(EnemySpawnInfo enemyInfo)
	{
		// Generate a random position within the specified ranges
		Vector3 spawnPosition = spawnBorder.GetRandomPointInCircle(transform.position, 0f);

		GameObject enemyObject = Instantiate(enemyInfo.enemyPrefab, spawnPosition, transform.rotation);
		EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();

		if (enemy != null)
		{
			enemy.enemyBorder = enemyBorder;
			enemy.enemyBorderSubmarine = enemyBorderSubmarine;
		}
		else
		{
			Debug.LogError("Enemy prefab does not have an EnemyBase component attached.");
		}
		activeEnemies.Add(enemyObject);
	}

	private void CheckWaveCompletion()
	{
		activeEnemies.RemoveAll(item => item == null);

		if (waves[currentWaveIndex].enemies.All(e => e.spawned) && activeEnemies.Count == 0)
		{
			StartCoroutine(WaitAndStartNextWave());
		}
	}

	IEnumerator WaitAndStartNextWave()
	{
		Debug.Log("wave ended");
		waitingForNextWave = true;
		yield return new WaitForSeconds(waves[currentWaveIndex].nextWaveSeconds); // Wait for 30 seconds

		currentWaveIndex = (currentWaveIndex + 1) % waves.Count;
		StartWave(currentWaveIndex);
	}


}
