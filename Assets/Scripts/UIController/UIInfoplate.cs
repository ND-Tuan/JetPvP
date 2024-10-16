using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer
{
	/// <summary>
	/// Component that handle showing nicknames above player
	/// </summary>
	public class UIInfoplate : MonoBehaviour
	{
		public TextMeshProUGUI NicknameText;
		[SerializeField] private Material[] materials = new Material[2];
		[SerializeField] private MeshRenderer DisplayOnMiniMap;
		[SerializeField] private Slider HealthBar;
		[SerializeField] private Image HealthBarFill;
		private Color Red = new(255,42,0);
		private	Color Blue = new(0,197,255);


		private Transform _cameraTransform;

		public void SetNickname(string nickname)
		{
	
			NicknameText.text = nickname;
			
		}

		public void SetTeamColor(Team team, Color color){
			
			NicknameText.color = color;
			HealthBarFill.color = color;
			DisplayOnMiniMap.material = materials[(int)team];
			
		}

		public void UpdateHP(int currentHP, int maxHP){
			HealthBar.value = currentHP;
			HealthBar.maxValue = maxHP;
		}

		private void Awake()
		{
			_cameraTransform = Camera.main.transform;
			NicknameText.text = string.Empty;
		}

		private void LateUpdate()
		{
			// Rotate nameplate toward camera
			transform.rotation = _cameraTransform.rotation;
		}
	}
}
