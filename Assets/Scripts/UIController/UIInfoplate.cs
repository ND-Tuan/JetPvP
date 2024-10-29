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
		[SerializeField] private GameObject Panel;
		[SerializeField] private Transform lookAt;
		[SerializeField] private Vector3 offset;
		[SerializeField] private TextMeshProUGUI NicknameText;
		[SerializeField] private Slider HealthBar;
		[SerializeField] private Image[] PanelImage;
		public bool IsOnRange;



		public void SetNickname(string nickname)
		{
	
			NicknameText.text = nickname;
			
		}

		public void SetTeamColor(Color color){
			
			NicknameText.color = color;
			foreach(Image image in PanelImage){
				image.color = color;
			}
			
		}

		public void UpdateHP(int currentHP, int maxHP){
			if(currentHP <0) currentHP = 0;
			HealthBar.value = currentHP;
			HealthBar.maxValue = maxHP;
		}

		private void Awake()
		{
			NicknameText.text = string.Empty;
		}

		private void LateUpdate()
		{
			Panel.SetActive(IsOnRange);
			if(IsOnRange)
				Panel.transform.position =  Camera.main.WorldToScreenPoint(lookAt.position + offset);
		}
	}
}
