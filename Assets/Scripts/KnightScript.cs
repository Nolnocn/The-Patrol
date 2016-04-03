using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]

public class KnightScript : AgentScript {
	
	public bool isleader;
	public Transform leader;

	void Start () {
		alive = true;
		attacking = false;
		target = null;
		health = 10;
		canAttack = true;

		acceleration = Vector3.zero;
		velocity = transform.forward;
		characterController = gameObject.GetComponent<CharacterController>();
		
		separationWt = 15.0f;
		flowWt = -5.0f;
		avoidWt = 10.0f;
		seekWt = 10.0f;
		maxSpeed *= .5f;
		range = 15;

		GetComponent<Animation>()["Walk"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["Walk"].speed = .75f;
		GetComponent<Animation>()["Idle"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["Sneak"].wrapMode = WrapMode.Loop;
	}
	
	// Update is called once per frame
	void Update () {
		if(alive) {
			if(!attacking) {
				CalcSteeringForce();
				//update velocity
				velocity += acceleration * Time.deltaTime;
				velocity.y = 0;	// we are staying in the x/z plane
				velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
				
				//orient the transform to face where we going
				if(velocity != Vector3.zero)
					transform.forward = velocity.normalized;
				
				// keep us grounded
				velocity.y -= gravity * Time.deltaTime;
				
				// the CharacterController moves us subject to physical constraints
				characterController.Move(velocity * Time.deltaTime);
				
				//reset acceleration for next cycle
				acceleration = Vector3.zero;
			}

			if(target != null) {
				if(Vector3.Distance(target.position, transform.position) < attackReach) {
					attacking = true;
					velocity = Vector3.zero;

					transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
				}
				else attacking = false;
			}
			else FindClosestEnemy();

			HandleAnimations();
		}
	}

	protected void CalcSteeringForce()
	{
		Vector3 force = Vector3.zero;

		// Don't run into others
		force += -separationWt * (Separate(wm.knights, tooClose) - velocity);
		force += -separationWt * (Separate(wm.skellies, tooClose) - velocity);

		if(target != null) {
			force += seekWt * Seek(target.position);
		}

		if(transform.position.z > 100) {
			if(target == null) {
				force += flowWt * Flow();
				force += seekWt * Seek(new Vector3(50, 0, 190));
			}
			foreach(Vector3 tree in wm.trees) {
				force += avoidWt * AvoidObstacle(tree, tooClose);
			}
		}
		else {
			if(target == null) {
				if(isleader) {
					force += seekWt * Seek (new Vector3(50, 0, 100));
				}
				else {
					if(leader.GetComponent<KnightScript>().Alive) force += seekWt * FollowLeader(leader);
					else force += seekWt * Seek (new Vector3(50, 0, 100));
				}
			}
		}
		
		force = Vector3.ClampMagnitude(force, maxForce);
		ApplyForce(force);
	}

	protected override void FindClosestEnemy() {
		float dist = 12345;
		Transform tempTar = null;
		foreach(Transform skelly in wm.skellies) {
			if(skelly.GetComponent<SkellyScript>().Alive) {
				float tempDist = Vector3.Distance(skelly.position, transform.position);
				if(tempDist < dist && !skelly.GetComponent<SkellyScript>().targetted) {
					dist = tempDist;
					tempTar = skelly;
				}
			}
		}

		if(dist < range) {
			target = tempTar;
			target.GetComponent<SkellyScript>().targetted = true;
		}
	}

	protected override void HandleAnimations() {
		if(!GetComponent<Animation>().IsPlaying("Attack")) {
			if(velocity.sqrMagnitude > 0) GetComponent<Animation>().Play("Walk");
			else GetComponent<Animation>().Play("Idle");

			if(attacking && canAttack) {
				StartCoroutine(AttackDelay());
			}
		}
	}

	private IEnumerator AttackDelay() {
		GetComponent<Animation>().Play("Attack");
		canAttack = false;
		yield return new WaitForSeconds(1);
		SkellyScript ss = target.GetComponent<SkellyScript>();
		if(alive && ss.Alive) ss.LowerHealth(transform);
		if(!ss.Alive) {
			target = null;
			attacking = false;
		}
		yield return new WaitForSeconds(1);
		canAttack = true;
	}

	public void LowerHealth(Transform attacker) {
		health--;

		if(!attacking) {
			if(attacker.GetComponent<SkellyScript>().Alive) {
				if(target != null) target.GetComponent<SkellyScript>().targetted = false;
				target = attacker;
			}
		}

		if(health <= 0) {
			alive = false;
			if(target != null) target.GetComponent<SkellyScript>().targetted = false;
			GetComponent<Animation>().Play("Die");
			wm.knights.Remove(transform);
			wm.CheckKnightCount();
			characterController.GetComponent<Collider>().enabled = false;
		}
	}
}
