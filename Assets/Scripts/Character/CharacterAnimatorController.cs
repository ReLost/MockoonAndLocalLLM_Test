using Immersion.MetaCouch.Networking;
using Immersion.MetaCouch.Utils;
using UnityEngine;

namespace Immersion.MetaCouch.Character
{
    public class CharacterAnimatorController : MonoBehaviour
    {
        [SerializeField] private NetworkHandler networkHandler;
        [SerializeField] private DataParser dataParser;
        
        [SerializeField, Space(5)] private CharacterStatusData characterStatusData;
        
        [SerializeField, Space(5)] private Animator animator;
        
        private void Start()
        {
            characterStatusData.Init();
            networkHandler.OnResponseReceivedSuccess += UpdateAnimator;
        }

        private void OnDestroy()
        { 
            networkHandler.OnResponseReceivedSuccess -= UpdateAnimator;
        }

        private void UpdateAnimator(string responseMessage)
        {
            var currentStatus = dataParser.GetStatus(responseMessage);
            animator.SetTrigger(characterStatusData.GetAnimatorParameter(currentStatus));
        }
    }
}
