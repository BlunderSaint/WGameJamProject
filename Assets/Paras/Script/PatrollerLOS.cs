using UnityEngine;
using System.Collections;

public class PatrollerLOS : EnemyBase
{
	[Header("Patrol Points")]
	public Transform pointA;
	public Transform pointB;
	public float patrolSpeed = 2f;
	public float chaseSpeed = 4f;
	public float waitTime = 1f;

	[Header("Detection Settings")]
	public float viewDistance = 5f;
	public LayerMask obstacleLayer; // Should include 'Ground' but NOT 'Player'
	public float detectionRefreshRate = 0.1f;

	[Header("Audio Settings")]
	public AudioClip attackSound; // Drag your attack sound here in Inspector
	public AudioSource attackSource;
	public AudioClip spawnSound;  // Drag your spawn sound here in Inspector
	public AudioSource spawnSource;

	private Transform targetPoint;
	private Transform player;
	private bool isWaiting = false;
	private bool isChasing = false;
	private bool playerInSight = false; // Track if player was in sight last frame
	//public PlayerCombat playercombat;

	protected override void Awake()
	{
		base.Awake();
		targetPoint = pointB;

		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		if (playerObj != null) player = playerObj.transform;

		// Get or add AudioSource components
		attackSource = GetComponent<AudioSource>();
		if (attackSource == null)
			attackSource = gameObject.AddComponent<AudioSource>();

		// Ensure we have spawnSource component
		spawnSource = GetComponent<AudioSource>();
		if (spawnSource == null)
			spawnSource = gameObject.AddComponent<AudioSource>();
	}

	void Update()
	{
		// 1. Look for player
		CheckLineOfSight();

		if (isChasing)
		{
			ChasePlayer();
		}
		else if (!isWaiting)
		{
			Patrol();
		}

		if (Input.GetKeyDown(KeyCode.K))
			DebugTakeDamage();
	}

	void CheckLineOfSight()
	{
		if (player == null) return;

		// Calculate direction based on where the enemy is facing
		Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

		// Get player position relative to enemy
		Vector2 toPlayer = player.position - transform.position;
		float distanceToPlayer = toPlayer.magnitude;

		// First check if player is in view distance
		bool playerInRange = distanceToPlayer <= viewDistance;

		// Check if player is in front of the enemy (within field of view)
		float dotProduct = Vector2.Dot(direction.normalized, toPlayer.normalized);
		bool playerInFront = dotProduct > 0.7f; // ~45 degree FOV

		// Cast a ray to check for obstacles
		RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, viewDistance, obstacleLayer);

		// Check if player is visible (no obstacles in the way OR player is closer than obstacle)
		bool playerVisible = false;
		if (hit.collider != null)
		{
			// Player is visible if the ray hits the player (no obstacle in between)
			if (hit.collider.CompareTag("Player"))
			{
				playerVisible = true;
			}
			// If we hit something else first, check if player is closer than the obstacle
			else if (distanceToPlayer < hit.distance)
			{
				playerVisible = true;
			}
		}
		else
		{
			// No obstacles hit, but check if player is in range and in front
			playerVisible = playerInRange && playerInFront;
		}

		// Player just came into sight
		if (playerVisible && !playerInSight)
		{
			Debug.Log($"Player detected! Distance: {distanceToPlayer}");
			PlaySpawnSound(); // Play spawn sound when first detecting player
			isChasing = true;
		}
		// Player just left sight
		else if (!playerVisible && isChasing)
		{
			// Optionally, add a short delay before losing chase
			// For now, immediately lose chase when player leaves sight
			isChasing = false;
			targetPoint = GetNearestPoint();
		}

		playerInSight = playerVisible;

		// Debug visualization
		Debug.DrawRay(transform.position, direction * viewDistance, playerVisible ? Color.green : Color.red);
		if (playerVisible)
		{
			Debug.DrawLine(transform.position, player.position, Color.yellow);
		}
	}

	void Patrol()
	{
		MoveTowards(targetPoint.position, patrolSpeed);

		if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
		{
			StartCoroutine(SwitchTarget());
		}
	}

	void ChasePlayer()
	{
		// Move towards player on X axis only (Ground Patroller logic)
		Vector2 playerPosOnLevel = new Vector2(player.position.x, transform.position.y);
		MoveTowards(playerPosOnLevel, chaseSpeed);

		// Stop chasing if player gets too far away
		if (Vector2.Distance(transform.position, player.position) > viewDistance * 1.5f)
		{
			isChasing = false;
			targetPoint = GetNearestPoint();
		}
	}

	void MoveTowards(Vector2 destination, float currentSpeed)
	{
		float step = currentSpeed * Time.deltaTime;
		transform.position = Vector2.MoveTowards(transform.position, destination, step);

		// Flip based on movement direction toward player when chasing
		if (isChasing)
		{
			// When chasing, always face the player
			if (player != null)
			{
				if (player.position.x > transform.position.x && transform.localScale.x < 0) Flip();
				else if (player.position.x < transform.position.x && transform.localScale.x > 0) Flip();
			}
		}
		else
		{
			// When patrolling, flip based on patrol destination
			if (destination.x > transform.position.x && transform.localScale.x < 0) Flip();
			else if (destination.x < transform.position.x && transform.localScale.x > 0) Flip();
		}
	}

	Transform GetNearestPoint()
	{
		return Vector2.Distance(transform.position, pointA.position) < Vector2.Distance(transform.position, pointB.position) ? pointA : pointB;
	}

	IEnumerator SwitchTarget()
	{
		isWaiting = true;
		yield return new WaitForSeconds(waitTime);
		targetPoint = (targetPoint == pointA) ? pointB : pointA;
		isWaiting = false;
	}

	void Flip()
	{
		Vector3 localScale = transform.localScale;
		localScale.x *= -1;
		transform.localScale = localScale;
	}

	void PlayAttackSound()
	{
		if (attackSound != null && attackSource != null)
		{
			attackSource.pitch = Random.Range(0.9f, 1.1f);
			attackSource.PlayOneShot(attackSound);
		}
	}

	void PlaySpawnSound()
	{
		if (spawnSound != null && spawnSource != null)
		{
			spawnSource.pitch = Random.Range(0.9f, 1.1f);
			spawnSource.PlayOneShot(spawnSound);
		}
	}

	public void DebugTakeDamage()
	{
		EnemyBase[] allEnemies = GameObject.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

		foreach (EnemyBase enemy in allEnemies)
			enemy.TakeDamage(20);

		Debug.Log("Debug: Damaged all enemies!");
	}

	private void OnDrawGizmos()
	{
		// Patrol Path
		if (pointA != null && pointB != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(pointA.position, pointB.position);
		}

		// Sight Line
		Gizmos.color = isChasing ? Color.red : Color.cyan;
		Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
		Gizmos.DrawRay(transform.position, direction * viewDistance);

		// Draw FOV cone
		if (Application.isPlaying && player != null)
		{
			Gizmos.color = playerInSight ? Color.green : Color.gray;
			float fovAngle = 45f;
			Vector3 forward = direction * viewDistance;
			Vector3 leftBoundary = Quaternion.Euler(0, 0, fovAngle) * forward;
			Vector3 rightBoundary = Quaternion.Euler(0, 0, -fovAngle) * forward;
			Gizmos.DrawRay(transform.position, leftBoundary);
			Gizmos.DrawRay(transform.position, rightBoundary);
			Gizmos.DrawLine(transform.position + leftBoundary, transform.position + rightBoundary);
		}
	}

	//protected virtual void OnTriggerEnter2D(Collider2D other)
	//{
	//	if (other.CompareTag("Player"))
	//	{
	//		Debug.Log($"{gameObject.name} hit the player!");

	//		// Play attack sound on contact
	//		PlayAttackSound();

	//		playercombat.TakeDamage(contactDamage, transform.position);
	//	}
	//}
}