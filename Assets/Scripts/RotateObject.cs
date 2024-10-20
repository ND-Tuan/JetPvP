using System.Diagnostics;
using UnityEngine;


	public class RotateObject : MonoBehaviour
	{	public enum RotationAxis
		{
			X,
			Y,
			Z
		}
		public float Speed = 90f;
		public RotationAxis rotationAxis;
		public bool RandomizeStart = true;

		private int Xindex = 0, Yindex = 0, Zindex = 0;

		private void Awake()
		{
			switch(rotationAxis)
			{
				case RotationAxis.X:
					Xindex = 1;
					break;
				case RotationAxis.Y:
					Yindex = 1;
					break;
				case RotationAxis.Z:
					Zindex = 1;
					break;
			}
			
			if (RandomizeStart)
			{
				float random = Random.Range(0f, 360f);
				transform.rotation = Quaternion.Euler(random * Xindex, random * Yindex, random * Zindex);
			}
		}

		private void Update()
		{	
			float value = Speed * Time.deltaTime;
			transform.Rotate(value* Xindex, value* Yindex, value* Zindex);
		}
	}
