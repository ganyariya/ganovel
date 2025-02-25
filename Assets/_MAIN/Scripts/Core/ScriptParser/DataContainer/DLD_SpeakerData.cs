using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Core.ScriptParser
{
    /// <summary>
    /// speaker Name をさらにパースしたもの
    /// 名前・cast,position, layer をもつ
    /// </summary>
    public class DLD_SpeakerData
    {
        public string name;

        /// <summary>
        /// `???` のように cast する
        /// </summary>
        public string castName;

        /// <summary>
        /// 画面に表示される名前
        /// </summary>
        public string DisplayName => castName != string.Empty ? castName : name;

        public Vector2 castPosition;
        public List<(int layer, string expression)> CastExpressions { get; set; }
        /// <summary>
        /// キャラを画面に登場させるかどうか
        /// </summary>
        public bool isAppearanceCharacter = false;
        /// <summary>
        /// キャラの位置が指定されているか
        /// </summary>
        public bool isCastingPosition = false;
        public bool isCastingExpressions => CastExpressions.Count > 0;

        /// <summary>
        /// 画面でキャラが必ず必要になるか？
        /// キャラを登場させたり 位置を変更したり 表情を変更する場合は必要になる
        /// </summary>
        public bool needCharacterInstanceCreation => isAppearanceCharacter || isCastingPosition || isCastingExpressions;

        private const string NAME_CAST_ID = " as ";
        private const string POSITION_CAST_ID = " at ";
        private const string EXPRESSION_CAST_ID = @" [";
        private const char POSITION_AXIS_DELIMITER = ':';
        private const char EXPRESSION_LAYER_JOINER = ',';
        private const char EXPRESSION_LAYER_DELIMITER = ':';
        /// <summary>
        /// テキストファイルからキャラを同時に表示するためのキーワード
        /// </summary>
        private const string ENTER_KEYWORD = "enter ";

        private readonly static Regex parsePattern = new Regex($@"{NAME_CAST_ID}|{POSITION_CAST_ID}|{EXPRESSION_CAST_ID.Insert(EXPRESSION_CAST_ID.Length - 1, @"\")}");

        public bool HasSpeaker => DisplayName != string.Empty;

        public bool isCastingName => castName != string.Empty;

        public DLD_SpeakerData(string rawSpeaker)
        {
            var parsed = ParseSpeakerData(rawSpeaker);
            this.name = parsed.name;
            this.castName = parsed.castName;
            this.castPosition = parsed.castPosition;
            this.CastExpressions = parsed.castExpressions;
            Debug.Log(@$"DLD_SpeakerData Parsed [original={rawSpeaker}][name={name}][castName={castName}][castPosition={castPosition}][castExpressions={string.Join(',', CastExpressions.Select(x => $"{x.layer}:{x.expression}"))}]");
        }

        public DLD_SpeakerData(string name, string castName, Vector2 castPosition, List<(int layer, string expression)> castExpressions, bool isAppearanceCharacter, bool isCastingPosition)
        {
            this.name = name;
            this.castName = castName;
            this.castPosition = castPosition;
            this.CastExpressions = castExpressions;
            this.isAppearanceCharacter = isAppearanceCharacter;
            this.isCastingPosition = isCastingPosition;
        }

        private string PreProcessKeywords(string rawSpeaker)
        {
            if (rawSpeaker.StartsWith(ENTER_KEYWORD))
            {
                rawSpeaker = rawSpeaker.Substring(ENTER_KEYWORD.Length);
                isAppearanceCharacter = true;
            }
            return rawSpeaker;
        }

        public (string name, string castName, Vector2 castPosition, List<(int layer, string expression)> castExpressions) ParseSpeakerData(string rawSpeaker)
        {
            rawSpeaker = PreProcessKeywords(rawSpeaker);

            MatchCollection matches = parsePattern.Matches(rawSpeaker);
            string name = "";
            string castName = "";
            Vector2 castPosition = Vector2.zero;
            var castExpressions = new List<(int layer, string expression)>();

            if (matches.Count == 0)
            {
                name = rawSpeaker;
                return (name, castName, castPosition, castExpressions);
            }

            int index = matches[0].Index;
            name = rawSpeaker.Substring(0, index);

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int startIndex = 0, endIndex = 0;

                if (match.Value == NAME_CAST_ID)
                {
                    startIndex = match.Index + NAME_CAST_ID.Length;
                    endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                    castName = rawSpeaker.Substring(startIndex, endIndex - startIndex);
                }
                if (match.Value == POSITION_CAST_ID)
                {
                    isCastingPosition = true;

                    startIndex = match.Index + POSITION_CAST_ID.Length;
                    endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                    string castPositionStr = rawSpeaker.Substring(startIndex, endIndex - startIndex);
                    string[] axis = castPositionStr.Split(POSITION_AXIS_DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);

                    float.TryParse(axis[0], out castPosition.x);
                    if (axis.Length > 1) float.TryParse(axis[1], out castPosition.y);
                }
                if (match.Value == EXPRESSION_CAST_ID)
                {
                    startIndex = match.Index + EXPRESSION_CAST_ID.Length;
                    endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                    string expressionStr = rawSpeaker.Substring(startIndex, endIndex - (startIndex + 1));

                    castExpressions = expressionStr.Split(EXPRESSION_LAYER_JOINER).Select(x =>
                    {
                        var parts = x.Trim().Split(EXPRESSION_LAYER_DELIMITER);
                        if (parts.Length == 2)
                        {
                            return (int.Parse(parts[0]), parts[1]);
                        }
                        else
                        {
                            return (0, parts[0]);
                        }
                    }).ToList();
                }
            }

            return (name, castName, castPosition, castExpressions);
        }
    }
}
