using System.Collections.Generic;
using System.Linq;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kroltan.GrapplingHook.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Scoreboard : NetworkBehaviour
    {
        public static Scoreboard Instance { get; private set; }

        [Required]
        [SerializeField]
        private ScoreboardRow _template;

        [Required]
        [SerializeField]
        private DeathMarker _marker;

        private readonly Dictionary<string, int> _scores = new();
        private readonly List<ScoreboardRow> _rows = new();
        private CanvasGroup _group;

        [Server]
        public void AddScore(string subject, Vector3 position, Vector3 direction)
        {
            _scores.TryGetValue(subject, out var value);
            _scores[subject] = value + 1;
            SetScoreRpc(subject, _scores[subject]);
            
            // TODO: Not the right place for this but jam
            if (direction.magnitude < 0.1)
            {
                direction = Vector3.down;
            }

            var instance = Instantiate(_marker, position, Quaternion.LookRotation(direction.normalized));
            NetworkServer.Spawn(instance.gameObject);
        }

        public override void OnStartClient()
        {
            NetworkClient.RegisterPrefab(_marker.gameObject);
        }

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            
            Instance = this;
        }

        private void Update()
        {
            _group.alpha = Input.GetButton("Show Score")
                ? 1
                : 0;
        }

        [ClientRpc]
        private void SetScoreRpc(string subject, int score)
        {
            var changedRow = _rows.FirstOrDefault(row => row.Name == subject);
            if (changedRow == null)
            {
                changedRow = Instantiate(_template, _template.transform.parent);
                changedRow.Name = subject;
                changedRow.gameObject.SetActive(true);
                _rows.Add(changedRow);
            }

            changedRow.Score = score;
            
            _rows.Sort((a, b) => a.Score - b.Score);

            foreach (var (i, row) in _rows.WithIndex())
            {
                row.transform.SetSiblingIndex(i);
            }
        }
    }
}