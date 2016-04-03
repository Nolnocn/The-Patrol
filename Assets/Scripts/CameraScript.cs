using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	private bool freeCam;
	private bool hideGUI;
	private bool showCredits;

	// Use this for initialization
	void Start () {
		freeCam = true;
		hideGUI = false;
		showCredits = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)) {
			Screen.lockCursor = true;
		}

		if(Input.GetKeyDown(KeyCode.H)) hideGUI = !hideGUI;
		else if(Input.GetKeyDown(KeyCode.C)) showCredits = !showCredits;
		else if(Input.GetKeyDown(KeyCode.R)) Application.LoadLevel(0);

		if(freeCam) {
			Vector3 mouseOffset = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f);
			Camera.main.transform.Rotate(mouseOffset * 2, Space.Self);
			Camera.main.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward, Vector3.up);

			transform.Translate(5 * Time.deltaTime * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));

			if(Input.anyKey == false) {
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
		}

		StayInBounds();
	}

	void OnGUI() {
		if(!hideGUI) {
			GUI.Label(new Rect(0, 0, 300, 200), 
			          "Controls:\nMouse - Look\nWASD / Arrows - Move\nH - Hide/Show GUI\nC - Show/Hide Credits\nR - Reset\nEsc - Unlock Cursor");

			if(showCredits) {
				GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Credits\n\n\n" +
					"Programming:\n" +
					"Nick Conlon\n\n" +
					"Art:\n" +
					"Knight Model by lancel\n" +
					"(opengameart.org)\n" +
				    "Skeleton Model by bisaniyehocam\n" +
				    "(Unity Asset Store)\n" +
				    "Village Building Model by fistik\n" +
				    "(tf3dm.com)\n" +
				    "Village Scenery by Dreamdev Studios\n" +
				    "(Unity Asset Store)\n\n" +
				    "Other textures & models provided by Unity Standard Assets");
			}
		}
	}

	private void StayInBounds() {
		if(transform.position.x > 100) {
			transform.position = new Vector3(100, transform.position.y, transform.position.z);
		}
		else if(transform.position.x < 0) {
			transform.position = new Vector3(0, transform.position.y, transform.position.z);
		}

		if(transform.position.z > 200) {
			transform.position = new Vector3(transform.position.x, transform.position.y, 200);
		}
		else if(transform.position.z < 0) {
			transform.position = new Vector3(transform.position.x, transform.position.y, 0);
		}

		if(transform.position.y > 10) {
			transform.position = new Vector3(transform.position.x, 10, transform.position.z);
		}
		else if(transform.position.y < 0) {
			transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		}
	}
}
