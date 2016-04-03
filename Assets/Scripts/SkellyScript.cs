using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]

public class SkellyScript : AgentScript {

	public Transform village;
	public bool dance;

	// Use this for initialization
	void Start () {
		alive = true;
		acceleration = Vector3.zero;
		velocity = transform.forward;
		characterController = gameObject.GetComponent<CharacterController>();
		health = 5 + Random.Range(-2, 2);
		canAttack = true;
		
		separationWt = 15.0f;
		flowWt = 5.0f;
		avoidWt = 10.0f;
		seekWt = 10.0f;
		range = 15;

		dance = false;
		attacking = false;
		target = null;
		targetted = false;

		GetComponent<Animation>()["dance"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["idle"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["run"].wrapMode = WrapMode.Loop;
		GetComponent<Animation>()["waitingforbattle"].wrapMode = WrapMode.Loop;
	}
	
	// Update is called once per frame
	void Update () {
		if(alive) {
			if(!dance && !attacking) {
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
				if(Vector3.Distance(target.position, transform.position) < 1) {
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
		force += -separationWt * (Separate(wm.skellies, tooClose) - velocity);
		force += -separationWt * (Separate(wm.knights, tooClose) - velocity);

		if(target != null) {
			force += seekWt * Seek(target.position);
		}

		if(transform.position.z > 100) {
			if(target == null) force += flowWt * Flow();
			foreach(Vector3 tree in wm.trees) {
				force += avoidWt * AvoidObstacle(tree, tooClose);
			}
		}
		else {
			if(!dance) {
				if(target == null) {
					force += seekWt * Seek(village.position);
					if(Vector3.Distance(transform.position, village.position) < attackReach) {
						force = Vector3.zero;
						velocity = Vector3.zero;
						dance = true;
					}
				}
			}
		}
		
		force = Vector3.ClampMagnitude(force, maxForce);
		ApplyForce(force);
	}

	protected override void FindClosestEnemy() {
		float dist = 12345;
		Transform tempTar = null;
		foreach(Transform knight in wm.knights) {
			if(knight.GetComponent<KnightScript>().Alive) {
				float tempDist = Vector3.Distance(knight.position, transform.position);
				if(tempDist < dist && !knight.GetComponent<KnightScript>().targetted) {
					dist = tempDist;
					tempTar = knight;
				}
			}
		}
		
		if(dist < range) {
			target = tempTar;
			target.GetComponent<KnightScript>().targetted = true;
		}
	}

	protected override void HandleAnimations() {
		if(alive) {
			if(!dance) {
				if(!GetComponent<Animation>().IsPlaying("attack")) {
					if(velocity.sqrMagnitude > 0) GetComponent<Animation>().Play("run");
					else GetComponent<Animation>().Play("waitingforbattle");

					if(attacking && canAttack) {
						StartCoroutine(AttackDelay());
					}
				}
			}
			else GetComponent<Animation>().Play("dance");
		}
	}

	private IEnumerator AttackDelay() {
		GetComponent<Animation>().Play("attack");
		canAttack = false;
		yield return new WaitForSeconds(.75f);
		KnightScript ks = target.GetComponent<KnightScript>();
		if(alive && ks.Alive) ks.LowerHealth(transform);
		if(!ks.Alive) {
			target = null;
			attacking = false;
		}
		yield return new WaitForSeconds(.75f);
		canAttack = true;
	}

	public void LowerHealth(Transform attacker) {
		health--;

		if(!attacking) {
			if(attacker.GetComponent<KnightScript>().Alive) {
				if(target != null) target.GetComponent<KnightScript>().targetted = false;
				target = attacker;
			}
		}

		if(health <= 0) {
			alive = false;
			if(target != null) target.GetComponent<KnightScript>().targetted = false;
			GetComponent<Animation>().Play("die");
			wm.skellies.Remove(transform);
			characterController.GetComponent<Collider>().enabled = false;
		}
	}
}
