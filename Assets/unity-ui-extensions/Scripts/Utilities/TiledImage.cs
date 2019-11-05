using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;
using System;

namespace UnityEngine.UI
{
    public class TiledImage : MaskableGraphic
    {
        [SerializeField]
        private float m_seg = 50f;
        [SerializeField]
        private Sprite m_Sprite;

        public Sprite sprite
        {
            get
            {
                return this.m_Sprite;
            }
            set
            {
                this.m_Sprite = value;
                this.SetAllDirty();
            }
        }

        public override Texture mainTexture
        {
            get
            {
                if (!(this.sprite == null))
                    return this.sprite.texture;
                if (this.material != null && this.material.mainTexture != null)
                    return this.material.mainTexture;
                return Graphic.s_WhiteTexture;
            }
        }

        public override Material material
        {
            get
            {
                if (this.m_Material != null)
                    return this.m_Material;
                if (Application.isPlaying && this.sprite && this.sprite.associatedAlphaSplitTexture != null)
                    return Image.defaultETC1GraphicMaterial;
                return this.defaultMaterial;
            }
            set
            {
                base.material = value;
            }
        }

        public float pixelsPerUnit
        {
            get
            {
                float num1 = 100f;
                if (this.sprite)
                    num1 = this.sprite.pixelsPerUnit;
                float num2 = 100f;
                if (this.canvas)
                    num2 = this.canvas.referencePixelsPerUnit;
                return num1 / num2;
            }
        }

        public float seg
        {
            get { return m_seg; }
            set { m_seg = value; }
        }

        public override void SetNativeSize()
        {
            if (!(this.sprite != null))
                return;
            float x = this.sprite.rect.width / this.pixelsPerUnit;
            float y = this.sprite.rect.height / this.pixelsPerUnit;
            this.rectTransform.anchorMax = this.rectTransform.anchorMin;
            this.rectTransform.sizeDelta = new Vector2(x, y);
            this.SetAllDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            Vector4 drawingDimensions = this.GetDrawingDimensions();
            Vector4 vector4 = !(this.sprite != null) ? Vector4.zero : DataUtility.GetOuterUV(this.sprite);
            Color color = this.color;
            vh.Clear();

            var UVs = new Vector2[4];
            UVs[0] = new Vector2(vector4.x, vector4.y);
            UVs[1] = new Vector2(vector4.x, vector4.w);
            UVs[2] = new Vector2(vector4.z, vector4.w);
            UVs[3] = new Vector2(vector4.z, vector4.y);
            float seg = (drawingDimensions.z - drawingDimensions.x) / m_seg;
            for (float i = 0; i < seg; i++)
            {
                Vector3[] quadPositions = new Vector3[4];
                float sx = drawingDimensions.x + i * m_seg;
                float sy = drawingDimensions.y;
                float sz = Mathf.Min(sx + m_seg, drawingDimensions.z);
                float sw = drawingDimensions.w;
                quadPositions[0] = new Vector3(sx, sy);
                quadPositions[1] = new Vector3(sx, sw);
                quadPositions[2] = new Vector3(sz, sw);
                quadPositions[3] = new Vector3(sz, sy);

                float extra = sx + m_seg - drawingDimensions.z;
                if (extra > 0)
                {
                    var newUVs = new Vector2[4];
                    float u = (vector4.z - vector4.x) * (extra / m_seg);
                    newUVs[0] = new Vector2(vector4.x, vector4.y);
                    newUVs[1] = new Vector2(vector4.x, vector4.w);
                    newUVs[2] = new Vector2(vector4.z - u, vector4.w);
                    newUVs[3] = new Vector2(vector4.z - u, vector4.y);
                    AddQuad(vh, quadPositions, color, newUVs);
                }
                else
                {
                    AddQuad(vh, quadPositions, color, UVs);
                }
            }
        }

        private Vector4 GetDrawingDimensions()
        {
            Vector4 vector4_1 = !(this.sprite == null) ? DataUtility.GetPadding(this.sprite) : Vector4.zero;
            Vector2 vector2 = !(this.sprite == null) ? new Vector2(this.sprite.rect.width, this.sprite.rect.height) : Vector2.zero;
            Rect pixelAdjustedRect = this.GetPixelAdjustedRect();
            int num1 = Mathf.RoundToInt(vector2.x);
            int num2 = Mathf.RoundToInt(vector2.y);
            Vector4 vector4_2 = new Vector4(vector4_1.x / num1, vector4_1.y / num2, (num1 - vector4_1.z) / num1, (num2 - vector4_1.w) / num2);
            vector4_2 = new Vector4(pixelAdjustedRect.x + pixelAdjustedRect.width * vector4_2.x, pixelAdjustedRect.y + pixelAdjustedRect.height * vector4_2.y, pixelAdjustedRect.x + pixelAdjustedRect.width * vector4_2.z, pixelAdjustedRect.y + pixelAdjustedRect.height * vector4_2.w);
            return vector4_2;
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector2[] quadUVs)
        {
            int currentVertCount = vertexHelper.currentVertCount;
            for (int index = 0; index < 4; ++index)
                vertexHelper.AddVert(quadPositions[index], color, quadUVs[index]);
            vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
        }
    }
}