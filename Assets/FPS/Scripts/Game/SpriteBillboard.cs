using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
	private Camera theCam;

	// Start is called before the first frame update
	void Start()
	{
		theCam = Camera.main;
	}

	// Update is called once per frame
	void LateUpdate()
	{
		transform.rotation = theCam.transform.rotation;
		transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
	}
}
