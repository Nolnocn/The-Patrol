using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AgentScript : MonoBehaviour {

	public WorldManager wm;

	protected bool alive;
	public bool targetted;
	public int health;
	protected CharacterController characterController;
	protected Transform target;
	protected bool attacking;
	protected bool canAttack;
	protected float range;
	protected float attackReach = 1;
	
	protected float separationWt;
	protected float flowWt;
	protected float avoidWt;
	protected float seekWt;
	protected float tooClose = 1.0f;
	protected float gravity = 100.0f;
	protected float maxSpeed = 2.0f;
	protected float maxForce = 100.0f;
	protected float mass = 1.0f;
	protected float radius = 1.0f;
	
	protected Vector3 dv;
	protected Vector3 acceleration;
	protected Vector3 velocity;

	protected Animation myAnimation;

	public bool Alive {
		get { return alive; }
	}

	protected Vector3 Seek (Vector3 targetPos)
	{
		//find dv, desired velocity
		dv = targetPos - transform.position;		
		dv = dv.normalized * maxSpeed; 	//scale by maxSpeed
		dv -= characterController.velocity;
		dv.y = 0;								// only steer in the x/z plane
		return dv;
	}

	protected Vector3 Arrive (Vector3 targetPos)
	{
		dv = targetPos - transform.position;

		float d = dv.magnitude;
		dv.Normalize();
		if (d < tooClose) {
			float m = 1 / d;
			dv *= m;
		} else {
			dv *= maxSpeed;
		}

		dv -= characterController.velocity;
		dv.y = 0;
		return dv;
	}

	protected Vector3 Separate(List<Transform> others, float tooClose) {
		dv = Vector3.zero;
		foreach(Transform other in others) {
			if(other != transform) {
				float dist = Vector3.Distance(transform.position, other.position);
				if(dist < tooClose) {
					Vector3 targetPos =  Seek(other.position);
					targetPos.Normalize();
					targetPos *= 1 / dist;
					dv += targetPos;
				}
			}
		}
		dv.Normalize();
		dv *= maxSpeed;
		return dv;
	}

	protected Vector3 AvoidObstacle (Vector3 obst, float safeDistance)
	{ 
		dv = Vector3.zero;
		float obRadius = 5;
		
		//vector from vehicle to center of obstacle
		Vector3 vecToCenter = obst - transform.position;
		//eliminate y component so we have a 2D vector in the x, z plane
		vecToCenter.y = 0;
		float dist = vecToCenter.magnitude;
		
		// if too far to worry about, out of here
		if (dist > safeDistance + obRadius + radius)
			return Vector3.zero;
		
		//if behind us, out of here
		if (Vector3.Dot (vecToCenter, transform.forward) < 0)
			return Vector3.zero;
		
		float rightDotVTC = Vector3.Dot (vecToCenter, transform.right);
		
		//if we can pass safely, out of here
		if (Mathf.Abs (rightDotVTC) > radius + obRadius)
			return Vector3.zero;
		
		//obstacle on right so we steer to left
		if (rightDotVTC > 0)
			dv += transform.right * -maxSpeed * safeDistance / dist;
		else
			//obstacle on left so we steer to right
			dv += transform.right * maxSpeed * safeDistance / dist;
		
		return dv;	
	}

	protected Vector3 FollowLeader(Transform leader) {
		Vector3 followPos = leader.position - (leader.forward * tooClose);
		return Arrive(followPos);
	}

	protected Vector3 Flow() {
		dv = Vector3.zero;
		int x = Mathf.FloorToInt(transform.position.x / 10);
		int y = Mathf.FloorToInt((transform.position.z - 100) / 10);
		dv = wm.flowField[x,y] * maxSpeed;
		dv.y = 0;

		return dv;
	}

	protected void ApplyForce(Vector3 steeringForce)
	{
		acceleration += steeringForce / mass;
	}

	protected abstract void FindClosestEnemy();

	protected abstract void HandleAnimations();
}
