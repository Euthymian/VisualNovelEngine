using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTER
{
    [CreateAssetMenu(fileName = "Model3D_ExpressionsSO", menuName = "Model3D/ExpressionsSO")]
    public class Model3D_ExpressionsSO : ScriptableObject
    {
        [System.Serializable]
        public class EyesBlendShape_Weight
        {
            public string eyesBlendShapeName;
            public float weight;
        }

        [System.Serializable]
        public class HeadBlendShape_Weight
        {
            public string headBlendShapeName;
            public float weight;
        }

        [System.Serializable]
        public class LashesBlendShape_Weight
        {
            public string lashesBlendShapeName;
            public float weight;
        }

        [System.Serializable]
        public class Expression
        {
            public string name;
            public EyesBlendShape_Weight[] eyeBlendShapes;
            public HeadBlendShape_Weight[] headBlendShapes;
            public LashesBlendShape_Weight[] lashesBlendShapes;
        }

        public Expression[] expressions;

        public Expression GetExpressionByName(string expressionName)
        {
            foreach (Expression expression in expressions)
            {
                if (expression.name == expressionName)
                {
                    return expression;
                }
            }
            Debug.LogWarning($"Expression '{expressionName}' not found in Model3D_ExpressionsSO.");
            return null;
        }
    }
}