using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CarouselView : MonoBehaviour
{


	public RectTransform[] images;
	public RectTransform view_window;


	private bool canSwipe;
	private float image_width;
	private float lerpTimer;
	private float lerpPosition;
	private float mousePositionStartX;
	private float mousePositionEndX;
	private float dragAmount;
	private float screenPosition;
	private float lastScreenPosition;
	public GameObject imageprefab;
	public GameObject Carousel;
	private Dictionary<int, string> DataLoaded = new Dictionary<int, string>();

	/// <summary>
	/// Space between images.
	/// </summary>
	public  float image_gap = 0;

	public int swipeThrustHold = 30;
	[HideInInspector]
	/// <summary>
    /// The index of the current image on display.
    /// </summary>
    public int current_index;

	public Text titel;

	[Serializable]
	public class MyClass
	{
		public string message;
		public CarouselData[] promotions;
	}
	[Serializable]
	public class CarouselData
	{
		public string index;
		public string imageLink;
		public string promotionLink;
	}
	public static MyClass Akansha;

	#region mono



	// Use this for initialization
	void Start ()
	{StartCoroutine(screenmove());
		string filePath = (Application.persistentDataPath+"/data.txt");
		if (File.Exists(filePath))
		{
			// Read the json from the file into a string
			string dataAsJson = File.ReadAllText(filePath);
			Akansha = JsonUtility.FromJson<MyClass>(dataAsJson);
			images = new RectTransform[Akansha.promotions.Length];
			//Debug.Log(Akansha);
			

		}
		else
		{
			Debug.LogError("Cannot load game data!");
		}

		
		image_width = view_window.rect.width;

		for (int i = 0; i < Akansha.promotions.Length; i++) {
			var Carousel = Instantiate (imageprefab, new Vector2 (0, 0), Quaternion.identity) as GameObject;
			Carousel.transform.parent = gameObject.transform;
			Carousel.name = "Image_" + i;
			DataLoaded.Add(i, Akansha.promotions[i].promotionLink);
			images [i] = Carousel.GetComponent<RectTransform> ();
			images [i].anchoredPosition = new Vector2 (((image_width) * i), 0);
			StartCoroutine(setImage(Akansha.promotions[i].imageLink, Carousel));
		
			//	Debug.Log (images [i].anchoredPosition);
		}
		StartCoroutine(screenmove());
		//}
	}


	IEnumerator setImage(string url, GameObject Prefab)
	{
		WWW www = new WWW(url);
		while (!www.isDone)
			yield return null;
		Prefab.GetComponent<RawImage>().texture = www.texture;
	//	panel.SetActive(false);
	}

	public void OpenUrl()
	{
		var go = EventSystem.current.currentSelectedGameObject;
		if (go != null)
			Application.OpenURL(DataLoaded[current_index]);
		else
			Debug.Log("currentSelectedGameObject is null");
	
}
	// Update is called once per frame
	void Update ()
	{

		titel.text = current_index.ToString ();

		lerpTimer = lerpTimer + Time.deltaTime;

		if (lerpTimer < 0.333f) {
			screenPosition = Mathf.Lerp (lastScreenPosition, lerpPosition * -1, lerpTimer * 3);
			lastScreenPosition = screenPosition;
		}

		if (Input.GetMouseButtonDown (0)) {
			canSwipe = true;
			mousePositionStartX = Input.mousePosition.x;
		}


		if (Input.GetMouseButton (0)) {
			if (canSwipe) {
				mousePositionEndX = Input.mousePosition.x;
				dragAmount = mousePositionEndX - mousePositionStartX;
				screenPosition = lastScreenPosition + dragAmount;
			}
		
		}

		if (Mathf.Abs (dragAmount) > swipeThrustHold && canSwipe) {
			canSwipe = false;
			lastScreenPosition = screenPosition;
			if (current_index < images.Length)
				OnSwipeComplete ();
			else if (current_index == images.Length && dragAmount < 0)
				lerpTimer = 0;
			else if (current_index == images.Length && dragAmount > 0)
				OnSwipeComplete ();
		}

		for (int i = 0; i < images.Length; i++) {
			images [i].anchoredPosition = new Vector2 (screenPosition + ((image_width + image_gap) * i), 0);
		}
	}

	#endregion


	#region private methods

	void OnSwipeComplete ()
	{
		lastScreenPosition = screenPosition;

		if (dragAmount > 0) {
			if (dragAmount >= swipeThrustHold) {
				if (current_index == 0) {
					lerpTimer = 0;
					lerpPosition = 0;
				} else {
					current_index--;
					lerpTimer = 0;
					if (current_index < 0)
						current_index = 0;
					lerpPosition = (image_width + image_gap) * current_index;
				}
			} else {
				lerpTimer = 0;
			}
		} else if (dragAmount < 0) {
			if (Mathf.Abs (dragAmount) >= swipeThrustHold) {
				if (current_index == images.Length - 1) {
					lerpTimer = 0;
					lerpPosition = (image_width + image_gap) * current_index;
				} else {
					lerpTimer = 0;
					current_index++;
					lerpPosition = (image_width + image_gap) * current_index;
				}
			} else {
				lerpTimer = 0;
			}
		}
		dragAmount = 0;
	}

	#endregion



	#region public methods

	public void  GoToIndex (int value)
	{ 
		current_index = value;
		lerpTimer = 0;
		lerpPosition = (image_width + image_gap) * current_index;
		screenPosition = lerpPosition * -1;
		lastScreenPosition = screenPosition;
		for (int i = 0; i < images.Length; i++) {
			images [i].anchoredPosition = new Vector2 (screenPosition + ((image_width + image_gap) * i), 0);
		}
	}

	public void GoToIndexSmooth (int value)
	{
		current_index = value;
		lerpTimer = 0;
		lerpPosition = (image_width + image_gap) * current_index;
	}

	public IEnumerator screenmove ()
	{
		while (true) {
			yield return new WaitForSeconds (10f);
			dragAmount = -40;
			canSwipe = true;
		}
	}


	#endregion
}
