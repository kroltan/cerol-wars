using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Kroltan.GrapplingHook.UI
{
    public class ScoreboardRow : MonoBehaviour
    {
        public string Name
        {
            get => _name.text;
            set => _name.text = value;
        }
        public int Score
        {
            get => int.Parse(_score.text);
            set => _score.text = value.ToString();
        }

        [Required]
        [SerializeField]
        private TMP_Text _name;

        [Required]
        [SerializeField]
        private TMP_Text _score;
    }
}