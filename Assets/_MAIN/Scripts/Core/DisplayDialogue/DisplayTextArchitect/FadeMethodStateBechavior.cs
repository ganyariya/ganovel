using System;
using System.Collections;
using UnityEngine;

namespace Core.DisplayDialogue
{
    public class FadeMethodStateBehavior : IDisplayMethodStateBehavior
    {
        private const byte ALPHA_THRESHOLD = 20;
        private const float FADE_SPEED_MULTIPLIER = 2f;
        private bool isForcedCompletePressed = false;

        private readonly DisplayTextArchitect arch;

        public FadeMethodStateBehavior(DisplayTextArchitect displayTextArchitect)
        {
            arch = displayTextArchitect;
        }

        public IEnumerator Displaying()
        {
            int left = arch.PrevText.Length;
            var textInfo = arch.TmProText.textInfo;
            int right = Math.Min(left + 1, textInfo.characterCount);
            var alphas = new float[textInfo.characterCount];

            // called per frame
            while (true)
            {
                if (isForcedCompletePressed) break;

                // テキスト情報が更新されている可能性があるので、毎フレーム更新
                // https://claude.ai/chat/3182a54e-d7d3-4c94-9705-4e710ea13a4d

                float fadeSpeed = (arch.HurryUp ? arch.AppearCharactersNumPerFrame * 5 : arch.AppearCharactersNumPerFrame) * arch.BaseSpeed * FADE_SPEED_MULTIPLIER;
                for (int i = left; i < right; i++)
                {
                    if (i >= textInfo.characterCount) break; // 範囲チェックを追加する

                    var cInfo = textInfo.characterInfo[i];
                    if (!cInfo.isVisible) continue;

                    var vColors = textInfo.meshInfo[cInfo.materialReferenceIndex].colors32;
                    // IndexOutOfRangeException: Index was outside the bounds of the array.
                    // 上記が発生したため 範囲チェックを追加した
                    alphas[i] = Mathf.MoveTowards(alphas[i], 255, fadeSpeed);

                    for (int v = 0; v < 4; v++) vColors[cInfo.vertexIndex + v].a = (byte)alphas[i];
                    if (alphas[i] >= 255) left++;
                }
                arch.TmProText.UpdateVertexData(TMPro.TMP_VertexDataUpdateFlags.Colors32);

                bool lastVisible = textInfo.characterInfo[right - 1].isVisible;
                if (!lastVisible || alphas[right - 1] >= ALPHA_THRESHOLD)
                {
                    if (right < textInfo.characterCount) right++;
                    else if (!lastVisible || alphas[right - 1] >= 255) break;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public void Prepare()
        {
            isForcedCompletePressed = false;
            arch.TmProText.text = arch.PrevText;
            arch.TmProText.text += arch.TargetText;
            arch.TmProText.maxVisibleCharacters = int.MaxValue;
            arch.TmProText.ForceMeshUpdate();

            /*
            https://coposuke.hateblo.jp/entry/2020/06/07/020330
            https://qiita.com/Asalato/items/f84ca0a0924a273a34fc#%E5%AE%9F%E8%A3%85%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6
            - TextMeshPro の文字は 4 つの頂点で構成される。 
            - textInfo は array(meshInfo, textInfo) を持ち i 番目は i 個目の文字の情報を指す
            */
            var textInfo = arch.TmProText.textInfo;
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var cInfo = textInfo.characterInfo[i];
                if (!cInfo.isVisible) continue;
                var vColors = textInfo.meshInfo[cInfo.materialReferenceIndex].colors32;
                for (int v = 0; v < 4; v++) vColors[cInfo.vertexIndex + v] = new Color(arch.TextColor.r, arch.TextColor.g, arch.TextColor.b, i < arch.PrevText.Length ? 1 : 0);
            }
            arch.TmProText.UpdateVertexData(TMPro.TMP_VertexDataUpdateFlags.Colors32);
        }

        public void ForceComplete()
        {
            arch.TmProText.ForceMeshUpdate();
            isForcedCompletePressed = true;
        }

        public DisplayMethod GetDisplayMethod()
        {
            return DisplayMethod.fade;
        }
    }
}