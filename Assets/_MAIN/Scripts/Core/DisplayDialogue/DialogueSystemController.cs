using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// DialogueContainer を管理する シングルトン
    /// dialogueContainer.dialogueText が DisplayTextArchitect に参照される
    /// </summary>
    public class DialogueSystemController : MonoBehaviour
    {
        public DialogueContainer dialogueContainer = new();

        public static DialogueSystemController instance;

        void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else DestroyImmediate(gameObject);
        }

        void Start()
        {

        }

        void Update()
        {

        }
    }
}