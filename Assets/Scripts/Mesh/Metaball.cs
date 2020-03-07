// taken from http://wiki.unity3d.com/index.php?title=MetaBalls&oldid=16461
/**
 * Metaball implementation by Brian R. Cowan http://www.briancowan.net/ 
 * Metaball tables at http://local.wasp.uwa.edu.au/~pbourke/geometry/polygonise/
 * Examples at http://www.briancowan.net/unity/fx
 *
 * Code provided as-is. You agree by using this code that I am not liable for any damage
 * it could possibly cause to you, your machine, or anything else. And the code is not meant
 * to be used for any medical uses or to run nuclear reactors or robots or such and so. 
 * 
 * Should be easily portable to any other language, all Unity Specific code is labeled so,
 * adapt it to any other environment. To use, attach the script to an empty object with a
 * Mesh Renderer and Mesh Filter. The created cubes will be one unit in size in XYZ.
 * Modify Update() and Start() to change the amount and movement and size of the blobs.
 *
 * Id love to see any project you use the code in.
 *
 * Mail any comments to: brian@briancowan.net (Vector or Vector426 on #otee-unity)
 *
 * Cheers & God bless
 */

/**
*There is a small bug in this code that produces 1 to 4 overlapping triangles 
*mostly in the centre of the mesh, sometimes on a side resulting in a white patch
*it appears in other shapes than Metaballs.
*if it's too hard to fix the code, scanning duplicate triangles is possible
*the duplicates occur in the 1st 4 triangles and anywhere after
*it seems the code doesn't check duplicates for the 1st 4 triangles
*the above issue is probably relate to the march() function doCube 1x time
*and then sending recurse() function docube in the same space
* so perhaps delete   if(doCube(cube)) {  condition in march() and just recurse
*also it processes 278700 pts instead of 32768 pts if made to march all spaces
*/

/*Unity Specific*/
using UnityEngine;
using System.Collections;

namespace UnityLibrary
{
    public class Metaball : MonoBehaviour
    {

        /*Amount of cubes in X/Y/Z directions, Dimension will always be from -.5f to .5f in XYZ
          remember to call Regen() if changing!
        */
        int _dimX = 30;
        int _dimY = 30;
        int _dimZ = 30;

        public int dimX
        {
            get { return _dimX; }
            set { _dimX = value; Regen(); }
        }
        public int dimY
        {
            get { return _dimY; }
            set { _dimY = value; Regen(); }
        }
        public int dimZ
        {
            get { return _dimZ; }
            set { _dimZ = value; Regen(); }
        }
        /*Blobs are a staggered array of floats, where first index is blob, and second is 0=x, 1=y 2=z 3=power
          Multidim might be slightly faster, but staggered made the code a little cleaner IMO*/
        public float[][] blobs;

        /*Cutoff intensity, where the surface of mesh will be created*/
        public float isoLevel = .5f;

        /*Scratch buffers for Vertices/Normals/Tris */
        private Vector3[] newVertex;
        private Vector3[] newNormal;
        private Vector2[] newUV;
        private int[] newTri;

        /*Pointer into scratch buffers for tris and vertices(also used for UVs and Normals)
          at the end of a frame it will give the total amount of each*/
        private int triP = 0;
        private int vertP = 0;

        /*Generated at startup for dimX,dimY,dimZ, 
          all the points, edges, cubes in the 3D lattice*/
        private mcPoint[] _points;
        private mcEdge[] _edges;
        private mcCube[] _cubes;


        /*Scratch buffers for use within functions, to eliminate the usage of new almost entirely each frame*/
        private Vector3[] tada;
        private Vector2[] tada2;
        private int tadac, tadac2;
        private int tadam = 50000;

        /*Current frame counter*/
        private int pctr = 0;

        /*Cube Class*/
        private class mcCube
        {
            public mcCube()
            {
                cntr = 0;
                edges = new mcEdge[12];
                for (int i = 0; i < 12; i++)
                {
                    edges[i] = null;
                }
                points = new mcPoint[8];
            }

            /*12 Edges, see march() for their positioning*/
            public mcEdge[] edges;

            /*8 Points, see march() for their positioning*/
            public mcPoint[] points;

            /*last frame this cube was processed*/
            public int cntr;

            /*Pointers into the latice array*/
            public int px;
            public int py;
            public int pz;
        }

        /*Edge class*/
        private class mcEdge
        {
            /*the vector of the calculated point*/
            public Vector3 v3;

            /*index into newVertex/Normal/Uv of calculated point*/
            public int vi;

            /*Last frame this was calculated at*/
            public int cntr;

            /*axis of edge*/
            public int axisI;

            public mcEdge(int axisI)
            {
                this.cntr = 0;
                this.axisI = axisI;
            }
        }

        /*Point (in lattice) class*/
        public class mcPoint
        {
            /*Calculated Intensity or Power of point*/
            public float _i;
            public int px, py, pz;
            private Metaball mcblob;
            public int cntr;

            /*Object Space position of point*/
            public float[] index;

            public mcPoint(float x, float y, float z, int px, int py, int pz, Metaball thismcblob)
            {
                this.index = new float[3];
                index[0] = x; index[1] = y; index[2] = z;

                this.px = px;
                this.py = py;
                this.pz = pz;
                this.cntr = 0;
                this.mcblob = thismcblob;
            }

            /*Axis letter accessors*/
            public float x
            {
                get { return index[0]; }
                set { index[0] = value; }
            }
            public float y
            {
                get { return index[1]; }
                set { index[1] = value; }
            }
            public float z
            {
                get { return index[2]; }
                set { index[2] = value; }
            }

            /*Calculate the power of a point only if it hasn't been calculated already for this frame*/
            public float i()
            {
                float pwr;
                if (cntr < mcblob.pctr)
                {
                    cntr = mcblob.pctr;
                    pwr = 0f;
                    for (int jc = 0; jc < this.mcblob.blobs.Length; jc++)
                    {
                        float[] pb = this.mcblob.blobs[jc];
                        pwr += (1.0f / Mathf.Sqrt(((pb[0] - this.x) * (pb[0] - this.x)) + ((pb[1] - this.y) * (pb[1] - this.y)) + ((pb[2] - this.z) * (pb[2] - this.z)))) * pb[3];
                    }
                    this._i = pwr;
                }
                return this._i;
            }

            public float this[int idx]
            {
                get
                {
                    return index[idx];
                }
                set
                {
                    index[idx] = value;
                }
            }
        }

        /* Normals are calculated by 'averaging' all the derivatives of the Blob power functions*/
        private Vector3 calcNormal(Vector3 pnt)
        {
            int jc;
            Vector3 result = tada[tadac++];
            result.x = 0; result.y = 0; result.z = 0;
            for (jc = 0; jc < blobs.Length; jc++)
            {
                float[] pb = blobs[jc];

                Vector3 current = tada[tadac++];
                current.x = pnt.x - pb[0];
                current.y = pnt.y - pb[1];
                current.z = pnt.z - pb[2];
                float mag = current.magnitude;
                float pwr = .5f * (1f / (mag * mag * mag)) * pb[3];
                result = result + (current * pwr);
            }
            return result.normalized;
        }

        /*Given xyz indices into lattice, return referring cube */
        private mcCube getCube(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= dimX || y >= dimY || z >= dimZ) { return null; }
            return _cubes[z + (y * (dimZ)) + (x * (dimZ) * (dimY))];
        }

        /*Given xyz indices into lattice, return referring vertex */
        private mcPoint getPoint(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x > dimX || y > dimY || z > dimZ) { return null; }
            return _points[z + (y * (dimZ + 1)) + (x * (dimZ + 1) * (dimY + 1))];
        }

        /*Return the interpolated position of point on an Axis*/
        private Vector3 mPos(mcPoint a, mcPoint b, int axisI)
        {
            float mu = (isoLevel - a.i()) / (b.i() - a.i());
            Vector3 tmp = tada[tadac++];
            tmp[0] = a[0]; tmp[1] = a[1]; tmp[2] = a[2];
            tmp[axisI] = a[axisI] + (mu * (b[axisI] - a[axisI]));

            return tmp;
        }

        /*If an edge of a cube has not been processed, find the interpolated point for 
          that edge (assumes the boundary crosses the edge) and compute the normal
          for that point, as well as assigning it an index into the vertex list*/
        private void genEdge(mcCube cube, int edgei, int p1i, int p2i)
        {
            Vector3 v;
            mcEdge e = cube.edges[edgei];
            if (e.cntr < pctr)
            {

                v = mPos(cube.points[p1i], cube.points[p2i], e.axisI);
                e.v3 = v;
                e.vi = vertP;
                newNormal[vertP] = calcNormal(v);
                newVertex[vertP++] = v;
                e.cntr = pctr;

            }
        }

        /*Calculate a cube:
          First set a boolean pointer made up of all the vertices within the cube
          then (if not all in or out of the surface) go through all the edges that 
          are crossed by the surface and make sure that a vertex&normal is assigned 
          at the point of crossing. Then add all the triangles that cover the surface
          within the cube.
          Returns true if the surface crosses the cube, false otherwise.*/
        private bool doCube(mcCube cube)
        {
            int edgec, vertc;
            edgec = 0; vertc = 0;

            int cubeIndex = 0;

            if (cube.points[0].i() > isoLevel) { cubeIndex |= 1; }
            if (cube.points[1].i() > isoLevel) { cubeIndex |= 2; }
            if (cube.points[2].i() > isoLevel) { cubeIndex |= 4; }
            if (cube.points[3].i() > isoLevel) { cubeIndex |= 8; }
            if (cube.points[4].i() > isoLevel) { cubeIndex |= 16; }
            if (cube.points[5].i() > isoLevel) { cubeIndex |= 32; }
            if (cube.points[6].i() > isoLevel) { cubeIndex |= 64; }
            if (cube.points[7].i() > isoLevel) { cubeIndex |= 128; }

            int edgeIndex = edgeTable[cubeIndex];
            edgec += edgeIndex;
            if (edgeIndex != 0)
            {
                if ((edgeIndex & 1) > 0) { genEdge(cube, 0, 0, 1); }
                if ((edgeIndex & 2) > 0) { genEdge(cube, 1, 1, 2); }
                if ((edgeIndex & 4) > 0) { genEdge(cube, 2, 2, 3); }
                if ((edgeIndex & 0x8) > 0) { genEdge(cube, 3, 3, 0); }
                if ((edgeIndex & 0x10) > 0) { genEdge(cube, 4, 4, 5); }
                if ((edgeIndex & 0x20) > 0) { genEdge(cube, 5, 5, 6); }
                if ((edgeIndex & 0x40) > 0) { genEdge(cube, 6, 6, 7); }
                if ((edgeIndex & 0x80) > 0) { genEdge(cube, 7, 7, 4); }
                if ((edgeIndex & 0x100) > 0) { genEdge(cube, 8, 0, 4); }
                if ((edgeIndex & 0x200) > 0) { genEdge(cube, 9, 1, 5); }
                if ((edgeIndex & 0x400) > 0) { genEdge(cube, 10, 2, 6); }
                if ((edgeIndex & 0x800) > 0) { genEdge(cube, 11, 3, 7); }

                int tpi = 0;
                int tmp;
                while (triTable[cubeIndex, tpi] != -1)
                {
                    tmp = cube.edges[triTable[cubeIndex, tpi + 2]].vi;
                    newTri[triP++] = tmp; vertc += tmp;
                    tmp = cube.edges[triTable[cubeIndex, tpi + 1]].vi;
                    newTri[triP++] = tmp; vertc += tmp;
                    tmp = cube.edges[triTable[cubeIndex, tpi]].vi;
                    newTri[triP++] = tmp; vertc += tmp;
                    tpi += 3;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /*Recurse all the neighboring cubes where thy contain part of the surface*/
        /*Counter to see how many cubes where processed*/
        int cubec;
        private void recurseCube(mcCube cube)
        {
            mcCube nCube;
            int jx, jy, jz;
            jx = cube.px; jy = cube.py; jz = cube.pz;
            cubec++;
            /* Test 6 axis cases. This seems to work well, no need to test all 26 cases */
            nCube = getCube(jx + 1, jy, jz);
            if (nCube != null && nCube.cntr < pctr) { nCube.cntr = pctr; if (doCube(nCube)) { recurseCube(nCube); } }
            nCube = getCube(jx - 1, jy, jz);
            if (nCube != null && nCube.cntr < pctr) { nCube.cntr = pctr; if (doCube(nCube)) { recurseCube(nCube); } }
            nCube = getCube(jx, jy + 1, jz);
            if (nCube != null && nCube.cntr < pctr) { nCube.cntr = pctr; if (doCube(nCube)) { recurseCube(nCube); } }
            nCube = getCube(jx, jy - 1, jz);
            if (nCube != null && nCube.cntr < pctr) { nCube.cntr = pctr; if (doCube(nCube)) { recurseCube(nCube); } }
            nCube = getCube(jx, jy, jz + 1);
            if (nCube != null && nCube.cntr < pctr) { nCube.cntr = pctr; if (doCube(nCube)) { recurseCube(nCube); } }
            nCube = getCube(jx, jy, jz - 1);
            if (nCube != null && nCube.cntr < pctr) { nCube.cntr = pctr; if (doCube(nCube)) { recurseCube(nCube); } }
        }

        /*Go through all the Blobs, and travel from the center outwards in a negative Z direction
        until we reach the surface, then begin to recurse around the surface. This isn't flawless
        if the blob isn't completely within the lattice boundaries in the minimal Z axis and no
        other blob that does check out is in contact with it. The blob will dissapear, but otherwise
        works well*/
        private void march()
        {
            int i, jx, jy, jz;
            for (i = 0; i < blobs.Length; i++)
            {
                float[] pb = blobs[i];
                jx = (int)((pb[0] + .5f) * dimX);
                jy = (int)((pb[1] + .5f) * dimY);
                jz = (int)((pb[2] + .5f) * dimZ);

                while (jz >= 0)
                {
                    mcCube cube = getCube(jx, jy, jz);
                    if (cube != null && cube.cntr < pctr)
                    {
                        if (doCube(cube))
                        {
                            recurseCube(cube);
                            jz = -1;
                        }
                        cube.cntr = pctr;
                    }
                    else
                    {
                        jz = -1;
                    }
                    jz -= 1;
                }
            }
        }

        /*Unity and Sample Specific, scratch caches to not reallocate vertices/tris/etc...*/
        Vector3[] fv, fn;
        int[] ft;
        Vector2[] fuv;

        //Last Status Post
        private float lt = 0f;

        /*Unity and Sample Specific*/
        private void renderMesh()
        {
            int i;

            /*Clear the Vertices that don't have any real information assigned to them */
            for (i = 0; i < vertP; i++)
            {
                fv[i] = newVertex[i]; fn[i] = newNormal[i];
                fuv[i] = tada2[tadac2++];
                Vector3 fuvt = transform.TransformPoint(fn[i]).normalized;
                fuv[i].x = (fuvt.x + 1f) * .5f; fuv[i].y = (fuvt.y + 1f) * .5f;
            }
            //							  fuv[i].x=fn[i].x;fuv[i].y=fn[i].y;}

            for (i = vertP; i < fv.Length; i++)
            {
                fv[i][0] = 0; fn[i][0] = 0; fuv[i][0] = 0;
                fv[i][1] = 0; fn[i][1] = 0; fuv[i][1] = 0;
                fv[i][2] = 0;
            }

            for (i = 0; i < triP; i++) { ft[i] = newTri[i]; }
            for (i = triP; i < ft.Length; i++) { ft[i] = 0; }

            Mesh mesh = ((MeshFilter)GetComponent("MeshFilter")).mesh;

            mesh.vertices = fv;
            mesh.uv = fuv;
            mesh.triangles = ft;
            mesh.normals = fn;

            /*For Disco Ball Effect*/
            //mesh.RecalculateNormals();	
        }

        /*What is needed to do every frame for the calculation and rendering of the Metaballs*/
        void doFrame()
        {
            tadac = 0;
            tadac2 = 0;
            cubec = 0;
            pctr++;
            triP = 0;
            vertP = 0;
            march();
            renderMesh();
        }

        /*Regenerate Lattice and Connections, when changing Dimensions of Lattice*/
        void Regen()
        {
            startObjs();
            startEngine();
        }

        //Unity and Sample specific
        void Update()
        {
            //Update FPS and counters every second
            //if (lt + 1 < Time.time)
            //{
            //	lt = Time.time;
            //	GUIText guit = (GUIText)GameObject.Find("guit").guiText;
            //	guit.text = "T:" + triP + " V:" + vertP + " C:" + cubec + " FPS:" + (int)(1f / Time.deltaTime);
            //}

            blobs[0][0] = .12f + .12f * (float)Mathf.Sin((float)Time.time * .50f);
            blobs[0][2] = .06f + .23f * (float)Mathf.Cos((float)Time.time * .2f);
            blobs[1][0] = .12f + .12f * (float)Mathf.Sin((float)Time.time * .2f);
            blobs[1][2] = -.23f + .10f * (float)Mathf.Cos((float)Time.time * 1f);
            blobs[2][1] = -.03f + .24f * (float)Mathf.Sin((float)Time.time * .35f);
            blobs[3][1] = .126f + .10f * (float)Mathf.Cos((float)Time.time * .1f);
            blobs[4][0] = .206f + .1f * (float)Mathf.Cos((float)Time.time * .5f);
            blobs[4][1] = .056f + .2f * (float)Mathf.Sin((float)Time.time * .3f);
            blobs[4][2] = .25f + .08f * (float)Mathf.Cos((float)Time.time * .2f);

            transform.Rotate(Time.deltaTime * 10f, 0, Time.deltaTime * .6f);

            doFrame();
        }

        //Unity and Sample Specific
        void Start()
        {
            lt = 0f;
            blobs = new float[5][];
            blobs[0] = new float[] { .16f, .26f, .16f, .13f };
            blobs[1] = new float[] { .13f, -.134f, .35f, .12f };
            blobs[2] = new float[] { -.18f, .125f, -.25f, .16f };
            blobs[3] = new float[] { -.13f, .23f, .255f, .13f };
            blobs[4] = new float[] { -.18f, .125f, .35f, .12f };
            isoLevel = 1.95f;
            Regen();
        }


        /*Unity Specific starting of engine*/
        void startEngine()
        {
            ((MeshFilter)GetComponent("MeshFilter")).mesh = new Mesh();
        }

        /*Generate the Cube Lattice
          All shared vertices and edges are connected across cubes,
          it's not perfect in that the edges along the lower index borders
          are not connected, but all the rest are, this shouldn't make any
          noticeable visual impact, and have no performance impact unless
          a blob lies along those borders*/
        private void startObjs()
        {
            int i;
            float jx, jy, jz;
            int ijx, ijy, ijz;
            int pointCount = ((dimX + 1) * (dimY + 1) * (dimZ + 1));
            int cubeCount = (dimX * dimY * dimZ);
            int edgeCount = (cubeCount * 3) + ((2 * dimX * dimY) + (2 * dimX * dimZ) + (2 * dimY * dimZ)) + dimX + dimY + dimZ; //Ideal Edge Count
            int edgeNow = edgeCount + ((dimX * dimY) + (dimY * dimZ) + (dimZ * dimX)) * 2; //Haven't combined the edges of the 0 index borders

            //Should be a pretty safe amount
            int tmpv = (int)(dimX * dimY * dimZ / 7);
            tadam = tmpv * 4;
            fv = new Vector3[tmpv];
            fn = new Vector3[tmpv];
            fuv = new Vector2[tmpv];

            //Pretty save amount of Tris as well
            ft = new int[(int)(cubeCount * .75)];

            newVertex = new Vector3[300000];
            newTri = new int[300000];
            newNormal = new Vector3[300000];
            tada = new Vector3[tadam * 2];
            tada2 = new Vector2[tadam * 2];

            //newUV=new Vector2[300000];
            _cubes = new mcCube[cubeCount];
            _points = new mcPoint[pointCount];
            _edges = new mcEdge[edgeNow];

            for (i = 0; i < tadam * 2; i++)
            {
                tada[i] = new Vector3(0, 0, 0);
                tada2[i] = new Vector2(0, 0);
            }

            for (i = 0; i < edgeNow; i++)
            {
                _edges[i] = new mcEdge(-1);
            }


            i = 0;
            for (jx = 0.0f; jx <= dimX; jx++)
            {
                for (jy = 0.0f; jy <= dimY; jy++)
                {
                    for (jz = 0.0f; jz <= dimZ; jz++)
                    {
                        _points[i] = new mcPoint((jx / dimX) - .5f, (jy / dimY) - .5f, (jz / dimZ) - .5f, (int)jx, (int)jy, (int)jz, this);

                        i++;
                    }
                }
            }

            for (i = 0; i < cubeCount; i++)
            {
                _cubes[i] = new mcCube();
            }
            int ep = 0;

            mcCube c;
            mcCube tc;
            i = 0;

            int topo = 0;
            for (ijx = 0; ijx < dimX; ijx++)
            {
                for (ijy = 0; ijy < dimY; ijy++)
                {
                    for (ijz = 0; ijz < dimZ; ijz++)
                    {
                        c = _cubes[i];
                        i++;
                        c.px = ijx; c.py = ijy; c.pz = ijz;

                        mcPoint[] cpt = c.points;
                        cpt[0] = getPoint(ijx, ijy, ijz);
                        cpt[1] = getPoint(ijx + 1, ijy, ijz);
                        cpt[2] = getPoint(ijx + 1, ijy + 1, ijz);
                        cpt[3] = getPoint(ijx, ijy + 1, ijz);
                        cpt[4] = getPoint(ijx, ijy, ijz + 1);
                        cpt[5] = getPoint(ijx + 1, ijy, ijz + 1);
                        cpt[6] = getPoint(ijx + 1, ijy + 1, ijz + 1);
                        cpt[7] = getPoint(ijx, ijy + 1, ijz + 1);

                        mcEdge[] e = c.edges;
                        e[5] = _edges[ep++]; e[5].axisI = 1;
                        e[6] = _edges[ep++]; e[6].axisI = 0;
                        e[10] = _edges[ep++]; e[10].axisI = 2;

                        tc = getCube(ijx + 1, ijy, ijz);
                        if (tc != null) { tc.edges[11] = e[10]; tc.edges[7] = e[5]; }

                        tc = getCube(ijx, ijy + 1, ijz);
                        if (tc != null) { tc.edges[4] = c.edges[6]; tc.edges[9] = c.edges[10]; }

                        tc = getCube(ijx, ijy + 1, ijz + 1);
                        if (tc != null) { tc.edges[0] = c.edges[6]; }

                        tc = getCube(ijx + 1, ijy, ijz + 1);
                        if (tc != null) { tc.edges[3] = c.edges[5]; }

                        tc = getCube(ijx + 1, ijy + 1, ijz);
                        if (tc != null) { tc.edges[8] = c.edges[10]; }

                        tc = getCube(ijx, ijy, ijz + 1);
                        if (tc != null) { tc.edges[1] = c.edges[5]; tc.edges[2] = c.edges[6]; }

                        if (e[0] == null)
                        {
                            e[0] = _edges[ep++]; e[0].axisI = 0;
                        }
                        if (e[1] == null)
                        {
                            e[1] = _edges[ep++]; e[1].axisI = 1;
                        }
                        if (e[2] == null)
                        {
                            e[2] = _edges[ep++]; e[2].axisI = 0;
                        }
                        else { topo++; }
                        if (e[3] == null)
                        {
                            e[3] = _edges[ep++]; e[3].axisI = 1;
                        }
                        if (e[4] == null)
                        {
                            e[4] = _edges[ep++]; e[4].axisI = 0;
                        }
                        if (e[7] == null)
                        {
                            e[7] = _edges[ep++]; e[7].axisI = 1;
                        }
                        if (e[8] == null)
                        {
                            e[8] = _edges[ep++]; e[8].axisI = 2;
                        }
                        if (e[9] == null)
                        {
                            e[9] = _edges[ep++]; e[9].axisI = 2;
                        }
                        if (e[11] == null)
                        {
                            e[11] = _edges[ep++]; e[11].axisI = 2;
                        }
                    }
                }
            }
        }

        /*Courtesy of http://local.wasp.uwa.edu.au/~pbourke/geometry/polygonise/*/
        private int[,] triTable = new int[,]
            {{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        {3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        {3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        {3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
        {9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        {8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
        {3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
        {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
        {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        {4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        {5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
        {9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        {0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
        {10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
        {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        {5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        {9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        {1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
        {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
        {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
        {2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        {7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
        {11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
        {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
        {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
        {11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
        {9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
        {2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
        {6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
        {6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        {6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
        {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
        {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
        {3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
        {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
        {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
        {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
        {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
        {10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},


        {10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        {1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
        {0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        {10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
        {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
        {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
        {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
        {3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        {6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
        {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
        {10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
        {7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        {7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
        {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
        {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
        {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
        {0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
        {7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        {10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        {2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
        {7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        {2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
        {10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
        {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
        {7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        {6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
        {8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
        {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
        {6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
        {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
        {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
        {8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        {0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
        {1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},


        {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
        {10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
        {10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
        {9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
        {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
        {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
        {7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
        {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
        {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
        {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
        {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
        {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
        {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
        {6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
        {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
        {6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
        {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
        {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
        {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
        {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
        {9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
        {1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
        {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
        {0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
        {5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
        {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
        {11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
        {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
        {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
        {2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
        {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
        {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
        {1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        {9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
        {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
        {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
        {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
        {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
        {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
        {9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
        {5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
        {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
        {8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        {0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
        {9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
        {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
        {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
        {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
        {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
        {11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
        {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
        {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
        {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
        {1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
        {4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        {3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
        {0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
        {1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}};


        private int[] edgeTable = new int[] {
        0x0  , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
        0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
        0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
        0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
        0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
        0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
        0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
        0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
        0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
        0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
        0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
        0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
        0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
        0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
        0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
        0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
        0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
        0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
        0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
        0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
        0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
        0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
        0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
        0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
        0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
        0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
        0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
        0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
        0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
        0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
        0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
        0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0   };
    }
}
