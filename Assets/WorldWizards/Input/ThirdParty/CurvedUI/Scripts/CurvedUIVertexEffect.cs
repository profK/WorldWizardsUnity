using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if CURVEDUI_TMP || TMP_PRESENT
using TMPro;
#endif




namespace CurvedUI
{
    //Added to every UI object by CurvedUISettings.
    //This is the script that subdivides and modifies the shape of the canvas object it is attached to.
    //
    //Requires Unity 5.3 or later
    public partial class CurvedUIVertexEffect : BaseMeshEffect
    {
        #region VARIABLES
        //public settings
        [Tooltip("Check to skip tesselation pass on this object. CurvedUI will not create additional vertices to make this object have a smoother curve. Checking this can solve some issues if you create your own procedural mesh for this object. Default false.")]
        public bool DoNotTesselate = false;


        //stored references
        Canvas myCanvas;
        CurvedUISettings mySettings;
        Graphic myGraphic;
        Image myImage;
        Text myText;
#if CURVEDUI_TMP || TMP_PRESENT
        TextMeshProUGUI myTMP;
        CurvedUITMPSubmesh myTMPSubMesh;
#endif


        //variables we operate on
        bool tesselationRequired = true;
        bool curvingRequired = true;
        float angle = 90;
        bool TransformMisaligned = false;
        Matrix4x4 CanvasToWorld;
        Matrix4x4 CanvasToLocal;
        Matrix4x4 MyToWorld;
        Matrix4x4 MyToLocal;
        VertexHelper m_savedVertexHelper = new VertexHelper();
        List<UIVertex> m_savedVerts = new List<UIVertex>();
        List<UIVertex> m_tesselatedVerts = new List<UIVertex>();
        List<UIVertex> m_copiedVerts = new List<UIVertex>();
        List<UIVertex> m_vertsInQuads = new List<UIVertex>();
        UIVertex m_ret;
        UIVertex[] m_quad = new UIVertex[4];
        float[] m_weights = new float[4];

        [SerializeField] [HideInInspector] Vector3 savedPos;
        [SerializeField] [HideInInspector] Vector3 savedUp;
        [SerializeField] [HideInInspector] Vector2 savedRectSize;
        [SerializeField] [HideInInspector] Color savedColor;
        [SerializeField] [HideInInspector] Vector2 savedTextUV0;
        [SerializeField] [HideInInspector] float savedFill;
        #endregion





        #region LIFECYCLE
        protected override void OnEnable()
        {
            //find the settings object and its canvas.
            FindParentSettings();

            //If there is an update to the graphic, we cant reuse old vertices, so new tesselation will be required
            myGraphic = GetComponent<Graphic>();
            if (myGraphic)
            {
                myGraphic.RegisterDirtyMaterialCallback(TesselationRequiredCallback);
                myGraphic.SetVerticesDirty();
            }

            //add text events and callbacks
            myText = GetComponent<Text>();
            if (myText)
            {
                myText.RegisterDirtyVerticesCallback(TesselationRequiredCallback);
                Font.textureRebuilt += FontTextureRebuiltCallback;
            }

#if CURVEDUI_TMP || TMP_PRESENT
            myTMP = GetComponent<TextMeshProUGUI>();
            myTMPSubMesh = GetComponent<CurvedUITMPSubmesh>();
#endif
        }

        /// <summary>
        /// Start is executed after OnEnable
        /// </summary>
        protected override void Start()
        {
            base.Start();

            //if we have an interactable component, make sure its inside the canvas and it's Z position is 0 in relation to the canvas.
            //Otherwise the interactions on it will not be accurate, or it may not work at all!
            //This is because, in order to save performance, CurvedUI creates a collider only for the space inside the Canvas rectangle.
            if (myCanvas && GetComponent<Selectable>())
            {
                Vector3 myPosOnCanvas = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position);
                RectTransform canvasRect = (myCanvas.transform as RectTransform);
                if (myPosOnCanvas.x.Abs() > canvasRect.rect.width / 2.0f || myPosOnCanvas.y.Abs() > canvasRect.rect.height / 2.0f)
                {
                    Debug.LogWarning("CurvedUI: " + GetComponent<Selectable>().GetType().Name + " \"" + this.gameObject.name + "\" is outside of the canvas. It will not be interactable. Move it inside the canvas rectangle (white border in scene view) for it to work.", this.gameObject);
                }

                if (myPosOnCanvas.z.Abs() > 0.1f)
                {
                    Debug.LogWarning("CurvedUI: The Pos Z of " + GetComponent<Selectable>().GetType().Name + " \"" + this.gameObject.name + "\" is not 0 in relation to the canvas. The interactions may not be perfectly accurate.", this.gameObject);
                }
            }
        }

        protected override void OnDisable()
        {
            //If there is an update to the graphic, we cant reuse old vertices, so new tesselation will be required
            if (myGraphic)
                myGraphic.UnregisterDirtyMaterialCallback(TesselationRequiredCallback);

            if (myText)
            {
                myText.UnregisterDirtyVerticesCallback(TesselationRequiredCallback);
                Font.textureRebuilt -= FontTextureRebuiltCallback;
            }
        }

        /// <summary>
        /// Subscribed to graphic componenet to find out when vertex information changes and we need to create new geometry based on that.
        /// </summary>
        void TesselationRequiredCallback()
        {
            tesselationRequired = true;
        }

        /// <summary>
        /// Called by Font class to let us know font atlas has ben rebuilt and we need to update our vertices.
        /// </summary>
        void FontTextureRebuiltCallback(Font fontie)
        {
            if (myText.font == fontie)
                tesselationRequired = true;
        }


        void Update()
        {
#if CURVEDUI_TMP || TMP_PRESENT  // CurvedUITMP handles updates for TextMeshPro objects.
            if (myTMP || myTMPSubMesh) return;
#endif

            //Find if the change in transform requires us to retesselate the UI
            // do not perform the check it it will happen anyway
            if (!tesselationRequired)
            { 
                if ((transform as RectTransform).rect.size != savedRectSize)
                {
                    //the size of this RectTransform has changed, we have to tesselate again! 
                    tesselationRequired = true;
                }
                else if (myGraphic != null)//test for color changes if it has a graphic component
                {
                    if (myGraphic.color != savedColor)
                    {
                        tesselationRequired = true;
                        savedColor = myGraphic.color;
                    }
                    else if (myImage != null)
                    {
                        if (myImage.fillAmount != savedFill)
                        {
                            tesselationRequired = true;
                            savedFill = myImage.fillAmount;
                        }
                    }
                }
            }

            if (!tesselationRequired && !curvingRequired) // do not perform a check if we're already tesselating or curving. Tesselation includes curving.
            {
                //test if position in canvas's local space has been changed. We would need to recalculate vertices again
                Vector3 testedPos = mySettings.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position);
                if (!testedPos.AlmostEqual(savedPos))
                {

                    //we dont have to curve vertices if we only moved the object vertically in a cylinder.
                    if (mySettings.Shape != CurvedUISettings.CurvedUIShape.CYLINDER || Mathf.Pow(testedPos.x - savedPos.x, 2) > 0.00001 || Mathf.Pow(testedPos.z - savedPos.z, 2) > 0.00001)
                    {
                        savedPos = testedPos;
                        curvingRequired = true;
                        // Debug.Log("crv req - tested pos: " + testedPos, this.gameObject);
                    }
                }

                //test this object's rotation in relation to canvas.
                Vector3 testedUp = mySettings.transform.worldToLocalMatrix.MultiplyVector(this.transform.up).normalized;
                if (!savedUp.AlmostEqual(testedUp, 0.0001))
                {
                    bool testedEqual = testedUp.AlmostEqual(Vector3.up.normalized);
                    bool savedEqual = savedUp.AlmostEqual(Vector3.up.normalized);

                    //special case - if we change the z angle from or to 0, we need to retesselate to properly display geometry in cyllinder
                    if ((!testedEqual && savedEqual) || (testedEqual && !savedEqual))
                        tesselationRequired = true;

                    savedUp = testedUp;
                    curvingRequired = true;
                    //Debug.Log("crv req - tested up: " + testedUp);
                }
            }

            ////if we find we need to make a change in the mesh, set vertices dirty to trigger BaseMeshEffect firing.
            if (myGraphic && (tesselationRequired || curvingRequired)) myGraphic.SetVerticesDirty();
        }
        #endregion

        #region MESH EFFECT
        //This is called by canvas after UI object's mesh is generated, but before it is rendered.
        //Best place to modify the vertices of the object.
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!this.IsActive())
                return;

            if (mySettings == null) FindParentSettings();


            if (mySettings == null || !mySettings.enabled)
                return;

            //check for changes in text font material that would mean a retesselation in required to get fresh UV's
            CheckTextFontMaterial();

            //if curving or tesselation is required, we'll run the code to calculate vertices.
            if (tesselationRequired || curvingRequired || m_savedVertexHelper == null || m_savedVertexHelper.currentVertCount == 0)
            {
                //Debug.Log("updating: tes:" + tesselationRequired + ", crv:" + curvingRequired, this.gameObject);
                //Get vertices from the vertex stream. These come as triangles.
                vh.GetUIVertexStream(m_savedVerts);

                // calls the old ModifyVertices which was used on pre 5.2. 
                ModifyVerts(m_savedVerts);

                //create or reuse our temp vertexhelper
                m_savedVertexHelper.Clear();

                //Save our tesselated and curved vertices to new vertex helper. They can come as quads or as triangles.
                if (m_savedVerts.Count % 4 == 0)
                {
                    for (int i = 0; i < m_savedVerts.Count; i += 4)
                    {
                        m_savedVertexHelper.AddUIVertexQuad(new UIVertex[]{
                            m_savedVerts[i + 0], m_savedVerts[i + 1], m_savedVerts[i + 2], m_savedVerts[i + 3],
                        });
                    }
                }
                else
                {
                    m_savedVertexHelper.AddUIVertexTriangleStream(m_savedVerts);
                }

                //download proper vertex stream to a list we're going to save
                m_savedVertexHelper.GetUIVertexStream(m_savedVerts);
                curvingRequired = false;
            }

            //copy the saved verts list to current VertexHelper
            vh.Clear();
            vh.AddUIVertexTriangleStream(m_savedVerts);
        }
        #endregion






        #region HELPERS
        void CheckTextFontMaterial()
        {
            //we check for a sudden change in text's fontMaterialTexture. This is a very hacky way, but the only one working reliably for now.
            if (myText)
            {
                if (myText.cachedTextGenerator.verts.Count > 0 && myText.cachedTextGenerator.verts[0].uv0 != savedTextUV0)
                {
                    //Debug.Log("tess req - texture");
                    savedTextUV0 = myText.cachedTextGenerator.verts[0].uv0;
                    tesselationRequired = true;
                }
            }
        }

        public CurvedUISettings FindParentSettings(bool forceNew = false)
        {
            if (mySettings == null || forceNew)
            {
                mySettings = GetComponentInParent<CurvedUISettings>();

                if (mySettings == null) return null;
                else
                {
                    myCanvas = mySettings.GetComponent<Canvas>();
                    angle = mySettings.Angle;

                    myImage = GetComponent<Image>();
                }
            }

            return mySettings;
        }
        #endregion






        #region VERTEX OPERATIONS
        void ModifyVerts(List<UIVertex> verts)
        {

            if (verts == null || verts.Count == 0) return;

            //update transformation matrices we're going to use in curving the verts.
            CanvasToWorld = myCanvas.transform.localToWorldMatrix;
            CanvasToLocal = myCanvas.transform.worldToLocalMatrix;
            MyToWorld = transform.localToWorldMatrix;
            MyToLocal = transform.worldToLocalMatrix;

            //tesselate the vertices if needed and save them to a list,
            //so we don't have to retesselate if RectTransform's size has not changed.
            if (tesselationRequired || !Application.isPlaying)
            {
                TesselateGeometry(verts);

                // Save the tesselated vertices, so if the size does not change,
                // we can use them when redrawing vertices.
                m_tesselatedVerts = verts;

                //save the transform properties we last tesselated for.
                savedRectSize = (transform as RectTransform).rect.size;

                tesselationRequired = false;
            }


            //lets get some values needed for curving from settings
            angle = mySettings.Angle;
            float radius = mySettings.GetCyllinderRadiusInCanvasSpace();
            Vector2 canvasSize = (myCanvas.transform as RectTransform).rect.size;

            int initialCount = verts.Count;

            m_copiedVerts.Clear();
            if (m_tesselatedVerts != null)
            { // use saved verts if we have those
                for (int i = 0; i < m_tesselatedVerts.Count; i++)
                {
                    m_copiedVerts.Add(CurveVertex(m_tesselatedVerts[i], angle, radius, canvasSize));
                }
            }
            else
            { // or just the mesh's vertices if we do not
                for (int i = 0; i < initialCount; i++)
                {
                    m_copiedVerts.Add(CurveVertex(verts[i], angle, radius, canvasSize));
                }
            }
            verts.AddRange(m_copiedVerts);
            verts.RemoveRange(0, initialCount);
        }
        #endregion





        #region CURVING
        /// <summary>
        /// Map position of a vertex to a section of a circle. calculated in canvas's local space
        /// </summary>
        UIVertex CurveVertex(UIVertex input, float cylinder_angle, float radius, Vector2 canvasSize)
        {

            Vector3 pos = input.position;

            //calculated in canvas local space version:
            pos = CanvasToLocal.MultiplyPoint3x4(MyToWorld.MultiplyPoint3x4(pos));
            // pos = mySettings.VertexPositionToCurvedCanvas(pos);

            if (mySettings.Shape == CurvedUISettings.CurvedUIShape.CYLINDER && mySettings.Angle != 0)
            {

                float theta = (pos.x / canvasSize.x) * cylinder_angle * Mathf.Deg2Rad;
                radius += pos.z; // change the radius depending on how far the element is moved in z direction from canvas plane
                pos.x = Mathf.Sin(theta) * radius;
                pos.z += Mathf.Cos(theta) * radius - radius;

            }
            else if (mySettings.Shape == CurvedUISettings.CurvedUIShape.CYLINDER_VERTICAL && mySettings.Angle != 0)
            {

                float theta = (pos.y / canvasSize.y) * cylinder_angle * Mathf.Deg2Rad;
                radius += pos.z; // change the radius depending on how far the element is moved in z direction from canvas plane
                pos.y = Mathf.Sin(theta) * radius;
                pos.z += Mathf.Cos(theta) * radius - radius;

            }
            else if (mySettings.Shape == CurvedUISettings.CurvedUIShape.RING)
            {

                float angleOffset = 0;
                float r = pos.y.Remap(canvasSize.y * 0.5f * (mySettings.RingFlipVertical ? 1 : -1), -canvasSize.y * 0.5f * (mySettings.RingFlipVertical ? 1 : -1), mySettings.RingExternalDiameter * (1 - mySettings.RingFill) * 0.5f, mySettings.RingExternalDiameter * 0.5f);
                float theta = (pos.x / canvasSize.x).Remap(-0.5f, 0.5f, Mathf.PI / 2.0f, cylinder_angle * Mathf.Deg2Rad + Mathf.PI / 2.0f) - angleOffset;
                pos.x = r * Mathf.Cos(theta);
                pos.y = r * Mathf.Sin(theta);

            }
            else if (mySettings.Shape == CurvedUISettings.CurvedUIShape.SPHERE && mySettings.Angle != 0)
            {

                float vangle = mySettings.VerticalAngle;
                float savedZ = -pos.z;

                if (mySettings.PreserveAspect)
                {
                    vangle = cylinder_angle * (canvasSize.y / canvasSize.x);
                }
                else
                {
                    radius = canvasSize.x / 2.0f;
                    if (vangle == 0) return input;
                }

                //convert planar coordinates to spherical coordinates
                float theta = (pos.x / canvasSize.x).Remap(-0.5f, 0.5f, (180 - cylinder_angle) / 2.0f - 90, 180 - (180 - cylinder_angle) / 2.0f - 90);
                theta *= Mathf.Deg2Rad;
                float gamma = (pos.y / canvasSize.y).Remap(-0.5f, 0.5f, (180 - vangle) / 2.0f, 180 - (180 - vangle) / 2.0f);
                gamma *= Mathf.Deg2Rad;

                pos.z = Mathf.Sin(gamma) * Mathf.Cos(theta) * (radius + savedZ);
                pos.y = -(radius + savedZ) * Mathf.Cos(gamma);
                pos.x = Mathf.Sin(gamma) * Mathf.Sin(theta) * (radius + savedZ);

                if (mySettings.PreserveAspect)
                    pos.z -= radius;
            }

            //4. write output
            input.position = MyToLocal.MultiplyPoint3x4(CanvasToWorld.MultiplyPoint3x4(pos));

            return input;
        }
        #endregion





        #region TESSELATION
        void TesselateGeometry(List<UIVertex> verts)
        {

            Vector2 tessellatedSize = mySettings.GetTesslationSize();

            //find if we are aligned with canvas main axis
            TransformMisaligned = !(savedUp.AlmostEqual(Vector3.up.normalized));

            // Convert the list from triangles to quads to be used by the tesselation
            TrisToQuads(verts);


            //do not tesselate text verts. Text usually is small and has plenty of verts already.
#if CURVEDUI_TMP || TMP_PRESENT
            if (myText == null && myTMP == null && !DoNotTesselate)
            {
#else
            if (myText == null && !DoNotTesselate)
            {
#endif
                // Tesselate quads and apply transformation
                int startingVertexCount = verts.Count;
                for (int i = 0; i < startingVertexCount; i += 4)
                    ModifyQuad(verts, i, tessellatedSize);

                // Remove old quads
                verts.RemoveRange(0, startingVertexCount);
            }
        }


        void ModifyQuad(List<UIVertex> verts, int vertexIndex, Vector2 requiredSize)
        {
            // Read the existing quad vertices
            for (int i = 0; i < 4; i++)
                m_quad[i] = verts[vertexIndex + i];

            // horizotal and vertical directions of a quad. We're going to tesselate parallel to these.
            Vector3 horizontalDir = m_quad[2].position - m_quad[1].position;
            Vector3 verticalDir = m_quad[1].position - m_quad[0].position;

            //To make sure filled image is properly tesselated, were going to find the bigger side of the quad.
            if (myImage != null && myImage.type == Image.Type.Filled)
            {
                horizontalDir = (horizontalDir).x > (m_quad[3].position - m_quad[0].position).x ? horizontalDir : m_quad[3].position - m_quad[0].position;
                verticalDir = (verticalDir).y > (m_quad[2].position - m_quad[3].position).y ? verticalDir : m_quad[2].position - m_quad[3].position;
            }

            // Find how many quads we need to create
            int horizontalQuads = 1;
            int verticalQuads = 1;

            // Tesselate vertically only if the recttransform (or parent) is rotated
            // This cuts down the time needed to tesselate by 90% in some cases.
            if (TransformMisaligned || mySettings.Shape == CurvedUISettings.CurvedUIShape.SPHERE || mySettings.Shape == CurvedUISettings.CurvedUIShape.CYLINDER_VERTICAL)
                verticalQuads = Mathf.CeilToInt(verticalDir.magnitude * (1.0f / Mathf.Max(0.0001f, requiredSize.y)));

            if (TransformMisaligned || mySettings.Shape != CurvedUISettings.CurvedUIShape.CYLINDER_VERTICAL)
            {
                horizontalQuads = Mathf.CeilToInt(horizontalDir.magnitude * (1.0f / Mathf.Max(0.0001f, requiredSize.x)));
            }
            //Debug.Log(this.transform.root.name + "'s panel: hori size:" + horizontalDir.magnitude + " req:" + requiredSize.x + " divs:"+horizontalQuads);

            bool oneVert = false;
            bool oneHori = false;

            // Create the quads!
            float yStart = 0.0f;
            for (int y = 0; y < verticalQuads || !oneVert; ++y)
            {
                oneVert = true;
                float yEnd = (y + 1.0f) / verticalQuads;
                float xStart = 0.0f;

                for (int x = 0; x < horizontalQuads || !oneHori; ++x)
                {
                    oneHori = true;

                    float xEnd = (x + 1.0f) / horizontalQuads;

                    //Add new quads to list
                    verts.Add(TesselateQuad(xStart, yStart));
                    verts.Add(TesselateQuad(xStart, yEnd));
                    verts.Add(TesselateQuad(xEnd, yEnd));
                    verts.Add(TesselateQuad(xEnd, yStart));

                    //begin the next quad where we ened this one
                    xStart = xEnd;
                }
                //begin the next row where we ended this one
                yStart = yEnd;
            }
        }

        /// <summary>
        /// Converts a List of triangle mesh vertices to a list of quad mesh vertices
        /// </summary>
        void TrisToQuads(List<UIVertex> verts)
        {
            int vertsInTrisCount = verts.Count;
            m_vertsInQuads.Clear();

            for (int i = 0; i < vertsInTrisCount; i += 6)
            {
                // Get four corners from two triangles. Basic UI always comes in quads anyway.
                m_vertsInQuads.Add(verts[i + 0]);
                m_vertsInQuads.Add(verts[i + 1]);
                m_vertsInQuads.Add(verts[i + 2]);
                m_vertsInQuads.Add(verts[i + 4]);
            }
            //add quads to the list and remove the triangles
            verts.AddRange(m_vertsInQuads);
            verts.RemoveRange(0, vertsInTrisCount);
        }


        /// <summary>
        /// Subdivides a quad into 4 quads.
        /// </summary>
        /// <returns>The quad.</returns>
        /// <param name="quad">Quad.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        private Vector2 _uv0;
        private Vector2 _uv1;
        private Vector3 _pos;
        UIVertex TesselateQuad(float x, float y)
        {
            //1. calculate weighting factors
            m_weights[0] = (1 - x) * (1 - y);
            m_weights[1] = (1 - x) * y;
            m_weights[2] = x * y;
            m_weights[3] = x * (1 - y);

            //2. interpolate all the vertex properties using weighting factors
            _uv0 = _uv1 = Vector2.zero;
            _pos = Vector3.zero;

            for (int i = 0; i < 4; i++)
            {
                _uv0 += m_quad[i].uv0 * m_weights[i];
                _uv1 += m_quad[i].uv1 * m_weights[i];
                _pos += m_quad[i].position * m_weights[i];
                //normal += quad[i].normal * weights[i]; // normals should be recalculated to take the curve into account. Skipped to save performance.
                //tan += quad[i].tangent * weights[i]; // tangents should be recalculated to take the curve into account. Skipped to save performance.
            }


            //4. return output
            m_ret.position = _pos;
            //ret.color = Color32.Lerp(Color32.Lerp(quad[3].color, quad[1].color, y), Color32.Lerp(quad[0].color, quad[2].color, y), x);
            m_ret.color = m_quad[0].color; //used instead to save performance. Color lerps are expensive.
            m_ret.uv0 = _uv0;
            m_ret.uv1 = _uv1;
            m_ret.normal = m_quad[0].normal;
            m_ret.tangent = m_quad[0].tangent;

            return m_ret;
        }
        #endregion




        #region PUBLIC

        /// <summary>
        /// Force Mesh to be rebuild during canvas' next update loop.
        /// </summary>
        public void SetDirty()
        {
            TesselationRequired = true;
        }

        /// <summary>
        /// Force vertices to be tesselated again from original vertices.
        /// Set by CurvedUIVertexEffect when updating object's visual property.
        /// </summary>
        public bool TesselationRequired
        {
            get { return tesselationRequired; }
            set { tesselationRequired = value; }
        }

        /// <summary>
        /// Force vertices to be repositioned on the curved canvas.
        /// set by CurvedUIVertexEffect when moving UI objects on canvas.
        /// </summary>
        public bool CurvingRequired
        {
            get { return curvingRequired; }
            set { curvingRequired = value; }
        }

        #endregion


    }// end of class
} //end of namespace
