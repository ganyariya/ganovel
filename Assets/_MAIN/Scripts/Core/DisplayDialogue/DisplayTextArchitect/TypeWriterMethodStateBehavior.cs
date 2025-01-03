using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.DisplayDialogue
{
    public class TypeWriterMethodStateBehavior : IDisplayMethodStateBehavior
    {
        private const float BASE_CHARACTER_WAIT_TIME = 0.015f;
        private readonly DisplayTextArchitect arch;

        public TypeWriterMethodStateBehavior(DisplayTextArchitect displayTextArchitect)
        {
            arch = displayTextArchitect;
        }

        public IEnumerator Displaying()
        {
            while (arch.TmProText.maxVisibleCharacters < arch.TmProText.textInfo.characterCount)
            {
                // maxVisibleCharacters を増やすことで表示テキストを増やしていく
                arch.TmProText.maxVisibleCharacters += arch.HurryUp ? arch.AppearCharactersNumPerFrame * 5 : arch.AppearCharactersNumPerFrame;
                yield return new WaitForSeconds(BASE_CHARACTER_WAIT_TIME / arch.BaseSpeed);
            }
        }

        public void Prepare()
        {
            arch.TmProText.color = arch.TmProText.color;
            arch.TmProText.maxVisibleCharacters = 0;
            arch.TmProText.text = arch.PrevText;

            // PrevText はすぐに表示できるようにする (maxVisibleCharacters)
            if (arch.PrevText != "")
            {
                arch.TmProText.ForceMeshUpdate();
                arch.TmProText.maxVisibleCharacters = arch.TmProText.textInfo.characterCount;
            }
            // text のみ増やす (maxVisibleCharacters をあとから増やしていく)
            arch.TmProText.text += arch.TargetText;
            arch.TmProText.ForceMeshUpdate();
        }

        public void ForceComplete()
        {
            arch.TmProText.maxVisibleCharacters = arch.TmProText.textInfo.characterCount;
        }

        public DisplayMethod GetDisplayMethod()
        {
            return DisplayMethod.typewriter;
        }
    }

}

