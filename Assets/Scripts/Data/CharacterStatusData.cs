using System.Collections.Generic;
using UnityEngine;

namespace Immersion.MetaCouch
{
    [CreateAssetMenu(fileName = "CharacterStatusData", menuName = "Immersion/CharacterStatusData")]
    public class CharacterStatusData : ScriptableObject
    {
        [SerializeField] private List<string> availableCharacterStatuses = new();
        [SerializeField] private string defaultCharacterStatus;
        
        private Dictionary<string, int> animatorParameters = new();
        private int defaultParameterHash;

        public void Init()
        {
            for (var i = 0; i < availableCharacterStatuses.Count; i++)
            {
                var status = availableCharacterStatuses[i];
                animatorParameters.Add(status, Animator.StringToHash(status));
            }
            
            defaultParameterHash  = Animator.StringToHash(defaultCharacterStatus);
        }

        public int GetAnimatorParameter(string status)
        {
            return animatorParameters.GetValueOrDefault(status, defaultParameterHash);
        }
    }
}
